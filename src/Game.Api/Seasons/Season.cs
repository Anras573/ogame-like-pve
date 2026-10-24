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

    // EF Core constructor
    private Season()
    {
        Id = default!;
        Name = default!;
    }

    private Season(SeasonId id, string name, DateTimeOffset startsAt, DateTimeOffset endsAt)
    {
        Id = id;
        Name = name;
        StartsAt = startsAt;
        EndsAt = endsAt;
    }

    public static Season Create(string name, DateTimeOffset startsAt, DateTimeOffset endsAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Length, MaxNameLength, nameof(name));

        if (endsAt <= startsAt)
        {
            throw new ArgumentException("A season must end after it starts.", nameof(endsAt));
        }

        return new(SeasonId.New(), name, startsAt.ToUniversalTime(), endsAt.ToUniversalTime());
    }

    public bool IsOpenAt(DateTimeOffset moment)
    {
        return moment >= StartsAt && moment < EndsAt;
    }
}