import axios from 'axios'
import type { InternalAxiosRequestConfig } from 'axios'

const client = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
})

client.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// Track whether a refresh is already in flight to avoid concurrent refresh loops
let isRefreshing = false
let pendingQueue: Array<{ resolve: (token: string) => void; reject: (err: unknown) => void }> = []

function processPendingQueue(error: unknown, token: string | null) {
  pendingQueue.forEach(({ resolve, reject }) => {
    if (error) reject(error)
    else resolve(token!)
  })
  pendingQueue = []
}

client.interceptors.response.use(
  (res) => res,
  async (error) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean }
    const isRefreshRequest = originalRequest?.url?.includes('/auth/refresh')
    const isLoginRequest = originalRequest?.url?.includes('/auth/login')

    if (
      error.response?.status === 401 &&
      !originalRequest._retry &&
      !isRefreshRequest &&
      !isLoginRequest
    ) {
      const storedRefreshToken = localStorage.getItem('refreshToken')

      if (!storedRefreshToken) {
        // No refresh token — clear session and redirect
        localStorage.removeItem('token')
        localStorage.removeItem('refreshToken')
        localStorage.removeItem('user')
        window.location.href = '/login'
        return Promise.reject(error)
      }

      if (isRefreshing) {
        // Queue this request until the in-flight refresh completes
        return new Promise((resolve, reject) => {
          pendingQueue.push({ resolve, reject })
        }).then((newToken) => {
          originalRequest.headers.Authorization = `Bearer ${newToken}`
          return client(originalRequest)
        })
      }

      originalRequest._retry = true
      isRefreshing = true

      try {
        const response = await client.post<{
          token: string
          refreshToken: string
          user: { id: number; username: string; email: string; role: string; department: string }
        }>('/auth/refresh', JSON.stringify(storedRefreshToken), {
          headers: { 'Content-Type': 'application/json' },
        })

        const { token, refreshToken: newRefreshToken, user } = response.data

        // Persist updated tokens
        localStorage.setItem('token', token)
        localStorage.setItem('refreshToken', newRefreshToken)
        localStorage.setItem('user', JSON.stringify(user))

        // Update the Authorization header for future requests
        client.defaults.headers.common.Authorization = `Bearer ${token}`

        processPendingQueue(null, token)

        // Retry the original request with the new token
        originalRequest.headers.Authorization = `Bearer ${token}`
        return client(originalRequest)
      } catch (refreshError) {
        processPendingQueue(refreshError, null)

        // Refresh failed — clear session
        localStorage.removeItem('token')
        localStorage.removeItem('refreshToken')
        localStorage.removeItem('user')
        window.location.href = '/login'
        return Promise.reject(refreshError)
      } finally {
        isRefreshing = false
      }
    }

    return Promise.reject(error)
  },
)

export default client
