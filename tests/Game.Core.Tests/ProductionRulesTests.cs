using Game.Core.Buildings;
using Game.Core.Economy;
using Game.Core.Resources;

namespace Game.Core.Tests;

public class ProductionRulesTests
{
    private const int TemperatePlanet = 40;

    [Fact]
    public void Calculate_WithoutBuildings_ProducesBaseOutput()
    {
        // Arrange
        var levels = new Dictionary<BuildingType, int>();

        // Act
        var report = ProductionRules.Calculate(levels, TemperatePlanet, economySpeed: 1);

        // Assert
        Assert.Equal(new ResourceAmounts(30, 15, 0), report.HourlyProduction);
        Assert.Equal(new ResourceAmounts(10_000, 10_000, 10_000), report.StorageCapacity);
        Assert.Equal(0, report.EnergyProduced);
        Assert.Equal(0, report.EnergyConsumed);
        Assert.Equal(1, report.ProductionFactor);
    }

    [Fact]
    public void Calculate_MineWithoutPower_ProducesOnlyBaseOutput()
    {
        // Arrange
        var levels = new Dictionary<BuildingType, int> { [BuildingType.MetalMine] = 1 };

        // Act
        var report = ProductionRules.Calculate(levels, TemperatePlanet, economySpeed: 1);

        // Assert
        Assert.Equal(11, report.EnergyConsumed);
        Assert.Equal(0, report.ProductionFactor);
        Assert.Equal(30, report.HourlyProduction.Metal);
    }

    [Fact]
    public void Calculate_PoweredMine_AddsMineOutput()
    {
        // Arrange
        var levels = new Dictionary<BuildingType, int>
        {
            [BuildingType.MetalMine] = 1,
            [BuildingType.SolarPlant] = 1,
        };

        // Act
        var report = ProductionRules.Calculate(levels, TemperatePlanet, economySpeed: 1);

        // Assert
        Assert.Equal(22, report.EnergyProduced);
        Assert.Equal(1, report.ProductionFactor);
        Assert.Equal(63, report.HourlyProduction.Metal, precision: 6); // 30 base + 30 × 1 × 1.1
    }

    [Fact]
    public void Calculate_WithEnergyShortage_ScalesMineOutput()
    {
        // Arrange
        var levels = new Dictionary<BuildingType, int>
        {
            [BuildingType.MetalMine] = 1,
            [BuildingType.CrystalMine] = 1,
            [BuildingType.DeuteriumSynthesizer] = 1,
            [BuildingType.SolarPlant] = 1,
        };

        // Act
        var report = ProductionRules.Calculate(levels, TemperatePlanet, economySpeed: 1);

        // Assert
        Assert.Equal(44, report.EnergyConsumed); // 11 + 11 + 22
        Assert.Equal(0.5, report.ProductionFactor);
        Assert.Equal(30 + 16.5, report.HourlyProduction.Metal, precision: 6);
        Assert.Equal(15 + 11, report.HourlyProduction.Crystal, precision: 6);
    }

    [Fact]
    public void Calculate_ColderPlanet_ProducesMoreDeuterium()
    {
        // Arrange
        var levels = new Dictionary<BuildingType, int>
        {
            [BuildingType.DeuteriumSynthesizer] = 1,
            [BuildingType.SolarPlant] = 2,
        };

        // Act
        var hot = ProductionRules.Calculate(levels, maxTemperature: 100, economySpeed: 1);
        var cold = ProductionRules.Calculate(levels, maxTemperature: -100, economySpeed: 1);

        // Assert
        Assert.Equal(11 * 1.04, hot.HourlyProduction.Deuterium, precision: 6);
        Assert.Equal(11 * 1.84, cold.HourlyProduction.Deuterium, precision: 6);
    }

    [Fact]
    public void Calculate_WithFasterEconomy_MultipliesProduction()
    {
        // Arrange
        var levels = new Dictionary<BuildingType, int>();

        // Act
        var report = ProductionRules.Calculate(levels, TemperatePlanet, economySpeed: 3);

        // Assert
        Assert.Equal(new ResourceAmounts(90, 45, 0), report.HourlyProduction);
    }

    [Fact]
    public void Calculate_WithStorageBuilding_RaisesCapacity()
    {
        // Arrange
        var levels = new Dictionary<BuildingType, int> { [BuildingType.MetalStorage] = 1 };

        // Act
        var report = ProductionRules.Calculate(levels, TemperatePlanet, economySpeed: 1);

        // Assert
        Assert.Equal(20_000, report.StorageCapacity.Metal);
        Assert.Equal(10_000, report.StorageCapacity.Crystal);
    }
}