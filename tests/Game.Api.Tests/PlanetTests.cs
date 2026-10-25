using Game.Api.Planets;
using Game.Api.Players;
using Game.Api.Seasons;
using Game.Core.Buildings;
using Game.Core.Research;
using Game.Core.Resources;
using Game.Core.Universe;

namespace Game.Api.Tests;

public class PlanetTests
{
    private const int Speed = 1;
    private static readonly Dictionary<ResearchType, int> s_noResearch = [];
    private static readonly DateTimeOffset s_start = new(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);

    private static Planet CreatePlanet()
    {
        var season = Season.Create("Season 1", s_start.AddDays(-1), s_start.AddDays(84));
        var player = Player.Join(season, Guid.NewGuid(), s_start)!;
        return Planet.CreateHome(player, new Coordinates(1, 2, 8), maxTemperature: 40, s_start);
    }

    [Fact]
    public void CreateHome_ForPlayer_StartsWithStartingResourcesAndNoBuildings()
    {
        // Arrange
        var season = Season.Create("Season 1", s_start.AddDays(-1), s_start.AddDays(84));
        var player = Player.Join(season, Guid.NewGuid(), s_start)!;

        // Act
        var planet = Planet.CreateHome(player, new Coordinates(1, 2, 8), maxTemperature: 40, s_start);

        // Assert
        Assert.Equal(player.Id, planet.PlayerId);
        Assert.Equal(season.Id, planet.SeasonId);
        Assert.Equal(new Coordinates(1, 2, 8), planet.Coordinates);
        Assert.Equal(Planet.StartingResources, planet.Resources);
        Assert.Empty(planet.BuildingLevels);
        Assert.Null(planet.Construction);
    }

    [Fact]
    public void SettleTo_AfterOneHour_AccruesBaseProduction()
    {
        // Arrange
        var planet = CreatePlanet();

        // Act
        var completed = planet.SettleTo(s_start.AddHours(1), Speed);

        // Assert
        Assert.Null(completed);
        Assert.Equal(new ResourceAmounts(530, 515, 0), planet.Resources);
        Assert.Equal(s_start.AddHours(1), planet.ResourcesUpdatedAt);
    }

    [Fact]
    public void SettleTo_EarlierMoment_ChangesNothing()
    {
        // Arrange
        var planet = CreatePlanet();
        planet.SettleTo(s_start.AddHours(1), Speed);

        // Act
        planet.SettleTo(s_start, Speed);

        // Assert
        Assert.Equal(new ResourceAmounts(530, 515, 0), planet.Resources);
        Assert.Equal(s_start.AddHours(1), planet.ResourcesUpdatedAt);
    }

    [Fact]
    public void StartConstruction_WithEnoughResources_PaysCostAndSchedulesCompletion()
    {
        // Arrange
        var planet = CreatePlanet();

        // Act
        var result = planet.StartConstruction(BuildingType.MetalMine, s_noResearch, s_start, Speed);

        // Assert
        Assert.Equal(StartConstructionResult.Started, result);
        Assert.Equal(new ResourceAmounts(440, 485, 0), planet.Resources);
        Assert.NotNull(planet.Construction);
        Assert.Equal(BuildingType.MetalMine, planet.Construction.Building);
        Assert.Equal(1, planet.Construction.TargetLevel);
        Assert.Equal(s_start.AddSeconds(108), planet.Construction.CompletesAt);
    }

    [Fact]
    public void StartConstruction_WhileBuilding_ReturnsAlreadyBuilding()
    {
        // Arrange
        var planet = CreatePlanet();
        planet.StartConstruction(BuildingType.MetalMine, s_noResearch, s_start, Speed);

        // Act
        var result = planet.StartConstruction(BuildingType.CrystalMine, s_noResearch, s_start.AddSeconds(10), Speed);

        // Assert
        Assert.Equal(StartConstructionResult.AlreadyBuilding, result);
        Assert.Equal(BuildingType.MetalMine, planet.Construction!.Building);
    }

