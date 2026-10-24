using System.Security.Claims;
using Game.Api.Infrastructure.Repositories;
using Game.Api.Players;
using Game.Api.Seasons;
using Microsoft.EntityFrameworkCore;

namespace Game.Api.Planets;

public sealed class CancelConstructionEndpoint
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

        var now = timeProvider.GetUtcNow();

        // Already finished constructions can't be cancelled; settling first makes sure of that
        var cancelled = owned.Planet.CancelConstruction(now, owned.Season.EconomySpeed);
        if (cancelled is null)
        {
            return PlanetProblems.NothingToCancel();
        }

        try
        {
            await planetRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return PlanetProblems.ChangedConcurrently();
        }

        return Results.Ok(PlanetDto.From(owned.Planet, owned.Season.EconomySpeed, now));
    }
}