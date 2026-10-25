import { formatAmount, formatDuration } from './economy'
import type { Resources } from './types'

/** A cost line, with resources the planet can't cover yet highlighted. */
export function Cost(props: { cost: Resources; available: Resources; seconds: number; multiplier?: number }) {
  const m = props.multiplier ?? 1
  const items: [string, number, number][] = [
    ['Metal', props.cost.metal * m, props.available.metal],
    ['Crystal', props.cost.crystal * m, props.available.crystal],
    ['Deuterium', props.cost.deuterium * m, props.available.deuterium],
  ]

  return (
    <ul className="cost">
      {items
        .filter(([, amount]) => amount > 0)
        .map(([label, amount, available]) => (
          <li key={label} className={available < amount ? 'missing' : undefined}>
            {label} {formatAmount(amount)}
          </li>
        ))}
      <li className="muted">{formatDuration(props.seconds * m)}</li>
    </ul>
  )
}
