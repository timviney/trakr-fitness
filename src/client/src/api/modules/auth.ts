import { ApiResponse } from '../api-response'
import { ApiClient } from '../client'

export type LoginRequest = {
  email: string
  password: string
}

export type RefreshRequest = {
  refreshToken: string
}

export type LoginResult = {
  token: string
  expiresAt: string
  userId: string
  email: string
  refreshToken: string
  refreshTokenExpiresAt: string
}

export type RegisterRequest = {
  email: string
  password: string
}

export type RegisterResult = {
  userId: string
}

export type ResetPasswordRequest = {
  email: string
  oldPassword: string
  newPassword: string
}

export class AuthApi {
  constructor(private client: ApiClient) { }

  async login(payload: LoginRequest): Promise<ApiResponse<LoginResult>> {
    return await this.client.post<LoginResult>('/auth/login', payload)
  }

  async refresh(payload: RefreshRequest): Promise<ApiResponse<LoginResult>> {
    return await this.client.post<LoginResult>('/auth/refresh', payload)
  }

  async register(payload: RegisterRequest): Promise<ApiResponse<RegisterResult>> {
    return await this.client.post<RegisterResult>('/auth/register', payload)
  }

  async resetPassword(payload: ResetPasswordRequest): Promise<ApiResponse<void>> {
    return await this.client.post<void>('/auth/reset', payload)
  }
}