import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api } from '../api/api'
import { buildApiUrl } from '../api/config'
import type { LoginRequest, LoginResult } from '../api/modules/auth'
import type { ApiResponse } from '../api/api-response'

const TOKEN_KEY = 'auth_token'
const USER_ID_KEY = 'auth_user_id'
const EMAIL_KEY = 'auth_email'
const EXPIRES_AT_KEY = 'auth_expires_at'
const REFRESH_TOKEN_KEY = 'auth_refresh_token'
const REFRESH_EXPIRES_AT_KEY = 'auth_refresh_expires_at'
const LOGOUT_REASON_KEY = 'auth_logout_reason'

export const useAuthStore = defineStore('auth', () => {
  // State
  const token = ref<string | null>(null)
  const userId = ref<string | null>(null)
  const email = ref<string | null>(null)
  const expiresAt = ref<string | null>(null)
  const refreshToken = ref<string | null>(null)
  const refreshTokenExpiresAt = ref<string | null>(null)
  const logoutReason = ref<string | null>(null)

  // Getters
  const isAuthenticated = computed(() => {
    if (!token.value || !expiresAt.value) return false
    return !isTokenExpired.value
  })

  const isTokenExpired = computed(() => {
    if (!expiresAt.value) return true
    return new Date(expiresAt.value) <= new Date()
  })

  const isRefreshTokenExpired = computed(() => {
    if (!refreshTokenExpiresAt.value) return true
    return new Date(refreshTokenExpiresAt.value) <= new Date()
  })
  
  // Actions
  const initialize = () => {
    token.value = localStorage.getItem(TOKEN_KEY)
    userId.value = localStorage.getItem(USER_ID_KEY)
    email.value = localStorage.getItem(EMAIL_KEY)
    expiresAt.value = localStorage.getItem(EXPIRES_AT_KEY)
    refreshToken.value = localStorage.getItem(REFRESH_TOKEN_KEY)
    refreshTokenExpiresAt.value = localStorage.getItem(REFRESH_EXPIRES_AT_KEY)

    const storedLogoutReason = localStorage.getItem(LOGOUT_REASON_KEY)
    if (storedLogoutReason) logoutReason.value = storedLogoutReason

    if (isTokenExpired.value && isRefreshTokenExpired.value) {
      logout()
    }
  }

  const persistTokens = (data: LoginResult) => {
    token.value = data.token
    userId.value = data.userId
    email.value = data.email
    expiresAt.value = data.expiresAt
    refreshToken.value = data.refreshToken
    refreshTokenExpiresAt.value = data.refreshTokenExpiresAt

    // Persist to localStorage
    localStorage.setItem(TOKEN_KEY, data.token)
    localStorage.setItem(USER_ID_KEY, data.userId)
    localStorage.setItem(EMAIL_KEY, data.email)
    localStorage.setItem(EXPIRES_AT_KEY, data.expiresAt)
    localStorage.setItem(REFRESH_TOKEN_KEY, data.refreshToken)
    localStorage.setItem(REFRESH_EXPIRES_AT_KEY, data.refreshTokenExpiresAt)
  }

  const login = async (credentials: LoginRequest): Promise<ApiResponse<LoginResult>> => {
    const response = await api.auth.login(credentials)

    if (!response.isSuccess) {
      console.log('Login failed:', response.error)
      return response
    }

    persistTokens(response.data!)
    return response
  }

  const refreshAuth = async (): Promise<boolean> => {
    const rt = refreshToken.value
    if (!rt || isRefreshTokenExpired.value) return false

    try {
      const response = await fetch(buildApiUrl('/auth/refresh'), {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken: rt })
      })

      if (!response.ok) return false

      const result: ApiResponse<LoginResult> = await response.json()
      if (!result.isSuccess || !result.data) return false

      persistTokens(result.data)
      return true
    } catch {
      return false
    }
  }

  const logout = () => {
    token.value = null
    userId.value = null
    email.value = null
    expiresAt.value = null
    refreshToken.value = null
    refreshTokenExpiresAt.value = null

    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_ID_KEY)
    localStorage.removeItem(EMAIL_KEY)
    localStorage.removeItem(EXPIRES_AT_KEY)
    localStorage.removeItem(REFRESH_TOKEN_KEY)
    localStorage.removeItem(REFRESH_EXPIRES_AT_KEY)
    localStorage.removeItem(LOGOUT_REASON_KEY)
    logoutReason.value = null
  }

  const forceLogout = (reason?: string) => {
    // perform normal logout then persist the reason so UI can show a message
    logout()
    if (reason) {
      logoutReason.value = reason
      localStorage.setItem(LOGOUT_REASON_KEY, reason)
    }
  }

  return {
    // State
    token,
    userId,
    email,
    expiresAt,
    refreshToken,
    refreshTokenExpiresAt,
    // Getters
    isAuthenticated,
    isTokenExpired,
    isRefreshTokenExpired,
    // Actions
    initialise: initialize,
    login,
    refreshAuth,
    logout,
    forceLogout,
    // Info
    logoutReason
  }
})
