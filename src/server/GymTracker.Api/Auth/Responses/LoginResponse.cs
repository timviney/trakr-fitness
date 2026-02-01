namespace GymTracker.Api.Auth.Responses;

public record LoginResponse(string Token, DateTime ExpiresAt, string UserId, LoginError? Error = null);

public enum LoginError
{
    InvalidCredentials,
    UserNotFound,
    UnknownError
}