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

        // Categories
        group.MapGet("/categories", GetUserMuscleCategories);
        group.MapGet("/categories/{id}", GetMuscleCategoryById);
        group.MapPost("/categories", CreateMuscleCategory);
        group.MapPut("/categories/{id}", UpdateMuscleCategory);
        group.MapDelete("/categories/{id}", DeleteMuscleCategory);

        // Groups
        group.MapGet("/groups", GetUserMuscleGroups);
        group.MapGet("/groups/{id}", GetMuscleGroupById);
        group.MapPost("/groups", CreateMuscleGroup);
        group.MapPut("/groups/{id}", UpdateMuscleGroup);
        group.MapDelete("/groups/{id}", DeleteMuscleGroup);
    }

    // ===== Muscle Categories =====

    private static async Task<IResult> GetUserMuscleCategories(
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository repository)
    {
        var result = await repository.GetMuscleCategoriesByUserIdAsync(authContext.UserId);
        return result.ToApiResult().ToOkResult();
    }

    private static async Task<IResult> GetMuscleCategoryById(
        Guid id,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository repository)
    {
        var result = await repository.GetMuscleCategoryByIdAsync(id);
        if (!result.IsSuccess)
            return Results.NotFound();

        var category = result.Data!;
        // Allow access to system categories (UserId == null) or user's own categories
        if (category.UserId != null && category.UserId != authContext.UserId)
            return Results.NotFound();

        return result.ToApiResult().ToOkResult();
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

    private static async Task<IResult> UpdateMuscleCategory(
        Guid id,
        UpdateMuscleCategoryRequest req,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository repository)
    {
        var result = await repository.GetMuscleCategoryByIdAsync(id);
        if (!result.IsSuccess)
            return Results.NotFound();

        var category = result.Data!;
        // Only allow updating user's own categories (not system categories)
        if (category.UserId != authContext.UserId)
            return Results.NotFound();

        category.Name = req.Name;

        var updateResult = await repository.UpdateMuscleCategoryAsync(category);
        return updateResult.ToApiResult().ToOkResult();
    }

    private static async Task<IResult> DeleteMuscleCategory(
        Guid id,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository repository)
    {
        var result = await repository.GetMuscleCategoryByIdAsync(id);
        if (!result.IsSuccess)
            return Results.NotFound();

        var category = result.Data!;
        // Only allow deleting user's own categories (not system categories)
        if (category.UserId != authContext.UserId)
            return Results.NotFound();

        var deleteResult = await repository.DeleteMuscleCategoryAsync(id);
        return deleteResult.ToApiResult().ToOkResult();
    }

    // ===== Muscle Groups =====

    private static async Task<IResult> GetUserMuscleGroups(
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository repository)
    {
        var result = await repository.GetMuscleGroupsByUserIdAsync(authContext.UserId);
        return result.ToApiResult().ToOkResult();
    }

    private static async Task<IResult> GetMuscleGroupById(
        Guid id,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository repository)
    {
        var result = await repository.GetMuscleGroupByIdAsync(id);
        if (!result.IsSuccess)
            return Results.NotFound();

        var muscleGroup = result.Data!;
        // Allow access to system groups (UserId == null) or user's own groups
        if (muscleGroup.UserId != null && muscleGroup.UserId != authContext.UserId)
            return Results.NotFound();

        return result.ToApiResult().ToOkResult();
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

    private static async Task<IResult> UpdateMuscleGroup(
        Guid id,
        UpdateMuscleGroupRequest req,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository repository)
    {
        var result = await repository.GetMuscleGroupByIdAsync(id);
        if (!result.IsSuccess)
            return Results.NotFound();

        var muscleGroup = result.Data!;
        // Only allow updating user's own groups (not system groups)
        if (muscleGroup.UserId != authContext.UserId)
            return Results.NotFound();

        muscleGroup.Name = req.Name;
        muscleGroup.CategoryId = req.CategoryId;

        var updateResult = await repository.UpdateMuscleGroupAsync(muscleGroup);
        return updateResult.ToApiResult().ToOkResult();
    }

    private static async Task<IResult> DeleteMuscleGroup(
        Guid id,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository repository)
    {
        var result = await repository.GetMuscleGroupByIdAsync(id);
        if (!result.IsSuccess)
            return Results.NotFound();

        var muscleGroup = result.Data!;
        // Only allow deleting user's own groups (not system groups)
        if (muscleGroup.UserId != authContext.UserId)
            return Results.NotFound();

        var deleteResult = await repository.DeleteMuscleGroupAsync(id);
        return deleteResult.ToApiResult().ToOkResult();
    }
}
