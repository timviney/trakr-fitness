using GymTracker.Api.Auth.Responses;

namespace GymTracker.Api.Auth;

public interface IAuthService
{
    /// <summary>
    /// Generates a JWT for the given email. Returns an AuthResponse containing the token and expiry.
    /// </summary>
    Task<LoginResponse> Login(string email, string password);
    
    /// <summary>
    /// Registers a user with the given email and password. Returns a RegisterResponse indicating success or failure.
    /// </summary>
    Task<RegisterResponse> Register(string email, string password);
}
