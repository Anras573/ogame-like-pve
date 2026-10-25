namespace Game.Core.Universe;

public static class PlanetPlacement
{
    /// <summary>
    /// Home planets go in the temperate middle of a system, away from the hottest and coldest orbits.
    /// </summary>
    public const int FirstHomePosition = 4;
    public const int LastHomePosition = 12;

    /// <summary>
    /// Every system in the universe exactly once, starting from a random one, so new players spread out
    /// but a free slot is always found while one exists.
    /// </summary>
    public static IEnumerable<(int Galaxy, int System)> HomeSearchOrder(UniverseSettings settings, Random random)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(random);

        var totalSystems = settings.Galaxies * settings.SystemsPerGalaxy;
        var start = random.Next(totalSystems);

        for (var offset = 0; offset < totalSystems; offset++)
        {
            var index = (start + offset) % totalSystems;
            yield return ((index / settings.SystemsPerGalaxy) + 1, (index % settings.SystemsPerGalaxy) + 1);
        }
    }

    /// <summary>
    /// A random free home position in a system, or <c>null</c> when the system's home positions are all taken.
    /// </summary>
    public static int? PickFreeHomePosition(IReadOnlyCollection<int> occupiedPositions, Random random)
    {
        ArgumentNullException.ThrowIfNull(occupiedPositions);
        ArgumentNullException.ThrowIfNull(random);

        var free = Enumerable.Range(FirstHomePosition, LastHomePosition - FirstHomePosition + 1)
            .Where(p => !occupiedPositions.Contains(p))
            .ToList();

        return free.Count == 0 ? null : free[random.Next(free.Count)];
    }

    /// <summary>
    /// Planets close to their star are hot, distant ones cold: position 1 lands around 230°C, position 15 around -120°C.
    /// </summary>
    public static int RollMaxTemperature(int position, Random random)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(position, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(position, UniverseSettings.PositionsPerSystem);
        ArgumentNullException.ThrowIfNull(random);

        var baseline = 230 - (25 * (position - 1));
        return baseline + random.Next(-10, 11);
    }
}