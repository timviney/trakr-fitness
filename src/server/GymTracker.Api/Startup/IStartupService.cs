using GymTracker.Api.Endpoints.Responses.Results;
using GymTracker.Api.Endpoints.Responses.Structure;

namespace GymTracker.Api.Startup;

public interface IStartupService
{
    Task<ApiResponse<StartupResult>> PingAsync();
}
