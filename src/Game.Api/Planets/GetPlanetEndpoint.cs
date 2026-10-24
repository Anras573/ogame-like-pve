using System.Security.Claims;
using Game.Api.Infrastructure.Repositories;
using Game.Api.Players;
using Game.Api.Seasons;

namespace Game.Api.Planets;

public sealed class GetPlanetEndpoint
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

        // Settle in memory for an up-to-date view; nothing is saved, so reading never races with the worker
        var now = timeProvider.GetUtcNow();
        owned.Planet.SettleTo(now, owned.Season.EconomySpeed);

        return Results.Ok(PlanetDto.From(owned.Planet, owned.Season.EconomySpeed, now));
    }
}