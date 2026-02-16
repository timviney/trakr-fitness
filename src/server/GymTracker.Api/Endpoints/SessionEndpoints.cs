using GymTracker.Api.Auth;
using GymTracker.Api.Endpoints.Requests;
using GymTracker.Api.Endpoints.Responses.Structure;
using GymTracker.Core.Entities;
using GymTracker.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Endpoints;

public static class SessionEndpoints
{
    public static void MapSessionEndpoints(this WebApplication app)
    {
        // Sessions nested under workouts
        var workoutSessions = app.MapGroup("/workouts/{workoutId}/sessions")
            .WithTags("Sessions")
            .RequireAuthorization();

        workoutSessions.MapGet("/", GetSessionsByWorkout);
        workoutSessions.MapPost("/", CreateSession);

        // Session direct access
        var sessions = app.MapGroup("/sessions")
            .WithTags("Sessions")
            .RequireAuthorization();

        sessions.MapGet("/{id}", GetSessionById);
        sessions.MapDelete("/{id}", DeleteSession);

        // Session exercises nested under sessions
        var sessionExercises = app.MapGroup("/sessions/{sessionId}/exercises")
            .WithTags("Session Exercises")
            .RequireAuthorization();

        sessionExercises.MapGet("/", GetSessionExercises);
        sessionExercises.MapPost("/", CreateSessionExercise);

        // Session exercise direct access
        var sessionExerciseDirect = app.MapGroup("/session-exercises")
            .WithTags("Session Exercises")
            .RequireAuthorization();

        sessionExerciseDirect.MapGet("/{id}", GetSessionExerciseById);
        sessionExerciseDirect.MapPut("/{id}", UpdateSessionExercise);
        sessionExerciseDirect.MapDelete("/{id}", DeleteSessionExercise);

        // Sets nested under session exercises
        var sets = app.MapGroup("/session-exercises/{sessionExerciseId}/sets")
            .WithTags("Sets")
            .RequireAuthorization();

        sets.MapGet("/", GetSetsBySessionExercise);
        sets.MapPost("/", CreateSet);

        // Set direct access
        var setsDirect = app.MapGroup("/sets")
            .WithTags("Sets")
            .RequireAuthorization();

        setsDirect.MapGet("/{id}", GetSetById);
        setsDirect.MapPut("/{id}", UpdateSet);
        setsDirect.MapDelete("/{id}", DeleteSet);
    }

    // ===== Sessions =====

    private static async Task<IResult> GetSessionsByWorkout(
        Guid workoutId,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository exerciseLibraryRepository,
        [FromServices] ISessionRepository sessionRepository)
    {
        // Verify workout ownership
        var workoutResult = await exerciseLibraryRepository.GetWorkoutByIdAsync(workoutId);
        if (!workoutResult.IsSuccess)
            return Results.NotFound();

        if (workoutResult.Data!.UserId != authContext.UserId)
            return Results.NotFound();

        var result = await sessionRepository.GetSessionsByWorkoutIdAsync(workoutId);
        return result.ToApiResponse().ToOkResult();
    }

    private static async Task<IResult> GetSessionById(
        Guid id,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository exerciseLibraryRepository,
        [FromServices] ISessionRepository sessionRepository)
    {
        var result = await sessionRepository.GetSessionByIdAsync(id);
        if (!result.IsSuccess)
            return Results.NotFound();

        var session = result.Data!;

        // Verify ownership through workout
        var workoutResult = await exerciseLibraryRepository.GetWorkoutByIdAsync(session.WorkoutId);
        if (!workoutResult.IsSuccess || workoutResult.Data!.UserId != authContext.UserId)
            return Results.NotFound();

        return result.ToApiResponse().ToOkResult();
    }

    private static async Task<IResult> CreateSession(
        Guid workoutId,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository exerciseLibraryRepository,
        [FromServices] ISessionRepository sessionRepository)
    {
        // Verify workout ownership
        var workoutResult = await exerciseLibraryRepository.GetWorkoutByIdAsync(workoutId);
        if (!workoutResult.IsSuccess)
            return Results.NotFound();

        if (workoutResult.Data!.UserId != authContext.UserId)
            return Results.NotFound();

        var session = new Session
        {
            Id = Guid.NewGuid(),
            WorkoutId = workoutId,
            CreatedAt = DateTime.UtcNow
        };

        var result = await sessionRepository.AddSessionAsync(session);
        return result.ToApiResponse().ToCreatedResult($"/sessions/{session.Id}");
    }

