using GymTracker.Api.Endpoints.Responses;
using GymTracker.Api.Endpoints.Responses.Results;
using GymTracker.Api.Endpoints.Responses.Structure;

namespace GymTracker.Api.Auth;

public interface IAuthService
{
    /// <summary>
    /// Generates a JWT for the given email. Returns an AuthResponse containing the token and expiry.
    /// </summary>
    Task<ApiResponse<LoginResult>> Login(string email, string password);
    
    /// <summary>
    /// Registers a user with the given email and password. Returns a RegisterResult indicating success or failure.
    /// </summary>
    Task<ApiResponse<RegisterResult>> Register(string email, string password);
}
