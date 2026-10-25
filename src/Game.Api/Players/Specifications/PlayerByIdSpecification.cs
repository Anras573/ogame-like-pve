using System.Linq.Expressions;
using Game.Api.Infrastructure.Specifications;

namespace Game.Api.Players.Specifications;

public sealed class PlayerByIdSpecification(PlayerId playerId) : Specification<Player>
{
    public override Expression<Func<Player, bool>> Criteria => p => p.Id == playerId;
}