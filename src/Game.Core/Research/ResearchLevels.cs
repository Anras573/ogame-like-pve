namespace Game.Core.Research;

public static class ResearchLevels
{
    /// <summary>
    /// The level of <paramref name="type"/>; research that was never done is level 0.
    /// </summary>
    public static int LevelOf(this IReadOnlyDictionary<ResearchType, int> levels, ResearchType type)
    {
        ArgumentNullException.ThrowIfNull(levels);

        return levels.TryGetValue(type, out var level) ? level : 0;
    }
}