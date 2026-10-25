import '@testing-library/jest-dom/vitest'
import { cleanup } from '@testing-library/react'
import { afterEach, vi } from 'vitest'

afterEach(() => {
  cleanup()
  vi.unstubAllGlobals()
})

// Components under test shouldn't open real SignalR connections
vi.mock('../realtime/useGameHub', () => ({ useGameHub: () => undefined }))
