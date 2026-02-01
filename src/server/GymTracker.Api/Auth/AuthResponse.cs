namespace GymTracker.Api.Auth;

public record AuthResponse(string Token, DateTime ExpiresAt, string UserId, string? Role = null);
