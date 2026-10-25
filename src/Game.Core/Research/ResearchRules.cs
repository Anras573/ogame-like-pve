using Game.Core.Resources;

namespace Game.Core.Research;

public static class ResearchRules
{
    /// <summary>
    /// Resources researched per hour per (lab level + 1) at economy speed 1, so a level-1 lab manages 2000 per hour.
    /// </summary>
    private const double ResearchRatePerHour = 1000;

    public static ResourceAmounts CostOf(ResearchType type, int level)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(level, 1);

        var definition = ResearchCatalog.Get(type);
        return (definition.BaseCost * Math.Pow(definition.CostFactor, level - 1)).Floor();
    }

    /// <summary>
    /// How long research costing <paramref name="cost"/> takes in a lab of <paramref name="researchLabLevel"/>.
    /// </summary>
    public static TimeSpan ResearchTime(ResourceAmounts cost, int researchLabLevel, int economySpeed)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(researchLabLevel);
        ArgumentOutOfRangeException.ThrowIfLessThan(economySpeed, 1);

        var hours = (cost.Metal + cost.Crystal) / (ResearchRatePerHour * (1 + researchLabLevel) * economySpeed);
        return TimeSpan.FromSeconds(Math.Max(1, Math.Floor(hours * 3600)));
    }
}