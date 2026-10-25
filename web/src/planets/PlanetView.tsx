import { useState } from 'react'
import { ResearchTab } from '../research/ResearchTab'
import { UnitsTab } from '../shipyard/UnitsTab'
import { BuildingsTab } from './BuildingsTab'
import { formatAmount, projectResources } from './economy'
import type { Planet, Resources } from './types'
import { usePlanet } from './usePlanets'
import { useServerNow } from './useServerNow'

const tabs = ['Buildings', 'Research', 'Shipyard', 'Defense'] as const
type Tab = (typeof tabs)[number]

export function PlanetView({ planetId }: { planetId: string }) {
  const { data: planet, isPending, isError, dataUpdatedAt } = usePlanet(planetId)

  if (isPending) return <p className="muted">Loading planet…</p>
  if (isError) return <p role="alert">Could not load your planet.</p>

  return <LoadedPlanet planet={planet} receivedAt={dataUpdatedAt} />
}

function LoadedPlanet({ planet, receivedAt }: { planet: Planet; receivedAt: number }) {
  const now = useServerNow(planet.serverTime, receivedAt)
  const resources = projectResources(planet, now)
  const [tab, setTab] = useState<Tab>('Buildings')

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

      <nav className="tabs" role="tablist" aria-label="Planet">
        {tabs.map((t) => (
          <button
            key={t}
            type="button"
            role="tab"
            aria-selected={tab === t}
            className={tab === t ? 'tab active' : 'tab'}
            onClick={() => setTab(t)}
          >
            {t}
          </button>
        ))}
      </nav>

      <div role="tabpanel" aria-label={tab}>
        {tab === 'Buildings' && <BuildingsTab planet={planet} resources={resources} now={now} />}
        {tab === 'Research' && <ResearchTab planetId={planet.id} resources={resources} now={now} />}
        {tab === 'Shipyard' && <UnitsTab planet={planet} category="Ship" resources={resources} now={now} />}
        {tab === 'Defense' && <UnitsTab planet={planet} category="Defense" resources={resources} now={now} />}
      </div>
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
