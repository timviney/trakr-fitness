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
        group.MapPost("/", CreateExercise);
    }

    private static async Task<IResult> GetUserExercises(
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository repository)
    {
        var result = await repository.GetExercisesByUserIdAsync(authContext.UserId);
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
}