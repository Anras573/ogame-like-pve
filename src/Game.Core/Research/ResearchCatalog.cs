using Game.Core.Buildings;
using Game.Core.Resources;
using Game.Core.TechTree;

namespace Game.Core.Research;

/// <summary>
/// Every technology's cost curve and prerequisites. Like the building catalog, the numbers are a starting point
/// shaped like the genre's classic tech tree and are expected to be tuned.
/// </summary>
public static class ResearchCatalog
{
    private static readonly Dictionary<ResearchType, ResearchDefinition> s_definitions = new ResearchDefinition[]
    {
        new(ResearchType.EnergyTechnology, new ResourceAmounts(0, 800, 400), 2)
        {
            Requirements = [Lab(1)],
        },
        new(ResearchType.LaserTechnology, new ResourceAmounts(200, 100, 0), 2)
        {
            Requirements = [Lab(1), Tech(ResearchType.EnergyTechnology, 2)],
        },
        new(ResearchType.IonTechnology, new ResourceAmounts(1000, 300, 100), 2)
        {
            Requirements = [Lab(4), Tech(ResearchType.EnergyTechnology, 4), Tech(ResearchType.LaserTechnology, 5)],
        },
        new(ResearchType.HyperspaceTechnology, new ResourceAmounts(0, 4000, 2000), 2)
        {
            Requirements = [Lab(7), Tech(ResearchType.EnergyTechnology, 5), Tech(ResearchType.ShieldingTechnology, 5)],
        },
        new(ResearchType.PlasmaTechnology, new ResourceAmounts(2000, 4000, 1000), 2)
        {
            Requirements =
            [
                Lab(4),
                Tech(ResearchType.EnergyTechnology, 8),
                Tech(ResearchType.LaserTechnology, 10),
                Tech(ResearchType.IonTechnology, 5),
            ],
        },
        new(ResearchType.CombustionDrive, new ResourceAmounts(400, 0, 600), 2)
        {
            Requirements = [Lab(1), Tech(ResearchType.EnergyTechnology, 1)],
        },
        new(ResearchType.ImpulseDrive, new ResourceAmounts(2000, 4000, 600), 2)
        {
            Requirements = [Lab(2), Tech(ResearchType.EnergyTechnology, 1)],
        },
        new(ResearchType.HyperspaceDrive, new ResourceAmounts(10000, 20000, 6000), 2)
        {
            Requirements = [Lab(7), Tech(ResearchType.HyperspaceTechnology, 3)],
        },
        new(ResearchType.EspionageTechnology, new ResourceAmounts(200, 1000, 200), 2)
        {
            Requirements = [Lab(3)],
        },
        new(ResearchType.ComputerTechnology, new ResourceAmounts(0, 400, 600), 2)
        {
            Requirements = [Lab(1)],
        },
        new(ResearchType.Astrophysics, new ResourceAmounts(4000, 8000, 4000), 1.75)
        {
            Requirements = [Lab(3), Tech(ResearchType.EspionageTechnology, 4), Tech(ResearchType.ImpulseDrive, 3)],
        },
        new(ResearchType.WeaponsTechnology, new ResourceAmounts(800, 200, 0), 2)
        {
            Requirements = [Lab(4)],
        },
        new(ResearchType.ShieldingTechnology, new ResourceAmounts(200, 600, 0), 2)
        {
            Requirements = [Lab(6), Tech(ResearchType.EnergyTechnology, 3)],
        },
        new(ResearchType.ArmourTechnology, new ResourceAmounts(1000, 0, 0), 2)
        {
            Requirements = [Lab(2)],
        },
    }.ToDictionary(d => d.Type);

    public static IReadOnlyCollection<ResearchDefinition> All => s_definitions.Values;

    public static ResearchDefinition Get(ResearchType type)
    {
        return s_definitions.TryGetValue(type, out var definition)
            ? definition
            : throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown research type.");
    }

    private static BuildingRequirement Lab(int level) => new(BuildingType.ResearchLab, level);

    private static ResearchRequirement Tech(ResearchType research, int level) => new(research, level);
}