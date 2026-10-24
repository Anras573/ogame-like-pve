using System.Linq.Expressions;
using Game.Api.Infrastructure.Specifications;
using Game.Api.Seasons;

namespace Game.Api.Players.Specifications;

public sealed class PlayerByUserAndSeasonSpecification(Guid userId, SeasonId seasonId) : Specification<Player>
{
    public override Expression<Func<Player, bool>> Criteria =>
        p => p.UserId == userId && p.SeasonId == seasonId;
}