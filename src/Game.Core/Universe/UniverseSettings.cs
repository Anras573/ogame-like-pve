namespace Game.Core.Universe;

/// <summary>
/// The shape and pace of one season's universe.
/// </summary>
public sealed record UniverseSettings
{
    public const int PositionsPerSystem = 15;

    public static UniverseSettings Default { get; } = new(galaxies: 5, systemsPerGalaxy: 499, economySpeed: 1);

    public int Galaxies { get; }
    public int SystemsPerGalaxy { get; }

    /// <summary>
    /// Multiplies resource production and divides build times.
    /// </summary>
    public int EconomySpeed { get; }

    public UniverseSettings(int galaxies, int systemsPerGalaxy, int economySpeed)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(galaxies, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(systemsPerGalaxy, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(economySpeed, 1);

        Galaxies = galaxies;
        SystemsPerGalaxy = systemsPerGalaxy;
        EconomySpeed = economySpeed;
    }

    public bool Contains(Coordinates coordinates)
    {
        ArgumentNullException.ThrowIfNull(coordinates);

        return coordinates.Galaxy >= 1 && coordinates.Galaxy <= Galaxies
            && coordinates.System >= 1 && coordinates.System <= SystemsPerGalaxy
            && coordinates.Position >= 1 && coordinates.Position <= PositionsPerSystem;
    }
}