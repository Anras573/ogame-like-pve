using Game.Api.Planets;
using Game.Core.Buildings;
using Game.Core.Research;
using Game.Core.Resources;
using Game.Core.Units;

namespace Game.Api.Tests;

public class ShipyardTests
{
    private const int Speed = 1;
    private static readonly DateTimeOffset s_start = TestPlanets.Start;
    private static readonly Dictionary<ResearchType, int> s_noResearch = [];
    private static readonly ResourceAmounts s_plenty = new(1_000_000, 1_000_000, 1_000_000);

    private static Planet PlanetWithShipyard()
    {
        return TestPlanets.Create().Planet
            .WithBuildings((BuildingType.Shipyard, 1))
            .WithResources(s_plenty);
    }

    [Fact]
    public void EnqueueUnits_WithShipyardAndResources_PaysAndQueues()
    {
        // Arrange
        var planet = PlanetWithShipyard();

        // Act
        var result = planet.EnqueueUnits(UnitType.RocketLauncher, 3, s_noResearch, s_start, Speed);

        // Assert
        Assert.Equal(EnqueueUnitsResult.Queued, result);
        Assert.Equal(s_plenty - new ResourceAmounts(6000, 0, 0), planet.Resources);
        Assert.Equal([new ShipyardOrder(UnitType.RocketLauncher, 3, 1440)], planet.ShipyardOrders); // 2000 / 5000 h
        Assert.Equal(s_start, planet.ShipyardProgressAt);
    }

    [Fact]
    public void EnqueueUnits_WithoutShipyard_ReturnsRequirementsNotMet()
    {
        // Arrange
        var planet = TestPlanets.Create().Planet.WithResources(s_plenty);

        // Act
        var result = planet.EnqueueUnits(UnitType.RocketLauncher, 1, s_noResearch, s_start, Speed);

        // Assert
        Assert.Equal(EnqueueUnitsResult.RequirementsNotMet, result);
        Assert.Empty(planet.ShipyardOrders);
    }

    [Fact]
    public void EnqueueUnits_WithoutRequiredResearch_ReturnsRequirementsNotMet()
    {
        // Arrange
        var planet = PlanetWithShipyard();

        // Act
        var result = planet.EnqueueUnits(UnitType.LightFighter, 1, s_noResearch, s_start, Speed);

        // Assert
        Assert.Equal(EnqueueUnitsResult.RequirementsNotMet, result);
    }

    [Fact]
    public void EnqueueUnits_WithRequiredResearch_Queues()
    {
        // Arrange
        var planet = PlanetWithShipyard();
        var research = new Dictionary<ResearchType, int> { [ResearchType.CombustionDrive] = 1 };

        // Act
        var result = planet.EnqueueUnits(UnitType.LightFighter, 1, research, s_start, Speed);

        // Assert
        Assert.Equal(EnqueueUnitsResult.Queued, result);
    }

    [Fact]
    public void EnqueueUnits_WithoutEnoughResources_ReturnsNotEnoughResourcesAndKeepsResources()
    {
        // Arrange
        var planet = PlanetWithShipyard().WithResources(new ResourceAmounts(3000, 0, 0));

        // Act
        var result = planet.EnqueueUnits(UnitType.RocketLauncher, 2, s_noResearch, s_start, Speed);

        // Assert
        Assert.Equal(EnqueueUnitsResult.NotEnoughResources, result);
        Assert.Equal(3000, planet.Resources.Metal);
    }

    [Fact]
    public void EnqueueUnits_WithZeroQuantity_ReturnsInvalidQuantity()
    {
        // Arrange
        var planet = PlanetWithShipyard();

        // Act
        var result = planet.EnqueueUnits(UnitType.RocketLauncher, 0, s_noResearch, s_start, Speed);

        // Assert
        Assert.Equal(EnqueueUnitsResult.InvalidQuantity, result);
    }

    [Fact]
    public void EnqueueUnits_WhenQueueIsFull_ReturnsQueueFull()
    {
        // Arrange
        var planet = PlanetWithShipyard();
        for (var i = 0; i < ShipyardQueue.MaxOrders; i++)
        {
            planet.EnqueueUnits(UnitType.RocketLauncher, 1, s_noResearch, s_start, Speed);
        }

        // Act
        var result = planet.EnqueueUnits(UnitType.RocketLauncher, 1, s_noResearch, s_start, Speed);

        // Assert
        Assert.Equal(EnqueueUnitsResult.QueueFull, result);
    }

    [Fact]
    public void EnqueueUnits_SecondShieldDome_ReturnsLimitReached()
    {
        // Arrange
        var planet = PlanetWithShipyard();
        var research = new Dictionary<ResearchType, int> { [ResearchType.ShieldingTechnology] = 2 };
        planet.EnqueueUnits(UnitType.SmallShieldDome, 1, research, s_start, Speed);

        // Act
        var result = planet.EnqueueUnits(UnitType.SmallShieldDome, 1, research, s_start, Speed);

        // Assert
        Assert.Equal(EnqueueUnitsResult.LimitReached, result);
    }

    [Fact]
    public void SettleTo_AfterBuildTime_DeliversUnits()
    {
        // Arrange
        var planet = PlanetWithShipyard();
        planet.EnqueueUnits(UnitType.RocketLauncher, 3, s_noResearch, s_start, Speed);

        // Act
        planet.SettleTo(s_start.AddSeconds((1440 * 2) + 100), Speed);

        // Assert
        Assert.Equal(2, planet.Units[UnitType.RocketLauncher]);
        Assert.Equal([new ShipyardOrder(UnitType.RocketLauncher, 1, 1440)], planet.ShipyardOrders);
        Assert.Equal(s_start.AddSeconds(1440 * 2), planet.ShipyardProgressAt);
    }

    [Fact]
    public void EnqueueUnits_AfterQueueRanEmpty_StartsFromNow()
    {
        // Arrange
        var planet = PlanetWithShipyard();
        planet.EnqueueUnits(UnitType.RocketLauncher, 1, s_noResearch, s_start, Speed);
        var later = s_start.AddHours(5);

        // Act
        planet.EnqueueUnits(UnitType.RocketLauncher, 1, s_noResearch, later, Speed);

        // Assert
        Assert.Equal(1, planet.Units[UnitType.RocketLauncher]);
        Assert.Equal(later, planet.ShipyardProgressAt);
    }
}