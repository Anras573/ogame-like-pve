import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { renderHook } from '@testing-library/react'
import type { ReactNode } from 'react'
import { describe, expect, it, vi } from 'vitest'

const handlers = new Map<string, (message: unknown) => void>()
const connection = {
  on: vi.fn((event: string, handler: (message: unknown) => void) => handlers.set(event, handler)),
  start: vi.fn(() => Promise.resolve()),
  stop: vi.fn(() => Promise.resolve()),
}

vi.mock('@microsoft/signalr', () => ({
  LogLevel: { Warning: 3 },
  HubConnectionBuilder: class {
    withUrl() {
      return this
    }
    withAutomaticReconnect() {
      return this
    }
    configureLogging() {
      return this
    }
    build() {
      return connection
    }
  },
}))

// The setup file replaces this hook for component tests; here we want the real one
const { useGameHub } = await vi.importActual<typeof import('./useGameHub')>('./useGameHub')

describe('useGameHub', () => {
  it('refreshes the planet when one of its buildings completes', async () => {
    const queryClient = new QueryClient()
    const invalidate = vi.spyOn(queryClient, 'invalidateQueries')
    const wrapper = ({ children }: { children: ReactNode }) => (
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    )

    const { unmount } = renderHook(() => useGameHub(), { wrapper })
    handlers.get('BuildingCompleted')!({ planetId: 'p1', building: 'MetalMine', level: 2 })

    expect(connection.start).toHaveBeenCalled()
    expect(invalidate).toHaveBeenCalledWith({ queryKey: ['planet', 'p1'] })

    unmount()
    await vi.waitFor(() => expect(connection.stop).toHaveBeenCalled())
  })
})
