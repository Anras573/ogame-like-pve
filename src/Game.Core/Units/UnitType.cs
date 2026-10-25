namespace Game.Core.Units;

/// <summary>
/// Everything a shipyard can build: ships (which will fly) and defenses (which stay on the planet).
/// </summary>
public enum UnitType
{
    SmallCargo,
    LargeCargo,
    LightFighter,
    HeavyFighter,
    Cruiser,
    Battleship,
    Bomber,
    Destroyer,
    Recycler,
    EspionageProbe,
    ColonyShip,

    RocketLauncher,
    LightLaser,
    HeavyLaser,
    GaussCannon,
    IonCannon,
    PlasmaTurret,
    SmallShieldDome,
    LargeShieldDome,
}

public enum UnitCategory
{
    Ship,
    Defense,
}