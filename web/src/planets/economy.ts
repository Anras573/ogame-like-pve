import type { Planet, Resources } from './types'

const msPerHour = 3_600_000

/**
 * The planet's resources at `nowMs` (server time), extrapolated from the last snapshot the same way the server
 * accrues them: production stops at storage capacity, amounts already above it are kept.
 */
export function projectResources(planet: Planet, nowMs: number): Resources {
  const elapsedHours = Math.max(0, nowMs - Date.parse(planet.resourcesAsOf)) / msPerHour

  const fill = (current: number, hourly: number, capacity: number) =>
    current >= capacity ? current : Math.min(capacity, current + hourly * elapsedHours)

  return {
    metal: fill(planet.resources.metal, planet.hourlyProduction.metal, planet.storageCapacity.metal),
    crystal: fill(planet.resources.crystal, planet.hourlyProduction.crystal, planet.storageCapacity.crystal),
    deuterium: fill(planet.resources.deuterium, planet.hourlyProduction.deuterium, planet.storageCapacity.deuterium),
  }
}

export function canAfford(available: Resources, cost: Resources): boolean {
  return available.metal >= cost.metal && available.crystal >= cost.crystal && available.deuterium >= cost.deuterium
}

const numberFormat = new Intl.NumberFormat()

/** Whole units, like the game shows them. */
export function formatAmount(value: number): string {
  return numberFormat.format(Math.floor(value))
}

/** e.g. "2h 05m 09s", "4m 30s", "12s". */
export function formatDuration(totalSeconds: number): string {
  const seconds = Math.max(0, Math.ceil(totalSeconds))
  const h = Math.floor(seconds / 3600)
  const m = Math.floor((seconds % 3600) / 60)
  const s = seconds % 60
  const pad = (n: number) => n.toString().padStart(2, '0')

  if (h > 0) return `${h}h ${pad(m)}m ${pad(s)}s`
  if (m > 0) return `${m}m ${pad(s)}s`
  return `${s}s`
}
