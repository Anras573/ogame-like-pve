using System.Net;
using System.Net.Http.Json;
using Game.Api.IntegrationTests.Infrastructure;
using Game.Api.Planets;
using Game.Core.Buildings;
using Game.Core.Resources;
using Game.Core.Units;

namespace Game.Api.IntegrationTests;

public class ShipyardEndpointsTests(CustomWebApplicationFactory factory) : ApiTestBase(factory)
{
    private static readonly ResourceAmounts s_plenty = new(100_000, 100_000, 100_000);

    private async Task<(HttpClient Client, Guid PlanetId)> PlayerWithShipyardAsync()
    {
        await Factory.AddOpenSeasonAsync();
        var (client, planetId, _) = await Factory.CreatePlayerAsync();
        await Factory.UpdatePlanetAsync(planetId, s_plenty, (BuildingType.RoboticsFactory, 2), (BuildingType.Shipyard, 1));
        return (client, planetId);
    }

    private static Task<HttpResponseMessage> EnqueueAsync(HttpClient client, Guid planetId, UnitType unit, int quantity)
        => client.PostAsJsonAsync($"/api/planets/{planetId}/shipyard", new EnqueueUnitsRequest(unit, quantity), ClientFactory.JsonOptions);

    [Fact]
    public async Task EnqueueUnits_WithShipyard_PaysAndQueues()
    {
        // Arrange
        var (client, planetId) = await PlayerWithShipyardAsync();

        // Act
        var response = await EnqueueAsync(client, planetId, UnitType.RocketLauncher, 3);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var planet = await response.Content.ReadFromJsonAsync<PlanetDto>(ClientFactory.JsonOptions);
        Assert.Equal(100_000 - 6000, planet!.Resources.Metal, precision: 6);
        Assert.Equal([new ShipyardOrderDto(UnitType.RocketLauncher, 3, 1440)], planet.ShipyardQueue);
        Assert.Equal(Factory.TimeProvider.GetUtcNow(), planet.ShipyardProgressAt);
    }

    [Fact]
    public async Task GetPlanet_AfterBuildTime_ShowsDeliveredUnits()
    {
        // Arrange
        var (client, planetId) = await PlayerWithShipyardAsync();
        await EnqueueAsync(client, planetId, UnitType.RocketLauncher, 3);

        // Act
        Factory.TimeProvider.Advance(TimeSpan.FromSeconds((1440 * 2) + 30));
        var planet = await client.GetFromJsonAsync<PlanetDto>($"/api/planets/{planetId}", ClientFactory.JsonOptions);

        // Assert
        Assert.Equal(2, planet!.Units.Single(u => u.Type == UnitType.RocketLauncher).Count);
        Assert.Equal([new ShipyardOrderDto(UnitType.RocketLauncher, 1, 1440)], planet.ShipyardQueue);
    }

    [Fact]
    public async Task EnqueueUnits_WithoutResearch_ReturnsConflict()
    {
        // Arrange
        var (client, planetId) = await PlayerWithShipyardAsync();

        // Act
        var response = await EnqueueAsync(client, planetId, UnitType.LightFighter, 1);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task EnqueueUnits_WithoutShipyard_ReturnsConflict()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var (client, planetId, _) = await Factory.CreatePlayerAsync();

        // Act
        var response = await EnqueueAsync(client, planetId, UnitType.RocketLauncher, 1);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task EnqueueUnits_WithZeroQuantity_ReturnsConflict()
    {
        // Arrange
        var (client, planetId) = await PlayerWithShipyardAsync();

        // Act
        var response = await EnqueueAsync(client, planetId, UnitType.RocketLauncher, 0);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task EnqueueUnits_WithUnknownUnit_ReturnsBadRequest()
    {
        // Arrange
        var (client, planetId) = await PlayerWithShipyardAsync();

        // Act
        var response = await client.PostAsJsonAsync($"/api/planets/{planetId}/shipyard", new { unit = "Mothership", quantity = 1 });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EnqueueUnits_WithConcurrentRequests_PaysForEachAcceptedOrder()
    {
        // Arrange
        var (client, planetId) = await PlayerWithShipyardAsync();

        // Act
        var responses = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => EnqueueAsync(client, planetId, UnitType.RocketLauncher, 1)));
        var planet = await client.GetFromJsonAsync<PlanetDto>($"/api/planets/{planetId}", ClientFactory.JsonOptions);

        // Assert: every accepted order is in the queue and paid for exactly once
        var accepted = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        Assert.InRange(accepted, 1, 5);
        Assert.Equal(accepted, planet!.ShipyardQueue.Sum(o => o.Quantity));
        Assert.Equal(100_000 - (2000 * accepted), planet.Resources.Metal, precision: 6);
    }

    [Fact]
    public async Task StartConstruction_ShipyardWithoutRoboticsFactory_ReturnsConflict()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var (client, planetId, _) = await Factory.CreatePlayerAsync();
        await Factory.UpdatePlanetAsync(planetId, s_plenty);

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/planets/{planetId}/construction",
            new StartConstructionRequest(BuildingType.Shipyard),
            ClientFactory.JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}