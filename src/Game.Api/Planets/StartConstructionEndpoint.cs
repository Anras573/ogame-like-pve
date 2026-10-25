using System.Security.Claims;
using Game.Api.Infrastructure.Repositories;
using Game.Api.Players;
using Game.Api.Seasons;
using Game.Core.Buildings;
using Microsoft.EntityFrameworkCore;

namespace Game.Api.Planets;

public sealed class StartConstructionEndpoint
{
    public static async Task<IResult> Handle(
        Guid planetId,
        StartConstructionRequest request,
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
        var result = owned.Planet.StartConstruction(request.Building, owned.Player.ResearchLevels, now, owned.EconomySpeed);
        switch (result)
        {
            case StartConstructionResult.AlreadyBuilding:
                return PlanetProblems.AlreadyBuilding();
            case StartConstructionResult.RequirementsNotMet:
                return PlanetProblems.RequirementsNotMet();
            case StartConstructionResult.NotEnoughResources:
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

public record StartConstructionRequest(BuildingType Building);