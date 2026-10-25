import type {
  Building,
  BuildingType,
  MissingRequirements,
  Planet,
  ResearchOverview,
  ResearchType,
  Technology,
  Unit,
  UnitType,
} from '../planets/types'

const none: MissingRequirements = { buildings: [], research: [] }

const buildingCosts: Record<BuildingType, Building['nextLevelCost']> = {
  MetalMine: { metal: 60, crystal: 15, deuterium: 0 },
  CrystalMine: { metal: 48, crystal: 24, deuterium: 0 },
  DeuteriumSynthesizer: { metal: 225, crystal: 75, deuterium: 0 },
  SolarPlant: { metal: 75, crystal: 30, deuterium: 0 },
  RoboticsFactory: { metal: 400, crystal: 120, deuterium: 200 },
  Shipyard: { metal: 400, crystal: 200, deuterium: 100 },
  ResearchLab: { metal: 200, crystal: 400, deuterium: 200 },
  MetalStorage: { metal: 1000, crystal: 0, deuterium: 0 },
  CrystalStorage: { metal: 1000, crystal: 500, deuterium: 0 },
  DeuteriumTank: { metal: 1000, crystal: 1000, deuterium: 0 },
}

export const planetId = '00000000-0000-0000-0000-0000000000cc'

export function unit(type: UnitType, overrides: Partial<Unit> = {}): Unit {
  const ship = !['RocketLauncher', 'LightLaser', 'HeavyLaser', 'GaussCannon', 'IonCannon', 'PlasmaTurret', 'SmallShieldDome', 'LargeShieldDome'].includes(type)
  return {
    type,
    category: ship ? 'Ship' : 'Defense',
    count: 0,
    cost: { metal: 2000, crystal: 0, deuterium: 0 },
    buildSeconds: 144,
    weapon: 80,
    shield: 20,
    structuralIntegrity: 2000,
    speed: ship ? 12500 : 0,
    cargo: ship ? 50 : 0,
    maxPerPlanet: null,
    missingRequirements: none,
    ...overrides,
  }
}

/** A fresh home planet as the API returns it, snapshotted "now". */
export function homePlanet(overrides: Partial<Planet> = {}): Planet {
  const now = new Date().toISOString()

  return {
    id: planetId,
    name: 'Homeworld',
    coordinates: { galaxy: 1, system: 42, position: 8 },
    maxTemperature: 40,
    resources: { metal: 500, crystal: 500, deuterium: 0 },
    resourcesAsOf: now,
    hourlyProduction: { metal: 30, crystal: 15, deuterium: 0 },
    storageCapacity: { metal: 10_000, crystal: 10_000, deuterium: 10_000 },
    energy: { produced: 0, consumed: 0, productionFactor: 1 },
    buildings: (Object.keys(buildingCosts) as BuildingType[]).map((type) => ({
      type,
      level: 0,
      nextLevelCost: buildingCosts[type],
      nextLevelBuildSeconds: 108,
      missingRequirements:
        type === 'Shipyard' ? { buildings: [{ building: 'RoboticsFactory', level: 2 }], research: [] } : none,
    })),
    construction: null,
    units: [
      unit('LightFighter', {
        cost: { metal: 3000, crystal: 1000, deuterium: 0 },
        missingRequirements: { buildings: [{ building: 'Shipyard', level: 1 }], research: [{ research: 'CombustionDrive', level: 1 }] },
      }),
      unit('RocketLauncher', { missingRequirements: { buildings: [{ building: 'Shipyard', level: 1 }], research: [] } }),
    ],
    shipyardQueue: [],
    shipyardProgressAt: null,
    serverTime: now,
    ...overrides,
  }
}

/** A planet with a shipyard where rocket launchers and light fighters can be built. */
export function shipyardPlanet(overrides: Partial<Planet> = {}): Planet {
  const base = homePlanet()
  return homePlanet({
    resources: { metal: 10_000, crystal: 5_000, deuterium: 0 },
    buildings: base.buildings.map((b) => (b.type === 'Shipyard' ? { ...b, level: 1, missingRequirements: none } : b)),
    units: [
      unit('LightFighter', { cost: { metal: 3000, crystal: 1000, deuterium: 0 } }),
      unit('RocketLauncher', { count: 4 }),
      unit('SmallShieldDome', { cost: { metal: 10000, crystal: 10000, deuterium: 0 }, maxPerPlanet: 1, missingRequirements: { buildings: [], research: [{ research: 'ShieldingTechnology', level: 2 }] } }),
    ],
    ...overrides,
  })
}

function technology(type: ResearchType, overrides: Partial<Technology> = {}): Technology {
  return {
    type,
    level: 0,
    nextLevelCost: { metal: 0, crystal: 400, deuterium: 600 },
    nextLevelResearchSeconds: 144,
    missingRequirements: none,
    ...overrides,
  }
}

export function researchOverview(overrides: Partial<ResearchOverview> = {}): ResearchOverview {
  return {
    planetId,
    researchLabLevel: 1,
    technologies: [
      technology('ComputerTechnology'),
      technology('CombustionDrive', {
        nextLevelCost: { metal: 400, crystal: 0, deuterium: 600 },
        missingRequirements: { buildings: [], research: [{ research: 'EnergyTechnology', level: 1 }] },
      }),
    ],
    currentResearch: null,
    serverTime: new Date().toISOString(),
    ...overrides,
  }
}
