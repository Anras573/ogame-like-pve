using Game.Core.Buildings;
using Game.Core.Research;
using Game.Core.Resources;
using Game.Core.TechTree;

namespace Game.Core.Units;

/// <summary>
/// Every ship and defense. Stats beyond cost are used by fleets (M3) and combat (M4); they live here already so
/// the catalog is complete and can be shown to players. Numbers are a starting point and expected to be tuned.
/// </summary>
public static class UnitCatalog
{
    private static readonly Dictionary<UnitType, UnitDefinition> s_definitions = new UnitDefinition[]
    {
        Ship(UnitType.SmallCargo, new(2000, 2000, 0), weapon: 5, shield: 10, speed: 5000, cargo: 5000, fuel: 10,
            [Shipyard(2), Tech(ResearchType.CombustionDrive, 2)]),
        Ship(UnitType.LargeCargo, new(6000, 6000, 0), weapon: 5, shield: 25, speed: 7500, cargo: 25000, fuel: 50,
            [Shipyard(4), Tech(ResearchType.CombustionDrive, 6)]),
        Ship(UnitType.LightFighter, new(3000, 1000, 0), weapon: 50, shield: 10, speed: 12500, cargo: 50, fuel: 20,
            [Shipyard(1), Tech(ResearchType.CombustionDrive, 1)]),
        Ship(UnitType.HeavyFighter, new(6000, 4000, 0), weapon: 150, shield: 25, speed: 10000, cargo: 100, fuel: 75,
            [Shipyard(3), Tech(ResearchType.ArmourTechnology, 2), Tech(ResearchType.ImpulseDrive, 2)]),
        Ship(UnitType.Cruiser, new(20000, 7000, 2000), weapon: 400, shield: 50, speed: 15000, cargo: 800, fuel: 300,
            [Shipyard(5), Tech(ResearchType.ImpulseDrive, 4), Tech(ResearchType.IonTechnology, 2)]),
        Ship(UnitType.Battleship, new(45000, 15000, 0), weapon: 1000, shield: 200, speed: 10000, cargo: 1500, fuel: 500,
            [Shipyard(7), Tech(ResearchType.HyperspaceDrive, 4)]),
        Ship(UnitType.Bomber, new(50000, 25000, 15000), weapon: 1000, shield: 500, speed: 4000, cargo: 500, fuel: 700,
            [Shipyard(8), Tech(ResearchType.ImpulseDrive, 6), Tech(ResearchType.PlasmaTechnology, 5)]),
        Ship(UnitType.Destroyer, new(60000, 50000, 15000), weapon: 2000, shield: 500, speed: 5000, cargo: 2000, fuel: 1000,
            [Shipyard(9), Tech(ResearchType.HyperspaceDrive, 6), Tech(ResearchType.HyperspaceTechnology, 5)]),
        Ship(UnitType.Recycler, new(10000, 6000, 2000), weapon: 1, shield: 10, speed: 2000, cargo: 20000, fuel: 300,
            [Shipyard(4), Tech(ResearchType.CombustionDrive, 6), Tech(ResearchType.ShieldingTechnology, 2)]),
        Ship(UnitType.EspionageProbe, new(0, 1000, 0), weapon: 0, shield: 0, speed: 100_000_000, cargo: 5, fuel: 1,
            [Shipyard(3), Tech(ResearchType.CombustionDrive, 3), Tech(ResearchType.EspionageTechnology, 2)]),
        Ship(UnitType.ColonyShip, new(10000, 20000, 10000), weapon: 50, shield: 100, speed: 2500, cargo: 7500, fuel: 1000,
            [Shipyard(4), Tech(ResearchType.ImpulseDrive, 3)]),

        Defense(UnitType.RocketLauncher, new(2000, 0, 0), weapon: 80, shield: 20,
            [Shipyard(1)]),
        Defense(UnitType.LightLaser, new(1500, 500, 0), weapon: 100, shield: 25,
            [Shipyard(2), Tech(ResearchType.EnergyTechnology, 1), Tech(ResearchType.LaserTechnology, 3)]),
        Defense(UnitType.HeavyLaser, new(6000, 2000, 0), weapon: 250, shield: 100,
            [Shipyard(4), Tech(ResearchType.EnergyTechnology, 3), Tech(ResearchType.LaserTechnology, 6)]),
        Defense(UnitType.GaussCannon, new(20000, 15000, 2000), weapon: 1100, shield: 200,
            [Shipyard(6), Tech(ResearchType.EnergyTechnology, 6), Tech(ResearchType.WeaponsTechnology, 3), Tech(ResearchType.ShieldingTechnology, 1)]),
        Defense(UnitType.IonCannon, new(5000, 3000, 0), weapon: 150, shield: 500,
            [Shipyard(4), Tech(ResearchType.IonTechnology, 4)]),
        Defense(UnitType.PlasmaTurret, new(50000, 50000, 30000), weapon: 3000, shield: 300,
            [Shipyard(8), Tech(ResearchType.PlasmaTechnology, 7)]),
        Defense(UnitType.SmallShieldDome, new(10000, 10000, 0), weapon: 1, shield: 2000,
            [Shipyard(1), Tech(ResearchType.ShieldingTechnology, 2)]) with { MaxPerPlanet = 1 },
        Defense(UnitType.LargeShieldDome, new(50000, 50000, 0), weapon: 1, shield: 10000,
            [Shipyard(6), Tech(ResearchType.ShieldingTechnology, 6)]) with { MaxPerPlanet = 1 },
    }.ToDictionary(d => d.Type);

    public static IReadOnlyCollection<UnitDefinition> All => s_definitions.Values;

    public static UnitDefinition Get(UnitType type)
    {
        return s_definitions.TryGetValue(type, out var definition)
            ? definition
            : throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown unit type.");
    }

    private static UnitDefinition Ship(
        UnitType type,
        ResourceAmounts cost,
        int weapon,
        int shield,
        int speed,
        int cargo,
        int fuel,
        IReadOnlyList<Requirement> requirements)
    {
        return new UnitDefinition(type, UnitCategory.Ship, cost, weapon, shield)
        {
            Speed = speed,
            Cargo = cargo,
            FuelConsumption = fuel,
            Requirements = requirements,
        };
    }

    private static UnitDefinition Defense(UnitType type, ResourceAmounts cost, int weapon, int shield, IReadOnlyList<Requirement> requirements)
    {
        return new UnitDefinition(type, UnitCategory.Defense, cost, weapon, shield) { Requirements = requirements };
    }

    private static BuildingRequirement Shipyard(int level) => new(BuildingType.Shipyard, level);

    private static ResearchRequirement Tech(ResearchType research, int level) => new(research, level);
}