using System.Linq.Expressions;
using Game.Api.Infrastructure.Specifications;

namespace Game.Api.Planets.Specifications;

public sealed class PlanetByIdSpecification(PlanetId planetId) : Specification<Planet>
{
    public override Expression<Func<Planet, bool>> Criteria => p => p.Id == planetId;
}