import { apiClient } from './client'
import type { Order, OrderStatus, PagedResult } from '@/types/order'

export interface OrderFilterParams {
  status?: OrderStatus
  fromDate?: string
  toDate?: string
  page?: number
  pageSize?: number
}

export const checkoutCart = async (): Promise<Order> => {
  const { data } = await apiClient.post<Order>('/order/checkout')
  return data
}

export const getMyOrders = async (params?: OrderFilterParams): Promise<PagedResult<Order>> => {
  const { data } = await apiClient.get<PagedResult<Order>>('/order', { params })
  return data
}

export const getOrderById = async (id: string): Promise<Order> => {
  const { data } = await apiClient.get<Order>(`/order/${id}`)
  return data
}

export const cancelOrder = async (id: string): Promise<Order> => {
  const { data } = await apiClient.post<Order>(`/order/${id}/cancel`)
  return data
}
