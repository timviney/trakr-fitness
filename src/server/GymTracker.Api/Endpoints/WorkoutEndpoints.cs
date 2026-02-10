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
        group.MapGet("/{id}", GetWorkoutById);
        group.MapPost("/", CreateWorkout);
        group.MapPut("/{id}", UpdateWorkout);
        group.MapDelete("/{id}", DeleteWorkout);
    }

    private static async Task<IResult> GetUserWorkouts(
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository repository)
    {
        var result = await repository.GetWorkoutsByUserIdAsync(authContext.UserId);
        return result.ToApiResult().ToOkResult();
    }

    private static async Task<IResult> GetWorkoutById(
        Guid id,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository repository)
    {
        var result = await repository.GetWorkoutByIdAsync(id);
        if (!result.IsSuccess)
            return Results.NotFound();

        var workout = result.Data!;
        // Only allow access to user's own workouts
        if (workout.UserId != authContext.UserId)
            return Results.NotFound();

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
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository repository)
    {
        var result = await repository.GetWorkoutByIdAsync(id);
        if (!result.IsSuccess)
            return Results.NotFound();

        var workout = result.Data!;
        // Only allow updating user's own workouts
        if (workout.UserId != authContext.UserId)
            return Results.NotFound();

        workout.Name = req.Name;

        // New default exercises will have Id == Guid.Empty, existing ones will have their Id set
        var updatedDefaultExercises = req.DefaultExercises
            .Where(x => x.Id != Guid.Empty)
            .ToDictionary(x => x.Id, x => x);
        var newDefaultExercises = req.DefaultExercises
            .Where(x => x.Id == Guid.Empty)
            .ToList();
        
        foreach (var exercise in workout.DefaultExercises.ToList())
        {
            if (updatedDefaultExercises.TryGetValue(exercise.Id, out var newExercise))
            {
                exercise.ExerciseNumber = newExercise.ExerciseNumber;
                exercise.ExerciseId = newExercise.ExerciseId;
            }
            else
            {
                workout.DefaultExercises.Remove(exercise);
            }
        }

        foreach (var defaultExercise in newDefaultExercises)
        {
            workout.DefaultExercises.Add(new WorkoutDefaultExercise
            {
                WorkoutId = workout.Id,
                ExerciseId = defaultExercise.ExerciseId,
                ExerciseNumber = defaultExercise.ExerciseNumber
            });
        }

        var updateResult = await repository.UpdateWorkoutAsync(workout);
        return updateResult.ToApiResult().ToOkResult();
    }

    private static async Task<IResult> DeleteWorkout(
        Guid id,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository repository)
    {
        var result = await repository.GetWorkoutByIdAsync(id);
        if (!result.IsSuccess)
            return Results.NotFound();

        var workout = result.Data!;
        // Only allow deleting user's own workouts
        if (workout.UserId != authContext.UserId)
            return Results.NotFound();

        var deleteResult = await repository.DeleteWorkoutAsync(id);
        return deleteResult.ToApiResult().ToOkResult();
    }
}
