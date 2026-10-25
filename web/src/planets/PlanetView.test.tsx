import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'
import { homePlanet, planetId } from '../test/fixtures'
import { json, stubApi } from '../test/utils'
import { PlanetView } from './PlanetView'

const planetPath = `/api/planets/${planetId}`
const constructionPath = `${planetPath}/construction`

function renderPlanet() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={queryClient}>
      <PlanetView planetId={planetId} />
    </QueryClientProvider>,
  )
}

function inFuture(seconds: number) {
  return new Date(Date.now() + seconds * 1000).toISOString()
}

describe('PlanetView', () => {
  it('shows resources, production and storage', async () => {
    stubApi({ [`GET ${planetPath}`]: () => json(homePlanet()) })

    renderPlanet()

    const metal = await screen.findByTestId('resource-metal')
    expect(metal).toHaveTextContent('500')
    expect(metal).toHaveTextContent('+30/h')
    expect(metal).toHaveTextContent(`cap ${new Intl.NumberFormat().format(10_000)}`)
  })

  it('warns about an energy shortage', async () => {
    stubApi({
      [`GET ${planetPath}`]: () => json(homePlanet({ energy: { produced: 22, consumed: 44, productionFactor: 0.5 } })),
    })

    renderPlanet()

    expect(await screen.findByTestId('resource-energy')).toHaveTextContent('-22')
    expect(screen.getByTestId('resource-energy')).toHaveTextContent('Mines at 50%')
  })

  it('disables upgrades the planet cannot afford', async () => {
    stubApi({ [`GET ${planetPath}`]: () => json(homePlanet()) })

    renderPlanet()

    const robotics = await screen.findByRole('article', { name: 'Robotics Factory' })
    expect(within(robotics).getByRole('button', { name: 'Upgrade to 1' })).toBeDisabled()
    expect(within(robotics).getByText('Deuterium 200')).toHaveClass('missing')
    const mine = screen.getByRole('article', { name: 'Metal Mine' })
    expect(within(mine).getByRole('button', { name: 'Upgrade to 1' })).toBeEnabled()
  })

  it('starts an upgrade and shows its countdown', async () => {
    const building = homePlanet({
      resources: { metal: 440, crystal: 485, deuterium: 0 },
      construction: { building: 'MetalMine', targetLevel: 1, startedAt: inFuture(0), completesAt: inFuture(108) },
    })
    const fetchMock = stubApi({
      [`GET ${planetPath}`]: () => json(homePlanet()),
      [`POST ${constructionPath}`]: () => json(building),
    })

    renderPlanet()
    const mine = await screen.findByRole('article', { name: 'Metal Mine' })
    await userEvent.click(within(mine).getByRole('button', { name: 'Upgrade to 1' }))

    const construction = await screen.findByRole('region', { name: 'Construction' })
    expect(construction).toHaveTextContent('Metal Mine → level 1')
    expect(construction).toHaveTextContent(/1m 4[78]s left/)
    expect(screen.getByRole('article', { name: 'Crystal Mine' }).querySelector('button')).toBeDisabled()

    const post = fetchMock.mock.calls.map(([request]) => request).find((r) => r.method === 'POST')!
    expect(await post.json()).toEqual({ building: 'MetalMine' })
  })

  it('cancels the construction in progress', async () => {
    let cancelled = false
    stubApi({
      [`GET ${planetPath}`]: () =>
        json(
          cancelled
            ? homePlanet()
            : homePlanet({
                construction: { building: 'SolarPlant', targetLevel: 1, startedAt: new Date().toISOString(), completesAt: inFuture(150) },
              }),
        ),
      [`DELETE ${constructionPath}`]: () => {
        cancelled = true
        return json(homePlanet())
      },
    })

    renderPlanet()
    const construction = await screen.findByRole('region', { name: 'Construction' })
    await userEvent.click(within(construction).getByRole('button', { name: 'Cancel' }))

    expect(await screen.findByRole('article', { name: 'Solar Plant' })).toBeInTheDocument()
    expect(screen.queryByRole('region', { name: 'Construction' })).not.toBeInTheDocument()
  })

  it("shows the server's reason when an action is refused", async () => {
    stubApi({
      [`GET ${planetPath}`]: () => json(homePlanet()),
      [`POST ${constructionPath}`]: () =>
        json({ title: 'Already building', status: 409, detail: 'This planet is already constructing a building.' }, 409),
    })

    renderPlanet()
    const mine = await screen.findByRole('article', { name: 'Metal Mine' })
    await userEvent.click(within(mine).getByRole('button', { name: 'Upgrade to 1' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('This planet is already constructing a building.')
  })
})
