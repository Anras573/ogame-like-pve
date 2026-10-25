using Game.Core.Resources;
using Game.Core.TechTree;

namespace Game.Core.Research;

/// <param name="BaseCost">Cost of level 1.</param>
/// <param name="CostFactor">Each further level costs this many times the previous one.</param>
public sealed record ResearchDefinition(ResearchType Type, ResourceAmounts BaseCost, double CostFactor)
{
    public IReadOnlyList<Requirement> Requirements { get; init; } = [];
}