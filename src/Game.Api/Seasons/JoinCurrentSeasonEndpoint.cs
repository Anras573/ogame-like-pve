using System.Security.Claims;
using Game.Api.Infrastructure.Repositories;
using Game.Api.Planets;
using Game.Api.Planets.Specifications;
using Game.Api.Players;
using Game.Api.Players.Specifications;
using Game.Api.Users;
using Game.Core.Universe;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Game.Api.Seasons;

public sealed class JoinCurrentSeasonEndpoint
{
    private const string PlayerPerSeasonIndex = "IX_Players_UserId_SeasonId";
    private const string PlanetSlotIndex = "IX_Planets_SeasonId_Galaxy_SolarSystem_Position";

    // Another player can take the slot we picked between our check and our save; that race is rare, so a few tries do
    private const int MaxSaveAttempts = 3;

    public static async Task<IResult> Handle(
        ClaimsPrincipal principal,
        IRepository<Season> seasonRepository,
        IRepository<Player> playerRepository,
        IRepository<Planet> planetRepository,
        TimeProvider timeProvider,
        Random random,
        CancellationToken cancellationToken)
    {
        if (!principal.TryGetUserId(out var userId))
        {
            return Results.Unauthorized();
        }

        var season = await CurrentSeason.FindAsync(seasonRepository, timeProvider, cancellationToken);
        if (season is null)
        {
            return Results.NotFound();
        }

        // Joining twice is a conflict, not a second player
        var existing = await playerRepository.FirstOrDefaultAsync(
            new PlayerByUserAndSeasonSpecification(userId, season.Id),
            cancellationToken);
        if (existing is not null)
        {
            return Results.Conflict();
        }

        for (var attempt = 0; attempt < MaxSaveAttempts; attempt++)
        {
            var now = timeProvider.GetUtcNow();
            var player = Player.Join(season, userId, now);
            if (player is null)
            {
                return Results.NotFound(); // Season closed between lookup and join
            }

            var coordinates = await FindFreeHomeSlotAsync(planetRepository, season, random, cancellationToken);
            if (coordinates is null)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "The universe is full",
                    detail: "There is no free slot for a new home planet in this season.");
            }

            var planet = Planet.CreateHome(
                player,
                coordinates,
                PlanetPlacement.RollMaxTemperature(coordinates.Position, random),
                now);

            // The player and their home planet are saved together: there is never a player without a planet
            playerRepository.Add(player);
            planetRepository.Add(planet);

            try
            {
                await playerRepository.SaveChangesAsync(cancellationToken);
                return Results.Created(
                    SeasonEndpoints.GetCurrentSeasonRoute,
                    new SeasonPlayerDto(player.Id.Value, player.JoinedAt));
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex, PlayerPerSeasonIndex))
            {
                return Results.Conflict(); // Lost a race with a concurrent join request
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex, PlanetSlotIndex))
            {
                // Someone took the slot first; forget this attempt and pick again
                playerRepository.Detach(player);
                planetRepository.Detach(planet);
            }
        }

        return Results.Problem(
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "Could not place your home planet",
            detail: "Please try again.");
    }

    private static async Task<Coordinates?> FindFreeHomeSlotAsync(
        IRepository<Planet> planetRepository,
        Season season,
        Random random,
        CancellationToken cancellationToken)
    {
        foreach (var (galaxy, system) in PlanetPlacement.HomeSearchOrder(season.Universe, random))
        {
            var planets = await planetRepository.ListAsync(
                new PlanetsInSystemSpecification(season.Id, galaxy, system),
                cancellationToken);

            var position = PlanetPlacement.PickFreeHomePosition([.. planets.Select(p => p.Position)], random);
            if (position is { } free)
            {
                return new Coordinates(galaxy, system, free);
            }
        }

        return null;
    }

    private static bool IsUniqueViolation(DbUpdateException exception, string constraintName)
    {
        return exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgres
            && postgres.ConstraintName == constraintName;
    }
}