using System.Linq.Expressions;
using Game.Api.Infrastructure.Specifications;

namespace Game.Api.Seasons.Specifications;

public sealed class SeasonByIdSpecification(SeasonId seasonId) : Specification<Season>
{
    public override Expression<Func<Season, bool>> Criteria => s => s.Id == seasonId;
}