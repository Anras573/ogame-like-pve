using Game.Core.Resources;

namespace Game.Core.Economy;

/// <param name="HourlyProduction">Resources produced per hour, energy shortage and economy speed included.</param>
/// <param name="ProductionFactor">Share of full mine output the planet's energy supports, from 0 to 1.</param>
public sealed record ProductionReport(
    ResourceAmounts HourlyProduction,
    ResourceAmounts StorageCapacity,
    double EnergyProduced,
    double EnergyConsumed,
    double ProductionFactor);