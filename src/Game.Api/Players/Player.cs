using Game.Api.Planets;
using Game.Api.Seasons;
using Game.Core.Buildings;
using Game.Core.Research;
using Game.Core.TechTree;

namespace Game.Api.Players;

/// <summary>
/// A user's participation in one season. Season-scoped game state (planets, research, fleets) hangs off the player.
/// Research belongs to the player, not a planet: it unlocks things on every planet. Like planets, research
/// settles lazily: every read or change first completes research that is due.
/// </summary>
public sealed class Player
{
    private Dictionary<ResearchType, int> _researchLevels = [];

    public PlayerId Id { get; private set; }
    public Guid UserId { get; private set; }
    public SeasonId SeasonId { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }
    public ResearchProject? CurrentResearch { get; private set; }

    /// <summary>
    /// Optimistic concurrency token, so a player action and the background worker can't both settle the same state.
    /// </summary>
    public uint Version { get; private set; }

    public IReadOnlyDictionary<ResearchType, int> ResearchLevels => _researchLevels;

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

    /// <summary>
    /// Completes the current research if it's due at <paramref name="now"/>.
    /// </summary>
    /// <returns>The research that completed, or <c>null</c>.</returns>
    public ResearchProject? SettleResearch(DateTimeOffset now)
    {
        if (CurrentResearch is not { } research || research.CompletesAt > now)
        {
            return null;
        }

        _researchLevels = new(_researchLevels) { [research.Research] = research.TargetLevel };
        CurrentResearch = null;
        return research;
    }

    /// <summary>
    /// The prerequisites of the next level of <paramref name="research"/> that <paramref name="planet"/> and this
    /// player don't meet yet.
    /// </summary>
    public IReadOnlyList<Requirement> MissingRequirements(ResearchType research, Planet planet)
    {
        ArgumentNullException.ThrowIfNull(planet);

        return RequirementCheck.Missing(ResearchCatalog.Get(research).Requirements, planet.BuildingLevels, _researchLevels);
    }

    public ResourceAmountsAndTime NextLevel(ResearchType research, Planet planet, int economySpeed)
    {
        ArgumentNullException.ThrowIfNull(planet);

        var cost = ResearchRules.CostOf(research, _researchLevels.LevelOf(research) + 1);
        var time = ResearchRules.ResearchTime(cost, planet.BuildingLevels.LevelOf(BuildingType.ResearchLab), economySpeed);
        return new ResourceAmountsAndTime(cost, time);
    }

    /// <summary>
    /// Starts researching the next level of <paramref name="research"/> in <paramref name="planet"/>'s lab, paying
    /// from that planet. Only one research runs at a time per player.
    /// </summary>
    public StartResearchResult StartResearch(ResearchType research, Planet planet, DateTimeOffset now, int economySpeed)
    {
        ArgumentNullException.ThrowIfNull(planet);
        EnsureOwns(planet);

        SettleResearch(now);
        planet.SettleTo(now, economySpeed);

        if (CurrentResearch is not null)
        {
            return StartResearchResult.AlreadyResearching;
        }

        if (MissingRequirements(research, planet).Count > 0)
        {
            return StartResearchResult.RequirementsNotMet;
        }

        var next = NextLevel(research, planet, economySpeed);
        if (!planet.TrySpend(next.Cost, now, economySpeed))
        {
            return StartResearchResult.NotEnoughResources;
        }

        CurrentResearch = new ResearchProject(
            research,
            _researchLevels.LevelOf(research) + 1,
            planet.Id,
            next.Cost,
            now,
            now + next.Time);

        return StartResearchResult.Started;
    }

    /// <summary>
    /// Cancels the research in progress and refunds its full cost to <paramref name="planet"/>, which must be the
    /// planet it was started on.
    /// </summary>
    /// <returns>The cancelled research, or <c>null</c> when nothing was being researched.</returns>
    public ResearchProject? CancelResearch(Planet planet, DateTimeOffset now, int economySpeed)
    {
        ArgumentNullException.ThrowIfNull(planet);
        EnsureOwns(planet);

        SettleResearch(now);
        if (CurrentResearch is not { } research)
        {
            return null;
        }

        if (research.PlanetId != planet.Id)
        {
            throw new ArgumentException("Research is refunded to the planet it was started on.", nameof(planet));
        }

        planet.Refund(research.Cost, now, economySpeed);
        CurrentResearch = null;
        return research;
    }

    private void EnsureOwns(Planet planet)
    {
        if (planet.PlayerId != Id)
        {
            throw new ArgumentException("The planet belongs to another player.", nameof(planet));
        }
    }
}

public enum StartResearchResult
{
    Started,
    AlreadyResearching,
    RequirementsNotMet,
    NotEnoughResources,
}