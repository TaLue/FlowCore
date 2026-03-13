import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi, type LoginRequest } from '@/api/auth'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('token'))
  const refreshToken = ref<string | null>(localStorage.getItem('refreshToken'))
  const user = ref<{ id: number; username: string; email: string; role: string; department: string } | null>(
    JSON.parse(localStorage.getItem('user') || 'null'),
  )

  const isAuthenticated = computed(() => !!token.value)
  const isAdmin = computed(() => user.value?.role === 'Admin')

  async function login(credentials: LoginRequest) {
    const res = await authApi.login(credentials)
    token.value = res.data.token
    refreshToken.value = res.data.refreshToken
    user.value = res.data.user
    localStorage.setItem('token', res.data.token)
    localStorage.setItem('refreshToken', res.data.refreshToken)
    localStorage.setItem('user', JSON.stringify(res.data.user))
  }

  function logout() {
    token.value = null
    refreshToken.value = null
    user.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('user')
  }

  return { token, refreshToken, user, isAuthenticated, isAdmin, login, logout }
})
