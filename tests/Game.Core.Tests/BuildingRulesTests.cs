using Game.Core.Buildings;
using Game.Core.Resources;

namespace Game.Core.Tests;

public class BuildingRulesTests
{
    [Fact]
    public void CostOf_FirstLevel_ReturnsBaseCost()
    {
        // Arrange & Act
        var cost = BuildingRules.CostOf(BuildingType.MetalMine, 1);

        // Assert
        Assert.Equal(new ResourceAmounts(60, 15, 0), cost);
    }

    [Fact]
    public void CostOf_HigherLevel_GrowsByFactorAndRoundsDown()
    {
        // Arrange & Act
        var cost = BuildingRules.CostOf(BuildingType.MetalMine, 3);

        // Assert
        Assert.Equal(new ResourceAmounts(135, 33, 0), cost); // 60 × 1.5², 15 × 1.5² = 33.75
    }

    [Fact]
    public void CostOf_LevelZero_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => BuildingRules.CostOf(BuildingType.MetalMine, 0));
    }

    [Fact]
    public void CostOf_EveryCatalogBuilding_IsDefined()
    {
        // Arrange & Act & Assert
        foreach (var type in Enum.GetValues<BuildingType>())
        {
            Assert.True(BuildingRules.CostOf(type, 1).Covers(ResourceAmounts.Zero));
        }
    }

    [Fact]
    public void BuildTime_WithoutRobotics_UsesBaseRate()
    {
        // Arrange
        var cost = new ResourceAmounts(60, 15, 0);

        // Act
        var time = BuildingRules.BuildTime(cost, roboticsFactoryLevel: 0, economySpeed: 1);

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(108), time); // 75 / 2500 h
    }

    [Fact]
    public void BuildTime_WithRoboticsFactory_IsFaster()
    {
        // Arrange
        var cost = new ResourceAmounts(60, 15, 0);

        // Act
        var time = BuildingRules.BuildTime(cost, roboticsFactoryLevel: 1, economySpeed: 1);

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(54), time);
    }

    [Fact]
    public void BuildTime_WithFasterEconomy_IsFaster()
    {
        // Arrange
        var cost = new ResourceAmounts(60, 15, 0);

        // Act
        var time = BuildingRules.BuildTime(cost, roboticsFactoryLevel: 0, economySpeed: 4);

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(27), time);
    }

    [Fact]
    public void BuildTime_ForTinyCost_IsAtLeastOneSecond()
    {
        // Arrange
        var cost = new ResourceAmounts(1, 0, 0);

        // Act
        var time = BuildingRules.BuildTime(cost, roboticsFactoryLevel: 10, economySpeed: 100);

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(1), time);
    }
}