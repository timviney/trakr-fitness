using GymTracker.Core.Interfaces;
using GymTracker.Infrastructure.Data;
using GymTracker.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Api.Setup;

public static class DbSetup
{
    public static void SetupDb(WebApplicationBuilder webApplicationBuilder)
    {
        var connectionString = BuildConnectionString(webApplicationBuilder.Configuration);

        // Use Postgres when a connection string is available
        webApplicationBuilder.Services.AddDbContext<GymTrackerDbContext>(options =>
            options.UseNpgsql(connectionString,
                    npgsqlOptions => npgsqlOptions.MigrationsAssembly("GymTracker.Infrastructure"))
                .UseSnakeCaseNamingConvention());

        // Register repositories
        webApplicationBuilder.Services.AddScoped<IUserRepository, UserRepository>();
        webApplicationBuilder.Services.AddScoped<IExerciseLibraryRepository, ExerciseLibraryRepository>();
        webApplicationBuilder.Services.AddScoped<ISessionRepository, SessionRepository>();
    }

    private static string BuildConnectionString(IConfiguration configuration)
    {
        switch (configuration["PostgresSettings:Env"])
        {
            case "Production":
                var connectionString = Environment.GetEnvironmentVariable("TRAKR_DB_CONNECTION_STRING_PROD");
                return !string.IsNullOrWhiteSpace(connectionString) 
                    ? connectionString 
                    : throw new Exception("Environment variable 'TRAKR_DB_CONNECTION_STRING_PROD' is not set.");
            case "Development":
                var connectionStringDev = Environment.GetEnvironmentVariable("TRAKR_DB_CONNECTION_STRING_DEV");
                return !string.IsNullOrWhiteSpace(connectionStringDev) 
                    ? connectionStringDev 
                    : throw new Exception("Environment variable 'TRAKR_DB_CONNECTION_STRING_DEV' is not set.");
            default:
                throw new Exception("configuration[\"PostgresSettings:Env\"]"+$" must be set to either 'Production' or 'Development'. Current value: '{configuration["PostgresSettings:Env"]}'");
        }
    }

    public static async Task SeedDataAsync(WebApplication app)
    {
        // Apply pending migrations only in development (not in Lambda/production)
        if (!app.Environment.IsDevelopment()) return;

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GymTrackerDbContext>();
        await context.Database.MigrateAsync();
    }
}