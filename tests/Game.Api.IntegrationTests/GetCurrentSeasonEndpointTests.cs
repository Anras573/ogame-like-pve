using System.Net;
using System.Net.Http.Json;
using Game.Api.IntegrationTests.Infrastructure;
using Game.Api.Seasons;

namespace Game.Api.IntegrationTests;

public class GetCurrentSeasonEndpointTests(CustomWebApplicationFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task Handle_WithOpenSeason_ReturnsSeasonWithoutPlayer()
    {
        // Arrange
        var season = await Factory.AddOpenSeasonAsync("Season 1");
        var client = await Factory.CreateAuthenticatedClientAsync();

        // Act
        var current = await client.GetFromJsonAsync<CurrentSeasonDto>("/api/seasons/current");

        // Assert
        Assert.NotNull(current);
        Assert.Equal(season.Id.Value, current.Id);
        Assert.Equal("Season 1", current.Name);
        Assert.Null(current.Player);
    }

    [Fact]
    public async Task Handle_WithOnlyEndedAndUpcomingSeasons_ReturnsNotFound()
    {
        // Arrange
        var now = Factory.TimeProvider.GetUtcNow();
        await Factory.AddSeasonAsync("Ended", now.AddDays(-90), now.AddDays(-1));
        await Factory.AddSeasonAsync("Upcoming", now.AddDays(1), now.AddDays(90));
        var client = await Factory.CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/seasons/current");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Handle_WithOverlappingSeasons_ReturnsMostRecentlyStarted()
    {
        // Arrange
        var now = Factory.TimeProvider.GetUtcNow();
        await Factory.AddSeasonAsync("Older", now.AddDays(-30), now.AddDays(30));
        var newer = await Factory.AddSeasonAsync("Newer", now.AddDays(-1), now.AddDays(60));
        var client = await Factory.CreateAuthenticatedClientAsync();

        // Act
        var current = await client.GetFromJsonAsync<CurrentSeasonDto>("/api/seasons/current");

        // Assert
        Assert.NotNull(current);
        Assert.Equal(newer.Id.Value, current.Id);
    }

    [Fact]
    public async Task Handle_WithoutCookie_ReturnsUnauthorized()
    {
        // Arrange
        await Factory.AddOpenSeasonAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/seasons/current");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}