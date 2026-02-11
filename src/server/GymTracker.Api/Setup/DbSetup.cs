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
        // First check for full connection string in environment or config
        var connectionString = Environment.GetEnvironmentVariable("TRACKR_DB_CONNECTION_STRING");

        if (!string.IsNullOrWhiteSpace(connectionString))
            return connectionString;

        // Build connection string from PostgresSettings
        var host = configuration["PostgresSettings:Host"];
        var port = configuration["PostgresSettings:Port"];
        var database = configuration["PostgresSettings:Database"];
        var username = configuration["PostgresSettings:Username"];
        var password = Environment.GetEnvironmentVariable("TRAKR_POSTGRES_PASSWORD");

        // If any required setting is missing, return null to fall back to in-memory
        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(database) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new Exception("Missing postgres configuration");
        }

        return $"Host={host};Port={port ?? "5432"};Database={database};Username={username};Password={password}";
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