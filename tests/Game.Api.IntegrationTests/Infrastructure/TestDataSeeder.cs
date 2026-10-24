using Game.Api.Infrastructure;
using Game.Api.Seasons;
using Game.Core.Universe;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Api.IntegrationTests.Infrastructure;

public static class TestDataSeeder
{
    public static async Task<Season> AddSeasonAsync(
        this CustomWebApplicationFactory factory,
        string name,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        UniverseSettings? universe = null)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();

        var season = Season.Create(name, startsAt, endsAt, universe);
        context.Seasons.Add(season);
        await context.SaveChangesAsync();

        return season;
    }

    /// <summary>
    /// Adds a season that opened an hour ago and runs for twelve weeks.
    /// </summary>
    public static Task<Season> AddOpenSeasonAsync(
        this CustomWebApplicationFactory factory,
        string name = "Season 1",
        UniverseSettings? universe = null)
    {
        var now = factory.TimeProvider.GetUtcNow();
        return factory.AddSeasonAsync(name, now.AddHours(-1), now.AddDays(84), universe);
    }
}