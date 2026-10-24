using System.Security.Claims;
using Game.Api.Infrastructure.Repositories;
using Game.Api.Players;
using Game.Api.Players.Specifications;
using Game.Api.Users;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Game.Api.Seasons;

public sealed class JoinCurrentSeasonEndpoint
{
    public static async Task<IResult> Handle(
        ClaimsPrincipal principal,
        IRepository<Season> seasonRepository,
        IRepository<Player> playerRepository,
        TimeProvider timeProvider,
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

        var player = Player.Join(season, userId, timeProvider.GetUtcNow());
        if (player is null)
        {
            return Results.NotFound(); // Season closed between lookup and join
        }

        try
        {
            await playerRepository.AddAsync(player, cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return Results.Conflict(); // Lost a race with a concurrent join request
        }

        return Results.Created(SeasonEndpoints.GetCurrentSeasonRoute, new SeasonPlayerDto(player.Id.Value, player.JoinedAt));
    }
}