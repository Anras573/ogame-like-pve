import { useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { buildingInfo, describeMissing, hasMissing, unitInfo } from '../catalog/names'
import { Cost } from '../planets/Cost'
import { formatAmount, formatDuration } from '../planets/economy'
import type { Planet, Resources, Unit } from '../planets/types'
import { planetQueryKey } from '../planets/usePlanets'
import { maxAffordable, projectShipyard } from './shipyard'
import { useEnqueueUnits } from './useEnqueueUnits'

const maxOrders = 10

interface UnitsTabProps {
  planet: Planet
  category: Unit['category']
  resources: Resources
  now: number
}

/** The shipyard, showing either ships or defenses; both share one queue. */
export function UnitsTab({ planet, category, resources, now }: UnitsTabProps) {
  const enqueue = useEnqueueUnits(planet.id)
  const projection = projectShipyard(planet, now)
  const queryClient = useQueryClient()
  const hasShipyard = planet.buildings.some((b) => b.type === 'Shipyard' && b.level > 0)

  // Refresh once the queue has run out, so counts come from the server again
  const queueDone = planet.shipyardQueue.length > 0 && projection.queue.length === 0
  useEffect(() => {
    if (queueDone) void queryClient.invalidateQueries({ queryKey: planetQueryKey(planet.id) })
  }, [queueDone, planet.id, queryClient])

  const queued = (unit: Unit['type']) => projection.queue.filter((o) => o.unit === unit).reduce((n, o) => n + o.quantity, 0)

  return (
    <>
      {!hasShipyard && <p className="card muted">Build a {buildingInfo.Shipyard.name} on this planet to build ships and defenses.</p>}

      {projection.queue.length > 0 && (
        <section className="card shipyard-queue" aria-label="Shipyard queue">
          <strong>Shipyard queue</strong>
          <ol>
            {projection.queue.map((order, index) => (
              <li key={index}>
                {formatAmount(order.quantity)} × {unitInfo[order.unit].name}
                {index === 0 && projection.currentStartedAt !== null && (
                  <span className="muted">
                    {' '}
                    · next in {formatDuration((projection.currentStartedAt + order.unitBuildSeconds * 1000 - now) / 1000)}
                  </span>
                )}
              </li>
            ))}
          </ol>
        </section>
      )}

      {enqueue.error && <p role="alert">{enqueue.error.message}</p>}

      <div className="card-grid">
        {planet.units
          .filter((unit) => unit.category === category)
          .map((unit) => (
            <UnitCard
              key={unit.type}
              unit={unit}
              count={unit.count + (projection.delivered[unit.type] ?? 0)}
              queued={queued(unit.type)}
              available={resources}
              queueFull={projection.queue.length >= maxOrders}
              busy={enqueue.isPending}
              onBuild={(quantity) => enqueue.mutate({ unit: unit.type, quantity })}
            />
          ))}
      </div>
    </>
  )
}

interface UnitCardProps {
  unit: Unit
  count: number
  queued: number
  available: Resources
  queueFull: boolean
  busy: boolean
  onBuild: (quantity: number) => void
}

function UnitCard({ unit, count, queued, available, queueFull, busy, onBuild }: UnitCardProps) {
  const info = unitInfo[unit.type]
  const locked = hasMissing(unit.missingRequirements)
  const [quantity, setQuantity] = useState(1)

  const affordable = maxAffordable(available, unit.cost)
  const limitLeft = unit.maxPerPlanet == null ? Infinity : unit.maxPerPlanet - count - queued
  const max = Math.max(0, Math.min(affordable, limitLeft))
  const valid = Number.isInteger(quantity) && quantity >= 1

  return (
    <article className="card item" aria-label={info.name}>
      <h3>
        {info.name} <span className="level">{formatAmount(count)} owned</span>
        {queued > 0 && <span className="muted"> · {formatAmount(queued)} queued</span>}
      </h3>
      <p className="muted">{info.description}</p>
      <p className="stats">
        Attack {formatAmount(unit.weapon)} · Shield {formatAmount(unit.shield)} · Hull {formatAmount(unit.structuralIntegrity)}
        {unit.category === 'Ship' && (
          <>
            {' '}
            · Speed {formatAmount(unit.speed)} · Cargo {formatAmount(unit.cargo)}
          </>
        )}
      </p>
      {locked ? (
        <p className="requires">Requires {describeMissing(unit.missingRequirements)}</p>
      ) : (
        <>
          <Cost cost={unit.cost} available={available} seconds={unit.buildSeconds} multiplier={valid ? quantity : 1} />
          <div className="order">
            <input
              type="number"
              min={1}
              aria-label={`${info.name} quantity`}
              value={Number.isNaN(quantity) ? '' : quantity}
              onChange={(e) => setQuantity(e.target.valueAsNumber)}
            />
            <button type="button" className="secondary" onClick={() => setQuantity(Math.max(1, max))} disabled={max < 1}>
              Max
            </button>
            <button
              type="button"
              onClick={() => onBuild(quantity)}
              disabled={busy || queueFull || !valid || quantity > max}
            >
              Build
            </button>
          </div>
        </>
      )}
    </article>
  )
}
