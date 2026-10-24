import type { Building, BuildingType, Planet } from '../planets/types'

const buildingCosts: Record<BuildingType, Building['nextLevelCost']> = {
  MetalMine: { metal: 60, crystal: 15, deuterium: 0 },
  CrystalMine: { metal: 48, crystal: 24, deuterium: 0 },
  DeuteriumSynthesizer: { metal: 225, crystal: 75, deuterium: 0 },
  SolarPlant: { metal: 75, crystal: 30, deuterium: 0 },
  RoboticsFactory: { metal: 400, crystal: 120, deuterium: 200 },
  MetalStorage: { metal: 1000, crystal: 0, deuterium: 0 },
  CrystalStorage: { metal: 1000, crystal: 500, deuterium: 0 },
  DeuteriumTank: { metal: 1000, crystal: 1000, deuterium: 0 },
}

export const planetId = '00000000-0000-0000-0000-0000000000cc'

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
    })),
    construction: null,
    serverTime: now,
    ...overrides,
  }
}
