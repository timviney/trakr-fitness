using GymTracker.Api.Endpoints;
using GymTracker.Api.Setup;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

DbSetup.SetupDb(builder);

AuthSetup.ConfigureAuth(builder);

var runSwagger = builder.Configuration.GetValue<bool>("RunSwagger");

if (runSwagger) SwaggerSetup.AddSwaggerServices(builder);

var app = builder.Build();

if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();

if (runSwagger) SwaggerSetup.UseSwagger(app);

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapExerciseEndpoints();
app.MapWorkoutEndpoints();
app.MapMuscleEndpoints();

app.Run();