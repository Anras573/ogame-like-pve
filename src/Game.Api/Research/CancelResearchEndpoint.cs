using System.Security.Claims;
using Game.Api.Infrastructure.Repositories;
using Game.Api.Planets;
using Game.Api.Players;
using Game.Api.Seasons;
using Microsoft.EntityFrameworkCore;

namespace Game.Api.Research;

public sealed class CancelResearchEndpoint
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

        // Finished research can't be cancelled; settling first makes sure of that
        owned.SettleTo(now);
        if (owned.Player.CurrentResearch is null)
        {
            return PlanetProblems.NoResearchToCancel();
        }

        if (owned.Player.CurrentResearch.PlanetId != owned.Planet.Id)
        {
            return PlanetProblems.ResearchOnOtherPlanet();
        }

        owned.Player.CancelResearch(owned.Planet, now, owned.EconomySpeed);

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