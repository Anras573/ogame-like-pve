using System.Net;
using System.Net.Http.Json;
using Game.Api.IntegrationTests.Infrastructure;
using Game.Api.Planets;
using Game.Api.Players;
using Game.Api.Realtime;
using Game.Api.Research;
using Game.Core.Buildings;
using Game.Core.Research;
using Game.Core.Resources;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Api.IntegrationTests;

public class ResearchEndpointsTests(CustomWebApplicationFactory factory) : ApiTestBase(factory)
{
    private static readonly ResourceAmounts s_plenty = new(100_000, 100_000, 100_000);

    private async Task<(HttpClient Client, Guid PlanetId, string Email)> PlayerWithLabAsync()
    {
        await Factory.AddOpenSeasonAsync();
        var player = await Factory.CreatePlayerAsync();
        await Factory.UpdatePlanetAsync(player.PlanetId, s_plenty, (BuildingType.ResearchLab, 1));
        return player;
    }

    private static Task<HttpResponseMessage> StartAsync(HttpClient client, Guid planetId, ResearchType research)
        => client.PostAsJsonAsync($"/api/planets/{planetId}/research", new StartResearchRequest(research), ClientFactory.JsonOptions);

    private static Task<ResearchOverviewDto?> GetAsync(HttpClient client, Guid planetId)
        => client.GetFromJsonAsync<ResearchOverviewDto>($"/api/planets/{planetId}/research", ClientFactory.JsonOptions);

    [Fact]
    public async Task GetResearch_WithoutLab_ListsLabAsMissingEverywhere()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var (client, planetId, _) = await Factory.CreatePlayerAsync();

        // Act
        var overview = await GetAsync(client, planetId);

        // Assert
        Assert.NotNull(overview);
        Assert.Equal(0, overview.ResearchLabLevel);
        Assert.Equal(Enum.GetValues<ResearchType>().Length, overview.Technologies.Count);
        Assert.All(overview.Technologies, t => Assert.Contains(t.MissingRequirements.Buildings, r => r.Building == BuildingType.ResearchLab));
        Assert.Null(overview.CurrentResearch);
    }

    [Fact]
    public async Task StartResearch_WithLab_PaysFromPlanetAndShowsProgress()
    {
        // Arrange
        var (client, planetId, _) = await PlayerWithLabAsync();

        // Act
        var response = await StartAsync(client, planetId, ResearchType.EnergyTechnology);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var overview = await response.Content.ReadFromJsonAsync<ResearchOverviewDto>(ClientFactory.JsonOptions);
        Assert.Equal(ResearchType.EnergyTechnology, overview!.CurrentResearch!.Research);
        Assert.Equal(planetId, overview.CurrentResearch.PlanetId);

        var planet = await client.GetFromJsonAsync<PlanetDto>($"/api/planets/{planetId}", ClientFactory.JsonOptions);
        Assert.Equal(100_000 - 800, planet!.Resources.Crystal, precision: 6);
    }

    [Fact]
    public async Task StartResearch_WithoutPrerequisites_ReturnsConflict()
    {
        // Arrange
        var (client, planetId, _) = await PlayerWithLabAsync();

        // Act
        var response = await StartAsync(client, planetId, ResearchType.CombustionDrive);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task StartResearch_WhileResearching_ReturnsConflict()
    {
        // Arrange
        var (client, planetId, _) = await PlayerWithLabAsync();
        await StartAsync(client, planetId, ResearchType.EnergyTechnology);

        // Act
        var response = await StartAsync(client, planetId, ResearchType.ComputerTechnology);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task StartResearch_WithConcurrentRequests_StartsExactlyOne()
    {
        // Arrange
        var (client, planetId, _) = await PlayerWithLabAsync();

        // Act
        var responses = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => StartAsync(client, planetId, ResearchType.EnergyTechnology)));
        var planet = await client.GetFromJsonAsync<PlanetDto>($"/api/planets/{planetId}", ClientFactory.JsonOptions);

        // Assert
        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.OK);
        Assert.Equal(100_000 - 800, planet!.Resources.Crystal, precision: 6); // Paid once
    }

    [Fact]
    public async Task GetResearch_AfterResearchTime_ShowsNewLevel()
    {
        // Arrange
        var (client, planetId, _) = await PlayerWithLabAsync();
        await StartAsync(client, planetId, ResearchType.EnergyTechnology);

        // Act
        Factory.TimeProvider.Advance(TimeSpan.FromHours(1));
        var overview = await GetAsync(client, planetId);

        // Assert
        Assert.Null(overview!.CurrentResearch);
        Assert.Equal(1, overview.Technologies.Single(t => t.Type == ResearchType.EnergyTechnology).Level);
        var combustion = overview.Technologies.Single(t => t.Type == ResearchType.CombustionDrive).MissingRequirements;
        Assert.Empty(combustion.Buildings);
        Assert.Empty(combustion.Research);
    }

    [Fact]
    public async Task CancelResearch_WhileResearching_RefundsPlanet()
    {
        // Arrange
        var (client, planetId, _) = await PlayerWithLabAsync();
        await StartAsync(client, planetId, ResearchType.EnergyTechnology);

        // Act
        var response = await client.DeleteAsync($"/api/planets/{planetId}/research");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var planet = await client.GetFromJsonAsync<PlanetDto>($"/api/planets/{planetId}", ClientFactory.JsonOptions);
        Assert.Equal(100_000, planet!.Resources.Crystal, precision: 6);
    }

    [Fact]
    public async Task CancelResearch_WithNothingResearching_ReturnsConflict()
    {
        // Arrange
        var (client, planetId, _) = await PlayerWithLabAsync();

        // Act
        var response = await client.DeleteAsync($"/api/planets/{planetId}/research");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetResearch_ForAnotherPlayersPlanet_ReturnsNotFound()
    {
        // Arrange
        var (_, planetId, _) = await PlayerWithLabAsync();
        var (otherClient, _, _) = await Factory.CreatePlayerAsync();

        // Act
        var response = await otherClient.GetAsync($"/api/planets/{planetId}/research");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ResearchCompletionProcessor_WithDueResearch_PersistsAndNotifies()
    {
        // Arrange
        var (client, planetId, email) = await PlayerWithLabAsync();
        await using var connection = await Factory.ConnectToGameHubAsync(email);
        var received = new TaskCompletionSource<ResearchCompletedMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<ResearchCompletedMessage>(nameof(IGameClient.ResearchCompleted), message => received.TrySetResult(message));
        await StartAsync(client, planetId, ResearchType.EnergyTechnology);
        Factory.TimeProvider.Advance(TimeSpan.FromHours(1));

        // Act
        var completed = await Factory.Services.GetRequiredService<ResearchCompletionProcessor>().ProcessDueAsync(CancellationToken.None);
        var message = await received.Task.WaitAsync(TimeSpan.FromSeconds(10));

        // Assert
        Assert.Equal(1, completed);
        Assert.Equal(new ResearchCompletedMessage(ResearchType.EnergyTechnology, 1), message);
    }
}