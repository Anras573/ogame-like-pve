import { useQueryClient } from '@tanstack/react-query'
import { useCallback } from 'react'
import { buildingGroups, buildingInfo, describeMissing, hasMissing } from '../catalog/names'
import { Cost } from './Cost'
import { canAfford } from './economy'
import { ProgressPanel } from './ProgressPanel'
import type { Building, Planet, Resources } from './types'
import { planetQueryKey, useCancelConstruction, useStartConstruction } from './usePlanets'

export function BuildingsTab({ planet, resources, now }: { planet: Planet; resources: Resources; now: number }) {
  const start = useStartConstruction(planet.id)
  const cancel = useCancelConstruction(planet.id)
  const error = start.error ?? cancel.error
  const queryClient = useQueryClient()
  const refresh = useCallback(
    () => void queryClient.invalidateQueries({ queryKey: planetQueryKey(planet.id) }),
    [queryClient, planet.id],
  )

  return (
    <>
      {planet.construction && (
        <ProgressPanel
          label="Construction"
          title={`${buildingInfo[planet.construction.building].name} → level ${planet.construction.targetLevel}`}
          startedAt={planet.construction.startedAt}
          completesAt={planet.construction.completesAt}
          now={now}
          onDone={refresh}
          onCancel={() => cancel.mutate()}
          cancelling={cancel.isPending}
        />
      )}

      {error && <p role="alert">{error.message}</p>}

      {buildingGroups.map((group) => (
        <section key={group.title}>
          <h2>{group.title}</h2>
          <div className="card-grid">
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
    </>
  )
}

function BuildingCard(props: { building: Building; available: Resources; busy: boolean; onUpgrade: () => void }) {
  const { building } = props
  const info = buildingInfo[building.type]
  const locked = hasMissing(building.missingRequirements)
  const affordable = canAfford(props.available, building.nextLevelCost)

  return (
    <article className="card item" aria-label={info.name}>
      <h3>
        {info.name} <span className="level">Level {building.level}</span>
      </h3>
      <p className="muted">{info.description}</p>
      {locked ? (
        <p className="requires">Requires {describeMissing(building.missingRequirements)}</p>
      ) : (
        <Cost cost={building.nextLevelCost} available={props.available} seconds={building.nextLevelBuildSeconds} />
      )}
      <button type="button" onClick={props.onUpgrade} disabled={props.busy || locked || !affordable}>
        Upgrade to {building.level + 1}
      </button>
    </article>
  )
}
