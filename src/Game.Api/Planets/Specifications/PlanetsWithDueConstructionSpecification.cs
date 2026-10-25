using System.Linq.Expressions;
using Game.Api.Infrastructure.Specifications;

namespace Game.Api.Planets.Specifications;

public sealed class PlanetsWithDueConstructionSpecification : Specification<Planet>
{
    private readonly DateTimeOffset _now;

    public PlanetsWithDueConstructionSpecification(DateTimeOffset now, int batchSize)
    {
        _now = now;

        // Oldest first, so nothing starves when there's a backlog
        ApplyOrderBy(p => p.Construction!.CompletesAt);
        ApplyTake(batchSize);
    }

    public override Expression<Func<Planet, bool>> Criteria =>
        p => p.Construction != null && p.Construction.CompletesAt <= _now;
}