import type { Planet, ShipyardOrder, UnitType } from '../planets/types'

export interface ShipyardProjection {
  /** Units finished since the snapshot, on top of the counts the server reported. */
  delivered: Partial<Record<UnitType, number>>
  queue: ShipyardOrder[]
  /** When the unit now being built started (ms), or null when the shipyard is idle. */
  currentStartedAt: number | null
}

/**
 * Runs the shipyard queue forward to `nowMs` (server time) exactly like the server does: orders in turn, one unit at
 * a time, each delivered as soon as it's done.
 */
export function projectShipyard(planet: Planet, nowMs: number): ShipyardProjection {
  const delivered: Partial<Record<UnitType, number>> = {}
  const queue = planet.shipyardQueue.map((order) => ({ ...order }))
  if (!planet.shipyardProgressAt) return { delivered, queue, currentStartedAt: null }

  let cursor = Date.parse(planet.shipyardProgressAt)
  while (queue.length > 0) {
    const order = queue[0]
    const unitMs = order.unitBuildSeconds * 1000
    const finishable = nowMs <= cursor ? 0 : Math.min(order.quantity, Math.floor((nowMs - cursor) / unitMs))

    delivered[order.unit] = (delivered[order.unit] ?? 0) + finishable
    cursor += finishable * unitMs

    if (finishable < order.quantity) {
      order.quantity -= finishable
      break
    }
    queue.shift()
  }

  return { delivered, queue, currentStartedAt: queue.length > 0 ? cursor : null }
}

/** How many units the available resources pay for. */
export function maxAffordable(available: Planet['resources'], cost: Planet['resources']): number {
  const limits = [
    cost.metal > 0 ? available.metal / cost.metal : Infinity,
    cost.crystal > 0 ? available.crystal / cost.crystal : Infinity,
    cost.deuterium > 0 ? available.deuterium / cost.deuterium : Infinity,
  ]
  const max = Math.floor(Math.min(...limits))
  return Number.isFinite(max) ? max : 0
}
