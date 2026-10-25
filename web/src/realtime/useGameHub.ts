import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { planetQueryKey } from '../planets/usePlanets'
import type { BuildingType, ResearchType } from '../planets/types'
import { allResearchQueryKey } from '../research/useResearch'

interface BuildingCompletedMessage {
  planetId: string
  building: BuildingType
  level: number
}

interface ResearchCompletedMessage {
  research: ResearchType
  level: number
}

/**
 * Listens for game events while mounted and refreshes the affected data. The auth cookie authenticates the
 * connection, and the Vite dev server proxies /hubs to the API.
 */
export function useGameHub() {
  const queryClient = useQueryClient()

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl('/hubs/game')
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build()

    connection.on('BuildingCompleted', (message: BuildingCompletedMessage) => {
      void queryClient.invalidateQueries({ queryKey: planetQueryKey(message.planetId) })
    })

    // Research unlocks things on every planet, so refresh them all
    connection.on('ResearchCompleted', (_message: ResearchCompletedMessage) => {
      void queryClient.invalidateQueries({ queryKey: allResearchQueryKey })
      void queryClient.invalidateQueries({ queryKey: ['planet'] })
    })

    // Live updates are a nicety: the page also refreshes when a countdown runs out, so a failed connection is fine
    const started = connection.start().catch(() => undefined)

    return () => {
      void started.then(() => connection.stop())
    }
  }, [queryClient])
}
