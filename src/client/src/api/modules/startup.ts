import { ApiResponse } from '../api-response'
import { ApiClient } from '../client'

export type StartupRequest = {
}

export type StartupResult = {
  isDatabaseLive: boolean
}

export class StartupApi {
  constructor(private client: ApiClient) { }

  async startup(payload: StartupRequest): Promise<ApiResponse<StartupResult>> {
    return await this.client.post<StartupResult>('/startup', payload)
  }
}