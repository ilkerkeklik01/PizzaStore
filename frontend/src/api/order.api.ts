import { apiClient } from './client'
import type { Order } from '@/types/order'

export const checkoutCart = async (): Promise<Order> => {
  const { data } = await apiClient.post<Order>('/order/checkout')
  return data
}

export const getMyOrders = async (): Promise<Order[]> => {
  const { data } = await apiClient.get<Order[]>('/order')
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
