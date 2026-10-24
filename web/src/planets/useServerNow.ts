import { useEffect, useState } from 'react'

/**
 * Server time in milliseconds, re-rendering every second. `serverTime` and `receivedAt` come from the latest
 * response, so a client clock that's off doesn't skew countdowns or resource projections.
 */
export function useServerNow(serverTime: string, receivedAt: number): number {
  const offset = Date.parse(serverTime) - receivedAt
  const [now, setNow] = useState(() => Date.now())

  useEffect(() => {
    const timer = setInterval(() => setNow(Date.now()), 1000)
    return () => clearInterval(timer)
  }, [])

  return now + offset
}
