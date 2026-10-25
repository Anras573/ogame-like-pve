using Game.Core.Buildings;
using Game.Core.Resources;

namespace Game.Api.Planets;

/// <summary>
/// A planet as of <see cref="ResourcesAsOf"/>. Clients extrapolate resources between refreshes from the hourly
/// production and storage capacity, and use <see cref="ServerTime"/> to correct for clock differences.
/// </summary>
public record PlanetDto(
    Guid Id,
    string Name,
    CoordinatesDto Coordinates,
    int MaxTemperature,
    ResourcesDto Resources,
    DateTimeOffset ResourcesAsOf,
    ResourcesDto HourlyProduction,
    ResourcesDto StorageCapacity,
    EnergyDto Energy,
    List<BuildingDto> Buildings,
    ConstructionDto? Construction,
    DateTimeOffset ServerTime)
{
    public static PlanetDto From(Planet planet, int economySpeed, DateTimeOffset serverTime)
    {
        var production = planet.Production(economySpeed);

        return new PlanetDto(
            planet.Id.Value,
            planet.Name,
            CoordinatesDto.From(planet),
            planet.MaxTemperature,
            ResourcesDto.From(planet.Resources),
            planet.ResourcesUpdatedAt,
            ResourcesDto.From(production.HourlyProduction),
            ResourcesDto.From(production.StorageCapacity),
            new EnergyDto(production.EnergyProduced, production.EnergyConsumed, production.ProductionFactor),
            [.. BuildingCatalog.All.Select(definition => BuildingDto.From(planet, definition.Type, economySpeed))],
            planet.Construction is { } construction ? ConstructionDto.From(construction) : null,
            serverTime);
    }
}

public record PlanetSummaryDto(Guid Id, string Name, CoordinatesDto Coordinates)
{
    public static PlanetSummaryDto From(Planet planet) => new(planet.Id.Value, planet.Name, CoordinatesDto.From(planet));
}

public record CoordinatesDto(int Galaxy, int System, int Position)
{
    public static CoordinatesDto From(Planet planet) => new(planet.Galaxy, planet.SolarSystem, planet.Position);
}

public record ResourcesDto(double Metal, double Crystal, double Deuterium)
{
    public static ResourcesDto From(ResourceAmounts amounts) => new(amounts.Metal, amounts.Crystal, amounts.Deuterium);
}

/// <param name="ProductionFactor">Share of full mine output the planet's energy supports, from 0 to 1.</param>
public record EnergyDto(double Produced, double Consumed, double ProductionFactor);

public record BuildingDto(BuildingType Type, int Level, ResourcesDto NextLevelCost, int NextLevelBuildSeconds)
{
    public static BuildingDto From(Planet planet, BuildingType type, int economySpeed)
    {
        var cost = planet.NextLevelCost(type);

        return new BuildingDto(
            type,
            planet.BuildingLevels.LevelOf(type),
            ResourcesDto.From(cost),
            (int)planet.BuildTime(cost, economySpeed).TotalSeconds);
    }
}

public record ConstructionDto(BuildingType Building, int TargetLevel, DateTimeOffset StartedAt, DateTimeOffset CompletesAt)
{
    public static ConstructionDto From(Construction construction)
        => new(construction.Building, construction.TargetLevel, construction.StartedAt, construction.CompletesAt);
}