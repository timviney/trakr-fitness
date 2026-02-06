export enum ApiError {
    UnknownError = 'UnknownError',
    InvalidCredentials = 'InvalidCredentials',
    UserNotFound = 'UserNotFound',
    EmailTaken = 'EmailTaken',
    WeakPassword = 'WeakPassword',
    InvalidEmail = 'InvalidEmail',
    NameAlreadyExists = 'NameAlreadyExists',
    NotFound = 'NotFound',
}

export const ApiErrorMessages: Record<ApiError, string> = {
    [ApiError.UnknownError]: 'An unknown error occurred',
    [ApiError.InvalidCredentials]: 'Invalid credentials',
    [ApiError.UserNotFound]: 'User not found',
    [ApiError.EmailTaken]: 'Email is already taken',
    [ApiError.WeakPassword]: 'Password is too weak',
    [ApiError.InvalidEmail]: 'Invalid email address',
    [ApiError.NameAlreadyExists]: 'Name already exists',
    [ApiError.NotFound]: 'Not found',
}