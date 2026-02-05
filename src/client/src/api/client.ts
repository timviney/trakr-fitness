import { buildApiUrl } from './config'

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE'

type RequestOptions = {
  method?: HttpMethod
  body?: unknown
  headers?: Record<string, string>
}

// Function to get auth token - will be called at request time
let getAuthToken: (() => string | null) | null = null

export function setAuthTokenGetter(getter: () => string | null) {
  getAuthToken = getter
}

export class ApiClient {
  private async request<T>(path: string, options: RequestOptions): Promise<T> {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      ...(options.headers ?? {})
    }

    // Auto-inject Authorization header if token exists
    if (getAuthToken) {
      const token = getAuthToken()
      if (token) {
        headers['Authorization'] = `Bearer ${token}`
      }
    }

    const response = await fetch(buildApiUrl(path), {
      method: options.method,
      headers,
      body: options.body ? JSON.stringify(options.body) : undefined
    })

    if (!response.ok) {
      const message = await response.text()
      throw new Error(message || 'Request failed.')
    }

    if (response.status === 204) {
      return undefined as T
    }

    return (await response.json()) as T
  }

  get<T>(path: string, headers?: Record<string, string>) {
    return this.request<T>(path, { method: 'GET', headers })
  }

  post<T>(path: string, body?: unknown, headers?: Record<string, string>) {
    return this.request<T>(path, { method: 'POST', body, headers })
  }

  put<T>(path: string, body?: unknown, headers?: Record<string, string>) {
    return this.request<T>(path, { method: 'PUT', body, headers })
  }

  patch<T>(path: string, body?: unknown, headers?: Record<string, string>) {
    return this.request<T>(path, { method: 'PATCH', body, headers })
  }

  delete<T>(path: string, headers?: Record<string, string>) {
    return this.request<T>(path, { method: 'DELETE', headers })
  }
}
