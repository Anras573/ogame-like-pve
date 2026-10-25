using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Game.Api.Infrastructure;

/// <summary>
/// Stores small pieces of aggregate state (building levels, unit counts, the shipyard queue) as one jsonb value,
/// so each aggregate stays a single row and its concurrency token covers all of it.
/// </summary>
public static class JsonColumn
{
    // Enums as names keep the stored JSON readable, e.g. {"MetalMine":3}
    internal static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.General)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    internal static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    internal static T Deserialize<T>(string json) where T : new() => JsonSerializer.Deserialize<T>(json, Options) ?? new T();
}

public sealed class JsonColumnConverter<T>() : ValueConverter<T, string>(
    value => JsonColumn.Serialize(value),
    json => JsonColumn.Deserialize<T>(json))
    where T : new();

/// <summary>
/// Compares by serialized content so EF Core notices changes inside the value.
/// </summary>
public sealed class JsonColumnComparer<T>() : ValueComparer<T>(
    (left, right) => JsonColumn.Serialize(left) == JsonColumn.Serialize(right),
    value => JsonColumn.Serialize(value).GetHashCode(),
    value => JsonColumn.Deserialize<T>(JsonColumn.Serialize(value)))
    where T : new();