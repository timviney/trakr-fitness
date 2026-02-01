using GymTracker.Api.Auth;

namespace GymTracker.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth").WithTags("Authentication");
        
        group.MapPost("/login", async (LoginRequest req, IAuthService authService) =>
        {
            // TODO: credentials validation
            var resp = await authService.GenerateTokenAsync(req.Username);
            return Results.Ok(resp);
        });
    }
}