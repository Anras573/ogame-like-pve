import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'
import { json, renderApp, stubApi } from '../test/utils'

describe('RegisterPage', () => {
  it('lists validation errors from the server', async () => {
    stubApi({
      'GET /api/auth/me': () => new Response(null, { status: 401 }),
      'POST /api/auth/register': () =>
        json(
          {
            title: 'One or more validation errors occurred.',
            status: 400,
            errors: {
              PasswordTooShort: ['Passwords must be at least 6 characters.'],
              PasswordRequiresDigit: ["Passwords must have at least one digit ('0'-'9')."],
            },
          },
          400,
        ),
    })

    renderApp('/register')
    await userEvent.type(screen.getByLabelText('Email'), 'commander@example.com')
    await userEvent.type(screen.getByLabelText('Password'), 'abc')
    await userEvent.click(screen.getByRole('button', { name: 'Create account' }))

    const alert = await screen.findByRole('alert')
    expect(alert).toHaveTextContent('Passwords must be at least 6 characters.')
    expect(alert).toHaveTextContent('Passwords must have at least one digit')
  })
})
