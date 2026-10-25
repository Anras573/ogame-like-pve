using Game.Api.Infrastructure.Repositories;
using Game.Api.Players.Specifications;
using Game.Api.Realtime;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Game.Api.Players;

/// <summary>
/// Completes research whose time has come and tells the player. Research also completes lazily whenever the
/// player reads or acts, so this is about persisting the change and notifying promptly, not correctness.
/// </summary>
public sealed class ResearchCompletionProcessor(
    IServiceScopeFactory scopeFactory,
    IHubContext<GameHub, IGameClient> hubContext,
    TimeProvider timeProvider,
    ILogger<ResearchCompletionProcessor> logger) : ICompletionProcessor
{
    private const int BatchSize = 100;

    public async Task<int> ProcessDueAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        List<PlayerId> due;
        using (var scope = scopeFactory.CreateScope())
        {
            var players = scope.ServiceProvider.GetRequiredService<IRepository<Player>>();
            due = [.. (await players.ListAsync(new PlayersWithDueResearchSpecification(now, BatchSize), cancellationToken)).Select(p => p.Id)];
        }

        var completedCount = 0;
        foreach (var playerId in due)
        {
            // A fresh scope per player, so one failed save doesn't leave stale changes behind for the next
            if (await CompleteAsync(playerId, now, cancellationToken))
            {
                completedCount++;
            }
        }

        return completedCount;
    }

    private async Task<bool> CompleteAsync(PlayerId playerId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var players = scope.ServiceProvider.GetRequiredService<IRepository<Player>>();

        var player = await players.FirstOrDefaultAsync(new PlayerByIdSpecification(playerId), cancellationToken);
        var completed = player?.SettleResearch(now);
        if (player is null || completed is null)
        {
            return false; // A player action got there first
        }

        try
        {
            await players.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogDebug("Player {PlayerId} changed while completing research; it was settled by someone else", playerId.Value);
            return false;
        }

        await hubContext.Clients.User(player.UserId.ToString()).ResearchCompleted(
            new ResearchCompletedMessage(completed.Research, completed.TargetLevel));

        return true;
    }
}