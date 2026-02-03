import { ApiClient } from '../client'

export type LoginRequest = {
  email: string
  password: string
}

export type LoginResponse = {
  token: string
}

export class AuthApi {
  constructor(private client: ApiClient) {}

  login(payload: LoginRequest) {
    return this.client.post<LoginResponse>('/auth/login', payload)
  }
}