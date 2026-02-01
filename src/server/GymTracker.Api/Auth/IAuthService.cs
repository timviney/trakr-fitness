namespace GymTracker.Api.Auth;

public interface IAuthService
{
    /// <summary>
    /// Generates a JWT for the given username. Returns an AuthResponse containing the token and expiry.
    /// </summary>
    Task<AuthResponse> GenerateTokenAsync(string username);
}
