using System.Linq.Expressions;
using Game.Api.Infrastructure.Specifications;

namespace Game.Api.Players.Specifications;

public sealed class PlayersWithDueResearchSpecification : Specification<Player>
{
    private readonly DateTimeOffset _now;

    public PlayersWithDueResearchSpecification(DateTimeOffset now, int batchSize)
    {
        _now = now;

        // Oldest first, so nothing starves when there's a backlog
        ApplyOrderBy(p => p.CurrentResearch!.CompletesAt);
        ApplyTake(batchSize);
    }

    public override Expression<Func<Player, bool>> Criteria =>
        p => p.CurrentResearch != null && p.CurrentResearch.CompletesAt <= _now;
}