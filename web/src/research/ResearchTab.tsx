import { useQueryClient } from '@tanstack/react-query'
import { useCallback } from 'react'
import { describeMissing, hasMissing, researchInfo } from '../catalog/names'
import { Cost } from '../planets/Cost'
import { canAfford } from '../planets/economy'
import { ProgressPanel } from '../planets/ProgressPanel'
import type { Resources, Technology } from '../planets/types'
import { researchQueryKey, useCancelResearch, useResearch, useStartResearch } from './useResearch'

export function ResearchTab({ planetId, resources, now }: { planetId: string; resources: Resources; now: number }) {
  const { data: overview, isPending, isError } = useResearch(planetId)
  const start = useStartResearch(planetId)
  const cancel = useCancelResearch(planetId)
  const queryClient = useQueryClient()
  const refresh = useCallback(
    () => void queryClient.invalidateQueries({ queryKey: researchQueryKey(planetId) }),
    [queryClient, planetId],
  )

  if (isPending) return <p className="muted">Loading research…</p>
  if (isError) return <p role="alert">Could not load research.</p>

  const current = overview.currentResearch
  const error = start.error ?? cancel.error

  return (
    <>
      {overview.researchLabLevel === 0 && (
        <p className="card muted">Build a Research Lab on this planet to start researching.</p>
      )}

      {current && (
        <ProgressPanel
          label="Research"
          title={`${researchInfo[current.research].name} → level ${current.targetLevel}`}
          startedAt={current.startedAt}
          completesAt={current.completesAt}
          now={now}
          onDone={refresh}
          // Research can only be cancelled from the planet that paid for it
          onCancel={current.planetId === planetId ? () => cancel.mutate() : undefined}
          cancelling={cancel.isPending}
        />
      )}

      {error && <p role="alert">{error.message}</p>}

      <div className="card-grid">
        {overview.technologies.map((technology) => (
          <TechnologyCard
            key={technology.type}
            technology={technology}
            available={resources}
            busy={current !== null || start.isPending}
            onResearch={() => start.mutate(technology.type)}
          />
        ))}
      </div>
    </>
  )
}

function TechnologyCard(props: { technology: Technology; available: Resources; busy: boolean; onResearch: () => void }) {
  const { technology } = props
  const info = researchInfo[technology.type]
  const locked = hasMissing(technology.missingRequirements)
  const affordable = canAfford(props.available, technology.nextLevelCost)

  return (
    <article className="card item" aria-label={info.name}>
      <h3>
        {info.name} <span className="level">Level {technology.level}</span>
      </h3>
      <p className="muted">{info.description}</p>
      {locked ? (
        <p className="requires">Requires {describeMissing(technology.missingRequirements)}</p>
      ) : (
        <Cost cost={technology.nextLevelCost} available={props.available} seconds={technology.nextLevelResearchSeconds} />
      )}
      <button type="button" onClick={props.onResearch} disabled={props.busy || locked || !affordable}>
        Research level {technology.level + 1}
      </button>
    </article>
  )
}
