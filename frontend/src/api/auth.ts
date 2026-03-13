import client from './client'

export interface LoginRequest {
  username: string
  password: string
}

export interface LoginResponse {
  token: string
  refreshToken: string
  expiresAt: string
  user: {
    id: number
    username: string
    email: string
    role: string
    department: string
  }
}

export const authApi = {
  login: (data: LoginRequest) => client.post<LoginResponse>('/auth/login', data),
  refresh: (refreshToken: string) =>
    client.post<LoginResponse>('/auth/refresh', JSON.stringify(refreshToken), {
      headers: { 'Content-Type': 'application/json' },
      // Skip the response interceptor's auto-refresh for this call
      _skipRefresh: true,
    } as never),
}
