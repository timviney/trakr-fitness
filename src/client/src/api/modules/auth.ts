import { ApiResponse } from '../api-response'
import { ApiClient } from '../client'

export type LoginRequest = {
  email: string
  password: string
}

export type LoginResult = {
  token: string
  expiresAt: string
  userId: string
  email: string
}

export type RegisterRequest = {
  email: string
  password: string
}

export type RegisterResult = {
  userId: string
}

export class AuthApi {
  constructor(private client: ApiClient) { }

  async login(payload: LoginRequest): Promise<ApiResponse<LoginResult>> {
    return await this.client.post<LoginResult>('/auth/login', payload)
  }

  async register(payload: RegisterRequest): Promise<ApiResponse<RegisterResult>> {
    return await this.client.post<RegisterResult>('/auth/register', payload)
  }
}