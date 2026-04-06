import { useAuthStore } from '@/store/authStore'
import { shallow } from 'zustand/shallow'

export function useAuth() {
  const { token, user, role, isAuthenticated, setAuth, logout } = useAuthStore(
    (s) => ({
      token: s.token,
      user: s.user,
      role: s.role,
      isAuthenticated: s.isAuthenticated,
      setAuth: s.setAuth,
      logout: s.logout,
    }),
    shallow
  )

  return { token, user, role, isAuthenticated, isAdmin: role === 'Admin', setAuth, logout }
}
