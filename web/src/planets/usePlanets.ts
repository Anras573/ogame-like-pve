import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '../api/client'
import type { BuildingType, Planet, PlanetSummary } from './types'

export const planetsQueryKey = ['planets'] as const
export const planetQueryKey = (planetId: string) => ['planet', planetId] as const

export function usePlanets(enabled: boolean) {
  return useQuery({
    queryKey: planetsQueryKey,
    enabled,
    queryFn: async (): Promise<PlanetSummary[]> => {
      const { data, response } = await api.GET('/api/planets')
      if (!data) throw new Error(`Failed to load planets (${response.status})`)
      return data
    },
  })
}

export function usePlanet(planetId: string) {
  return useQuery({
    queryKey: planetQueryKey(planetId),
    queryFn: async (): Promise<Planet> => {
      const { data, response } = await api.GET('/api/planets/{planetId}', { params: { path: { planetId } } })
      if (!data) throw new Error(`Failed to load planet (${response.status})`)
      return data
    },
  })
}

/** An action the server refused, with its explanation (e.g. "This planet can't afford that upgrade yet."). */
export class PlanetActionError extends Error {}

type ProblemDetails = { detail?: string | null; title?: string | null }

function actionError(error: unknown, status: number): PlanetActionError {
  const problem = (error ?? {}) as ProblemDetails
  return new PlanetActionError(problem.detail ?? problem.title ?? `Something went wrong (${status}).`)
}

export function useStartConstruction(planetId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (building: BuildingType): Promise<Planet> => {
      const { data, error, response } = await api.POST('/api/planets/{planetId}/construction', {
        params: { path: { planetId } },
        body: { building },
      })
      if (!data) throw actionError(error, response.status)
      return data
    },
    onSuccess: (planet) => queryClient.setQueryData(planetQueryKey(planetId), planet),
    // A refusal usually means our view was stale, so refresh it
    onError: () => queryClient.invalidateQueries({ queryKey: planetQueryKey(planetId) }),
  })
}

export function useCancelConstruction(planetId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (): Promise<Planet> => {
      const { data, error, response } = await api.DELETE('/api/planets/{planetId}/construction', {
        params: { path: { planetId } },
      })
      if (!data) throw actionError(error, response.status)
      return data
    },
    onSuccess: (planet) => queryClient.setQueryData(planetQueryKey(planetId), planet),
    onError: () => queryClient.invalidateQueries({ queryKey: planetQueryKey(planetId) }),
  })
}
