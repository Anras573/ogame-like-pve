import { describe, expect, it } from 'vitest'
import { homePlanet } from '../test/fixtures'
import { maxAffordable, projectShipyard } from './shipyard'

const start = Date.parse('2026-11-01T12:00:00Z')

describe('projectShipyard', () => {
  const planet = homePlanet({
    shipyardQueue: [
      { unit: 'RocketLauncher', quantity: 2, unitBuildSeconds: 30 },
      { unit: 'LightFighter', quantity: 3, unitBuildSeconds: 60 },
    ],
    shipyardProgressAt: new Date(start).toISOString(),
  })

  it('delivers finished units and keeps the rest queued', () => {
    const projection = projectShipyard(planet, start + (60 + 90) * 1000)

    expect(projection.delivered).toEqual({ RocketLauncher: 2, LightFighter: 1 })
    expect(projection.queue).toEqual([{ unit: 'LightFighter', quantity: 2, unitBuildSeconds: 60 }])
    expect(projection.currentStartedAt).toBe(start + 120 * 1000)
  })

  it('empties the queue once everything is done', () => {
    const projection = projectShipyard(planet, start + 3_600_000)

    expect(projection.delivered).toEqual({ RocketLauncher: 2, LightFighter: 3 })
    expect(projection.queue).toEqual([])
    expect(projection.currentStartedAt).toBeNull()
  })

  it('does nothing for an idle shipyard', () => {
    expect(projectShipyard(homePlanet(), start)).toEqual({ delivered: {}, queue: [], currentStartedAt: null })
  })
})

describe('maxAffordable', () => {
  it('is limited by the scarcest resource', () => {
    expect(maxAffordable({ metal: 10_000, crystal: 2_500, deuterium: 0 }, { metal: 3000, crystal: 1000, deuterium: 0 })).toBe(2)
  })

  it('is zero when a needed resource is missing', () => {
    expect(maxAffordable({ metal: 10_000, crystal: 0, deuterium: 0 }, { metal: 1000, crystal: 1, deuterium: 0 })).toBe(0)
  })
})
