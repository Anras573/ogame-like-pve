namespace Game.Api.Planets;

/// <summary>
/// Runs <see cref="ConstructionCompletionProcessor"/> every second.
/// </summary>
public sealed class ConstructionCompletionWorker(
    ConstructionCompletionProcessor processor,
    TimeProvider timeProvider,
    ILogger<ConstructionCompletionWorker> logger) : BackgroundService
{
    private static readonly TimeSpan s_interval = TimeSpan.FromSeconds(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(s_interval, timeProvider);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var completed = await processor.ProcessDueAsync(stoppingToken);
                if (completed > 0)
                {
                    logger.LogInformation("Completed {Count} constructions", completed);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Keep running: the next tick picks up whatever this one missed
                logger.LogError(ex, "Error while completing constructions");
            }
        }
    }
}