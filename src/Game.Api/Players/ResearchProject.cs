using Game.Api.Planets;
using Game.Core.Research;
using Game.Core.Resources;

namespace Game.Api.Players;

/// <summary>
/// Research in progress. It runs in one planet's lab and was paid from that planet, which is also where a
/// cancellation refunds to.
/// </summary>
public sealed class ResearchProject
{
    public ResearchType Research { get; private set; }
    public int TargetLevel { get; private set; }
    public PlanetId PlanetId { get; private set; }
    public double CostMetal { get; private set; }
    public double CostCrystal { get; private set; }
    public double CostDeuterium { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset CompletesAt { get; private set; }

    public ResourceAmounts Cost => new(CostMetal, CostCrystal, CostDeuterium);

    // EF Core constructor
    private ResearchProject()
    {
        PlanetId = default!;
    }

    public ResearchProject(
        ResearchType research,
        int targetLevel,
        PlanetId planetId,
        ResourceAmounts cost,
        DateTimeOffset startedAt,
        DateTimeOffset completesAt)
    {
        Research = research;
        TargetLevel = targetLevel;
        PlanetId = planetId;
        CostMetal = cost.Metal;
        CostCrystal = cost.Crystal;
        CostDeuterium = cost.Deuterium;
        StartedAt = startedAt;
        CompletesAt = completesAt;
    }
}