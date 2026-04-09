import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import ProtectedRoute from '@/components/ProtectedRoute'
import LoginPage from '@/pages/LoginPage'
import RegisterPage from '@/pages/RegisterPage'
import HomePage from '@/pages/HomePage'
import AdminPage from '@/pages/AdminPage'
import CheckoutPage from '@/pages/CheckoutPage'
import OrderHistoryPage from '@/pages/OrderHistoryPage'
import OrderDetailPage from '@/pages/OrderDetailPage'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5,
      retry: 1,
    },
  },
})

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          {/* Public routes */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          {/* Public home – all roles can browse; auth features shown conditionally */}
          <Route path="/" element={<HomePage />} />

          {/* Admin-only routes */}
          <Route element={<ProtectedRoute requireAdmin />}>
            <Route path="/admin" element={<AdminPage />} />
          </Route>

          {/* Authenticated user routes */}
          <Route element={<ProtectedRoute />}>
            <Route path="/checkout" element={<CheckoutPage />} />
            <Route path="/orders" element={<OrderHistoryPage />} />
            <Route path="/orders/:id" element={<OrderDetailPage />} />
          </Route>

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  )
}
