import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'
import { planetId, researchOverview } from '../test/fixtures'
import { json, stubApi } from '../test/utils'
import { ResearchTab } from './ResearchTab'

const researchPath = `/api/planets/${planetId}/research`
const plenty = { metal: 10_000, crystal: 10_000, deuterium: 10_000 }

function renderTab(resources = plenty) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={queryClient}>
      <ResearchTab planetId={planetId} resources={resources} now={Date.now()} />
    </QueryClientProvider>,
  )
}

const inFuture = (seconds: number) => new Date(Date.now() + seconds * 1000).toISOString()

describe('ResearchTab', () => {
  it('shows what a locked technology still needs', async () => {
    stubApi({ [`GET ${researchPath}`]: () => json(researchOverview()) })

    renderTab()

    const combustion = await screen.findByRole('article', { name: 'Combustion Drive' })
    expect(combustion).toHaveTextContent('Requires Energy Technology 1')
    expect(within(combustion).getByRole('button')).toBeDisabled()
    const computer = screen.getByRole('article', { name: 'Computer Technology' })
    expect(within(computer).getByRole('button', { name: 'Research level 1' })).toBeEnabled()
  })

  it('asks for a lab when there is none', async () => {
    stubApi({ [`GET ${researchPath}`]: () => json(researchOverview({ researchLabLevel: 0 })) })

    renderTab()

    expect(await screen.findByText(/Build a Research Lab/)).toBeInTheDocument()
  })

  it('starts research and shows its progress', async () => {
    const researching = researchOverview({
      currentResearch: { research: 'ComputerTechnology', targetLevel: 1, planetId, startedAt: inFuture(0), completesAt: inFuture(144) },
    })
    const fetchMock = stubApi({
      [`GET ${researchPath}`]: () => json(researchOverview()),
      [`POST ${researchPath}`]: () => json(researching),
      [`GET /api/planets/${planetId}`]: () => json({}),
    })

    renderTab()
    const computer = await screen.findByRole('article', { name: 'Computer Technology' })
    await userEvent.click(within(computer).getByRole('button', { name: 'Research level 1' }))

    const progress = await screen.findByRole('region', { name: 'Research' })
    expect(progress).toHaveTextContent('Computer Technology → level 1')
    expect(within(progress).getByRole('button', { name: 'Cancel' })).toBeEnabled()
    expect(within(computer).getByRole('button')).toBeDisabled()

    const post = fetchMock.mock.calls.map(([request]) => request).find((r) => r.method === 'POST')!
    expect(await post.json()).toEqual({ research: 'ComputerTechnology' })
  })

  it('only offers cancel on the planet that paid for the research', async () => {
    stubApi({
      [`GET ${researchPath}`]: () =>
        json(
          researchOverview({
            currentResearch: {
              research: 'ComputerTechnology',
              targetLevel: 1,
              planetId: 'another-planet',
              startedAt: inFuture(0),
              completesAt: inFuture(144),
            },
          }),
        ),
    })

    renderTab()

    const progress = await screen.findByRole('region', { name: 'Research' })
    expect(within(progress).queryByRole('button', { name: 'Cancel' })).not.toBeInTheDocument()
  })

  it('cannot start what the planet cannot afford', async () => {
    stubApi({ [`GET ${researchPath}`]: () => json(researchOverview()) })

    renderTab({ metal: 0, crystal: 0, deuterium: 0 })

    const computer = await screen.findByRole('article', { name: 'Computer Technology' })
    expect(within(computer).getByRole('button', { name: 'Research level 1' })).toBeDisabled()
  })
})
