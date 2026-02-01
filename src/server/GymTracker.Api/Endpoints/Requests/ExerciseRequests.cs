namespace GymTracker.Api.Endpoints.Requests;

public record CreateExerciseRequest(string Name, Guid MuscleGroupId);

public record CreateWorkoutRequest(string Name);

public record UpdateWorkoutRequest(string Name);

public record CreateMuscleCategoryRequest(string Name);

public record CreateMuscleGroupRequest(string Name, Guid CategoryId);
