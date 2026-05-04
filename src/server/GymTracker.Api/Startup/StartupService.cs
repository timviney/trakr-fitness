using GymTracker.Api.Endpoints.Responses.Results;
using GymTracker.Api.Endpoints.Responses.Structure;
using GymTracker.Infrastructure.Data;

namespace GymTracker.Api.Startup;

public class StartupService(GymTrackerDbContext db) : IStartupService
{
    public async Task<ApiResponse<StartupResult>> PingAsync()
    {
        try
        {
            var canConnect = await db.Database.CanConnectAsync();
            return ApiResponse<StartupResult>.Success(new StartupResult(canConnect));
        }
        catch (Exception)
        {
            return ApiResponse<StartupResult>.Failure(ApiError.UnknownError);
        }
    }
}