    private static async Task<IResult> DeleteSession(
        Guid id,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository exerciseLibraryRepository,
        [FromServices] ISessionRepository sessionRepository)
    {
        var result = await sessionRepository.GetSessionByIdAsync(id);
        if (!result.IsSuccess)
            return Results.NotFound();

        var session = result.Data!;

        // Verify ownership through workout
        var workoutResult = await exerciseLibraryRepository.GetWorkoutByIdAsync(session.WorkoutId);
        if (!workoutResult.IsSuccess || workoutResult.Data!.UserId != authContext.UserId)
            return Results.NotFound();

        var deleteResult = await sessionRepository.DeleteSessionAsync(id);
        return deleteResult.ToApiResponse().ToOkResult();
    }

    // ===== Session Exercises =====

    private static async Task<IResult> GetSessionExercises(
        Guid sessionId,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository exerciseLibraryRepository,
        [FromServices] ISessionRepository sessionRepository)
    {
        // Verify session ownership
        var sessionResult = await sessionRepository.GetSessionByIdAsync(sessionId);
        if (!sessionResult.IsSuccess)
            return Results.NotFound();

        var workoutResult = await exerciseLibraryRepository.GetWorkoutByIdAsync(sessionResult.Data!.WorkoutId);
        if (!workoutResult.IsSuccess || workoutResult.Data!.UserId != authContext.UserId)
            return Results.NotFound();

        var result = await sessionRepository.GetSessionExercisesBySessionIdAsync(sessionId);
        return result.ToApiResponse().ToOkResult();
    }

    private static async Task<IResult> GetSessionExerciseById(
        Guid id,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository exerciseLibraryRepository,
        [FromServices] ISessionRepository sessionRepository)
    {
        var result = await sessionRepository.GetSessionExerciseByIdAsync(id);
        if (!result.IsSuccess)
            return Results.NotFound();

        var sessionExercise = result.Data!;

        // Verify ownership through session -> workout
        var sessionResult = await sessionRepository.GetSessionByIdAsync(sessionExercise.SessionId);
        if (!sessionResult.IsSuccess)
            return Results.NotFound();

        var workoutResult = await exerciseLibraryRepository.GetWorkoutByIdAsync(sessionResult.Data!.WorkoutId);
        if (!workoutResult.IsSuccess || workoutResult.Data!.UserId != authContext.UserId)
            return Results.NotFound();

        return result.ToApiResponse().ToOkResult();
    }

    private static async Task<IResult> CreateSessionExercise(
        Guid sessionId,
        SessionExerciseRequest req,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository exerciseLibraryRepository,
        [FromServices] ISessionRepository sessionRepository)
    {
        // Verify session ownership
        var sessionResult = await sessionRepository.GetSessionByIdAsync(sessionId);
        if (!sessionResult.IsSuccess)
            return Results.NotFound();

        var workoutResult = await exerciseLibraryRepository.GetWorkoutByIdAsync(sessionResult.Data!.WorkoutId);
        if (!workoutResult.IsSuccess || workoutResult.Data!.UserId != authContext.UserId)
            return Results.NotFound();

        var sessionExercise = new SessionExercise
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            ExerciseId = req.ExerciseId,
            ExerciseNumber = req.ExerciseNumber,
            Sets = new List<Set>()
        };

        foreach (var set in req.Sets) sessionExercise.Sets.Add(new Set
        {
            SessionExerciseId =  sessionExercise.Id,
            SetNumber = set.SetNumber,
            Weight = set.Weight,
            Reps = set.Reps,
            WarmUp = set.WarmUp
        });

