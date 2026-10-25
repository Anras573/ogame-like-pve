import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../api/client'
import { PlanetActionError, planetQueryKey } from '../planets/usePlanets'
import type { Planet, UnitType } from '../planets/types'

export function useEnqueueUnits(planetId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ unit, quantity }: { unit: UnitType; quantity: number }): Promise<Planet> => {
      const { data, error, response } = await api.POST('/api/planets/{planetId}/shipyard', {
        params: { path: { planetId } },
        body: { unit, quantity },
      })
      if (!data) {
        const problem = (error ?? {}) as { detail?: string | null; title?: string | null }
        throw new PlanetActionError(problem.detail ?? problem.title ?? `Something went wrong (${response.status}).`)
      }
      return data
    },
    onSuccess: (planet) => queryClient.setQueryData(planetQueryKey(planetId), planet),
    onError: () => queryClient.invalidateQueries({ queryKey: planetQueryKey(planetId) }),
  })
}
