using Microsoft.OpenApi.Models;

namespace GymTracker.Api.Setup;

public static class SwaggerSetup
{
    public static void AddSwaggerServices(WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.Services.AddEndpointsApiExplorer();
        webApplicationBuilder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "GymTracker API",
                Version = "v1"
            });
        });
    }

    public static void UseSwagger(WebApplication webApplication)
    {
        webApplication.UseSwagger();
        webApplication.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "GymTracker API V1");
            c.RoutePrefix = string.Empty;
        });
    }
}