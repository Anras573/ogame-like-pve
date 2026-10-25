using System.Security.Claims;
using Game.Api.Infrastructure.Repositories;
using Game.Api.Planets;
using Game.Api.Players;
using Game.Api.Seasons;
using Game.Core.Research;
using Microsoft.EntityFrameworkCore;

namespace Game.Api.Research;

public sealed class StartResearchEndpoint
{
    public static async Task<IResult> Handle(
        Guid planetId,
        StartResearchRequest request,
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

        var result = owned.Player.StartResearch(request.Research, owned.Planet, now, owned.EconomySpeed);
        switch (result)
        {
            case StartResearchResult.AlreadyResearching:
                return PlanetProblems.AlreadyResearching();
            case StartResearchResult.RequirementsNotMet:
                return PlanetProblems.RequirementsNotMet();
            case StartResearchResult.NotEnoughResources:
                return PlanetProblems.NotEnoughResources();
        }

        // The player and the planet that paid are saved together
        try
        {
            await playerRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return PlanetProblems.ChangedConcurrently();
        }

        return Results.Ok(ResearchOverviewDto.From(owned, now));
    }
}

public record StartResearchRequest(ResearchType Research);