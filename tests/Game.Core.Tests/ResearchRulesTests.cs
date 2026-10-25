using Game.Core.Buildings;
using Game.Core.Research;
using Game.Core.Resources;
using Game.Core.TechTree;

namespace Game.Core.Tests;

public class ResearchRulesTests
{
    [Fact]
    public void CostOf_SecondLevel_DoublesBaseCost()
    {
        // Arrange & Act
        var cost = ResearchRules.CostOf(ResearchType.EnergyTechnology, 2);

        // Assert
        Assert.Equal(new ResourceAmounts(0, 1600, 800), cost);
    }

    [Fact]
    public void ResearchTime_WithLevelOneLab_UsesDoubleRate()
    {
        // Arrange
        var cost = new ResourceAmounts(0, 800, 400);

        // Act
        var time = ResearchRules.ResearchTime(cost, researchLabLevel: 1, economySpeed: 1);

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(1440), time); // 800 / 2000 h
    }

    [Fact]
    public void ResearchTime_WithFasterEconomy_IsFaster()
    {
        // Arrange
        var cost = new ResourceAmounts(0, 800, 400);

        // Act
        var time = ResearchRules.ResearchTime(cost, researchLabLevel: 1, economySpeed: 10);

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(144), time);
    }

    [Fact]
    public void Catalog_EveryResearch_NeedsAResearchLab()
    {
        // Arrange & Act & Assert
        Assert.All(ResearchCatalog.All, definition =>
            Assert.Contains(definition.Requirements, r => r is BuildingRequirement { Building: BuildingType.ResearchLab }));
    }

    [Fact]
    public void Catalog_TechTree_HasNoCycles()
    {
        // Arrange
        var visiting = new HashSet<ResearchType>();
        var done = new HashSet<ResearchType>();

        void Visit(ResearchType type)
        {
            if (done.Contains(type))
            {
                return;
            }

            Assert.True(visiting.Add(type), $"{type} depends on itself");
            foreach (var requirement in ResearchCatalog.Get(type).Requirements.OfType<ResearchRequirement>())
            {
                Visit(requirement.Research);
            }

            visiting.Remove(type);
            done.Add(type);
        }

        // Act
        foreach (var type in Enum.GetValues<ResearchType>())
        {
            Visit(type);
        }

        // Assert
        Assert.Equal(Enum.GetValues<ResearchType>().Length, done.Count);
    }
}