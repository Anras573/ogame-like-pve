using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Game.Api.Planets;
using Game.Api.Realtime;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Api.IntegrationTests.Infrastructure;

public static class ClientFactory
{
    public const string DefaultPassword = "Sup3r-secret!";

    /// <summary>
    /// Matches the API's JSON: camelCase properties and enums as names.
    /// </summary>
    public static JsonSerializerOptions JsonOptions { get; } = CreateJsonOptions();

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

    /// <summary>
    /// Registers a new account, joins the open season and returns the client with the new home planet's id.
    /// </summary>
    public static async Task<(HttpClient Client, Guid PlanetId, string Email)> CreatePlayerAsync(this CustomWebApplicationFactory factory)
    {
        var email = $"{Guid.NewGuid():N}@example.com";
        var client = await factory.CreateAuthenticatedClientAsync(email);

        var join = await client.PostAsync("/api/seasons/current/join", null);
        join.EnsureSuccessStatusCode();

        var planets = await client.GetFromJsonAsync<List<PlanetSummaryDto>>("/api/planets", JsonOptions);
        return (client, planets!.Single().Id, email);
    }

    /// <summary>
    /// Opens a SignalR connection to the game hub as an existing account.
    /// </summary>
    public static async Task<HubConnection> ConnectToGameHubAsync(this CustomWebApplicationFactory factory, string email)
    {
        // Log in without a cookie container so we can hand the cookie to SignalR ourselves
        var client = factory.Server.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login?useCookies=true", new { email, password = DefaultPassword });
        login.EnsureSuccessStatusCode();
        var cookie = login.Headers.GetValues("Set-Cookie").First().Split(';')[0];

        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(factory.Server.BaseAddress, GameHub.Route.TrimStart('/')), options =>
            {
                options.Transports = HttpTransportType.LongPolling;
                options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
                options.Headers["Cookie"] = cookie;
            })
            .AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .Build();

        await connection.StartAsync();
        return connection;
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}