import { ApiResponse } from './api-response'
import { buildApiUrl } from './config'
import { ApiError } from './api-error'

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE'

type RequestOptions = {
    method?: HttpMethod
    body?: unknown
    headers?: Record<string, string>
}

// Function to get access token - will be called at request time
let getAccessToken: (() => string | null) | null = null

// Handler called when API returns 401 Unauthorized
let onAuthFailure: (() => void) | null = null

// Function to handle refresh token logic - will be called at request time
let refreshHandler: (() => Promise<boolean>) | null = null

let refreshPromise: Promise<boolean> | null = null

export function setAuthFailureHandler(handler: () => void) {
    onAuthFailure = handler
}

export function setAccessTokenGetter(getter: () => string | null) {
    getAccessToken = getter
}

export function setRefreshHandler(handler: () => Promise<boolean>) {
    refreshHandler = handler
}

export class ApiClient {
    private async request<T>(
        path: string,
        options: RequestOptions,
        allowRefresh = true
    ): Promise<ApiResponse<T>> {
        const headers: Record<string, string> = {
            'Content-Type': 'application/json',
            ...(options.headers ?? {})
        }

        if (getAccessToken) {
            const token = getAccessToken()
            if (token) {
                headers['Authorization'] = `Bearer ${token}`
            }
        }

        const response = await fetch(buildApiUrl(path), {
            method: options.method,
            headers,
            body: options.body ? JSON.stringify(options.body) : undefined
        })

        if (response.status === 401 && allowRefresh) {
            if (refreshPromise) {
                const refreshed = await refreshPromise
                if (refreshed) return await this.request<T>(path, options, false)
                return { isSuccess: false, error: ApiError.Unauthorized } as ApiResponse<T>
            }

            if (refreshHandler) {
                refreshPromise = refreshHandler()
                try {
                    const refreshed = await refreshPromise
                    if (refreshed) return await this.request<T>(path, options, false)
                } finally {
                    refreshPromise = null
                }
            }

            try {
                onAuthFailure?.()
            } catch (e) {
                console.error('auth failure handler error', e)
            }

            return { isSuccess: false, error: ApiError.Unauthorized } as ApiResponse<T>
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
