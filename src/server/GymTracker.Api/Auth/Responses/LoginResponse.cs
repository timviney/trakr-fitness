namespace GymTracker.Api.Auth.Responses;

public record LoginResponse(string Token, DateTime ExpiresAt, string UserId, string? Role = null);
