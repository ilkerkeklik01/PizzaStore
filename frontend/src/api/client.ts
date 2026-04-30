import axios from 'axios'
import { useAuthStore } from '@/store/authStore'

export const apiClient = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
})

// Attach JWT token from store to every request
apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// On 401 — clear auth and redirect to login.
// Skip only login/register so their form-level errors reach the UI.
const AUTH_FORM_PATHS = ['/auth/login', '/auth/register']
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const url = error.config?.url as string | undefined
    if (error.response?.status === 401 && !AUTH_FORM_PATHS.some((p) => url?.startsWith(p))) {
      useAuthStore.getState().logout()
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)
