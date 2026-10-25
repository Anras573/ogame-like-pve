using System.Reflection;
using Game.Api.Planets;
using Game.Api.Players;
using Game.Api.Seasons;
using Game.Core.Buildings;
using Game.Core.Resources;
using Game.Core.Universe;

namespace Game.Api.Tests;

/// <summary>
/// Builds players and planets in states that would take many real actions to reach.
/// </summary>
internal static class TestPlanets
{
    public static readonly DateTimeOffset Start = new(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);

    public static (Player Player, Planet Planet) Create()
    {
        var season = Season.Create("Season 1", Start.AddDays(-1), Start.AddDays(84));
        var player = Player.Join(season, Guid.NewGuid(), Start)!;
        var planet = Planet.CreateHome(player, new Coordinates(1, 2, 8), maxTemperature: 40, Start);
        return (player, planet);
    }

    public static Planet WithBuildings(this Planet planet, params (BuildingType Type, int Level)[] levels)
    {
        SetField(planet, "_buildingLevels", levels.ToDictionary(l => l.Type, l => l.Level));
        return planet;
    }

    public static Planet WithResources(this Planet planet, ResourceAmounts resources)
    {
        typeof(Planet).GetProperty(nameof(Planet.Resources))!.SetValue(planet, resources);
        return planet;
    }

    private static void SetField(object target, string field, object value)
    {
        target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(target, value);
    }
}