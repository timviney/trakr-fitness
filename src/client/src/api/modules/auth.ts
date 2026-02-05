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
  constructor(private client: ApiClient) {}

  login(payload: LoginRequest) {
    return this.client.post<ApiResponse<LoginResult>>('/auth/login', payload)
  }

  register(payload: RegisterRequest) {
    return this.client.post<ApiResponse<RegisterResult>>('/auth/register', payload)
  }
}