using System.Net;
using System.Net.Http.Json;
using Game.Api.Auth;
using Game.Api.IntegrationTests.Infrastructure;

namespace Game.Api.IntegrationTests;

public class AuthEndpointsTests(CustomWebApplicationFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task GetCurrentUser_AfterRegisterAndLogin_ReturnsEmail()
    {
        // Arrange
        var client = await Factory.CreateAuthenticatedClientAsync("commander@example.com");

        // Act
        var user = await client.GetFromJsonAsync<CurrentUserDto>("/api/auth/me");

        // Assert
        Assert.NotNull(user);
        Assert.Equal("commander@example.com", user.Email);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutCookie_ReturnsUnauthorized()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WhenSignedIn_ClearsSession()
    {
        // Arrange
        var client = await Factory.CreateAuthenticatedClientAsync();

        // Act
        var logout = await client.PostAsync("/api/auth/logout", null);
        var me = await client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        // Arrange
        var client = Factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register", new { email = "a@example.com", password = ClientFactory.DefaultPassword });

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login?useCookies=true", new { email = "a@example.com", password = "Wrong-password1" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ReturnsBadRequest()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", new { email = "weak@example.com", password = "123" });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}