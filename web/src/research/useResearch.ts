import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '../api/client'
import { PlanetActionError, planetQueryKey } from '../planets/usePlanets'
import type { ResearchOverview, ResearchType } from '../planets/types'

export const researchQueryKey = (planetId: string) => ['research', planetId] as const

/** Matches every planet's research view, for refreshing after research completes. */
export const allResearchQueryKey = ['research'] as const

export function useResearch(planetId: string) {
  return useQuery({
    queryKey: researchQueryKey(planetId),
    queryFn: async (): Promise<ResearchOverview> => {
      const { data, response } = await api.GET('/api/planets/{planetId}/research', { params: { path: { planetId } } })
      if (!data) throw new Error(`Failed to load research (${response.status})`)
      return data
    },
  })
}

type ProblemDetails = { detail?: string | null; title?: string | null }

function actionError(error: unknown, status: number): PlanetActionError {
  const problem = (error ?? {}) as ProblemDetails
  return new PlanetActionError(problem.detail ?? problem.title ?? `Something went wrong (${status}).`)
}

/** Research is paid from the planet, so both views change. */
function useResearchMutation<TVariables>(planetId: string, request: (variables: TVariables) => Promise<ResearchOverview>) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: request,
    onSuccess: (overview) => {
      queryClient.setQueryData(researchQueryKey(planetId), overview)
      void queryClient.invalidateQueries({ queryKey: planetQueryKey(planetId) })
    },
    onError: () => queryClient.invalidateQueries({ queryKey: researchQueryKey(planetId) }),
  })
}

export function useStartResearch(planetId: string) {
  return useResearchMutation(planetId, async (research: ResearchType) => {
    const { data, error, response } = await api.POST('/api/planets/{planetId}/research', {
      params: { path: { planetId } },
      body: { research },
    })
    if (!data) throw actionError(error, response.status)
    return data
  })
}

export function useCancelResearch(planetId: string) {
  return useResearchMutation(planetId, async () => {
    const { data, error, response } = await api.DELETE('/api/planets/{planetId}/research', {
      params: { path: { planetId } },
    })
    if (!data) throw actionError(error, response.status)
    return data
  })
}
