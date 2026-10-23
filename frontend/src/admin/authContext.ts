import { createContext } from 'react'
import type { AdminUser } from '../types/auth'

interface AuthContextValue {
  user: AdminUser | null
  loading: boolean
  error: string
  refresh: () => Promise<void>
  login: (email: string, password: string, rememberMe: boolean) => Promise<void>
  logout: () => Promise<void>
}

export const AuthContext = createContext<AuthContextValue | null>(null)
