using System.Reflection;
using Game.Api.Infrastructure;
using Game.Api.Planets;
using Game.Api.Seasons;
using Game.Core.Buildings;
using Game.Core.Resources;
using Game.Core.Universe;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Api.IntegrationTests.Infrastructure;

public static class TestDataSeeder
{
    public static async Task<Season> AddSeasonAsync(
        this CustomWebApplicationFactory factory,
        string name,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        UniverseSettings? universe = null)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();

        var season = Season.Create(name, startsAt, endsAt, universe);
        context.Seasons.Add(season);
        await context.SaveChangesAsync();

        return season;
    }

    /// <summary>
    /// Adds a season that opened an hour ago and runs for twelve weeks.
    /// </summary>
    public static Task<Season> AddOpenSeasonAsync(
        this CustomWebApplicationFactory factory,
        string name = "Season 1",
        UniverseSettings? universe = null)
    {
        var now = factory.TimeProvider.GetUtcNow();
        return factory.AddSeasonAsync(name, now.AddHours(-1), now.AddDays(84), universe);
    }

    /// <summary>
    /// Puts a planet into a state that would take many real actions to reach (labs, shipyards, a full treasury).
    /// </summary>
    public static async Task UpdatePlanetAsync(
        this CustomWebApplicationFactory factory,
        Guid planetId,
        ResourceAmounts? resources = null,
        params (BuildingType Type, int Level)[] buildings)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        var planet = await context.Planets.SingleAsync(p => p.Id == new PlanetId(planetId));

        if (buildings.Length > 0)
        {
            typeof(Planet).GetField("_buildingLevels", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(planet, buildings.ToDictionary(b => b.Type, b => b.Level));
        }

        if (resources is { } amounts)
        {
            typeof(Planet).GetProperty(nameof(Planet.Resources))!.SetValue(planet, amounts);
        }

        await context.SaveChangesAsync();
    }
}