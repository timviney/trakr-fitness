import { ApiResponse } from './api-response'
import { buildApiUrl } from './config'
import { ApiError } from './api-error'

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE'

type RequestOptions = {
    method?: HttpMethod
    body?: unknown
    headers?: Record<string, string>
}

// Function to get auth token - will be called at request time
let getAuthToken: (() => string | null) | null = null

// Handler called when API returns 401 Unauthorized
let onAuthFailure: (() => void) | null = null

export function setAuthFailureHandler(handler: () => void) {
    onAuthFailure = handler
}

export function setAuthTokenGetter(getter: () => string | null) {
    getAuthToken = getter
}

export class ApiClient {
    private async request<T>(path: string, options: RequestOptions): Promise<ApiResponse<T>> {
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

        // If unauthorized, notify the app so it can force logout
        if (response.status === 401) {
            try {
                onAuthFailure?.()
            } catch (e) {
                console.error('auth failure handler error', e)
            }
            finally {
                return { isSuccess: false, error: ApiError.Unauthorized } as ApiResponse<T>
            }
        }

        try {
            return (await response.json()) as ApiResponse<T>
        } catch (e) {
            console.error('api error', e)
            return { isSuccess: false, error: ApiError.UnknownError } as ApiResponse<T>
        }
    }

    async get<T>(path: string, headers?: Record<string, string>): Promise<ApiResponse<T>> {
        return await this.request<T>(path, { method: 'GET', headers })
    }

    async post<T>(path: string, body?: unknown, headers?: Record<string, string>): Promise<ApiResponse<T>> {
        return await this.request<T>(path, { method: 'POST', body, headers })
    }

    async put<T>(path: string, body?: unknown, headers?: Record<string, string>): Promise<ApiResponse<T>> {
        return await this.request<T>(path, { method: 'PUT', body, headers })
    }

    async patch<T>(path: string, body?: unknown, headers?: Record<string, string>): Promise<ApiResponse<T>> {
        return await this.request<T>(path, { method: 'PATCH', body, headers })
    }

    async delete<T>(path: string, headers?: Record<string, string>): Promise<ApiResponse<T>> {
        return await this.request<T>(path, { method: 'DELETE', headers })
    }
}
