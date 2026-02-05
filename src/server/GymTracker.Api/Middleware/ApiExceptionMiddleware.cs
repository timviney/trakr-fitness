using GymTracker.Api.Endpoints.Responses.Structure;

namespace GymTracker.Api.Middleware;

public sealed class ApiExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var response = ApiResponse.Failure(ApiError.UnknownError);
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
