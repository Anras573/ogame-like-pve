using Game.Api.Planets;
using Game.Api.Players;
using Game.Core.Buildings;
using Game.Core.Research;

namespace Game.Api.Research;

/// <summary>
/// The player's research as seen from one planet: costs are paid there and times depend on its lab.
/// </summary>
public record ResearchOverviewDto(
    Guid PlanetId,
    int ResearchLabLevel,
    List<ResearchDto> Technologies,
    CurrentResearchDto? CurrentResearch,
    DateTimeOffset ServerTime)
{
    public static ResearchOverviewDto From(OwnedPlanet owned, DateTimeOffset serverTime)
    {
        var player = owned.Player;
        var planet = owned.Planet;

        return new ResearchOverviewDto(
            planet.Id.Value,
            planet.BuildingLevels.LevelOf(BuildingType.ResearchLab),
            [.. ResearchCatalog.All.Select(definition =>
            {
                var next = player.NextLevel(definition.Type, planet, owned.EconomySpeed);
                return new ResearchDto(
                    definition.Type,
                    player.ResearchLevels.LevelOf(definition.Type),
                    ResourcesDto.From(next.Cost),
                    (int)next.Time.TotalSeconds,
                    MissingRequirementsDto.From(player.MissingRequirements(definition.Type, planet)));
            })],
            player.CurrentResearch is { } current ? CurrentResearchDto.From(current) : null,
            serverTime);
    }
}

public record ResearchDto(
    ResearchType Type,
    int Level,
    ResourcesDto NextLevelCost,
    int NextLevelResearchSeconds,
    MissingRequirementsDto MissingRequirements);

public record CurrentResearchDto(ResearchType Research, int TargetLevel, Guid PlanetId, DateTimeOffset StartedAt, DateTimeOffset CompletesAt)
{
    public static CurrentResearchDto From(ResearchProject project)
        => new(project.Research, project.TargetLevel, project.PlanetId.Value, project.StartedAt, project.CompletesAt);
}