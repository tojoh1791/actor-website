import { createContext, useCallback, useContext, useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import { ApiError, api } from './api'
import type { AdminUser } from './api'

interface AuthContextValue {
  user: AdminUser | null
  loading: boolean
  error: string
  refresh: () => Promise<void>
  login: (email: string, password: string, rememberMe: boolean) => Promise<void>
  logout: () => Promise<void>
}
const AuthContext = createContext<AuthContextValue | null>(null)
export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AdminUser | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const refresh = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      setUser(await api<AdminUser>('/api/auth/me'))
    } catch (reason) {
      if (reason instanceof ApiError && reason.status === 401) setUser(null)
      else
        setError(reason instanceof Error ? reason.message : 'Kunne ikke kontrollere innloggingen.')
    } finally {
      setLoading(false)
    }
  }, [])
  useEffect(() => {
    void refresh()
  }, [refresh])
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
export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) throw new Error('AuthProvider mangler.')
  return context
}
