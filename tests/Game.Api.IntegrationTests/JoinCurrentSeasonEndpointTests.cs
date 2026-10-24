using System.Net;
using System.Net.Http.Json;
using Game.Api.IntegrationTests.Infrastructure;
using Game.Api.Seasons;

namespace Game.Api.IntegrationTests;

public class JoinCurrentSeasonEndpointTests(CustomWebApplicationFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task Handle_WithOpenSeason_CreatesPlayer()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var client = await Factory.CreateAuthenticatedClientAsync();

        // Act
        var response = await client.PostAsync("/api/seasons/current/join", null);
        var current = await client.GetFromJsonAsync<CurrentSeasonDto>("/api/seasons/current");

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var player = await response.Content.ReadFromJsonAsync<SeasonPlayerDto>();
        Assert.NotNull(player);
        Assert.NotNull(current?.Player);
        Assert.Equal(player.PlayerId, current.Player.PlayerId);
    }

    [Fact]
    public async Task Handle_WhenAlreadyJoined_ReturnsConflict()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var client = await Factory.CreateAuthenticatedClientAsync();
        await client.PostAsync("/api/seasons/current/join", null);

        // Act
        var response = await client.PostAsync("/api/seasons/current/join", null);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Handle_WithConcurrentRequests_CreatesOnlyOnePlayer()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var client = await Factory.CreateAuthenticatedClientAsync();

        // Act
        var responses = await Task.WhenAll(
            Enumerable.Range(0, 5).Select(_ => client.PostAsync("/api/seasons/current/join", null)));

        // Assert
        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Created);
        Assert.All(responses, r => Assert.Contains(r.StatusCode, new[] { HttpStatusCode.Created, HttpStatusCode.Conflict }));
    }

    [Fact]
    public async Task Handle_WithDifferentUsers_CreatesPlayerForEach()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var first = await Factory.CreateAuthenticatedClientAsync();
        var second = await Factory.CreateAuthenticatedClientAsync();

        // Act
        var firstResponse = await first.PostAsync("/api/seasons/current/join", null);
        var secondResponse = await second.PostAsync("/api/seasons/current/join", null);

        // Assert
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Handle_WithoutOpenSeason_ReturnsNotFound()
    {
        // Arrange
        var client = await Factory.CreateAuthenticatedClientAsync();

        // Act
        var response = await client.PostAsync("/api/seasons/current/join", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Handle_WithoutCookie_ReturnsUnauthorized()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsync("/api/seasons/current/join", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}