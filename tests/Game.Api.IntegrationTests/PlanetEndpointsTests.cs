using System.Net;
using System.Net.Http.Json;
using Game.Api.IntegrationTests.Infrastructure;
using Game.Api.Planets;
using Game.Core.Buildings;
using Game.Core.Universe;

namespace Game.Api.IntegrationTests;

public class PlanetEndpointsTests(CustomWebApplicationFactory factory) : ApiTestBase(factory)
{
    private static Task<PlanetDto?> GetPlanetAsync(HttpClient client, Guid planetId)
        => client.GetFromJsonAsync<PlanetDto>($"/api/planets/{planetId}", ClientFactory.JsonOptions);

    private static Task<HttpResponseMessage> StartAsync(HttpClient client, Guid planetId, BuildingType building)
        => client.PostAsJsonAsync($"/api/planets/{planetId}/construction", new StartConstructionRequest(building), ClientFactory.JsonOptions);

    [Fact]
    public async Task JoinSeason_Always_CreatesHomePlanetInsideUniverse()
    {
        // Arrange
        var universe = new UniverseSettings(galaxies: 2, systemsPerGalaxy: 3, economySpeed: 1);
        await Factory.AddOpenSeasonAsync(universe: universe);

        // Act
        var (client, planetId, _) = await Factory.CreatePlayerAsync();
        var planet = await GetPlanetAsync(client, planetId);

        // Assert
        Assert.NotNull(planet);
        Assert.Equal(Planet.HomeName, planet.Name);
        Assert.True(universe.Contains(new Coordinates(planet.Coordinates.Galaxy, planet.Coordinates.System, planet.Coordinates.Position)));
        Assert.InRange(planet.Coordinates.Position, PlanetPlacement.FirstHomePosition, PlanetPlacement.LastHomePosition);
        Assert.Equal(new ResourcesDto(500, 500, 0), planet.Resources);
        Assert.All(planet.Buildings, b => Assert.Equal(0, b.Level));
        Assert.Equal(Enum.GetValues<BuildingType>().Length, planet.Buildings.Count);
    }

    [Fact]
    public async Task JoinSeason_WhenUniverseIsFull_ReturnsServiceUnavailable()
    {
        // Arrange: one system has 9 home positions
        await Factory.AddOpenSeasonAsync(universe: new UniverseSettings(galaxies: 1, systemsPerGalaxy: 1, economySpeed: 1));
        var planets = new List<PlanetDto>();
        for (var i = 0; i < 9; i++)
        {
            var (client, planetId, _) = await Factory.CreatePlayerAsync();
            planets.Add((await GetPlanetAsync(client, planetId))!);
        }

        var lateComer = await Factory.CreateAuthenticatedClientAsync();

        // Act
        var response = await lateComer.PostAsync("/api/seasons/current/join", null);

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal(9, planets.Select(p => p.Coordinates.Position).Distinct().Count());
    }

    [Fact]
    public async Task GetPlanets_BeforeJoining_ReturnsEmptyList()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var client = await Factory.CreateAuthenticatedClientAsync();

        // Act
        var planets = await client.GetFromJsonAsync<List<PlanetSummaryDto>>("/api/planets", ClientFactory.JsonOptions);

