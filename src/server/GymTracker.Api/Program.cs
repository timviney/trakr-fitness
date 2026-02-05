using GymTracker.Api.Endpoints;
using GymTracker.Api.Setup;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

DbSetup.SetupDb(builder);

AuthSetup.ConfigureAuth(builder);

var runSwagger = builder.Configuration.GetValue<bool>("RunSwagger");

if (runSwagger) SwaggerSetup.AddSwaggerServices(builder);

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        else if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

var app = builder.Build();

// Seed default data if enabled
await DbSetup.SeedDataAsync(app);

if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();

if (runSwagger) SwaggerSetup.UseSwagger(app);

app.UseCors("DefaultCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapExerciseEndpoints();
app.MapWorkoutEndpoints();
app.MapMuscleEndpoints();

app.Run();