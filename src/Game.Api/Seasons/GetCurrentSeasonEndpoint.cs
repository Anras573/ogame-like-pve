using System.Security.Claims;
using Game.Api.Infrastructure.Repositories;
using Game.Api.Players;
using Game.Api.Players.Specifications;
using Game.Api.Users;

namespace Game.Api.Seasons;

public sealed class GetCurrentSeasonEndpoint
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

        // Has this user already joined?
        var player = await playerRepository.FirstOrDefaultAsync(
            new PlayerByUserAndSeasonSpecification(userId, season.Id),
            cancellationToken);

        return Results.Ok(new CurrentSeasonDto(
            season.Id.Value,
            season.Name,
            season.StartsAt,
            season.EndsAt,
            player is null ? null : new SeasonPlayerDto(player.Id.Value, player.JoinedAt)));
    }
}

public record CurrentSeasonDto(Guid Id, string Name, DateTimeOffset StartsAt, DateTimeOffset EndsAt, SeasonPlayerDto? Player);
public record SeasonPlayerDto(Guid PlayerId, DateTimeOffset JoinedAt);