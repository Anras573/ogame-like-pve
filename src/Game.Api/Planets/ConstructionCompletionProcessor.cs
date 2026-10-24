using Game.Api.Infrastructure.Repositories;
using Game.Api.Planets.Specifications;
using Game.Api.Players;
using Game.Api.Players.Specifications;
using Game.Api.Realtime;
using Game.Api.Seasons;
using Game.Api.Seasons.Specifications;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Game.Api.Planets;

/// <summary>
/// Completes constructions whose time has come and tells their owners. Planets also complete lazily whenever
/// they're read or acted on, so this is about persisting the change and notifying promptly, not correctness.
/// </summary>
public sealed class ConstructionCompletionProcessor(
    IServiceScopeFactory scopeFactory,
    IHubContext<GameHub, IGameClient> hubContext,
    TimeProvider timeProvider,
    ILogger<ConstructionCompletionProcessor> logger)
{
    private const int BatchSize = 100;

    /// <returns>How many constructions were completed.</returns>
    public async Task<int> ProcessDueAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        List<PlanetId> due;
        using (var scope = scopeFactory.CreateScope())
        {
            var planets = scope.ServiceProvider.GetRequiredService<IRepository<Planet>>();
            due = [.. (await planets.ListAsync(new PlanetsWithDueConstructionSpecification(now, BatchSize), cancellationToken)).Select(p => p.Id)];
        }

        var completedCount = 0;
        foreach (var planetId in due)
        {
            // A fresh scope per planet, so one failed save doesn't leave stale changes behind for the next
            if (await CompleteAsync(planetId, now, cancellationToken))
            {
                completedCount++;
            }
        }

        return completedCount;
    }

    private async Task<bool> CompleteAsync(PlanetId planetId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var planets = scope.ServiceProvider.GetRequiredService<IRepository<Planet>>();
        var players = scope.ServiceProvider.GetRequiredService<IRepository<Player>>();
        var seasons = scope.ServiceProvider.GetRequiredService<IRepository<Season>>();

        var planet = await planets.FirstOrDefaultAsync(new PlanetByIdSpecification(planetId), cancellationToken);
        var season = planet is null ? null : await seasons.FirstOrDefaultAsync(new SeasonByIdSpecification(planet.SeasonId), cancellationToken);
        if (planet is null || season is null)
        {
            return false;
        }

        var completed = planet.SettleTo(now, season.EconomySpeed);
        if (completed is null)
        {
            return false; // A player action got there first
        }

        try
        {
            await planets.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogDebug("Planet {PlanetId} changed while completing its construction; it was settled by someone else", planetId.Value);
            return false;
        }

        var player = await players.FirstOrDefaultAsync(new PlayerByIdSpecification(planet.PlayerId), cancellationToken);
        if (player is not null)
        {
            await hubContext.Clients.User(player.UserId.ToString()).BuildingCompleted(
                new BuildingCompletedMessage(planet.Id.Value, completed.Building, completed.TargetLevel));
        }

        return true;
    }
}