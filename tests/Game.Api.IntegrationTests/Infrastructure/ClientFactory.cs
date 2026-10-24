using System.Net.Http.Json;

namespace Game.Api.IntegrationTests.Infrastructure;

public static class ClientFactory
{
    public const string DefaultPassword = "Sup3r-secret!";

    /// <summary>
    /// Registers a new account and returns a client carrying its auth cookie.
    /// </summary>
    public static async Task<HttpClient> CreateAuthenticatedClientAsync(
        this CustomWebApplicationFactory factory,
        string? email = null)
    {
        email ??= $"{Guid.NewGuid():N}@example.com";
        var client = factory.CreateClient();

        var register = await client.PostAsJsonAsync("/api/auth/register", new { email, password = DefaultPassword });
        register.EnsureSuccessStatusCode();

        var login = await client.PostAsJsonAsync("/api/auth/login?useCookies=true", new { email, password = DefaultPassword });
        login.EnsureSuccessStatusCode();

        return client;
    }
}