using System.Linq.Expressions;
using Game.Api.Infrastructure.Specifications;

namespace Game.Api.Seasons.Specifications;

public sealed class SeasonsOpenAtSpecification(DateTimeOffset moment) : Specification<Season>
{
    public override Expression<Func<Season, bool>> Criteria =>
        s => s.StartsAt <= moment && s.EndsAt > moment;
}