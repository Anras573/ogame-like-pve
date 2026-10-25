import { useEffect } from 'react'
import { formatDuration } from './economy'

interface ProgressPanelProps {
  label: string
  title: string
  startedAt: string
  completesAt: string
  /** Server time in ms. */
  now: number
  onDone: () => void
  onCancel?: () => void
  cancelling?: boolean
}

/** Something in progress (a construction or research) with its countdown and an optional cancel button. */
export function ProgressPanel(props: ProgressPanelProps) {
  const completesAt = Date.parse(props.completesAt)
  const totalSeconds = (completesAt - Date.parse(props.startedAt)) / 1000

  // Request latency makes our clock estimate run slightly behind, so never show more than the whole duration
  const remainingSeconds = Math.min(totalSeconds, (completesAt - props.now) / 1000)
  const done = remainingSeconds <= 0

  // Refresh once it should be done, in case the live notification doesn't arrive
  const { onDone } = props
  useEffect(() => {
    if (done) onDone()
  }, [done, onDone])

  return (
    <section className="card progress-panel" aria-label={props.label}>
      <div>
        <strong>{props.title}</strong>
        <p className="muted">{done ? 'Finishing…' : `${formatDuration(remainingSeconds)} left`}</p>
      </div>
      {props.onCancel && (
        <button type="button" className="secondary" onClick={props.onCancel} disabled={props.cancelling || done}>
          Cancel
        </button>
      )}
    </section>
  )
}
