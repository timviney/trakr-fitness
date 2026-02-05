namespace GymTracker.Api.Endpoints.Requests;

// Exercise requests
public record CreateExerciseRequest(string Name, Guid MuscleGroupId);
public record UpdateExerciseRequest(string Name, Guid MuscleGroupId);

// Workout requests
public record CreateWorkoutRequest(string Name);
public record UpdateWorkoutRequest(string Name);

// Muscle Category requests
public record CreateMuscleCategoryRequest(string Name);
public record UpdateMuscleCategoryRequest(string Name);

// Muscle Group requests
public record CreateMuscleGroupRequest(string Name, Guid CategoryId);
public record UpdateMuscleGroupRequest(string Name, Guid CategoryId);

// Session Exercise requests
public record CreateSessionExerciseRequest(Guid ExerciseId, int ExerciseNumber);
public record UpdateSessionExerciseRequest(int ExerciseNumber);

// Set requests
public record CreateSetRequest(int SetNumber, double Weight, int Reps, bool WarmUp);
public record UpdateSetRequest(int SetNumber, double Weight, int Reps, bool WarmUp);
