import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'
import { jwtDecode } from 'jwt-decode'

interface Props {
  requireAdmin?: boolean
}

export default function ProtectedRoute({ requireAdmin = false }: Props) {
  const { token, isAuthenticated, role } = useAuth()

  if (!isAuthenticated || !token) {
    return <Navigate to="/login" replace />
  }

  // Client-side expiry check — no logout() call here (side-effects in render
  // are forbidden). The store's onRehydrateStorage and the API 401 interceptor
  // already handle clearing stale tokens.
  try {
    const { exp } = jwtDecode<{ exp: number }>(token)
    if (exp * 1000 < Date.now()) {
      return <Navigate to="/login" replace />
    }
  } catch {
    return <Navigate to="/login" replace />
  }

  if (requireAdmin && role !== 'Admin') {
    return <Navigate to="/" replace />
  }

  return <Outlet />
}
