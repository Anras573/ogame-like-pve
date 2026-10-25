import { describe, expect, it } from 'vitest'
import { canAfford, formatAmount, formatDuration, projectResources } from './economy'
import type { Planet } from './types'

const asOf = Date.parse('2026-11-01T12:00:00Z')

function planet(overrides: Partial<Planet> = {}): Planet {
  return {
    id: 'p1',
    name: 'Homeworld',
    coordinates: { galaxy: 1, system: 2, position: 8 },
    maxTemperature: 40,
    resources: { metal: 500, crystal: 500, deuterium: 0 },
    resourcesAsOf: new Date(asOf).toISOString(),
    hourlyProduction: { metal: 30, crystal: 15, deuterium: 0 },
    storageCapacity: { metal: 10_000, crystal: 10_000, deuterium: 10_000 },
    energy: { produced: 0, consumed: 0, productionFactor: 1 },
    buildings: [],
    construction: null,
    serverTime: new Date(asOf).toISOString(),
    ...overrides,
  }
}

describe('projectResources', () => {
  it('adds production for the elapsed time', () => {
    expect(projectResources(planet(), asOf + 2 * 3_600_000)).toEqual({ metal: 560, crystal: 530, deuterium: 0 })
  })

  it('stops at storage capacity', () => {
    const full = planet({ resources: { metal: 9_990, crystal: 0, deuterium: 0 } })
    expect(projectResources(full, asOf + 3_600_000).metal).toBe(10_000)
  })

  it('keeps amounts that are already above capacity', () => {
    const over = planet({ resources: { metal: 12_000, crystal: 0, deuterium: 0 } })
    expect(projectResources(over, asOf + 3_600_000).metal).toBe(12_000)
  })

  it('never goes back before the snapshot', () => {
    expect(projectResources(planet(), asOf - 60_000).metal).toBe(500)
  })
})

describe('canAfford', () => {
  it('needs every resource to cover the cost', () => {
    const available = { metal: 100, crystal: 50, deuterium: 0 }
    expect(canAfford(available, { metal: 100, crystal: 50, deuterium: 0 })).toBe(true)
    expect(canAfford(available, { metal: 100, crystal: 51, deuterium: 0 })).toBe(false)
  })
})

describe('formatting', () => {
  it('floors amounts', () => {
    expect(formatAmount(1234.9)).toBe(new Intl.NumberFormat().format(1234))
  })

  it('formats durations', () => {
    expect(formatDuration(12)).toBe('12s')
    expect(formatDuration(270)).toBe('4m 30s')
    expect(formatDuration(7509)).toBe('2h 05m 09s')
    expect(formatDuration(0.2)).toBe('1s')
    expect(formatDuration(-5)).toBe('0s')
  })
})
