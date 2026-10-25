import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'
import { homePlanet, planetId } from '../test/fixtures'
import { json, renderApp, stubApi } from '../test/utils'

const user = { id: '00000000-0000-0000-0000-000000000001', email: 'commander@example.com' }
const season = {
  id: '00000000-0000-0000-0000-0000000000aa',
  name: 'Season 1',
  startsAt: '2026-11-01T12:00:00Z',
  endsAt: '2027-01-24T12:00:00Z',
  player: null,
}
const joinedSeason = {
  ...season,
  player: { playerId: '00000000-0000-0000-0000-0000000000bb', joinedAt: '2026-11-02T08:00:00Z' },
}

describe('DashboardPage', () => {
  it('redirects to the login page when signed out', async () => {
    stubApi({ 'GET /api/auth/me': () => new Response(null, { status: 401 }) })

    const { router } = renderApp('/')

    expect(await screen.findByRole('heading', { name: 'Sign in' })).toBeInTheDocument()
    expect(router.state.location.pathname).toBe('/login')
  })

  it('offers to join the open season', async () => {
    stubApi({
      'GET /api/auth/me': () => json(user),
      'GET /api/seasons/current': () => json(season),
    })

    renderApp('/')

    expect(await screen.findByRole('heading', { name: 'Season 1' })).toBeInTheDocument()
    expect(screen.getByText('commander@example.com')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Join season' })).toBeInTheDocument()
  })

  it('shows the home planet after joining', async () => {
    let joined = false
    const fetchMock = stubApi({
      'GET /api/auth/me': () => json(user),
      'GET /api/seasons/current': () => json(joined ? joinedSeason : season),
      'POST /api/seasons/current/join': () => {
        joined = true
        return json(joinedSeason.player, 201)
      },
      'GET /api/planets': () => json([{ id: planetId, name: 'Homeworld', coordinates: { galaxy: 1, system: 42, position: 8 } }]),
      [`GET /api/planets/${planetId}`]: () => json(homePlanet()),
    })

    renderApp('/')
    await userEvent.click(await screen.findByRole('button', { name: 'Join season' }))

    expect(await screen.findByRole('heading', { name: 'Homeworld' })).toBeInTheDocument()
    expect(screen.getByText('[1:42:8] · up to 40°C')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Join season' })).not.toBeInTheDocument()
    expect(fetchMock).toHaveBeenCalledWith(expect.objectContaining({ method: 'POST' }))
  })

  it('explains when no season is open', async () => {
    stubApi({
      'GET /api/auth/me': () => json(user),
      'GET /api/seasons/current': () => new Response(null, { status: 404 }),
    })

    renderApp('/')

    expect(await screen.findByRole('heading', { name: 'No season is open' })).toBeInTheDocument()
  })
})
