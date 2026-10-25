using Game.Core.Resources;
using Game.Core.TechTree;

namespace Game.Core.Buildings;

/// <summary>
/// Every building's cost curve and prerequisites. The numbers are a starting point shaped like the genre's classic curves;
/// expect them to be tuned once there is a balancing tool.
/// </summary>
public static class BuildingCatalog
{
    private static readonly Dictionary<BuildingType, BuildingDefinition> s_definitions = new BuildingDefinition[]
    {
        new(BuildingType.MetalMine, new ResourceAmounts(60, 15, 0), 1.5),
        new(BuildingType.CrystalMine, new ResourceAmounts(48, 24, 0), 1.6),
        new(BuildingType.DeuteriumSynthesizer, new ResourceAmounts(225, 75, 0), 1.5),
        new(BuildingType.SolarPlant, new ResourceAmounts(75, 30, 0), 1.5),
        new(BuildingType.RoboticsFactory, new ResourceAmounts(400, 120, 200), 2),
        new(BuildingType.MetalStorage, new ResourceAmounts(1000, 0, 0), 2),
        new(BuildingType.CrystalStorage, new ResourceAmounts(1000, 500, 0), 2),
        new(BuildingType.DeuteriumTank, new ResourceAmounts(1000, 1000, 0), 2),
        new(BuildingType.Shipyard, new ResourceAmounts(400, 200, 100), 2)
        {
            Requirements = [new BuildingRequirement(BuildingType.RoboticsFactory, 2)],
        },
        new(BuildingType.ResearchLab, new ResourceAmounts(200, 400, 200), 2),
    }.ToDictionary(d => d.Type);

    public static IReadOnlyCollection<BuildingDefinition> All => s_definitions.Values;

    public static BuildingDefinition Get(BuildingType type)
    {
        return s_definitions.TryGetValue(type, out var definition)
            ? definition
            : throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown building type.");
    }
}