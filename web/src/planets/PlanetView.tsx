import { useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { buildingGroups, buildingInfo } from './buildings'
import { canAfford, formatAmount, formatDuration, projectResources } from './economy'
import type { Building, Planet, Resources } from './types'
import { planetQueryKey, useCancelConstruction, usePlanet, useStartConstruction } from './usePlanets'
import { useServerNow } from './useServerNow'

export function PlanetView({ planetId }: { planetId: string }) {
  const { data: planet, isPending, isError, dataUpdatedAt } = usePlanet(planetId)

  if (isPending) return <p className="muted">Loading planet…</p>
  if (isError) return <p role="alert">Could not load your planet.</p>

  return <LoadedPlanet planet={planet} receivedAt={dataUpdatedAt} />
}

function LoadedPlanet({ planet, receivedAt }: { planet: Planet; receivedAt: number }) {
  const now = useServerNow(planet.serverTime, receivedAt)
  const resources = projectResources(planet, now)
  const start = useStartConstruction(planet.id)
  const cancel = useCancelConstruction(planet.id)
  const error = start.error ?? cancel.error

  return (
    <div className="planet">
      <header className="planet-header">
        <h1>{planet.name}</h1>
        <span className="muted">
          [{planet.coordinates.galaxy}:{planet.coordinates.system}:{planet.coordinates.position}] · up to{' '}
          {planet.maxTemperature}°C
        </span>
      </header>

      <ResourceBar planet={planet} resources={resources} />

      {planet.construction && (
        <ConstructionPanel
          planet={planet}
          now={now}
          onCancel={() => cancel.mutate()}
          cancelling={cancel.isPending}
        />
      )}

      {error && <p role="alert">{error.message}</p>}

      {buildingGroups.map((group) => (
        <section key={group.title}>
          <h2>{group.title}</h2>
          <div className="building-grid">
            {group.buildings.map((type) => {
              const building = planet.buildings.find((b) => b.type === type)
              return building ? (
                <BuildingCard
                  key={type}
                  building={building}
                  available={resources}
                  busy={planet.construction !== null || start.isPending}
                  onUpgrade={() => start.mutate(type)}
                />
              ) : null
            })}
          </div>
        </section>
      ))}
    </div>
  )
}

function ResourceBar({ planet, resources }: { planet: Planet; resources: Resources }) {
  const shortage = planet.energy.consumed > planet.energy.produced

  return (
    <dl className="resource-bar">
      <ResourceTile
        label="Metal"
        amount={resources.metal}
        capacity={planet.storageCapacity.metal}
        hourly={planet.hourlyProduction.metal}
      />
      <ResourceTile
        label="Crystal"
        amount={resources.crystal}
        capacity={planet.storageCapacity.crystal}
        hourly={planet.hourlyProduction.crystal}
      />
      <ResourceTile
        label="Deuterium"
        amount={resources.deuterium}
        capacity={planet.storageCapacity.deuterium}
        hourly={planet.hourlyProduction.deuterium}
      />
      <div className={shortage ? 'resource shortage' : 'resource'} data-testid="resource-energy">
        <dt>Energy</dt>
        <dd>{formatAmount(planet.energy.produced - planet.energy.consumed)}</dd>
        <dd className="hint">
          {shortage
            ? `Mines at ${Math.floor(planet.energy.productionFactor * 100)}%`
            : `${formatAmount(planet.energy.produced)} made · ${formatAmount(planet.energy.consumed)} used`}
        </dd>
      </div>
    </dl>
  )
}

function ResourceTile(props: { label: string; amount: number; capacity: number; hourly: number }) {
  const full = props.amount >= props.capacity

  return (
    <div className={full ? 'resource full' : 'resource'} data-testid={`resource-${props.label.toLowerCase()}`}>
      <dt>{props.label}</dt>
      <dd>{formatAmount(props.amount)}</dd>
      <dd className="hint">
        {full ? 'Storage full' : `+${formatAmount(props.hourly)}/h`} · cap {formatAmount(props.capacity)}
      </dd>
    </div>
  )
}

function ConstructionPanel(props: { planet: Planet; now: number; onCancel: () => void; cancelling: boolean }) {
  const construction = props.planet.construction!
  const completesAt = Date.parse(construction.completesAt)
  const totalSeconds = (completesAt - Date.parse(construction.startedAt)) / 1000

  // Request latency makes our clock estimate run slightly behind, so never show more than the whole build time
  const remainingSeconds = Math.min(totalSeconds, (completesAt - props.now) / 1000)
  const queryClient = useQueryClient()
  const planetId = props.planet.id

  // Refresh once it should be done, in case the live notification doesn't arrive
  const done = remainingSeconds <= 0
  useEffect(() => {
    if (done) void queryClient.invalidateQueries({ queryKey: planetQueryKey(planetId) })
  }, [done, planetId, queryClient])

  return (
    <section className="card construction" aria-label="Construction">
      <div>
        <strong>
          {buildingInfo[construction.building].name} → level {construction.targetLevel}
        </strong>
        <p className="muted">{done ? 'Finishing…' : `${formatDuration(remainingSeconds)} left`}</p>
      </div>
      <button type="button" className="secondary" onClick={props.onCancel} disabled={props.cancelling || done}>
        Cancel
      </button>
    </section>
  )
}

function BuildingCard(props: { building: Building; available: Resources; busy: boolean; onUpgrade: () => void }) {
  const { building } = props
  const info = buildingInfo[building.type]
  const affordable = canAfford(props.available, building.nextLevelCost)
  const cost = building.nextLevelCost

  return (
    <article className="card building" aria-label={info.name}>
      <h3>
        {info.name} <span className="level">Level {building.level}</span>
      </h3>
      <p className="muted">{info.description}</p>
      <ul className="cost">
        {cost.metal > 0 && <CostItem label="Metal" amount={cost.metal} available={props.available.metal} />}
        {cost.crystal > 0 && <CostItem label="Crystal" amount={cost.crystal} available={props.available.crystal} />}
        {cost.deuterium > 0 && (
          <CostItem label="Deuterium" amount={cost.deuterium} available={props.available.deuterium} />
        )}
        <li className="muted">{formatDuration(building.nextLevelBuildSeconds)}</li>
      </ul>
      <button type="button" onClick={props.onUpgrade} disabled={props.busy || !affordable}>
        Upgrade to {building.level + 1}
      </button>
    </article>
  )
}

function CostItem(props: { label: string; amount: number; available: number }) {
  return (
    <li className={props.available < props.amount ? 'missing' : undefined}>
      {props.label} {formatAmount(props.amount)}
    </li>
  )
}
