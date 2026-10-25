import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'
import type { Planet } from '../planets/types'
import { homePlanet, planetId, shipyardPlanet } from '../test/fixtures'
import { json, stubApi } from '../test/utils'
import { UnitsTab } from './UnitsTab'

const shipyardPath = `/api/planets/${planetId}/shipyard`

function renderTab(planet: Planet, category: 'Ship' | 'Defense', now = Date.now()) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={queryClient}>
      <UnitsTab planet={planet} category={category} resources={planet.resources} now={now} />
    </QueryClientProvider>,
  )
}

describe('UnitsTab', () => {
  it('asks for a shipyard and shows what each unit needs', () => {
    stubApi({})

    renderTab(homePlanet(), 'Ship')

    expect(screen.getByText(/Build a Shipyard/)).toBeInTheDocument()
    expect(screen.getByRole('article', { name: 'Light Fighter' })).toHaveTextContent('Requires Shipyard 1, Combustion Drive 1')
    expect(screen.queryByRole('article', { name: 'Rocket Launcher' })).not.toBeInTheDocument()
  })

  it('shows only defenses on the defense tab', () => {
    stubApi({})

    renderTab(shipyardPlanet(), 'Defense')

    expect(screen.getByRole('article', { name: 'Rocket Launcher' })).toHaveTextContent('4 owned')
    expect(screen.queryByRole('article', { name: 'Light Fighter' })).not.toBeInTheDocument()
  })

  it('orders the chosen quantity', async () => {
    const queued = shipyardPlanet({
      resources: { metal: 6_000, crystal: 5_000, deuterium: 0 },
      shipyardQueue: [{ unit: 'RocketLauncher', quantity: 2, unitBuildSeconds: 144 }],
      shipyardProgressAt: new Date().toISOString(),
    })
    const fetchMock = stubApi({ [`POST ${shipyardPath}`]: () => json(queued) })

    renderTab(shipyardPlanet(), 'Defense')
    const launcher = screen.getByRole('article', { name: 'Rocket Launcher' })
    const quantity = within(launcher).getByRole('spinbutton', { name: 'Rocket Launcher quantity' })
    await userEvent.clear(quantity)
    await userEvent.type(quantity, '2')
    expect(launcher).toHaveTextContent('Metal 4,000')
    await userEvent.click(within(launcher).getByRole('button', { name: 'Build' }))

    const post = fetchMock.mock.calls.map(([request]) => request).find((r) => r.method === 'POST')!
    expect(await post.json()).toEqual({ unit: 'RocketLauncher', quantity: 2 })
  })

  it('fills in the most the planet can afford', async () => {
    stubApi({})

    renderTab(shipyardPlanet(), 'Ship')
    const fighter = screen.getByRole('article', { name: 'Light Fighter' })
    await userEvent.click(within(fighter).getByRole('button', { name: 'Max' }))

    // 10,000 metal / 3,000 per fighter
    expect(within(fighter).getByRole('spinbutton')).toHaveValue(3)
  })

  it('will not order more than the planet can afford', async () => {
    stubApi({})

    renderTab(shipyardPlanet(), 'Ship')
    const fighter = screen.getByRole('article', { name: 'Light Fighter' })
    const quantity = within(fighter).getByRole('spinbutton')
    await userEvent.clear(quantity)
    await userEvent.type(quantity, '4')

    expect(within(fighter).getByRole('button', { name: 'Build' })).toBeDisabled()
  })

  it('shows the queue and counts units delivered since the last refresh', () => {
    stubApi({})
    const started = Date.now() - 150_000
    const planet = shipyardPlanet({
      shipyardQueue: [{ unit: 'RocketLauncher', quantity: 3, unitBuildSeconds: 60 }],
      shipyardProgressAt: new Date(started).toISOString(),
    })

    renderTab(planet, 'Defense', Date.now())

    // 150 s into a 3 × 60 s order: two delivered on top of the 4 owned, one still building
    expect(screen.getByRole('article', { name: 'Rocket Launcher' })).toHaveTextContent('6 owned')
    const queue = screen.getByRole('region', { name: 'Shipyard queue' })
    expect(queue).toHaveTextContent('1 × Rocket Launcher')
    expect(queue).toHaveTextContent(/next in \d+s/)
  })
})
