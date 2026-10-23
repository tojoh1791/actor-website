export class ApiError extends Error {
  status: number
  fields: Record<string, string[]>
  constructor(message: string, status: number, fields: Record<string, string[]> = {}) {
    super(message)
    this.status = status
    this.fields = fields
  }
}

export async function api<T>(path: string, options: RequestInit = {}): Promise<T> {
  const method = options.method ?? 'GET'
  const headers = new Headers(options.headers)
  headers.set('Accept', 'application/json')
  if (method !== 'GET') {
    if (!headers.has('X-CSRF-TOKEN')) {
      let tokenResponse: Response
      try {
        tokenResponse = await fetch('/api/auth/csrf', {
          credentials: 'same-origin',
          cache: 'no-store',
        })
      } catch {
        throw new ApiError(
          'Får ikke kontakt med serveren. Kontroller forbindelsen og prøv igjen.',
          0,
        )
      }
      if (!tokenResponse.ok)
        throw new ApiError('Kunne ikke kontakte serveren. Prøv igjen.', tokenResponse.status)
      const csrf = (await tokenResponse.json()) as { token: string }
      headers.set('X-CSRF-TOKEN', csrf.token)
    }
    if (!headers.has('Content-Type')) headers.set('Content-Type', 'application/json')
  }
  let response: Response
  try {
    response = await fetch(path, {
      ...options,
      headers,
      credentials: 'same-origin',
      cache: 'no-store',
    })
  } catch {
    throw new ApiError('Får ikke kontakt med serveren. Kontroller forbindelsen og prøv igjen.', 0)
  }
  if (!response.ok) {
    const problem = (await response.json().catch(() => ({}))) as {
      title?: string
      errors?: Record<string, string[]>
    }
    const message =
      response.status === 429
        ? path.startsWith('/api/bookings')
          ? 'For mange forsøk. Vent i 15 minutter før du prøver igjen.'
          : 'For mange forsøk. Vent litt før du prøver igjen.'
        : response.status >= 500
          ? 'Serveren er ikke tilgjengelig akkurat nå. Prøv igjen senere.'
          : problem.errors
            ? 'Se over feltene som er markert nedenfor.'
            : (problem.title ??
              (response.status === 401
                ? 'Du må logge inn for å fortsette.'
                : 'Handlingen kunne ikke fullføres.'))
    if (response.status === 401 && path !== '/api/auth/login' && path !== '/api/auth/me')
      window.dispatchEvent(new Event('admin-session-expired'))
    throw new ApiError(message, response.status, problem.errors)
  }
  return response.status === 204 ? (undefined as T) : (response.json() as Promise<T>)
}
