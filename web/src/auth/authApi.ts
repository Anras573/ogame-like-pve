import { api } from '../api/client'

export type AuthResult = { ok: true } | { ok: false; errors: string[] }

export async function login(email: string, password: string): Promise<AuthResult> {
  const { response } = await api.POST('/api/auth/login', {
    params: { query: { useCookies: true } },
    body: { email, password },
  })
  if (response.ok) return { ok: true }
  if (response.status === 429) return { ok: false, errors: ['Too many attempts. Please wait a minute and try again.'] }
  return { ok: false, errors: ['Invalid email or password.'] }
}

export async function register(email: string, password: string): Promise<AuthResult> {
  const { response, error } = await api.POST('/api/auth/register', { body: { email, password } })
  if (response.ok) return login(email, password)
  if (response.status === 429) return { ok: false, errors: ['Too many attempts. Please wait a minute and try again.'] }

  // Identity returns a validation problem keyed by error code (PasswordTooShort, DuplicateUserName, ...)
  const errors = Object.values(error?.errors ?? {}).flat()
  return { ok: false, errors: errors.length > 0 ? errors : ['Registration failed.'] }
}

export async function logout(): Promise<void> {
  await api.POST('/api/auth/logout')
}
