using Game.Core.Buildings;
using Game.Core.Research;
using Game.Core.TechTree;

namespace Game.Core.Tests;

public class RequirementCheckTests
{
    private static readonly Requirement[] s_requirements =
    [
        new BuildingRequirement(BuildingType.Shipyard, 2),
        new ResearchRequirement(ResearchType.CombustionDrive, 2),
    ];

    [Fact]
    public void Missing_WithEverythingInPlace_ReturnsEmpty()
    {
        // Arrange
        var buildings = new Dictionary<BuildingType, int> { [BuildingType.Shipyard] = 3 };
        var research = new Dictionary<ResearchType, int> { [ResearchType.CombustionDrive] = 2 };

        // Act
        var missing = RequirementCheck.Missing(s_requirements, buildings, research);

        // Assert
        Assert.Empty(missing);
    }

    [Fact]
    public void Missing_WithLowBuildingAndNoResearch_ReturnsBoth()
    {
        // Arrange
        var buildings = new Dictionary<BuildingType, int> { [BuildingType.Shipyard] = 1 };
        var research = new Dictionary<ResearchType, int>();

        // Act
        var missing = RequirementCheck.Missing(s_requirements, buildings, research);

        // Assert
        Assert.Equal(s_requirements, missing);
    }

    [Fact]
    public void Missing_WithOnlyResearchMissing_ReturnsResearch()
    {
        // Arrange
        var buildings = new Dictionary<BuildingType, int> { [BuildingType.Shipyard] = 2 };
        var research = new Dictionary<ResearchType, int> { [ResearchType.CombustionDrive] = 1 };

        // Act
        var missing = RequirementCheck.Missing(s_requirements, buildings, research);

        // Assert
        Assert.Equal([new ResearchRequirement(ResearchType.CombustionDrive, 2)], missing);
    }
}