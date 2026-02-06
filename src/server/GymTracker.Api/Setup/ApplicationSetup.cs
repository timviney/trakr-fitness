using GymTracker.Application.Services;
using GymTracker.Core.Interfaces;

namespace GymTracker.Api.Setup;

public static class ApplicationSetup
{
    public static void ConfigureApplication(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUserRegistrationService, UserRegistrationService>();
    }
}