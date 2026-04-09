import { useAuthStore } from '@/store/authStore'
import { useShallow } from 'zustand/react/shallow'

export function useAuth() {
  const { token, user, role, isAuthenticated, setAuth, logout } = useAuthStore(
    useShallow((s) => ({
      token: s.token,
      user: s.user,
      role: s.role,
      isAuthenticated: s.isAuthenticated,
      setAuth: s.setAuth,
      logout: s.logout,
    }))
  )

  return { token, user, role, isAuthenticated, isAdmin: role === 'Admin', setAuth, logout }
}
