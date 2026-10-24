using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Game.Api.Seasons;

public sealed record SeasonId(Guid Value)
{
    public static SeasonId New() => new(Guid.NewGuid());

    public static implicit operator string(SeasonId id) => id.Value.ToString();
    public static implicit operator SeasonId(string id) => new(Guid.Parse(id));
}

/// <summary>
/// EF Core value converter for SeasonId to enable database persistence.
/// </summary>
public sealed class SeasonIdConverter() : ValueConverter<SeasonId, Guid>(
    seasonId => seasonId.Value,
    value => new SeasonId(value));