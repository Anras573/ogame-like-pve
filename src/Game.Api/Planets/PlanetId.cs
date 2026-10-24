using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Game.Api.Planets;

public sealed record PlanetId(Guid Value)
{
    public static PlanetId New() => new(Guid.NewGuid());

    public static implicit operator string(PlanetId id) => id.Value.ToString();
    public static implicit operator PlanetId(string id) => new(Guid.Parse(id));
}

/// <summary>
/// EF Core value converter for PlanetId to enable database persistence.
/// </summary>
public sealed class PlanetIdConverter() : ValueConverter<PlanetId, Guid>(
    planetId => planetId.Value,
    value => new PlanetId(value));