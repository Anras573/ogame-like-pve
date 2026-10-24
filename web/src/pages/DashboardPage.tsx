import { useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router'
import { logout } from '../auth/authApi'
import { useCurrentUser } from '../auth/useCurrentUser'
import { useCurrentSeason, useJoinCurrentSeason, type CurrentSeason } from '../seasons/useCurrentSeason'

const dateFormat = new Intl.DateTimeFormat(undefined, { dateStyle: 'medium' })

export function DashboardPage() {
  const { data: user } = useCurrentUser()
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  async function handleLogout() {
    await logout()
    queryClient.clear()
    navigate('/login', { replace: true })
  }

  return (
    <div className="layout">
      <header className="topbar">
        <span className="brand">OGame-like PvE</span>
        <span className="muted">{user?.email}</span>
        <button type="button" className="link-button" onClick={handleLogout}>
          Sign out
        </button>
      </header>
      <main>
        <SeasonPanel />
      </main>
    </div>
  )
}

function SeasonPanel() {
  const { data: season, isPending, isError } = useCurrentSeason()

  if (isPending) return <p className="muted">Loading season…</p>
  if (isError) return <p role="alert">Could not load the current season.</p>
  if (!season) {
    return (
      <section className="card">
        <h1>No season is open</h1>
        <p className="muted">The next season hasn't started yet. Check back soon.</p>
      </section>
    )
  }

  return season.player ? <JoinedSeason season={season} /> : <JoinSeason season={season} />
}

function SeasonDates({ season }: { season: CurrentSeason }) {
  return (
    <p className="muted">
      {dateFormat.format(new Date(season.startsAt))} – {dateFormat.format(new Date(season.endsAt))}
    </p>
  )
}

function JoinSeason({ season }: { season: CurrentSeason }) {
  const join = useJoinCurrentSeason()

  return (
    <section className="card">
      <h1>{season.name}</h1>
      <SeasonDates season={season} />
      <p>A new universe is waiting. Join this season to claim your home planet.</p>
      {join.isError && <p role="alert">Could not join the season. Please try again.</p>}
      <button type="button" onClick={() => join.mutate()} disabled={join.isPending}>
        {join.isPending ? 'Joining…' : 'Join season'}
      </button>
    </section>
  )
}

function JoinedSeason({ season }: { season: CurrentSeason }) {
  return (
    <section className="card">
      <h1>{season.name}</h1>
      <SeasonDates season={season} />
      <p>You're in. Your empire will appear here soon.</p>
    </section>
  )
}
