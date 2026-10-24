import { useState, type FormEvent } from 'react'
import type { AuthResult } from './authApi'

interface AuthFormProps {
  submitLabel: string
  passwordAutoComplete: 'current-password' | 'new-password'
  onSubmit: (email: string, password: string) => Promise<AuthResult>
  onSuccess: () => void
}

export function AuthForm({ submitLabel, passwordAutoComplete, onSubmit, onSuccess }: AuthFormProps) {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [errors, setErrors] = useState<string[]>([])
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSubmitting(true)
    setErrors([])
    try {
      const result = await onSubmit(email, password)
      if (result.ok) onSuccess()
      else setErrors(result.errors)
    } catch {
      setErrors(['Could not reach the server. Please try again.'])
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <form className="card form" onSubmit={handleSubmit}>
      <label>
        Email
        <input type="email" autoComplete="email" required value={email} onChange={(e) => setEmail(e.target.value)} />
      </label>
      <label>
        Password
        <input
          type="password"
          autoComplete={passwordAutoComplete}
          required
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
      </label>
      {errors.length > 0 && (
        <ul role="alert" className="errors">
          {errors.map((error) => (
            <li key={error}>{error}</li>
          ))}
        </ul>
      )}
      <button type="submit" disabled={submitting}>
        {submitting ? 'Please wait…' : submitLabel}
      </button>
    </form>
  )
}
