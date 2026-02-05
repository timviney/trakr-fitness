import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api } from '../api/api'
import type { LoginRequest } from '../api/modules/auth'

const TOKEN_KEY = 'auth_token'
const USER_ID_KEY = 'auth_user_id'
const EMAIL_KEY = 'auth_email'
const EXPIRES_AT_KEY = 'auth_expires_at'

export const useAuthStore = defineStore('auth', () => {
  // State
  const token = ref<string | null>(null)
  const userId = ref<string | null>(null)
  const email = ref<string | null>(null)
  const expiresAt = ref<string | null>(null)

  // Getters
  const isAuthenticated = computed(() => {
    if (!token.value || !expiresAt.value) return false
    return !isTokenExpired.value
  })

  const isTokenExpired = computed(() => {
    if (!expiresAt.value) return true
    return new Date(expiresAt.value) <= new Date()
  })

  // Actions
  const initialize = () => {
    const storedToken = localStorage.getItem(TOKEN_KEY)
    const storedUserId = localStorage.getItem(USER_ID_KEY)
    const storedEmail = localStorage.getItem(EMAIL_KEY)
    const storedExpiresAt = localStorage.getItem(EXPIRES_AT_KEY)

    if (storedToken && storedUserId && storedEmail && storedExpiresAt) {
      token.value = storedToken
      userId.value = storedUserId
      email.value = storedEmail
      expiresAt.value = storedExpiresAt

      // Clear if expired
      if (isTokenExpired.value) {
        logout()
      }
    }
  }

  const login = async (credentials: LoginRequest) => {
    const response = await api.auth.login(credentials)
    
    token.value = response.token
    userId.value = response.userId
    email.value = response.email
    expiresAt.value = response.expiresAt

    // Persist to localStorage
    localStorage.setItem(TOKEN_KEY, response.token)
    localStorage.setItem(USER_ID_KEY, response.userId)
    localStorage.setItem(EMAIL_KEY, response.email)
    localStorage.setItem(EXPIRES_AT_KEY, response.expiresAt)
  }

  const logout = () => {
    token.value = null
    userId.value = null
    email.value = null
    expiresAt.value = null

    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_ID_KEY)
    localStorage.removeItem(EMAIL_KEY)
    localStorage.removeItem(EXPIRES_AT_KEY)
  }

  return {
    // State
    token,
    userId,
    email,
    expiresAt,
    // Getters
    isAuthenticated,
    isTokenExpired,
    // Actions
    initialize,
    login,
    logout
  }
})
