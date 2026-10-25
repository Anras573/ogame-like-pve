namespace Game.Core.Units;

/// <summary>
/// The shipyard builds orders one after another, one unit at a time; each unit is delivered as soon as it's done.
/// </summary>
public static class ShipyardQueue
{
    public const int MaxOrders = 10;
    public const int MaxQuantityPerOrder = 100_000;

    /// <param name="Remaining">Orders still in the queue; the first one is in progress.</param>
    /// <param name="Completed">Units delivered by this advance.</param>
    /// <param name="ProgressAt">When the unit now in progress started, or <c>null</c> when the queue is empty.</param>
    public sealed record AdvanceResult(
        IReadOnlyList<ShipyardOrder> Remaining,
        IReadOnlyDictionary<UnitType, int> Completed,
        DateTimeOffset? ProgressAt);

    /// <summary>
    /// Runs the queue from <paramref name="progressAt"/> (when its first unit started) up to <paramref name="now"/>.
    /// </summary>
    public static AdvanceResult Advance(IReadOnlyList<ShipyardOrder> queue, DateTimeOffset progressAt, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(queue);

        var remaining = queue.ToList();
        var completed = new Dictionary<UnitType, int>();
        var cursor = progressAt;

        while (remaining.Count > 0)
        {
            var order = remaining[0];
            var unitTime = TimeSpan.FromSeconds(order.UnitBuildSeconds);

            var finishable = now <= cursor ? 0 : (int)Math.Min(order.Quantity, (now - cursor).Ticks / unitTime.Ticks);
            if (finishable > 0)
            {
                completed[order.Unit] = completed.GetValueOrDefault(order.Unit) + finishable;
                cursor += unitTime * finishable;
            }

            if (finishable < order.Quantity)
            {
                remaining[0] = order with { Quantity = order.Quantity - finishable };
                break;
            }

            remaining.RemoveAt(0);
        }

        return new AdvanceResult(remaining, completed, remaining.Count > 0 ? cursor : null);
    }
}