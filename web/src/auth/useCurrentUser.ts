import { useQuery } from '@tanstack/react-query'
import { api } from '../api/client'
import type { components } from '../api/schema'

export type CurrentUser = components['schemas']['CurrentUserDto']

export const currentUserQueryKey = ['currentUser'] as const

/** The signed-in user, or `null` when nobody is signed in. */
export function useCurrentUser() {
  return useQuery({
    queryKey: currentUserQueryKey,
    queryFn: async (): Promise<CurrentUser | null> => {
      const { data, response } = await api.GET('/api/auth/me')
      if (response.status === 401) return null
      if (!data) throw new Error(`Failed to load user (${response.status})`)
      return data
    },
    retry: false,
  })
}
