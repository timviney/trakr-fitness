using GymTracker.Api.Endpoints.Responses;
using GymTracker.Api.Endpoints.Responses.Results;
using GymTracker.Api.Endpoints.Responses.Structure;

namespace GymTracker.Api.Auth;

public interface IAuthService
{
    /// <summary>
    /// Generates a JWT for the given Email. Returns an AuthResponse containing the token and expiry.
    /// </summary>
    Task<ApiResponse<LoginResult>> Login(string email, string password);
    
    /// <summary>
    /// Registers a user with the given Email and password. Returns a RegisterResult indicating success or failure.
    /// </summary>
    Task<ApiResponse<RegisterResult>> Register(string email, string password);

    Task<ApiResponse<ResetPasswordResult>> ResetPassword(string email, string oldPassword,
        string newPassword);

    /// <summary>
    /// Refreshes the user's refresh token.
    /// </summary>
    Task<ApiResponse<LoginResult>> RefreshToken(string refreshToken);
}
