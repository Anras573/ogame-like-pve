using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Game.Api.Players;

public sealed record PlayerId(Guid Value)
{
    public static PlayerId New() => new(Guid.NewGuid());

    public static implicit operator string(PlayerId id) => id.Value.ToString();
    public static implicit operator PlayerId(string id) => new(Guid.Parse(id));
}

/// <summary>
/// EF Core value converter for PlayerId to enable database persistence.
/// </summary>
public sealed class PlayerIdConverter() : ValueConverter<PlayerId, Guid>(
    playerId => playerId.Value,
    value => new PlayerId(value));