using Game.Api.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using Testcontainers.PostgreSql;

namespace Game.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Runs the API against a throwaway PostgreSQL container, with a controllable clock.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:17-alpine").Build();

    /// <summary>
    /// Starts on a whole second (PostgreSQL keeps microseconds, so finer ticks would round-trip inexactly) and only
    /// ever moves forward: tests advance it, and every test builds its data relative to its current value.
    /// </summary>
    public FakeTimeProvider TimeProvider { get; } = new(DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds()));

    public async Task InitializeAsync()
    {
        await _database.StartAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        await context.Database.MigrateAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        await context.Database.ExecuteSqlRawAsync("""TRUNCATE "Planets", "Players", "Seasons", "AspNetUsers" CASCADE""");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Game", _database.GetConnectionString());
        builder.UseSetting("RateLimiting:AuthPermitLimit", "10000");

        // Tests drive the completion processor directly instead of racing a background loop
        builder.UseSetting("Worker:Enabled", "false");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(TimeProvider);
        });
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<CustomWebApplicationFactory>
{
    public const string Name = "Api";
}