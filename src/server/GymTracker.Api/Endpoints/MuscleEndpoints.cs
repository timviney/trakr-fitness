using GymTracker.Api.Auth;
using GymTracker.Api.Endpoints.Requests;
using GymTracker.Api.Endpoints.Responses.Structure;
using GymTracker.Core.Entities;
using GymTracker.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Endpoints;

public static class MuscleEndpoints
{
    public static void MapMuscleEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/muscle")
            .WithTags("Muscle Groups & Categories")
            .RequireAuthorization();

        group.MapPost("/categories", CreateMuscleCategory);
        group.MapPost("/groups", CreateMuscleGroup);
    }

    private static async Task<IResult> CreateMuscleCategory(
        CreateMuscleCategoryRequest req,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository repository)
    {
        var category = new MuscleCategory
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            UserId = authContext.UserId
        };

        var result = await repository.AddMuscleCategoryAsync(category);
        return result.ToApiResult().ToCreatedResult($"/muscle/categories/{category.Id}");
    }

    private static async Task<IResult> CreateMuscleGroup(
        CreateMuscleGroupRequest req,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository repository)
    {
        var group = new MuscleGroup
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            CategoryId = req.CategoryId,
            UserId = authContext.UserId
        };

        var result = await repository.AddMuscleGroupAsync(group);
        return result.ToApiResult().ToCreatedResult($"/muscle/groups/{group.Id}");
    }
}
