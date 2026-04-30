import { apiClient } from './client'
import type { Topping } from '@/types/topping'

export const getAllToppings = async (): Promise<Topping[]> => {
  const { data } = await apiClient.get<Topping[]>('/topping')
  return data
}

export const getToppingById = async (id: string): Promise<Topping> => {
  const { data } = await apiClient.get<Topping>(`/topping/${id}`)
  return data
}
