using GymTracker.Api.Startup;

namespace GymTracker.Api.Setup;

public static class StartupSetup
{
    public static void ConfigureStartup(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IStartupService, StartupService>();
    }
}
