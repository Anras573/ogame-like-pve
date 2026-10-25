namespace Game.Api.Realtime;

/// <summary>
/// Finishes timed work that has come due (constructions, research) and tells the players concerned.
/// </summary>
public interface ICompletionProcessor
{
    /// <returns>How many things were completed.</returns>
    Task<int> ProcessDueAsync(CancellationToken cancellationToken);
}