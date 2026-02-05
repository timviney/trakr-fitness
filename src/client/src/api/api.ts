import { ApiClient } from './client'
import { AuthApi } from './modules/auth'

class Api {
  readonly auth: AuthApi

  constructor(client: ApiClient) {
    this.auth = new AuthApi(client)
  }
}

export const api = new Api(new ApiClient())
