import type { BuildingType, MissingRequirements, ResearchType, UnitType } from '../planets/types'

interface Info {
  name: string
  description: string
}

export const buildingInfo: Record<BuildingType, Info> = {
  MetalMine: { name: 'Metal Mine', description: 'Extracts metal, the backbone of every structure and hull.' },
  CrystalMine: { name: 'Crystal Mine', description: 'Grows crystal for electronics and alloys.' },
  DeuteriumSynthesizer: {
    name: 'Deuterium Synthesizer',
    description: "Filters deuterium from the planet's water. Works better on cold planets.",
  },
  SolarPlant: { name: 'Solar Plant', description: 'Turns starlight into the energy your mines run on.' },
  RoboticsFactory: { name: 'Robotics Factory', description: 'Construction drones that make every building go up faster.' },
  Shipyard: { name: 'Shipyard', description: 'Builds ships and defenses. Higher levels build them faster.' },
  ResearchLab: { name: 'Research Lab', description: 'Where your scientists work. Higher levels research faster.' },
  MetalStorage: { name: 'Metal Storage', description: 'Raises how much metal the planet can hold.' },
  CrystalStorage: { name: 'Crystal Storage', description: 'Raises how much crystal the planet can hold.' },
  DeuteriumTank: { name: 'Deuterium Tank', description: 'Raises how much deuterium the planet can hold.' },
}

export const buildingGroups: { title: string; buildings: BuildingType[] }[] = [
  { title: 'Resources', buildings: ['MetalMine', 'CrystalMine', 'DeuteriumSynthesizer', 'SolarPlant'] },
  { title: 'Facilities', buildings: ['RoboticsFactory', 'Shipyard', 'ResearchLab'] },
  { title: 'Storage', buildings: ['MetalStorage', 'CrystalStorage', 'DeuteriumTank'] },
]

export const researchInfo: Record<ResearchType, Info> = {
  EnergyTechnology: { name: 'Energy Technology', description: 'The basis of almost every other technology.' },
  LaserTechnology: { name: 'Laser Technology', description: 'Focused light for weapons and beyond.' },
  IonTechnology: { name: 'Ion Technology', description: 'Charged particle beams for heavier weapons.' },
  HyperspaceTechnology: { name: 'Hyperspace Technology', description: 'Folds space; needed for the fastest drives.' },
  PlasmaTechnology: { name: 'Plasma Technology', description: 'Superheated matter for the most destructive weapons.' },
  CombustionDrive: { name: 'Combustion Drive', description: 'The first ship drive. Powers cargo ships and fighters.' },
  ImpulseDrive: { name: 'Impulse Drive', description: 'A faster drive for heavier ships.' },
  HyperspaceDrive: { name: 'Hyperspace Drive', description: 'Bends space for capital ships.' },
  EspionageTechnology: { name: 'Espionage Technology', description: 'Better probes and better reports.' },
  ComputerTechnology: { name: 'Computer Technology', description: 'Coordinates more fleets at once.' },
  Astrophysics: { name: 'Astrophysics', description: 'Opens up colonies and deep-space expeditions.' },
  WeaponsTechnology: { name: 'Weapons Technology', description: 'Makes every weapon hit harder.' },
  ShieldingTechnology: { name: 'Shielding Technology', description: 'Stronger shields for ships and defenses.' },
  ArmourTechnology: { name: 'Armour Technology', description: 'Tougher hulls for ships and defenses.' },
}

export const unitInfo: Record<UnitType, Info> = {
  SmallCargo: { name: 'Small Cargo', description: 'A nimble freighter for moving resources.' },
  LargeCargo: { name: 'Large Cargo', description: 'A bulk freighter with five times the hold.' },
  LightFighter: { name: 'Light Fighter', description: 'Cheap and fast; the backbone of early fleets.' },
  HeavyFighter: { name: 'Heavy Fighter', description: 'Better armed and armoured than its light cousin.' },
  Cruiser: { name: 'Cruiser', description: 'Fast, well-armed hunter of fighters.' },
  Battleship: { name: 'Battleship', description: 'The heavy hitter of any fleet.' },
  Bomber: { name: 'Bomber', description: 'Built to break planetary defenses.' },
  Destroyer: { name: 'Destroyer', description: 'A massive warship that shreds smaller vessels.' },
  Recycler: { name: 'Recycler', description: 'Collects resources from debris fields after battles.' },
  EspionageProbe: { name: 'Espionage Probe', description: 'A tiny, very fast drone that reports what it sees.' },
  ColonyShip: { name: 'Colony Ship', description: 'Carries settlers to found a new colony.' },
  RocketLauncher: { name: 'Rocket Launcher', description: 'Simple and cheap. Good in numbers.' },
  LightLaser: { name: 'Light Laser', description: 'Concentrated light; a solid early defense.' },
  HeavyLaser: { name: 'Heavy Laser', description: 'A stronger laser for tougher attackers.' },
  GaussCannon: { name: 'Gauss Cannon', description: 'Hurls heavy projectiles with magnetic force.' },
  IonCannon: { name: 'Ion Cannon', description: 'Weak on damage but with a very strong shield.' },
  PlasmaTurret: { name: 'Plasma Turret', description: 'The strongest defense there is.' },
  SmallShieldDome: { name: 'Small Shield Dome', description: 'Covers the whole planet in a protective shield. One per planet.' },
  LargeShieldDome: { name: 'Large Shield Dome', description: 'A far stronger planetary shield. One per planet.' },
}

/** e.g. "Research Lab 1, Energy Technology 2" — or an empty string when nothing is missing. */
export function describeMissing(missing: MissingRequirements): string {
  return [
    ...missing.buildings.map((b) => `${buildingInfo[b.building].name} ${b.level}`),
    ...missing.research.map((r) => `${researchInfo[r.research].name} ${r.level}`),
  ].join(', ')
}

export function hasMissing(missing: MissingRequirements): boolean {
  return missing.buildings.length > 0 || missing.research.length > 0
}
