using GymTracker.Api.Auth;
using GymTracker.Api.Endpoints.Requests;
using GymTracker.Api.Endpoints.Responses.Structure;
using GymTracker.Core.Entities;
using GymTracker.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Endpoints;

public static class WorkoutEndpoints
{
    public static void MapWorkoutEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/workouts")
            .WithTags("Workouts")
            .RequireAuthorization();

        group.MapGet("/", GetUserWorkouts);
        group.MapPost("/", CreateWorkout);
        group.MapPut("/{id}", UpdateWorkout);
    }

    private static async Task<IResult> GetUserWorkouts(
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository repository)
    {
        var result = await repository.GetWorkoutsByUserIdAsync(authContext.UserId);
        return result.ToApiResult().ToOkResult();
    }

    private static async Task<IResult> CreateWorkout(
        CreateWorkoutRequest req,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository repository)
    {
        var workout = new Workout
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            UserId = authContext.UserId
        };

        var result = await repository.AddWorkoutAsync(workout);
        return result.ToApiResult().ToCreatedResult($"/workouts/{workout.Id}");
    }

    private static async Task<IResult> UpdateWorkout(
        Guid id,
        UpdateWorkoutRequest req,
        [FromServices] IExerciseLibraryRepository repository)
    {
        var result = await repository.GetWorkoutByIdAsync(id);
        if (!result.IsSuccess)
            return Results.NotFound();

        var workout = result.Data!;
        workout.Name = req.Name;
        var updateResult = await repository.UpdateWorkoutAsync(workout);
        return updateResult.ToApiResult().ToOkResult();
    }
}
