namespace Game.Api.Realtime;

/// <summary>
/// Runs every <see cref="ICompletionProcessor"/> once a second.
/// </summary>
public sealed class CompletionWorker(
    IEnumerable<ICompletionProcessor> processors,
    TimeProvider timeProvider,
    ILogger<CompletionWorker> logger) : BackgroundService
{
    private static readonly TimeSpan s_interval = TimeSpan.FromSeconds(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(s_interval, timeProvider);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            foreach (var processor in processors)
            {
                try
                {
                    var completed = await processor.ProcessDueAsync(stoppingToken);
                    if (completed > 0)
                    {
                        logger.LogInformation("{Processor} completed {Count} items", processor.GetType().Name, completed);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // Keep running: the next tick picks up whatever this one missed
                    logger.LogError(ex, "Error in {Processor}", processor.GetType().Name);
                }
            }
        }
    }
}