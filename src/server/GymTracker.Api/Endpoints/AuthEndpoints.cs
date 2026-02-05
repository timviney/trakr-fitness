using GymTracker.Api.Auth;
using GymTracker.Api.Endpoints.Requests;
using GymTracker.Api.Endpoints.Responses;
using GymTracker.Api.Endpoints.Responses.Structure;

namespace GymTracker.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth").WithTags("Authentication");
        
        group.MapPost("/login", async (LoginRequest req, IAuthService authService) =>
        {
            var resp = await authService.Login(req.Email, req.Password);

            return resp.ToOkResult();
        });

        group.MapPost("/register", async (RegisterRequest req, IAuthService authService) =>
        {
            var resp = await authService.Register(req.Email, req.Password);

            return resp.ToOkResult();
        });
    }
}