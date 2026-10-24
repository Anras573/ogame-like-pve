import { Link } from 'react-router'
import { AuthForm } from '../auth/AuthForm'
import { register } from '../auth/authApi'
import { useSignedIn } from '../auth/useSignedIn'

export function RegisterPage() {
  const handleSuccess = useSignedIn()

  return (
    <main className="auth-page">
      <h1>Create an account</h1>
      <AuthForm submitLabel="Create account" passwordAutoComplete="new-password" onSubmit={register} onSuccess={handleSuccess} />
      <p className="muted">
        Passwords need at least 6 characters, with an uppercase letter, a lowercase letter, a digit and a symbol.
      </p>
      <p className="muted">
        Already have an account? <Link to="/login">Sign in</Link>
      </p>
    </main>
  )
}
