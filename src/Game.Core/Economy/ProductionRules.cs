using Game.Core.Buildings;
using Game.Core.Resources;

namespace Game.Core.Economy;

public static class ProductionRules
{
    /// <summary>
    /// Every planet trickles in some metal and crystal without any mines, so nobody gets stuck.
    /// </summary>
    public static ResourceAmounts BaseHourlyProduction { get; } = new(30, 15, 0);

    private const double BaseStorageCapacity = 5000;

    public static ProductionReport Calculate(
        IReadOnlyDictionary<BuildingType, int> levels,
        int maxTemperature,
        int economySpeed)
    {
        ArgumentNullException.ThrowIfNull(levels);
        ArgumentOutOfRangeException.ThrowIfLessThan(economySpeed, 1);

        var metalMine = levels.LevelOf(BuildingType.MetalMine);
        var crystalMine = levels.LevelOf(BuildingType.CrystalMine);
        var synthesizer = levels.LevelOf(BuildingType.DeuteriumSynthesizer);

        var energyProduced = Math.Floor(20 * Growth(levels.LevelOf(BuildingType.SolarPlant)));
        var energyConsumed = Math.Ceiling(10 * Growth(metalMine))
            + Math.Ceiling(10 * Growth(crystalMine))
            + Math.Ceiling(20 * Growth(synthesizer));

        // Mines run at reduced output when the planet can't power all of them
        var productionFactor = energyConsumed == 0 ? 1 : Math.Min(1, energyProduced / energyConsumed);

        // Colder planets are better at extracting deuterium
        var deuteriumTemperatureBonus = Math.Max(0, 1.44 - (0.004 * maxTemperature));

        var mineOutput = new ResourceAmounts(
            30 * Growth(metalMine),
            20 * Growth(crystalMine),
            10 * Growth(synthesizer) * deuteriumTemperatureBonus);

        var hourlyProduction = (BaseHourlyProduction + (mineOutput * productionFactor)) * economySpeed;

        var storageCapacity = new ResourceAmounts(
            StorageCapacity(levels.LevelOf(BuildingType.MetalStorage)),
            StorageCapacity(levels.LevelOf(BuildingType.CrystalStorage)),
            StorageCapacity(levels.LevelOf(BuildingType.DeuteriumTank)));

        return new ProductionReport(hourlyProduction, storageCapacity, energyProduced, energyConsumed, productionFactor);
    }

    /// <summary>
    /// The shared growth curve of mines and power plants: level × 1.1^level.
    /// </summary>
    private static double Growth(int level)
    {
        return level * Math.Pow(1.1, level);
    }

    private static double StorageCapacity(int level)
    {
        return BaseStorageCapacity * Math.Floor(2.5 * Math.Exp(20.0 * level / 33));
    }
}