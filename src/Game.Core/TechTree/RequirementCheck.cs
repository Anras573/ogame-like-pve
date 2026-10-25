using Game.Core.Buildings;
using Game.Core.Research;

namespace Game.Core.TechTree;

public static class RequirementCheck
{
    /// <summary>
    /// The requirements not yet met by a planet's buildings and its owner's research.
    /// </summary>
    public static IReadOnlyList<Requirement> Missing(
        IEnumerable<Requirement> requirements,
        IReadOnlyDictionary<BuildingType, int> buildingLevels,
        IReadOnlyDictionary<ResearchType, int> researchLevels)
    {
        ArgumentNullException.ThrowIfNull(requirements);
        ArgumentNullException.ThrowIfNull(buildingLevels);
        ArgumentNullException.ThrowIfNull(researchLevels);

        return [.. requirements.Where(requirement => !IsMet(requirement, buildingLevels, researchLevels))];
    }

    private static bool IsMet(
        Requirement requirement,
        IReadOnlyDictionary<BuildingType, int> buildingLevels,
        IReadOnlyDictionary<ResearchType, int> researchLevels)
    {
        return requirement switch
        {
            BuildingRequirement building => buildingLevels.LevelOf(building.Building) >= building.Level,
            ResearchRequirement research => researchLevels.LevelOf(research.Research) >= research.Level,
            _ => throw new ArgumentOutOfRangeException(nameof(requirement), requirement, "Unknown requirement."),
        };
    }
}