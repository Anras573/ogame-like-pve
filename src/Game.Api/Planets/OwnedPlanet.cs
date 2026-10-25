using System.Security.Claims;
using Game.Api.Infrastructure.Repositories;
using Game.Api.Planets.Specifications;
using Game.Api.Players;
using Game.Api.Players.Specifications;
using Game.Api.Seasons;
using Game.Api.Seasons.Specifications;
using Game.Api.Users;

namespace Game.Api.Planets;

/// <summary>
/// A planet together with the season whose rules (economy speed, dates) apply to it.
/// </summary>
public sealed record OwnedPlanet(Planet Planet, Season Season)
{
    /// <summary>
    /// Loads <paramref name="planetId"/> if it belongs to the signed-in user; otherwise <c>null</c>, so other
    /// players' planets look exactly like planets that don't exist.
    /// </summary>
    public static async Task<OwnedPlanet?> LoadAsync(
        ClaimsPrincipal principal,
        Guid planetId,
        IRepository<Planet> planetRepository,
        IRepository<Player> playerRepository,
        IRepository<Season> seasonRepository,
        CancellationToken cancellationToken)
    {
        if (!principal.TryGetUserId(out var userId))
        {
            return null;
        }

        var planet = await planetRepository.FirstOrDefaultAsync(new PlanetByIdSpecification(new PlanetId(planetId)), cancellationToken);
        if (planet is null)
        {
            return null;
        }

        var player = await playerRepository.FirstOrDefaultAsync(new PlayerByIdSpecification(planet.PlayerId), cancellationToken);
        if (player is null || player.UserId != userId)
        {
            return null;
        }

        var season = await seasonRepository.FirstOrDefaultAsync(new SeasonByIdSpecification(planet.SeasonId), cancellationToken);
        return season is null ? null : new OwnedPlanet(planet, season);
    }
}