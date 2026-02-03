import { buildApiUrl } from './config'

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE'

type RequestOptions = {
  method?: HttpMethod
  body?: unknown
  headers?: Record<string, string>
}

export class ApiClient {
  private async request<T>(path: string, options: RequestOptions): Promise<T> {
    const response = await fetch(buildApiUrl(path), {
      method: options.method,
      headers: {
        'Content-Type': 'application/json',
        ...(options.headers ?? {})
      },
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
