using Game.Core.Resources;
using Game.Core.Units;

namespace Game.Core.Tests;

public class UnitRulesTests
{
    [Fact]
    public void BuildTime_LightFighterInLevelOneShipyard_UsesShipyardBonus()
    {
        // Arrange & Act
        var time = UnitRules.BuildTime(UnitType.LightFighter, shipyardLevel: 1, economySpeed: 1);

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(2880), time); // 4000 / (2500 × 2) h
    }

    [Fact]
    public void BuildTime_WithFasterEconomy_IsFaster()
    {
        // Arrange & Act
        var time = UnitRules.BuildTime(UnitType.RocketLauncher, shipyardLevel: 1, economySpeed: 10);

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(144), time);
    }

    [Fact]
    public void CostOf_Batch_MultipliesUnitCost()
    {
        // Arrange & Act
        var cost = UnitRules.CostOf(UnitType.LightFighter, 3);

        // Assert
        Assert.Equal(new ResourceAmounts(9000, 3000, 0), cost);
    }

    [Fact]
    public void Catalog_EveryUnitType_IsDefinedWithRequirements()
    {
        // Arrange & Act & Assert
        foreach (var type in Enum.GetValues<UnitType>())
        {
            var definition = UnitCatalog.Get(type);
            Assert.NotEmpty(definition.Requirements);
            Assert.True(definition.StructuralIntegrity > 0);
        }
    }

    [Fact]
    public void Catalog_Ships_HaveSpeed_AndDefenses_DoNot()
    {
        // Arrange & Act & Assert
        Assert.All(UnitCatalog.All.Where(d => d.Category == UnitCategory.Ship), d => Assert.True(d.Speed > 0));
        Assert.All(UnitCatalog.All.Where(d => d.Category == UnitCategory.Defense), d => Assert.Equal(0, d.Speed));
    }

    [Fact]
    public void Catalog_ShieldDomes_AreLimitedToOnePerPlanet()
    {
        // Arrange & Act & Assert
        Assert.Equal(1, UnitCatalog.Get(UnitType.SmallShieldDome).MaxPerPlanet);
        Assert.Equal(1, UnitCatalog.Get(UnitType.LargeShieldDome).MaxPerPlanet);
        Assert.Null(UnitCatalog.Get(UnitType.RocketLauncher).MaxPerPlanet);
    }
}