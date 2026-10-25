using Game.Core.Buildings;
using Game.Core.Research;
using Game.Core.Resources;
using Game.Core.TechTree;
using Game.Core.Units;

namespace Game.Api.Planets;

/// <summary>
/// A planet as of <see cref="ResourcesAsOf"/>. Clients extrapolate resources and shipyard progress between
/// refreshes, and use <see cref="ServerTime"/> to correct for clock differences.
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
    List<UnitDto> Units,
    List<ShipyardOrderDto> ShipyardQueue,
    DateTimeOffset? ShipyardProgressAt,
    DateTimeOffset ServerTime)
{
    public static PlanetDto From(OwnedPlanet owned, DateTimeOffset serverTime)
    {
        var planet = owned.Planet;
        var research = owned.Player.ResearchLevels;
        var economySpeed = owned.EconomySpeed;
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
            [.. BuildingCatalog.All.Select(definition => BuildingDto.From(planet, definition.Type, research, economySpeed))],
            planet.Construction is { } construction ? ConstructionDto.From(construction) : null,
            [.. UnitCatalog.All.Select(definition => UnitDto.From(planet, definition, research, economySpeed))],
            [.. planet.ShipyardOrders.Select(o => new ShipyardOrderDto(o.Unit, o.Quantity, o.UnitBuildSeconds))],
            planet.ShipyardProgressAt,
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

/// <summary>
/// Unmet prerequisites, split by kind so each list is precisely typed.
/// </summary>
public record MissingRequirementsDto(List<BuildingLevelDto> Buildings, List<ResearchLevelDto> Research)
{
    public static MissingRequirementsDto From(IEnumerable<Requirement> requirements)
    {
        var list = requirements.ToList();

        return new MissingRequirementsDto(
            [.. list.OfType<BuildingRequirement>().Select(b => new BuildingLevelDto(b.Building, b.Level))],
            [.. list.OfType<ResearchRequirement>().Select(r => new ResearchLevelDto(r.Research, r.Level))]);
    }
}

public record BuildingLevelDto(BuildingType Building, int Level);

public record ResearchLevelDto(ResearchType Research, int Level);

public record BuildingDto(
    BuildingType Type,
    int Level,
    ResourcesDto NextLevelCost,
    int NextLevelBuildSeconds,
    MissingRequirementsDto MissingRequirements)
{
    public static BuildingDto From(Planet planet, BuildingType type, IReadOnlyDictionary<ResearchType, int> research, int economySpeed)
    {
        var cost = planet.NextLevelCost(type);

        return new BuildingDto(
            type,
            planet.BuildingLevels.LevelOf(type),
            ResourcesDto.From(cost),
            (int)planet.BuildTime(cost, economySpeed).TotalSeconds,
            MissingRequirementsDto.From(planet.MissingRequirements(type, research)));
    }
}

public record ConstructionDto(BuildingType Building, int TargetLevel, DateTimeOffset StartedAt, DateTimeOffset CompletesAt)
{
    public static ConstructionDto From(Construction construction)
        => new(construction.Building, construction.TargetLevel, construction.StartedAt, construction.CompletesAt);
}

/// <param name="Count">Finished units on the planet (not counting queued ones).</param>
/// <param name="BuildSeconds">Time per unit in this planet's shipyard.</param>
/// <param name="StructuralIntegrity">Hull points.</param>
public record UnitDto(
    UnitType Type,
    UnitCategory Category,
    int Count,
    ResourcesDto Cost,
    int BuildSeconds,
    int Weapon,
    int Shield,
    int StructuralIntegrity,
    int Speed,
    int Cargo,
    int? MaxPerPlanet,
    MissingRequirementsDto MissingRequirements)
{
    public static UnitDto From(Planet planet, UnitDefinition definition, IReadOnlyDictionary<ResearchType, int> research, int economySpeed)
    {
        return new UnitDto(
            definition.Type,
            definition.Category,
            planet.Units.GetValueOrDefault(definition.Type),
            ResourcesDto.From(definition.Cost),
            (int)planet.UnitBuildTime(definition.Type, economySpeed).TotalSeconds,
            definition.Weapon,
            definition.Shield,
            definition.StructuralIntegrity,
            definition.Speed,
            definition.Cargo,
            definition.MaxPerPlanet,
            MissingRequirementsDto.From(planet.MissingRequirements(definition.Type, research)));
    }
}

/// <param name="Quantity">Units of this order not yet delivered.</param>
public record ShipyardOrderDto(UnitType Unit, int Quantity, int UnitBuildSeconds);