        // Assert
        Assert.NotNull(planets);
        Assert.Empty(planets);
    }

    [Fact]
    public async Task GetPlanet_OwnedByAnotherPlayer_ReturnsNotFound()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var (_, planetId, _) = await Factory.CreatePlayerAsync();
        var (otherClient, _, _) = await Factory.CreatePlayerAsync();

        // Act
        var response = await otherClient.GetAsync($"/api/planets/{planetId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPlanet_AfterTimePasses_ShowsAccruedResources()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var (client, planetId, _) = await Factory.CreatePlayerAsync();

        // Act
        Factory.TimeProvider.Advance(TimeSpan.FromHours(2));
        var planet = await GetPlanetAsync(client, planetId);

        // Assert
        Assert.Equal(560, planet!.Resources.Metal, precision: 6);
        Assert.Equal(530, planet.Resources.Crystal, precision: 6);
        Assert.Equal(Factory.TimeProvider.GetUtcNow(), planet.ServerTime);
    }

    [Fact]
    public async Task StartConstruction_WithEnoughResources_PaysAndSchedules()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var (client, planetId, _) = await Factory.CreatePlayerAsync();

        // Act
        var response = await StartAsync(client, planetId, BuildingType.MetalMine);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var planet = await response.Content.ReadFromJsonAsync<PlanetDto>(ClientFactory.JsonOptions);
        Assert.Equal(new ResourcesDto(440, 485, 0), planet!.Resources);
        Assert.NotNull(planet.Construction);
        Assert.Equal(BuildingType.MetalMine, planet.Construction.Building);
        Assert.Equal(1, planet.Construction.TargetLevel);
    }

    [Fact]
    public async Task StartConstruction_WhileBuilding_ReturnsConflict()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var (client, planetId, _) = await Factory.CreatePlayerAsync();
        await StartAsync(client, planetId, BuildingType.MetalMine);

        // Act
        var response = await StartAsync(client, planetId, BuildingType.CrystalMine);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task StartConstruction_WithConcurrentRequests_StartsExactlyOne()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var (client, planetId, _) = await Factory.CreatePlayerAsync();

        // Act
        var responses = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => StartAsync(client, planetId, BuildingType.MetalMine)));
        var planet = await GetPlanetAsync(client, planetId);

        // Assert
        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.OK);
        Assert.All(responses, r => Assert.Contains(r.StatusCode, new[] { HttpStatusCode.OK, HttpStatusCode.Conflict }));
        Assert.Equal(new ResourcesDto(440, 485, 0), planet!.Resources); // Paid once
    }

    [Fact]
    public async Task StartConstruction_WithoutEnoughResources_ReturnsConflict()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var (client, planetId, _) = await Factory.CreatePlayerAsync();

        // Act
        var response = await StartAsync(client, planetId, BuildingType.RoboticsFactory);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var planet = await GetPlanetAsync(client, planetId);
        Assert.Null(planet!.Construction);
    }

    [Fact]
    public async Task StartConstruction_WithUnknownBuilding_ReturnsBadRequest()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var (client, planetId, _) = await Factory.CreatePlayerAsync();

        // Act
        var response = await client.PostAsJsonAsync($"/api/planets/{planetId}/construction", new { building = "DeathStar" });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task StartConstruction_OnAnotherPlayersPlanet_ReturnsNotFound()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var (_, planetId, _) = await Factory.CreatePlayerAsync();
        var (otherClient, _, _) = await Factory.CreatePlayerAsync();

        // Act
        var response = await StartAsync(otherClient, planetId, BuildingType.MetalMine);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task StartConstruction_AfterSeasonEnded_ReturnsConflict()
    {
        // Arrange
        var now = Factory.TimeProvider.GetUtcNow();
        await Factory.AddSeasonAsync("Short", now.AddHours(-1), now.AddHours(1));
        var (client, planetId, _) = await Factory.CreatePlayerAsync();
        Factory.TimeProvider.Advance(TimeSpan.FromHours(2));

        // Act
        var response = await StartAsync(client, planetId, BuildingType.MetalMine);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetPlanet_AfterConstructionTime_ShowsNewLevel()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var (client, planetId, _) = await Factory.CreatePlayerAsync();
        await StartAsync(client, planetId, BuildingType.MetalMine);

        // Act
        Factory.TimeProvider.Advance(TimeSpan.FromMinutes(5));
        var planet = await GetPlanetAsync(client, planetId);

        // Assert
        Assert.Null(planet!.Construction);
        Assert.Equal(1, planet.Buildings.Single(b => b.Type == BuildingType.MetalMine).Level);
    }

    [Fact]
    public async Task CancelConstruction_WhileBuilding_RefundsCost()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var (client, planetId, _) = await Factory.CreatePlayerAsync();
        await StartAsync(client, planetId, BuildingType.MetalMine);

        // Act
        var response = await client.DeleteAsync($"/api/planets/{planetId}/construction");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var planet = await response.Content.ReadFromJsonAsync<PlanetDto>(ClientFactory.JsonOptions);
        Assert.Null(planet!.Construction);
        Assert.Equal(new ResourcesDto(500, 500, 0), planet.Resources);
    }

    [Fact]
    public async Task CancelConstruction_WithNothingBuilding_ReturnsConflict()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var (client, planetId, _) = await Factory.CreatePlayerAsync();

        // Act
        var response = await client.DeleteAsync($"/api/planets/{planetId}/construction");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}