namespace GymTracker.Api.Endpoints.Responses.Structure;

public enum ApiError
{
    UnknownError,
    InvalidCredentials,
    UserNotFound,
    EmailTaken,
    WeakPassword,
    InvalidEmail,
    NameAlreadyExists,
    NotFound,
}