namespace Game.Core.Buildings;

public static class BuildingLevels
{
    /// <summary>
    /// The level of <paramref name="type"/>; buildings that were never built are level 0.
    /// </summary>
    public static int LevelOf(this IReadOnlyDictionary<BuildingType, int> levels, BuildingType type)
    {
        ArgumentNullException.ThrowIfNull(levels);

        return levels.TryGetValue(type, out var level) ? level : 0;
    }
}