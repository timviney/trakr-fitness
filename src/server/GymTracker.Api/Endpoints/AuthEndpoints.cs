using System.Diagnostics;
using GymTracker.Api.Auth;
using GymTracker.Api.Auth.Requests;
using GymTracker.Api.Auth.Responses;

namespace GymTracker.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth").WithTags("Authentication");
        
        group.MapPost("/login", async (LoginRequest req, IAuthService authService) =>
        {
            var resp = await authService.Login(req.Username, req.Password);
            return Results.Ok(resp);
        });

        group.MapPost("/register", async (RegisterRequest req, IAuthService authService) =>
        {
            var resp = await authService.Register(req.Username, req.Password);
            return (resp) switch
            {
                { Success: true } => Results.Ok(resp),
                { Error: RegisterError.UsernameTaken } => Results.Conflict(resp),
                _ => Results.BadRequest(resp)
            };
        });
    }
}