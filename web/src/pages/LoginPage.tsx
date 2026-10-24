import { Link } from 'react-router'
import { AuthForm } from '../auth/AuthForm'
import { login } from '../auth/authApi'
import { useSignedIn } from '../auth/useSignedIn'

export function LoginPage() {
  const handleSuccess = useSignedIn()

  return (
    <main className="auth-page">
      <h1>Sign in</h1>
      <AuthForm submitLabel="Sign in" passwordAutoComplete="current-password" onSubmit={login} onSuccess={handleSuccess} />
      <p className="muted">
        New commander? <Link to="/register">Create an account</Link>
      </p>
    </main>
  )
}
