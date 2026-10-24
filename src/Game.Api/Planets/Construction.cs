using Game.Core.Buildings;
using Game.Core.Resources;

namespace Game.Api.Planets;

/// <summary>
/// A building upgrade in progress. The cost is kept so a cancellation refunds exactly what was paid.
/// </summary>
public sealed class Construction
{
    public BuildingType Building { get; private set; }
    public int TargetLevel { get; private set; }
    public double CostMetal { get; private set; }
    public double CostCrystal { get; private set; }
    public double CostDeuterium { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset CompletesAt { get; private set; }

    public ResourceAmounts Cost => new(CostMetal, CostCrystal, CostDeuterium);

    // EF Core constructor
    private Construction()
    {
    }

    public Construction(BuildingType building, int targetLevel, ResourceAmounts cost, DateTimeOffset startedAt, DateTimeOffset completesAt)
    {
        Building = building;
        TargetLevel = targetLevel;
        CostMetal = cost.Metal;
        CostCrystal = cost.Crystal;
        CostDeuterium = cost.Deuterium;
        StartedAt = startedAt;
        CompletesAt = completesAt;
    }
}