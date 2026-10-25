using Game.Core.Resources;
using Game.Core.TechTree;

namespace Game.Core.Units;

/// <param name="Cost">Cost of one unit.</param>
/// <param name="Weapon">Damage per shot.</param>
/// <param name="Shield">Damage absorbed per combat round before the hull is hit.</param>
public sealed record UnitDefinition(UnitType Type, UnitCategory Category, ResourceAmounts Cost, int Weapon, int Shield)
{
    public IReadOnlyList<Requirement> Requirements { get; init; } = [];

    /// <summary>Ships only: base flight speed.</summary>
    public int Speed { get; init; }

    /// <summary>Ships only: how many resources the ship can carry.</summary>
    public int Cargo { get; init; }

    /// <summary>Ships only: deuterium burned on a standard flight.</summary>
    public int FuelConsumption { get; init; }

    /// <summary>How many a planet may have at most (built plus queued), or <c>null</c> for no limit.</summary>
    public int? MaxPerPlanet { get; init; }

    /// <summary>Hull points: a unit is as sturdy as the metal and crystal that went into it.</summary>
    public int StructuralIntegrity => (int)(Cost.Metal + Cost.Crystal);
}