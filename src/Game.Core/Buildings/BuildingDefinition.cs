using Game.Core.Resources;
using Game.Core.TechTree;

namespace Game.Core.Buildings;

/// <param name="BaseCost">Cost of level 1.</param>
/// <param name="CostFactor">Each further level costs this many times the previous one.</param>
public sealed record BuildingDefinition(BuildingType Type, ResourceAmounts BaseCost, double CostFactor)
{
    public IReadOnlyList<Requirement> Requirements { get; init; } = [];
}