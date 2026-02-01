using GymTracker.Api.Auth.Responses;

namespace GymTracker.Api.Auth;

public interface IAuthService
{
    /// <summary>
    /// Generates a JWT for the given username. Returns an AuthResponse containing the token and expiry.
    /// </summary>
    Task<LoginResponse> Login(string username, string password);
    
    /// <summary>
    /// Registers a user with the given username and password. Returns a RegisterResponse indicating success or failure.
    /// </summary>
    Task<RegisterResponse> Register(string username, string password);
}
