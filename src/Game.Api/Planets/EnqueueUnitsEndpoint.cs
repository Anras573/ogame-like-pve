using System.Security.Claims;
using Game.Api.Infrastructure.Repositories;
using Game.Api.Players;
using Game.Api.Seasons;
using Game.Core.Units;
using Microsoft.EntityFrameworkCore;

namespace Game.Api.Planets;

public sealed class EnqueueUnitsEndpoint
{
    public static async Task<IResult> Handle(
        Guid planetId,
        EnqueueUnitsRequest request,
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
        if (!owned.Season.IsOpenAt(now))
        {
            return PlanetProblems.SeasonClosed();
        }

        owned.SettleTo(now);
        var result = owned.Planet.EnqueueUnits(request.Unit, request.Quantity, owned.Player.ResearchLevels, now, owned.EconomySpeed);
        switch (result)
        {
            case EnqueueUnitsResult.InvalidQuantity:
                return PlanetProblems.InvalidQuantity();
            case EnqueueUnitsResult.QueueFull:
                return PlanetProblems.ShipyardQueueFull();
            case EnqueueUnitsResult.RequirementsNotMet:
                return PlanetProblems.RequirementsNotMet();
            case EnqueueUnitsResult.LimitReached:
                return PlanetProblems.UnitLimitReached();
            case EnqueueUnitsResult.NotEnoughResources:
                return PlanetProblems.NotEnoughResources();
        }

        try
        {
            await planetRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return PlanetProblems.ChangedConcurrently();
        }

        return Results.Ok(PlanetDto.From(owned, now));
    }
}

public record EnqueueUnitsRequest(UnitType Unit, int Quantity);