        var result = await sessionRepository.AddSessionExerciseAsync(sessionExercise);
        return result.ToApiResponse().ToCreatedResult($"/session-exercises/{sessionExercise.Id}");
    }

    private static async Task<IResult> UpdateSessionExercise(
        Guid id,
        SessionExerciseRequest req,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository exerciseLibraryRepository,
        [FromServices] ISessionRepository sessionRepository)
    {
        var result = await sessionRepository.GetSessionExerciseByIdAsync(id);
        if (!result.IsSuccess)
            return Results.NotFound();

        var sessionExercise = result.Data!;

        // Verify ownership through session -> workout
        var sessionResult = await sessionRepository.GetSessionByIdAsync(sessionExercise.SessionId);
        if (!sessionResult.IsSuccess)
            return Results.NotFound();

        var workoutResult = await exerciseLibraryRepository.GetWorkoutByIdAsync(sessionResult.Data!.WorkoutId);
        if (!workoutResult.IsSuccess || workoutResult.Data!.UserId != authContext.UserId)
            return Results.NotFound();

        sessionExercise.ExerciseNumber = req.ExerciseNumber;
        sessionExercise.ExerciseId = req.ExerciseId;

        var updateResult = await sessionRepository.UpdateSessionExerciseAsync(sessionExercise, saveChanges: false);
        if (!updateResult.IsSuccess)
            return updateResult.ToApiResponse().ToErrorResult();

        var existingSets = sessionExercise.Sets.ToDictionary(s => s.Id);
        var updatedSets = req.Sets.Where(s => s.Id != null && s.Id != Guid.Empty).ToDictionary(s => (Guid)s.Id!);
        var newSets = req.Sets.Where(s => s.Id == null || s.Id == Guid.Empty).ToList();
        var deletedSetIds = existingSets.Keys.Except(updatedSets.Keys).ToList();

        foreach (var setId in deletedSetIds)
        {
            var deleteResult = await sessionRepository.DeleteSetAsync(setId, saveChanges: false);
            if (!deleteResult.IsSuccess) return deleteResult.ToApiResponse().ToErrorResult();
        }

        foreach (var updatedSet in updatedSets)
        {
            var set = existingSets[updatedSet.Key];
            set.SetNumber = updatedSet.Value.SetNumber;
            set.Weight = updatedSet.Value.Weight;
            set.Reps = updatedSet.Value.Reps;
            set.WarmUp = updatedSet.Value.WarmUp;

            // No need to save as this is a tracked entity, changes will be saved at the end
        }

        foreach (var setRequest in newSets)
        {
            var set = new Set
            {
                SessionExerciseId = sessionExercise.Id,
                SetNumber = setRequest.SetNumber,
                Weight = setRequest.Weight,
                Reps = setRequest.Reps,
                WarmUp = setRequest.WarmUp
            };
            var addResult = await sessionRepository.AddSetAsync(set);
            if (!addResult.IsSuccess) return addResult.ToApiResponse().ToErrorResult();
        }

        await sessionRepository.SaveChangesAsync();

        return updateResult.ToApiResponse().ToOkResult();
    }

    private static async Task<IResult> DeleteSessionExercise(
        Guid id,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository exerciseLibraryRepository,
        [FromServices] ISessionRepository sessionRepository)
    {
        var result = await sessionRepository.GetSessionExerciseByIdAsync(id);
        if (!result.IsSuccess)
            return Results.NotFound();

        var sessionExercise = result.Data!;

        // Verify ownership through session -> workout
        var sessionResult = await sessionRepository.GetSessionByIdAsync(sessionExercise.SessionId);
        if (!sessionResult.IsSuccess)
            return Results.NotFound();

        var workoutResult = await exerciseLibraryRepository.GetWorkoutByIdAsync(sessionResult.Data!.WorkoutId);
        if (!workoutResult.IsSuccess || workoutResult.Data!.UserId != authContext.UserId)
            return Results.NotFound();

        var deleteResult = await sessionRepository.DeleteSessionExerciseAsync(id);
        return deleteResult.ToApiResponse().ToOkResult();
    }

    // ===== Sets =====

    private static async Task<IResult> GetSetsBySessionExercise(
        Guid sessionExerciseId,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository exerciseLibraryRepository,
        [FromServices] ISessionRepository sessionRepository)
    {
        // Verify ownership through session exercise -> session -> workout
        var sessionExerciseResult = await sessionRepository.GetSessionExerciseByIdAsync(sessionExerciseId);
        if (!sessionExerciseResult.IsSuccess)
            return Results.NotFound();

        var sessionResult = await sessionRepository.GetSessionByIdAsync(sessionExerciseResult.Data!.SessionId);
        if (!sessionResult.IsSuccess)
            return Results.NotFound();

        var workoutResult = await exerciseLibraryRepository.GetWorkoutByIdAsync(sessionResult.Data!.WorkoutId);
        if (!workoutResult.IsSuccess || workoutResult.Data!.UserId != authContext.UserId)
            return Results.NotFound();

        var result = await sessionRepository.GetSetsBySessionExerciseIdAsync(sessionExerciseId);
        return result.ToApiResponse().ToOkResult();
    }

    private static async Task<IResult> GetSetById(
        Guid id,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository exerciseLibraryRepository,
        [FromServices] ISessionRepository sessionRepository)
    {
        var result = await sessionRepository.GetSetByIdAsync(id);
        if (!result.IsSuccess)
            return Results.NotFound();

        var set = result.Data!;

        // Verify ownership through session exercise -> session -> workout
        var sessionExerciseResult = await sessionRepository.GetSessionExerciseByIdAsync(set.SessionExerciseId);
        if (!sessionExerciseResult.IsSuccess)
            return Results.NotFound();

        var sessionResult = await sessionRepository.GetSessionByIdAsync(sessionExerciseResult.Data!.SessionId);
        if (!sessionResult.IsSuccess)
            return Results.NotFound();

        var workoutResult = await exerciseLibraryRepository.GetWorkoutByIdAsync(sessionResult.Data!.WorkoutId);
        if (!workoutResult.IsSuccess || workoutResult.Data!.UserId != authContext.UserId)
            return Results.NotFound();

        return result.ToApiResponse().ToOkResult();
    }

    private static async Task<IResult> CreateSet(
        Guid sessionExerciseId,
        SetRequest req,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository exerciseLibraryRepository,
        [FromServices] ISessionRepository sessionRepository)
    {
        // Verify ownership through session exercise -> session -> workout
        var sessionExerciseResult = await sessionRepository.GetSessionExerciseByIdAsync(sessionExerciseId);
        if (!sessionExerciseResult.IsSuccess)
            return Results.NotFound();

        var sessionResult = await sessionRepository.GetSessionByIdAsync(sessionExerciseResult.Data!.SessionId);
        if (!sessionResult.IsSuccess)
            return Results.NotFound();

        var workoutResult = await exerciseLibraryRepository.GetWorkoutByIdAsync(sessionResult.Data!.WorkoutId);
        if (!workoutResult.IsSuccess || workoutResult.Data!.UserId != authContext.UserId)
            return Results.NotFound();

        var set = new Set
        {
            Id = Guid.NewGuid(),
            SessionExerciseId = sessionExerciseId,
            SetNumber = req.SetNumber,
            Weight = req.Weight,
            Reps = req.Reps,
            WarmUp = req.WarmUp
        };

        var result = await sessionRepository.AddSetAsync(set);
        return result.ToApiResponse().ToCreatedResult($"/sets/{set.Id}");
    }

    private static async Task<IResult> UpdateSet(
        Guid id,
        SetRequest req,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository exerciseLibraryRepository,
        [FromServices] ISessionRepository sessionRepository)
    {
        var result = await sessionRepository.GetSetByIdAsync(id);
        if (!result.IsSuccess)
            return Results.NotFound();

        var set = result.Data!;

        // Verify ownership through session exercise -> session -> workout
        var sessionExerciseResult = await sessionRepository.GetSessionExerciseByIdAsync(set.SessionExerciseId);
        if (!sessionExerciseResult.IsSuccess)
            return Results.NotFound();

        var sessionResult = await sessionRepository.GetSessionByIdAsync(sessionExerciseResult.Data!.SessionId);
        if (!sessionResult.IsSuccess)
            return Results.NotFound();

        var workoutResult = await exerciseLibraryRepository.GetWorkoutByIdAsync(sessionResult.Data!.WorkoutId);
        if (!workoutResult.IsSuccess || workoutResult.Data!.UserId != authContext.UserId)
            return Results.NotFound();

        set.SetNumber = req.SetNumber;
        set.Weight = req.Weight;
        set.Reps = req.Reps;
        set.WarmUp = req.WarmUp;

        var updateResult = await sessionRepository.UpdateSetAsync(set);
        return updateResult.ToApiResponse().ToOkResult();
    }

    private static async Task<IResult> DeleteSet(
        Guid id,
        [FromServices] IAuthContext authContext,
        [FromServices] IExerciseLibraryRepository exerciseLibraryRepository,
        [FromServices] ISessionRepository sessionRepository)
    {
        var result = await sessionRepository.GetSetByIdAsync(id);
        if (!result.IsSuccess)
            return Results.NotFound();

        var set = result.Data!;

        // Verify ownership through session exercise -> session -> workout
        var sessionExerciseResult = await sessionRepository.GetSessionExerciseByIdAsync(set.SessionExerciseId);
        if (!sessionExerciseResult.IsSuccess)
            return Results.NotFound();

        var sessionResult = await sessionRepository.GetSessionByIdAsync(sessionExerciseResult.Data!.SessionId);
        if (!sessionResult.IsSuccess)
            return Results.NotFound();

        var workoutResult = await exerciseLibraryRepository.GetWorkoutByIdAsync(sessionResult.Data!.WorkoutId);
        if (!workoutResult.IsSuccess || workoutResult.Data!.UserId != authContext.UserId)
            return Results.NotFound();

        var deleteResult = await sessionRepository.DeleteSetAsync(id);
        return deleteResult.ToApiResponse().ToOkResult();
    }
}
