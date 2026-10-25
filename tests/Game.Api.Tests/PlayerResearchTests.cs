using Game.Api.Planets;
using Game.Api.Players;
using Game.Core.Buildings;
using Game.Core.Research;
using Game.Core.Resources;

namespace Game.Api.Tests;

public class PlayerResearchTests
{
    private const int Speed = 1;
    private static readonly DateTimeOffset s_start = TestPlanets.Start;
    private static readonly ResourceAmounts s_plenty = new(100_000, 100_000, 100_000);

    private static (Player Player, Planet Planet) PlayerWithLab()
    {
        var (player, planet) = TestPlanets.Create();
        planet.WithBuildings((BuildingType.ResearchLab, 1)).WithResources(s_plenty);
        return (player, planet);
    }

    [Fact]
    public void StartResearch_WithLabAndResources_PaysFromPlanetAndSchedules()
    {
        // Arrange
        var (player, planet) = PlayerWithLab();

        // Act
        var result = player.StartResearch(ResearchType.EnergyTechnology, planet, s_start, Speed);

        // Assert
        Assert.Equal(StartResearchResult.Started, result);
        Assert.Equal(s_plenty - new ResourceAmounts(0, 800, 400), planet.Resources);
        Assert.NotNull(player.CurrentResearch);
        Assert.Equal(ResearchType.EnergyTechnology, player.CurrentResearch.Research);
        Assert.Equal(1, player.CurrentResearch.TargetLevel);
        Assert.Equal(planet.Id, player.CurrentResearch.PlanetId);
        Assert.Equal(s_start.AddSeconds(1440), player.CurrentResearch.CompletesAt);
    }

    [Fact]
    public void StartResearch_WithoutLab_ReturnsRequirementsNotMet()
    {
        // Arrange
        var (player, planet) = TestPlanets.Create();
        planet.WithResources(s_plenty);

        // Act
        var result = player.StartResearch(ResearchType.EnergyTechnology, planet, s_start, Speed);

        // Assert
        Assert.Equal(StartResearchResult.RequirementsNotMet, result);
        Assert.Equal(s_plenty, planet.Resources);
    }

    [Fact]
    public void StartResearch_WithoutPrerequisiteResearch_ReturnsRequirementsNotMet()
    {
        // Arrange
        var (player, planet) = PlayerWithLab();

        // Act
        var result = player.StartResearch(ResearchType.CombustionDrive, planet, s_start, Speed); // needs Energy 1

        // Assert
        Assert.Equal(StartResearchResult.RequirementsNotMet, result);
    }

    [Fact]
    public void StartResearch_WhileResearching_ReturnsAlreadyResearching()
    {
        // Arrange
        var (player, planet) = PlayerWithLab();
        player.StartResearch(ResearchType.EnergyTechnology, planet, s_start, Speed);

        // Act
        var result = player.StartResearch(ResearchType.ComputerTechnology, planet, s_start.AddSeconds(10), Speed);

        // Assert
        Assert.Equal(StartResearchResult.AlreadyResearching, result);
    }

    [Fact]
    public void StartResearch_WithoutEnoughResources_ReturnsNotEnoughResources()
    {
        // Arrange
        var (player, planet) = PlayerWithLab();
        planet.WithResources(new ResourceAmounts(0, 0, 0));

        // Act
        var result = player.StartResearch(ResearchType.EnergyTechnology, planet, s_start, Speed);

        // Assert
        Assert.Equal(StartResearchResult.NotEnoughResources, result);
        Assert.Null(player.CurrentResearch);
    }

    [Fact]
    public void StartResearch_AfterPrerequisiteCompleted_Starts()
    {
        // Arrange
        var (player, planet) = PlayerWithLab();
        player.StartResearch(ResearchType.EnergyTechnology, planet, s_start, Speed);

        // Act
        var result = player.StartResearch(ResearchType.CombustionDrive, planet, s_start.AddHours(1), Speed);

        // Assert
        Assert.Equal(StartResearchResult.Started, result);
        Assert.Equal(1, player.ResearchLevels[ResearchType.EnergyTechnology]);
    }

    [Fact]
    public void StartResearch_OnAnotherPlayersPlanet_ThrowsArgumentException()
    {
        // Arrange
        var (player, _) = PlayerWithLab();
        var (_, otherPlanet) = PlayerWithLab();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => player.StartResearch(ResearchType.EnergyTechnology, otherPlanet, s_start, Speed));
    }

    [Fact]
    public void SettleResearch_BeforeCompletion_ReturnsNull()
    {
        // Arrange
        var (player, planet) = PlayerWithLab();
        player.StartResearch(ResearchType.EnergyTechnology, planet, s_start, Speed);

        // Act
        var completed = player.SettleResearch(s_start.AddSeconds(1439));

        // Assert
        Assert.Null(completed);
        Assert.NotNull(player.CurrentResearch);
    }

    [Fact]
    public void SettleResearch_AtCompletion_RaisesLevel()
    {
        // Arrange
        var (player, planet) = PlayerWithLab();
        player.StartResearch(ResearchType.EnergyTechnology, planet, s_start, Speed);

        // Act
        var completed = player.SettleResearch(s_start.AddSeconds(1440));

        // Assert
        Assert.NotNull(completed);
        Assert.Equal(1, player.ResearchLevels[ResearchType.EnergyTechnology]);
        Assert.Null(player.CurrentResearch);
    }

    [Fact]
    public void CancelResearch_WhileResearching_RefundsPlanet()
    {
        // Arrange
        var (player, planet) = PlayerWithLab();
        player.StartResearch(ResearchType.EnergyTechnology, planet, s_start, Speed);

        // Act
        var cancelled = player.CancelResearch(planet, s_start, Speed);

        // Assert
        Assert.NotNull(cancelled);
        Assert.Null(player.CurrentResearch);
        Assert.Equal(s_plenty, planet.Resources);
    }

    [Fact]
    public void CancelResearch_WithNothingResearching_ReturnsNull()
    {
        // Arrange
        var (player, planet) = PlayerWithLab();

        // Act
        var cancelled = player.CancelResearch(planet, s_start, Speed);

        // Assert
        Assert.Null(cancelled);
    }

    [Fact]
    public void MissingRequirements_ForShipyardBuilding_NeedsRoboticsFactory()
    {
        // Arrange
        var (player, planet) = TestPlanets.Create();

        // Act
        var missing = planet.MissingRequirements(BuildingType.Shipyard, player.ResearchLevels);

        // Assert
        Assert.Single(missing);
    }
}