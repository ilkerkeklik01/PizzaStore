import { apiClient } from './client'
import type { AdminUser } from '@/types/admin'
import type { Order, OrderStatus, PagedResult } from '@/types/order'

export interface AdminOrderFilterParams {
  status?: OrderStatus
  userId?: string
  fromDate?: string
  toDate?: string
  page?: number
  pageSize?: number
}

export const getAllUsers = async (): Promise<AdminUser[]> => {
  const { data } = await apiClient.get<AdminUser[]>('/admin/users')
  return data
}

export const getUserById = async (id: string): Promise<AdminUser> => {
  const { data } = await apiClient.get<AdminUser>(`/admin/users/${id}`)
  return data
}

export const getOrdersByUserId = async (id: string): Promise<Order[]> => {
  const { data } = await apiClient.get<Order[]>(`/admin/users/${id}/orders`)
  return data
}

export const getAllOrders = async (params?: AdminOrderFilterParams): Promise<PagedResult<Order>> => {
  const { data } = await apiClient.get<PagedResult<Order>>('/admin/orders', { params })
  return data
}

export const updateOrderStatus = async (orderId: string, newStatus: OrderStatus): Promise<Order> => {
  const { data } = await apiClient.put<Order>(`/admin/orders/${orderId}/status`, JSON.stringify(newStatus), {
    headers: { 'Content-Type': 'application/json' },
  })
  return data
}
