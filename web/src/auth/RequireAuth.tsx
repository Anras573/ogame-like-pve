import { Navigate, Outlet } from 'react-router'
import { useCurrentUser } from './useCurrentUser'

export function RequireAuth() {
  const { data: user, isPending, isError } = useCurrentUser()

  if (isPending) return <p className="muted">Loading…</p>
  if (isError) return <p role="alert">Could not reach the server. Please try again.</p>
  if (!user) return <Navigate to="/login" replace />

  return <Outlet />
}
