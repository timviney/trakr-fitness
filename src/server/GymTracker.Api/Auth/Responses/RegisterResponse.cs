namespace GymTracker.Api.Auth.Responses;

public record RegisterResponse(bool Success, Guid? UserId = null, RegisterError? Error = null, string? ErrorMessage = null);

public enum RegisterError
{
    EmailTaken,
    WeakPassword,
    InvalidEmail,
    UnknownError
}