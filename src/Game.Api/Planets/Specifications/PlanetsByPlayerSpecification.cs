using System.Linq.Expressions;
using Game.Api.Infrastructure.Specifications;
using Game.Api.Players;

namespace Game.Api.Planets.Specifications;

public sealed class PlanetsByPlayerSpecification(PlayerId playerId) : Specification<Planet>
{
    public override Expression<Func<Planet, bool>> Criteria => p => p.PlayerId == playerId;
}