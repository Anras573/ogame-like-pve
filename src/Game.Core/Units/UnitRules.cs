using Game.Core.Resources;

namespace Game.Core.Units;

public static class UnitRules
{
    /// <summary>
    /// Resources a level-0 shipyard turns into units per hour at economy speed 1; each shipyard level adds as much again.
    /// </summary>
    private const double BuildRatePerHour = 2500;

    public static TimeSpan BuildTime(UnitType type, int shipyardLevel, int economySpeed)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(shipyardLevel);
        ArgumentOutOfRangeException.ThrowIfLessThan(economySpeed, 1);

        var cost = UnitCatalog.Get(type).Cost;
        var hours = (cost.Metal + cost.Crystal) / (BuildRatePerHour * (1 + shipyardLevel) * economySpeed);
        return TimeSpan.FromSeconds(Math.Max(1, Math.Floor(hours * 3600)));
    }

    public static ResourceAmounts CostOf(UnitType type, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 1);

        return UnitCatalog.Get(type).Cost * quantity;
    }
}