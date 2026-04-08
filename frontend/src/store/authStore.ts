import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import { jwtDecode } from 'jwt-decode'
import type { UserInfo } from '@/types/auth'

// .NET ClaimTypes.Role serialises to this key in the JWT payload
const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'

interface JwtPayload {
  sub: string
  email: string
  exp: number
  [key: string]: unknown
}

function extractRole(token: string): string | null {
  try {
    const decoded = jwtDecode<JwtPayload>(token)
    const roleValue = decoded[ROLE_CLAIM]
    if (Array.isArray(roleValue)) return roleValue[0] ?? null
    return (roleValue as string) ?? null
  } catch {
    return null
  }
}

function isTokenExpired(token: string): boolean {
  try {
    const { exp } = jwtDecode<JwtPayload>(token)
    return exp * 1000 < Date.now()
  } catch {
    return true
  }
}

interface AuthState {
  token: string | null
  user: UserInfo | null
  role: string | null
  isAuthenticated: boolean
  setAuth: (token: string, user: UserInfo) => void
  logout: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      user: null,
      role: null,
      isAuthenticated: false,

      setAuth: (token, user) => {
        if (isTokenExpired(token)) {
          set({ token: null, user: null, role: null, isAuthenticated: false })
          return
        }
        set({
          token,
          user,
          role: extractRole(token),
          isAuthenticated: true,
        })
      },

      logout: () =>
        set({ token: null, user: null, role: null, isAuthenticated: false }),
    }),
    {
      name: 'pizzastore-auth',
      // Re-validate token expiry when hydrating from localStorage.
      // Use setState directly — calling state.logout() during hydration
      // dispatches a set() mid-hydration, which is unsafe in Zustand.
      onRehydrateStorage: () => (state) => {
        if (state?.token && isTokenExpired(state.token)) {
          useAuthStore.setState({ token: null, user: null, role: null, isAuthenticated: false })
        }
      },
    }
  )
)
