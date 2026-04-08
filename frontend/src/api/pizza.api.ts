import { apiClient } from './client'
import type { Pizza } from '@/types/pizza'

export const getAllPizzas = (): Promise<Pizza[]> =>
  apiClient.get<Pizza[]>('/pizza').then((r) => r.data)

export const getPizzaById = (id: string): Promise<Pizza> =>
  apiClient.get<Pizza>(`/pizza/${id}`).then((r) => r.data)

export const getPizzasByType = (type: string): Promise<Pizza[]> =>
  apiClient.get<Pizza[]>(`/pizza/type/${type}`).then((r) => r.data)
