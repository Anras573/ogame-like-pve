import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '../api/client'
import type { components } from '../api/schema'

export type CurrentSeason = components['schemas']['CurrentSeasonDto']

const currentSeasonQueryKey = ['currentSeason'] as const

/** The season open right now, or `null` when there is none. */
export function useCurrentSeason() {
  return useQuery({
    queryKey: currentSeasonQueryKey,
    queryFn: async (): Promise<CurrentSeason | null> => {
      const { data, response } = await api.GET('/api/seasons/current')
      if (response.status === 404) return null
      if (!data) throw new Error(`Failed to load season (${response.status})`)
      return data
    },
  })
}

export function useJoinCurrentSeason() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async () => {
      const { response } = await api.POST('/api/seasons/current/join')
      // 409 means we've already joined (e.g. from another tab), which is the state we wanted anyway
      if (!response.ok && response.status !== 409) {
        throw new Error(`Failed to join season (${response.status})`)
      }
    },
    onSettled: () => queryClient.invalidateQueries({ queryKey: currentSeasonQueryKey }),
  })
}
