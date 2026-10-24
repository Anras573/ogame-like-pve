namespace Game.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Gives every test a clean database while sharing one container across the collection.
/// </summary>
[Collection(ApiCollection.Name)]
public abstract class ApiTestBase(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    protected CustomWebApplicationFactory Factory { get; } = factory;

    public Task InitializeAsync() => Factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;
}