using GymTracker.Api.Auth;
using GymTracker.Api.Endpoints.Requests;
using GymTracker.Api.Endpoints.Responses.Structure;
using GymTracker.Core.Entities;
using GymTracker.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Endpoints;

public static class ExerciseEndpoints
{
    public static void MapExerciseEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/exercises")
            .WithTags("Exercises")
            .RequireAuthorization();

        group.MapGet("/", GetUserExercises);
        group.MapGet("/{id}", GetExerciseById);
        group.MapPost("/", CreateExercise);
        group.MapPut("/{id}", UpdateExercise);
        group.MapDelete("/{id}", DeleteExercise);
    }

    private static async Task<IResult> GetUserExercises(
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository repository)
    {
        var result = await repository.GetExercisesByUserIdAsync(authContext.UserId);
        return result.ToApiResult().ToOkResult();
    }

    private static async Task<IResult> GetExerciseById(
        Guid id,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository repository)
    {
        var result = await repository.GetExerciseByIdAsync(id);
        if (!result.IsSuccess)
            return Results.NotFound();

        var exercise = result.Data!;
        // Allow access to system exercises (UserId == null) or user's own exercises
        if (exercise.UserId != null && exercise.UserId != authContext.UserId)
            return Results.NotFound();

        return result.ToApiResult().ToOkResult();
    }

    private static async Task<IResult> CreateExercise(
        CreateExerciseRequest req,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository repository)
    {
        var exercise = new Exercise
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            UserId = authContext.UserId,
            MuscleGroupId = req.MuscleGroupId
        };

        var result = await repository.AddExerciseAsync(exercise);
        return result.ToApiResult().ToCreatedResult($"/exercises/{exercise.Id}");
    }

    private static async Task<IResult> UpdateExercise(
        Guid id,
        UpdateExerciseRequest req,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository repository)
    {
        var result = await repository.GetExerciseByIdAsync(id);
        if (!result.IsSuccess)
            return Results.NotFound();

        var exercise = result.Data!;
        // Only allow updating user's own exercises (not system exercises)
        if (exercise.UserId != authContext.UserId)
            return Results.NotFound();

        exercise.Name = req.Name;
        exercise.MuscleGroupId = req.MuscleGroupId;

        var updateResult = await repository.UpdateExerciseAsync(exercise);
        return updateResult.ToApiResult().ToOkResult();
    }

    private static async Task<IResult> DeleteExercise(
        Guid id,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository repository)
    {
        var result = await repository.GetExerciseByIdAsync(id);
        if (!result.IsSuccess)
            return Results.NotFound();

        var exercise = result.Data!;
        // Only allow deleting user's own exercises (not system exercises)
        if (exercise.UserId != authContext.UserId)
            return Results.NotFound();

        var deleteResult = await repository.DeleteExerciseAsync(id);
        return deleteResult.ToApiResult().ToOkResult();
    }
}