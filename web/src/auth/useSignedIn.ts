import { useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router'
import { currentUserQueryKey } from './useCurrentUser'

/** Returns a callback that refreshes the current user after sign-in and goes to the dashboard. */
export function useSignedIn() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  return async () => {
    // The guard may have cached "signed out" before redirecting here. That query is inactive now,
    // so refetch it explicitly; otherwise the guard would read the stale value and bounce back.
    await queryClient.invalidateQueries({ queryKey: currentUserQueryKey, refetchType: 'all' })
    navigate('/', { replace: true })
  }
}
