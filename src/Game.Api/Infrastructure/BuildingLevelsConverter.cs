using System.Text.Json;
using Game.Core.Buildings;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Game.Api.Infrastructure;

/// <summary>
/// Stores a planet's building levels as one JSON object (e.g. {"MetalMine":3}), keeping the planet a single row.
/// </summary>
public sealed class BuildingLevelsConverter() : ValueConverter<Dictionary<BuildingType, int>, string>(
    levels => JsonSerializer.Serialize(levels, JsonSerializerOptions.Default),
    json => JsonSerializer.Deserialize<Dictionary<BuildingType, int>>(json, JsonSerializerOptions.Default) ?? new());

/// <summary>
/// Compares building levels by content so EF Core notices when a level changes.
/// </summary>
public sealed class BuildingLevelsComparer() : ValueComparer<Dictionary<BuildingType, int>>(
    (left, right) => left!.Count == right!.Count && !left.Except(right).Any(),
    levels => levels.Aggregate(0, (hash, entry) => hash ^ HashCode.Combine(entry.Key, entry.Value)),
    levels => new Dictionary<BuildingType, int>(levels));