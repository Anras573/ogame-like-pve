using Game.Core.Resources;

namespace Game.Api.Planets;

/// <summary>
/// What the next level of something costs and how long it takes.
/// </summary>
public sealed record ResourceAmountsAndTime(ResourceAmounts Cost, TimeSpan Time);