using System.Security.Claims;
using Game.Api.Infrastructure.Repositories;
using Game.Api.Planets.Specifications;
using Game.Api.Players;
using Game.Api.Players.Specifications;
using Game.Api.Seasons;
using Game.Api.Users;

namespace Game.Api.Planets;

public sealed class GetPlanetsEndpoint
{
    public static async Task<IResult> Handle(
        ClaimsPrincipal principal,
        IRepository<Season> seasonRepository,
        IRepository<Player> playerRepository,
        IRepository<Planet> planetRepository,
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
            return Results.Ok(new List<PlanetSummaryDto>());
        }

        var player = await playerRepository.FirstOrDefaultAsync(
            new PlayerByUserAndSeasonSpecification(userId, season.Id),
            cancellationToken);
        if (player is null)
        {
            return Results.Ok(new List<PlanetSummaryDto>()); // Hasn't joined this season yet
        }

        var planets = await planetRepository.ListAsync(new PlanetsByPlayerSpecification(player.Id), cancellationToken);

        return Results.Ok(planets.OrderBy(p => p.Name).Select(PlanetSummaryDto.From).ToList());
    }
}