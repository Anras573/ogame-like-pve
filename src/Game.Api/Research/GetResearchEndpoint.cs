using System.Security.Claims;
using Game.Api.Infrastructure.Repositories;
using Game.Api.Planets;
using Game.Api.Players;
using Game.Api.Seasons;

namespace Game.Api.Research;

public sealed class GetResearchEndpoint
{
    public static async Task<IResult> Handle(
        Guid planetId,
        ClaimsPrincipal principal,
        IRepository<Planet> planetRepository,
        IRepository<Player> playerRepository,
        IRepository<Season> seasonRepository,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var owned = await OwnedPlanet.LoadAsync(principal, planetId, planetRepository, playerRepository, seasonRepository, cancellationToken);
        if (owned is null)
        {
            return Results.NotFound();
        }

        // Settled in memory only, like reading a planet
        var now = timeProvider.GetUtcNow();
        owned.SettleTo(now);

        return Results.Ok(ResearchOverviewDto.From(owned, now));
    }
}