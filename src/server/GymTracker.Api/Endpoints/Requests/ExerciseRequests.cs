using GymTracker.Core.Entities;

namespace GymTracker.Api.Endpoints.Requests;

// Exercise requests
public record CreateExerciseRequest(string Name, Guid MuscleGroupId);
public record UpdateExerciseRequest(string Name, Guid MuscleGroupId);

// Workout requests
public record CreateWorkoutRequest(string Name, WorkoutDefaultExercise[] DefaultExercises);
public record UpdateWorkoutRequest(string Name, WorkoutDefaultExercise[] DefaultExercises);

// Muscle Category requests
public record CreateMuscleCategoryRequest(string Name);
public record UpdateMuscleCategoryRequest(string Name);

// Muscle Group requests
public record CreateMuscleGroupRequest(string Name, Guid CategoryId);
public record UpdateMuscleGroupRequest(string Name, Guid CategoryId);

// Session Exercise requests
public record SessionExerciseRequest(Guid ExerciseId, int ExerciseNumber, SetRequest[] Sets);

// Set requests
public record SetRequest(Guid? Id, int SetNumber, double Weight, int Reps, bool WarmUp);