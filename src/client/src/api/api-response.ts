import { ApiError } from "./api-error";

export type ApiResponse<TData> = {
    isSuccess: boolean
    data?: TData
    error?: ApiError
}