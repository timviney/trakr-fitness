using GymTracker.Api.Endpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

// TODO: connect DB 

// var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
// builder.Services.AddDbContext<GymDbContext>(options =>
//     options.UseNpgsql(connectionString));

// TODO: add auth service
// builder.Services.AddScoped<IAuthService, AuthService>();

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
