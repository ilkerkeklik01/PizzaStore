export interface UserInfo {
  id: string
  firstName: string
  lastName: string
  email: string
}

export interface AuthResponse {
  token: string
  user: UserInfo
}

export interface LoginDto {
  email: string
  password: string
}

export interface RegisterDto {
  firstName: string
  lastName: string
  email: string
  password: string
}
