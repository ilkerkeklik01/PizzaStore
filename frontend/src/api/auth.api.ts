import { apiClient } from './client'
import type { AuthResponse, LoginDto, RegisterDto } from '@/types/auth'

export async function loginUser(dto: LoginDto): Promise<AuthResponse> {
  const { data } = await apiClient.post<AuthResponse>('/auth/login', dto)
  return data
}

export async function registerUser(dto: RegisterDto): Promise<AuthResponse> {
  const { data } = await apiClient.post<AuthResponse>('/auth/register', dto)
  return data
}
