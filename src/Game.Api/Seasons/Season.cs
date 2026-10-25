using Game.Core.Universe;

namespace Game.Api.Seasons;

/// <summary>
/// A time-boxed universe. All game state (players, planets, fleets, ...) belongs to exactly one season.
/// </summary>
public sealed class Season
{
    public const int MaxNameLength = 100;

    public SeasonId Id { get; private set; }
    public string Name { get; private set; }
    public DateTimeOffset StartsAt { get; private set; }
    public DateTimeOffset EndsAt { get; private set; }
    public int Galaxies { get; private set; }
    public int SystemsPerGalaxy { get; private set; }
    public int EconomySpeed { get; private set; }

    public UniverseSettings Universe => new(Galaxies, SystemsPerGalaxy, EconomySpeed);

    // EF Core constructor
    private Season()
    {
        Id = default!;
        Name = default!;
    }

    private Season(SeasonId id, string name, DateTimeOffset startsAt, DateTimeOffset endsAt, UniverseSettings universe)
    {
        Id = id;
        Name = name;
        StartsAt = startsAt;
        EndsAt = endsAt;
        Galaxies = universe.Galaxies;
        SystemsPerGalaxy = universe.SystemsPerGalaxy;
        EconomySpeed = universe.EconomySpeed;
    }

    public static Season Create(string name, DateTimeOffset startsAt, DateTimeOffset endsAt, UniverseSettings? universe = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Length, MaxNameLength, nameof(name));

        if (endsAt <= startsAt)
        {
            throw new ArgumentException("A season must end after it starts.", nameof(endsAt));
        }

        return new(SeasonId.New(), name, startsAt.ToUniversalTime(), endsAt.ToUniversalTime(), universe ?? UniverseSettings.Default);
    }

    public bool IsOpenAt(DateTimeOffset moment)
    {
        return moment >= StartsAt && moment < EndsAt;
    }
}