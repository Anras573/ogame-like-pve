using Game.Core.Buildings;
using Game.Core.Research;

namespace Game.Core.TechTree;

/// <summary>
/// Something that must be in place before a building, research or unit becomes available.
/// </summary>
public abstract record Requirement(int Level);

/// <summary>
/// The planet must have <see cref="Building"/> at <see cref="Requirement.Level"/> or higher.
/// </summary>
public sealed record BuildingRequirement(BuildingType Building, int Level) : Requirement(Level);

/// <summary>
/// The player must have researched <see cref="Research"/> to <see cref="Requirement.Level"/> or higher.
/// </summary>
public sealed record ResearchRequirement(ResearchType Research, int Level) : Requirement(Level);