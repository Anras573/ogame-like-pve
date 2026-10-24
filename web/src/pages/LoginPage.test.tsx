import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'
import { json, renderApp, stubApi } from '../test/utils'

describe('LoginPage', () => {
  it('shows an error for wrong credentials', async () => {
    stubApi({
      'GET /api/auth/me': () => new Response(null, { status: 401 }),
      'POST /api/auth/login': () => json({ title: 'Unauthorized', status: 401, detail: 'Failed' }, 401),
    })

    renderApp('/login')
    await userEvent.type(screen.getByLabelText('Email'), 'commander@example.com')
    await userEvent.type(screen.getByLabelText('Password'), 'wrong')
    await userEvent.click(screen.getByRole('button', { name: 'Sign in' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Invalid email or password.')
  })

  // Starting at "/" caches the signed-out user before the redirect, which must not stick after login
  it('logs in with cookies and goes to the dashboard', async () => {
    let signedIn = false
    const fetchMock = stubApi({
      'GET /api/auth/me': () =>
        signedIn ? json({ id: '00000000-0000-0000-0000-000000000001', email: 'commander@example.com' }) : new Response(null, { status: 401 }),
      'POST /api/auth/login': () => {
        signedIn = true
        return new Response(null, { status: 200 })
      },
      'GET /api/seasons/current': () => new Response(null, { status: 404 }),
    })

    const { router } = renderApp('/')
    await userEvent.type(await screen.findByLabelText('Email'), 'commander@example.com')
    await userEvent.type(screen.getByLabelText('Password'), 'Sup3r-secret!')
    await userEvent.click(screen.getByRole('button', { name: 'Sign in' }))

    expect(await screen.findByRole('heading', { name: 'No season is open' })).toBeInTheDocument()
    expect(router.state.location.pathname).toBe('/')
    const loginRequest = fetchMock.mock.calls.map(([request]) => request).find((r) => r.method === 'POST')
    expect(new URL(loginRequest!.url).searchParams.get('useCookies')).toBe('true')
  })
})
