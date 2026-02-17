namespace GymTracker.Api.Endpoints.Responses.Results;

public record SessionSetResponse
{
    public Guid Id { get; init; }
    public int SetNumber { get; init; }
    public double Weight { get; init; }
    public int Reps { get; init; }
    public bool WarmUp { get; init; }
}

public record SessionExerciseResponse
{
    public Guid Id { get; init; }
    public int ExerciseNumber { get; init; }
    public Guid ExerciseId { get; init; }
    public string ExerciseName { get; init; } = string.Empty;
    public IEnumerable<SessionSetResponse> Sets { get; init; } = Enumerable.Empty<SessionSetResponse>();
}

public record SessionWorkoutSummary
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

public record SessionHistoryItemResponse
{
    public Guid Id { get; init; }
    public Guid WorkoutId { get; init; }
    public DateTime CreatedAt { get; init; }
    public SessionWorkoutSummary? Workout { get; init; }
    public IEnumerable<SessionExerciseResponse> SessionExercises { get; init; } = [];
}