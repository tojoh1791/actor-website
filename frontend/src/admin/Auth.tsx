import { errorMessage, ApiError, api } from '../lib/http'
import { useCallback, useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import type { AdminUser } from '../types/auth'

import { AuthContext } from './authContext'

async function currentUser(signal?: AbortSignal): Promise<AdminUser | null> {
  try {
    return await api<AdminUser>('/api/auth/me', { signal })
  } catch (reason) {
    // An anonymous visitor is a normal login state; connection failures must stay visible.
    if (reason instanceof ApiError && reason.status === 401) return null
    throw reason
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AdminUser | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const refresh = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      setUser(await currentUser())
    } catch (reason) {
      setError(errorMessage(reason, 'Kunne ikke kontrollere innloggingen.'))
    } finally {
      setLoading(false)
    }
  }, [])
  useEffect(() => {
    const controller = new AbortController()
    currentUser(controller.signal)
      .then((value) => {
        if (!controller.signal.aborted) setUser(value)
      })
      .catch((reason) => {
        if (!controller.signal.aborted)
          setError(errorMessage(reason, 'Kunne ikke kontrollere innloggingen.'))
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false)
      })
    return () => controller.abort()
  }, [])
  useEffect(() => {
    const expire = () => setUser(null)
    window.addEventListener('admin-session-expired', expire)
    return () => window.removeEventListener('admin-session-expired', expire)
  }, [])
  async function login(email: string, password: string, rememberMe: boolean) {
    setUser(
      await api<AdminUser>('/api/auth/login', {
        method: 'POST',
        body: JSON.stringify({ email, password, rememberMe }),
      }),
    )
  }
  async function logout() {
    await api('/api/auth/logout', { method: 'POST', body: '{}' })
    setUser(null)
  }
  return (
    <AuthContext.Provider value={{ user, loading, error, refresh, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}
