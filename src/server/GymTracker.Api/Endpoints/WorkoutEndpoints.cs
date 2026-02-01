using GymTracker.Api.Auth;
using GymTracker.Api.Endpoints.Requests;
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
        [FromServices] IExerciseLibraryRepository repository) =>
        Results.Ok(await repository.GetWorkoutsByUserIdAsync(authContext.UserId));

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

        await repository.AddWorkoutAsync(workout);
        return Results.Created($"/workouts/{workout.Id}", workout);
    }

    private static async Task<IResult> UpdateWorkout(
        Guid id,
        UpdateWorkoutRequest req,
        [FromServices] IExerciseLibraryRepository repository)
    {
        var workout = await repository.GetWorkoutByIdAsync(id);
        if (workout == null)
            return Results.NotFound();

        workout.Name = req.Name;
        await repository.UpdateWorkoutAsync(workout);
        return Results.Ok();
    }
}
