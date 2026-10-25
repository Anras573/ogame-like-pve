namespace Game.Core.Units;

/// <summary>
/// A batch in the shipyard queue. The build time per unit is fixed when the order is placed.
/// </summary>
public sealed record ShipyardOrder(UnitType Unit, int Quantity, int UnitBuildSeconds);