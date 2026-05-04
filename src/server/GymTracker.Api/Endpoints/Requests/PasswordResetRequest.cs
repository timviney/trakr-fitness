namespace GymTracker.Api.Endpoints.Requests;

public record PasswordResetRequest(string Email, string OldPassword, string NewPassword);