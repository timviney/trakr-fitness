using GymTracker.Api.Endpoints.Requests;
using GymTracker.Api.Endpoints.Responses.Structure;
using GymTracker.Api.Startup;

namespace GymTracker.Api.Endpoints;

public static class StartupEndpoints
{
    public static void MapStartupEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/startup")
            .WithTags("Startup");

        group.MapPost("/", async (StartupRequest req, IStartupService startupService) =>
        {
            var resp = await startupService.PingAsync();

            return resp.ToOkResult();
        });
    }
}
