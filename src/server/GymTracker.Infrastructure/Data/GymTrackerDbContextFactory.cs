using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GymTracker.Infrastructure.Data;

public class GymTrackerDbContextFactory : IDesignTimeDbContextFactory<GymTrackerDbContext>
{
    public GymTrackerDbContext CreateDbContext(string[] args)
    {
        // Navigate from Infrastructure project to API project where appsettings.json is located
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "GymTracker.Api");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = BuildConnectionString(configuration);

        var optionsBuilder = new DbContextOptionsBuilder<GymTrackerDbContext>();
        optionsBuilder.UseNpgsql(connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsAssembly("GymTracker.Infrastructure"))
            .UseSnakeCaseNamingConvention();

        return new GymTrackerDbContext(optionsBuilder.Options);
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

        // If any required setting is missing, throw exception
        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(database) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new Exception("Missing postgres configuration. Ensure PostgresSettings are configured in appsettings.json and TRAKR_POSTGRES_PASSWORD environment variable is set.");
        }

        return $"Host={host};Port={port ?? "5432"};Database={database};Username={username};Password={password}";
    }
}