    [Fact]
    public void StartConstruction_WithoutEnoughResources_ReturnsNotEnoughResourcesAndKeepsResources()
    {
        // Arrange
        var planet = CreatePlanet();

        // Act
        var result = planet.StartConstruction(BuildingType.RoboticsFactory, s_noResearch, s_start, Speed); // needs 200 deuterium

        // Assert
        Assert.Equal(StartConstructionResult.NotEnoughResources, result);
        Assert.Equal(Planet.StartingResources, planet.Resources);
        Assert.Null(planet.Construction);
    }

    [Fact]
    public void StartConstruction_AfterPreviousOneCompleted_StartsNextLevel()
    {
        // Arrange
        var planet = CreatePlanet();
        planet.StartConstruction(BuildingType.MetalMine, s_noResearch, s_start, Speed);

        // Act
        var result = planet.StartConstruction(BuildingType.MetalMine, s_noResearch, s_start.AddMinutes(10), Speed);

        // Assert
        Assert.Equal(StartConstructionResult.Started, result);
        Assert.Equal(1, planet.BuildingLevels[BuildingType.MetalMine]);
        Assert.Equal(2, planet.Construction!.TargetLevel);
    }

    [Fact]
    public void SettleTo_PastCompletion_RaisesLevelAndSwitchesRatesAtCompletion()
    {
        // Arrange
        var planet = CreatePlanet();
        planet.StartConstruction(BuildingType.SolarPlant, s_noResearch, s_start, Speed); // 75 metal, 30 crystal, 151 s
        var completesAt = planet.Construction!.CompletesAt;

        // Act
        var completed = planet.SettleTo(completesAt.AddHours(1), Speed);

        // Assert
        Assert.NotNull(completed);
        Assert.Equal(BuildingType.SolarPlant, completed.Building);
        Assert.Equal(1, planet.BuildingLevels[BuildingType.SolarPlant]);
        Assert.Null(planet.Construction);

        // A solar plant alone doesn't change production, so this is base output for the whole period
        var hours = (completesAt.AddHours(1) - s_start).TotalHours;
        Assert.Equal(500 - 75 + (30 * hours), planet.Resources.Metal, precision: 6);
    }

    [Fact]
    public void SettleTo_PastMineCompletion_ProducesMoreOnlyAfterCompletion()
    {
        // Arrange
        var planet = CreatePlanet();
        planet.StartConstruction(BuildingType.SolarPlant, s_noResearch, s_start, Speed);
        var solarDone = planet.Construction!.CompletesAt;
        planet.StartConstruction(BuildingType.MetalMine, s_noResearch, solarDone, Speed);
        var mineDone = planet.Construction!.CompletesAt;
        var metalAtMineStart = planet.Resources.Metal;

        // Act
        planet.SettleTo(mineDone.AddHours(1), Speed);

        // Assert
        var beforeCompletion = 30 * (mineDone - solarDone).TotalHours;
        var afterCompletion = 63; // 30 base + 33 from a powered level-1 mine
        Assert.Equal(metalAtMineStart + beforeCompletion + afterCompletion, planet.Resources.Metal, precision: 6);
    }

    [Fact]
    public void CancelConstruction_WhileBuilding_RefundsFullCost()
    {
        // Arrange
        var planet = CreatePlanet();
        planet.StartConstruction(BuildingType.MetalMine, s_noResearch, s_start, Speed);

        // Act
        var cancelled = planet.CancelConstruction(s_start, Speed);

        // Assert
        Assert.NotNull(cancelled);
        Assert.Null(planet.Construction);
        Assert.Equal(Planet.StartingResources, planet.Resources);
        Assert.Empty(planet.BuildingLevels);
    }

    [Fact]
    public void CancelConstruction_AfterItCompleted_ReturnsNullAndKeepsLevel()
    {
        // Arrange
        var planet = CreatePlanet();
        planet.StartConstruction(BuildingType.MetalMine, s_noResearch, s_start, Speed);

        // Act
        var cancelled = planet.CancelConstruction(s_start.AddHours(1), Speed);

        // Assert
        Assert.Null(cancelled);
        Assert.Equal(1, planet.BuildingLevels[BuildingType.MetalMine]);
    }

    [Fact]
    public void CancelConstruction_WithNothingBuilding_ReturnsNull()
    {
        // Arrange
        var planet = CreatePlanet();

        // Act
        var cancelled = planet.CancelConstruction(s_start, Speed);

        // Assert
        Assert.Null(cancelled);
    }
}