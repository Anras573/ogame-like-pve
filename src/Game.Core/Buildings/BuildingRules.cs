using Game.Core.Resources;

namespace Game.Core.Buildings;

public static class BuildingRules
{
    /// <summary>
    /// Resources spent per hour of construction by a planet without a robotics factory, at economy speed 1.
    /// </summary>
    private const double BuildRatePerHour = 2500;

    private static readonly TimeSpan s_minimumBuildTime = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Cost of upgrading <paramref name="type"/> to <paramref name="level"/>.
    /// </summary>
    public static ResourceAmounts CostOf(BuildingType type, int level)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(level, 1);

        var definition = BuildingCatalog.Get(type);
        return (definition.BaseCost * Math.Pow(definition.CostFactor, level - 1)).Floor();
    }

    /// <summary>
    /// How long a construction costing <paramref name="cost"/> takes. Robotics speeds construction up.
    /// </summary>
    public static TimeSpan BuildTime(ResourceAmounts cost, int roboticsFactoryLevel, int economySpeed)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(roboticsFactoryLevel);
        ArgumentOutOfRangeException.ThrowIfLessThan(economySpeed, 1);

        var hours = (cost.Metal + cost.Crystal) / (BuildRatePerHour * (1 + roboticsFactoryLevel) * economySpeed);

        // Whole seconds keep completion times readable and stable across serialization
        var seconds = Math.Floor(hours * 3600);
        return TimeSpan.FromSeconds(Math.Max(seconds, s_minimumBuildTime.TotalSeconds));
    }
}