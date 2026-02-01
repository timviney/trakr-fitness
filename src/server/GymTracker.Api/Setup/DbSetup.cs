using GymTracker.Core.Interfaces;
using GymTracker.Infrastructure.Data;
using GymTracker.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Api.Setup;

public static class DbSetup
{
    public static void SetupDb(WebApplicationBuilder webApplicationBuilder)
    {
        // Database configuration: use in-memory by default in Development, otherwise look for DB_CONNECTION_STRING
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ??
                               webApplicationBuilder.Configuration.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Use in-memory for local development or if no connection string provided
            webApplicationBuilder.Services.AddDbContext<GymTrackerDbContext>(options =>
                options.UseInMemoryDatabase("GymTracker_Local"));
        }
        else
        {
            // Use Postgres when a connection string is available
            webApplicationBuilder.Services.AddDbContext<GymTrackerDbContext>(options =>
                options.UseNpgsql(connectionString,
                    npgsqlOptions => npgsqlOptions.MigrationsAssembly("GymTracker.Infrastructure")));
        }

        // Register repositories
        webApplicationBuilder.Services.AddScoped<IUserRepository, UserRepository>();
    }
}