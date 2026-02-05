import { ApiClient } from '../client'

export type LoginRequest = {
  email: string
  password: string
}

export type LoginResponse = {
  token: string
  expiresAt: string
  userId: string
  email: string
  error?: 'InvalidCredentials' | 'UserNotFound' | 'UnknownError'
}

export type RegisterRequest = {
  email: string
  password: string
}

export type RegisterResponse = {
  success: boolean
  userId?: string
  error?: 'EmailTaken' | 'WeakPassword' | 'InvalidEmail' | 'UnknownError'
  errorMessage?: string
}

export class AuthApi {
  constructor(private client: ApiClient) {}

  login(payload: LoginRequest) {
    return this.client.post<LoginResponse>('/auth/login', payload)
  }

  register(payload: RegisterRequest) {
    return this.client.post<RegisterResponse>('/auth/register', payload)
  }
}