using GymTracker.Api.Endpoints;
using GymTracker.Api.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using GymTracker.Infrastructure.Data;
using GymTracker.Infrastructure.Repositories;
using GymTracker.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

// Database configuration: use in-memory by default in Development, otherwise look for DB_CONNECTION_STRING
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? builder.Configuration.GetConnectionString("Default");

if (builder.Environment.IsDevelopment() || string.IsNullOrWhiteSpace(connectionString))
{
    // Use in-memory for local development or if no connection string provided
    builder.Services.AddDbContext<GymTrackerDbContext>(options =>
        options.UseInMemoryDatabase("GymTracker_Local"));
}
else
{
    // Use Postgres when a connection string is available
    builder.Services.AddDbContext<GymTrackerDbContext>(options =>
        options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.MigrationsAssembly("GymTracker.Infrastructure")));
}

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Bind JwtSettings and register AuthService
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<IAuthService, AuthService>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "GymTracker API",
            Version = "v1"
        });
    });
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Enable Swagger only locally
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GymTracker API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.MapAuthEndpoints();

app.Run();
