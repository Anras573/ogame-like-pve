using Game.Api.Seasons;
using Game.Api.Seasons.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Game.Api.Infrastructure;

/// <summary>
/// Development-only convenience: applies migrations and makes sure there is an open season to join.
/// Production schema changes and season scheduling are explicit operations, not startup side effects.
/// </summary>
public static class DatabaseInitializer
{
    private static readonly TimeSpan s_defaultSeasonLength = TimeSpan.FromDays(7 * 12);

    public static async Task InitializeDevelopmentDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<GameDbContext>>();

        await context.Database.MigrateAsync(cancellationToken);

        var now = timeProvider.GetUtcNow();
        var spec = new SeasonsOpenAtSpecification(now);
        if (await context.Seasons.AnyAsync(spec.Criteria, cancellationToken))
        {
            return;
        }

        var season = Season.Create("Development Season", now, now.Add(s_defaultSeasonLength));
        context.Seasons.Add(season);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded development season {SeasonId} ending {EndsAt}", season.Id.Value, season.EndsAt);
    }
}