import createClient from 'openapi-fetch'
import type { paths } from './schema'

// Types come from the backend's OpenAPI document (`npm run gen:api`), so API changes surface as type errors
export const api = createClient<paths>({
  baseUrl: window.location.origin,
  credentials: 'same-origin',
  // Resolve fetch per request so tests can stub it
  fetch: (request) => globalThis.fetch(request),
})
