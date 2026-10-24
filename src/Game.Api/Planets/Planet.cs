using Game.Api.Players;
using Game.Api.Seasons;
using Game.Core.Buildings;
using Game.Core.Economy;
using Game.Core.Resources;
using Game.Core.Universe;

namespace Game.Api.Planets;

/// <summary>
/// A planet and its economy. Resources are stored as of <see cref="ResourcesUpdatedAt"/> and brought up to date
/// lazily: every read or change first calls <see cref="SettleTo"/>, which completes due constructions in order
/// and accrues production for the time that passed.
/// </summary>
public sealed class Planet
{
    public const string HomeName = "Homeworld";
    public const int MaxNameLength = 40;

    public static ResourceAmounts StartingResources { get; } = new(500, 500, 0);

    private Dictionary<BuildingType, int> _buildingLevels = [];

    public PlanetId Id { get; private set; }
    public PlayerId PlayerId { get; private set; }
    public SeasonId SeasonId { get; private set; }
    public string Name { get; private set; }
    public int Galaxy { get; private set; }
    public int SolarSystem { get; private set; }
    public int Position { get; private set; }
    public int MaxTemperature { get; private set; }
    public ResourceAmounts Resources { get; private set; }
    public DateTimeOffset ResourcesUpdatedAt { get; private set; }
    public Construction? Construction { get; private set; }

    /// <summary>
    /// Optimistic concurrency token, so a player action and the background worker can't both settle the same state.
    /// </summary>
    public uint Version { get; private set; }

    public Coordinates Coordinates => new(Galaxy, SolarSystem, Position);
    public IReadOnlyDictionary<BuildingType, int> BuildingLevels => _buildingLevels;

    // EF Core constructor
    private Planet()
    {
        Id = default!;
        PlayerId = default!;
        SeasonId = default!;
        Name = default!;
    }

    private Planet(
        PlanetId id,
        PlayerId playerId,
        SeasonId seasonId,
        string name,
        Coordinates coordinates,
        int maxTemperature,
        ResourceAmounts resources,
        DateTimeOffset resourcesUpdatedAt)
    {
        Id = id;
        PlayerId = playerId;
        SeasonId = seasonId;
        Name = name;
        Galaxy = coordinates.Galaxy;
        SolarSystem = coordinates.System;
        Position = coordinates.Position;
        MaxTemperature = maxTemperature;
        Resources = resources;
        ResourcesUpdatedAt = resourcesUpdatedAt;
    }

    public static Planet CreateHome(Player player, Coordinates coordinates, int maxTemperature, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(coordinates);

        return new(
            PlanetId.New(),
            player.Id,
            player.SeasonId,
            HomeName,
            coordinates,
            maxTemperature,
            StartingResources,
            now.ToUniversalTime());
    }

    public ProductionReport Production(int economySpeed)
    {
        return ProductionRules.Calculate(_buildingLevels, MaxTemperature, economySpeed);
    }

    /// <summary>
    /// Brings the planet up to <paramref name="now"/>: finishes the construction if it's due (with production
    /// switching to the new rates at the moment it completes), then accrues resources.
    /// </summary>
    /// <returns>The construction that completed, or <c>null</c>.</returns>
    public Construction? SettleTo(DateTimeOffset now, int economySpeed)
    {
        Construction? completed = null;

        if (Construction is { } construction && construction.CompletesAt <= now)
        {
            AccrueTo(construction.CompletesAt, economySpeed);
            _buildingLevels = new(_buildingLevels) { [construction.Building] = construction.TargetLevel };
            Construction = null;
            completed = construction;
        }

        AccrueTo(now, economySpeed);
        return completed;
    }

    public ResourceAmounts NextLevelCost(BuildingType building)
    {
        return BuildingRules.CostOf(building, _buildingLevels.LevelOf(building) + 1);
    }

    public TimeSpan BuildTime(ResourceAmounts cost, int economySpeed)
    {
        return BuildingRules.BuildTime(cost, _buildingLevels.LevelOf(BuildingType.RoboticsFactory), economySpeed);
    }

    /// <summary>
    /// Starts upgrading <paramref name="building"/> by one level, paying its cost up front.
    /// </summary>
    public StartConstructionResult StartConstruction(BuildingType building, DateTimeOffset now, int economySpeed)
    {
        SettleTo(now, economySpeed);

        if (Construction is not null)
        {
            return StartConstructionResult.AlreadyBuilding;
        }

        var cost = NextLevelCost(building);
        if (!Resources.Covers(cost))
        {
            return StartConstructionResult.NotEnoughResources;
        }

        Resources -= cost;
        Construction = new Construction(
            building,
            _buildingLevels.LevelOf(building) + 1,
            cost,
            now,
            now + BuildTime(cost, economySpeed));

        return StartConstructionResult.Started;
    }

    /// <summary>
    /// Cancels the construction in progress and refunds its full cost.
    /// </summary>
    /// <returns>The cancelled construction, or <c>null</c> when nothing was being built.</returns>
    public Construction? CancelConstruction(DateTimeOffset now, int economySpeed)
    {
        SettleTo(now, economySpeed);

        if (Construction is not { } construction)
        {
            return null;
        }

        Resources += construction.Cost;
        Construction = null;
        return construction;
    }

    private void AccrueTo(DateTimeOffset moment, int economySpeed)
    {
        if (moment <= ResourcesUpdatedAt)
        {
            return;
        }

        var production = Production(economySpeed);
        Resources = ResourceAccrual.Accrue(Resources, production.HourlyProduction, production.StorageCapacity, moment - ResourcesUpdatedAt);
        ResourcesUpdatedAt = moment;
    }
}

public enum StartConstructionResult
{
    Started,
    AlreadyBuilding,
    NotEnoughResources,
}