using Game.Api.Seasons;

namespace Game.Api.Players;

/// <summary>
/// A user's participation in one season. Season-scoped game state (planets, research, fleets) hangs off the player.
/// </summary>
public sealed class Player
{
    public PlayerId Id { get; private set; }
    public Guid UserId { get; private set; }
    public SeasonId SeasonId { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }

    // EF Core constructor
    private Player()
    {
        Id = default!;
        SeasonId = default!;
    }

    private Player(PlayerId id, Guid userId, SeasonId seasonId, DateTimeOffset joinedAt)
    {
        Id = id;
        UserId = userId;
        SeasonId = seasonId;
        JoinedAt = joinedAt;
    }

    /// <summary>
    /// Creates a player for <paramref name="userId"/> in <paramref name="season"/>,
    /// or returns <c>null</c> when the season isn't open at <paramref name="now"/>.
    /// </summary>
    public static Player? Join(Season season, Guid userId, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(season);

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("A player must belong to a user.", nameof(userId));
        }

        if (!season.IsOpenAt(now))
        {
            return null; // Season hasn't started yet or has already ended
        }

        return new(PlayerId.New(), userId, season.Id, now.ToUniversalTime());
    }
}