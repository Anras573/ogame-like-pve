using System.Linq.Expressions;
using Game.Api.Infrastructure.Specifications;
using Game.Api.Seasons;

namespace Game.Api.Planets.Specifications;

public sealed class PlanetsInSystemSpecification(SeasonId seasonId, int galaxy, int solarSystem) : Specification<Planet>
{
    public override Expression<Func<Planet, bool>> Criteria =>
        p => p.SeasonId == seasonId && p.Galaxy == galaxy && p.SolarSystem == solarSystem;
}