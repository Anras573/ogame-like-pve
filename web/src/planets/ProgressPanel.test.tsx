import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { ProgressPanel } from './ProgressPanel'

const startedAt = Date.parse('2026-11-01T12:00:00Z')
const iso = (ms: number) => new Date(ms).toISOString()

function renderPanel(nowMs: number, props: { onDone?: () => void; onCancel?: () => void } = {}) {
  render(
    <ProgressPanel
      label="Construction"
      title="Metal Mine → level 2"
      startedAt={iso(startedAt)}
      completesAt={iso(startedAt + 108_000)}
      now={nowMs}
      onDone={props.onDone ?? (() => undefined)}
      onCancel={props.onCancel}
    />,
  )
}

describe('ProgressPanel', () => {
  it('shows the time left', () => {
    renderPanel(startedAt + 30_000)

    expect(screen.getByRole('region', { name: 'Construction' })).toHaveTextContent('1m 18s left')
  })

  it('never shows more than the whole duration when our clock runs behind', () => {
    renderPanel(startedAt - 2_000)

    expect(screen.getByRole('region', { name: 'Construction' })).toHaveTextContent('1m 48s left')
  })

  it('reports when it is done and stops offering cancel', () => {
    const onDone = vi.fn()
    renderPanel(startedAt + 108_000, { onDone, onCancel: () => undefined })

    expect(screen.getByText('Finishing…')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeDisabled()
    expect(onDone).toHaveBeenCalledOnce()
  })

  it('cancels on request', async () => {
    const onCancel = vi.fn()
    renderPanel(startedAt + 30_000, { onCancel })

    await userEvent.click(screen.getByRole('button', { name: 'Cancel' }))

    expect(onCancel).toHaveBeenCalledOnce()
  })

  it('has no cancel button when cancelling is not possible', () => {
    renderPanel(startedAt + 30_000)

    expect(screen.queryByRole('button', { name: 'Cancel' })).not.toBeInTheDocument()
  })
})
