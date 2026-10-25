using Game.Core.Resources;

namespace Game.Core.Economy;

public static class ResourceAccrual
{
    /// <summary>
    /// Resources after producing at <paramref name="hourlyProduction"/> for <paramref name="elapsed"/>.
    /// Production stops at the storage capacity, but amounts already above it (e.g. from a refund) are kept.
    /// </summary>
    public static ResourceAmounts Accrue(
        ResourceAmounts current,
        ResourceAmounts hourlyProduction,
        ResourceAmounts capacity,
        TimeSpan elapsed)
    {
        if (elapsed <= TimeSpan.Zero)
        {
            return current;
        }

        var produced = hourlyProduction * elapsed.TotalHours;

        return new ResourceAmounts(
            Fill(current.Metal, produced.Metal, capacity.Metal),
            Fill(current.Crystal, produced.Crystal, capacity.Crystal),
            Fill(current.Deuterium, produced.Deuterium, capacity.Deuterium));
    }

    private static double Fill(double current, double produced, double capacity)
    {
        return current >= capacity ? current : Math.Min(capacity, current + produced);
    }
}