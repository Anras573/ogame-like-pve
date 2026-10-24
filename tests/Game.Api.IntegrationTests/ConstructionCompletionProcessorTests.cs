using System.Net.Http.Json;
using Game.Api.Infrastructure;
using Game.Api.IntegrationTests.Infrastructure;
using Game.Api.Planets;
using Game.Api.Realtime;
using Game.Core.Buildings;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Api.IntegrationTests;

public class ConstructionCompletionProcessorTests(CustomWebApplicationFactory factory) : ApiTestBase(factory)
{
    private ConstructionCompletionProcessor Processor => Factory.Services.GetRequiredService<ConstructionCompletionProcessor>();

    private async Task<Planet> LoadPlanetAsync(Guid planetId)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        return await context.Planets.AsNoTracking().SingleAsync(p => p.Id == new PlanetId(planetId));
    }

    [Fact]
    public async Task ProcessDueAsync_WithDueConstruction_PersistsCompletion()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var (client, planetId, _) = await Factory.CreatePlayerAsync();
        await client.PostAsJsonAsync($"/api/planets/{planetId}/construction", new StartConstructionRequest(BuildingType.MetalMine), ClientFactory.JsonOptions);
        Factory.TimeProvider.Advance(TimeSpan.FromMinutes(5));

        // Act
        var completed = await Processor.ProcessDueAsync(CancellationToken.None);

        // Assert
        Assert.Equal(1, completed);
        var planet = await LoadPlanetAsync(planetId);
        Assert.Null(planet.Construction);
        Assert.Equal(1, planet.BuildingLevels[BuildingType.MetalMine]);
    }

    [Fact]
    public async Task ProcessDueAsync_BeforeConstructionIsDue_DoesNothing()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var (client, planetId, _) = await Factory.CreatePlayerAsync();
        await client.PostAsJsonAsync($"/api/planets/{planetId}/construction", new StartConstructionRequest(BuildingType.MetalMine), ClientFactory.JsonOptions);
        Factory.TimeProvider.Advance(TimeSpan.FromSeconds(30));

        // Act
        var completed = await Processor.ProcessDueAsync(CancellationToken.None);

        // Assert
        Assert.Equal(0, completed);
        var planet = await LoadPlanetAsync(planetId);
        Assert.NotNull(planet.Construction);
    }

    [Fact]
    public async Task ProcessDueAsync_WithDueConstruction_NotifiesOwnerOverSignalR()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var (client, planetId, email) = await Factory.CreatePlayerAsync();
        await using var connection = await Factory.ConnectToGameHubAsync(email);
        var received = new TaskCompletionSource<BuildingCompletedMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<BuildingCompletedMessage>(nameof(IGameClient.BuildingCompleted), message => received.TrySetResult(message));

        await client.PostAsJsonAsync($"/api/planets/{planetId}/construction", new StartConstructionRequest(BuildingType.SolarPlant), ClientFactory.JsonOptions);
        Factory.TimeProvider.Advance(TimeSpan.FromMinutes(5));

        // Act
        await Processor.ProcessDueAsync(CancellationToken.None);
        var message = await received.Task.WaitAsync(TimeSpan.FromSeconds(10));

        // Assert
        Assert.Equal(new BuildingCompletedMessage(planetId, BuildingType.SolarPlant, 1), message);
    }

    [Fact]
    public async Task ProcessDueAsync_WithDueConstruction_DoesNotNotifyOtherPlayers()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var (client, planetId, _) = await Factory.CreatePlayerAsync();
        var (_, _, otherEmail) = await Factory.CreatePlayerAsync();
        await using var otherConnection = await Factory.ConnectToGameHubAsync(otherEmail);
        var otherReceived = false;
        otherConnection.On<BuildingCompletedMessage>(nameof(IGameClient.BuildingCompleted), _ => otherReceived = true);

        await client.PostAsJsonAsync($"/api/planets/{planetId}/construction", new StartConstructionRequest(BuildingType.SolarPlant), ClientFactory.JsonOptions);
        Factory.TimeProvider.Advance(TimeSpan.FromMinutes(5));

        // Act
        await Processor.ProcessDueAsync(CancellationToken.None);
        await Task.Delay(500);

        // Assert
        Assert.False(otherReceived);
    }
}