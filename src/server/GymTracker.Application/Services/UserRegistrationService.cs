using GymTracker.Core.Entities;
using GymTracker.Core.Interfaces;
using GymTracker.Core.Results;

namespace GymTracker.Application.Services;

public class UserRegistrationService(
    IUserRepository userRepository,
    IExerciseLibraryRepository exerciseLibraryRepository) : IUserRegistrationService
{
    private static readonly string[] DefaultWorkoutNames = ["Push Day", "Pull Day", "Leg Day"];
    private static readonly Dictionary<string, List<string>> DefaultExercisesByWorkout = new()
    {
        ["Push Day"] = ["Bench Press", "Overhead Press", "Tricep Dips"],
        ["Pull Day"] = ["Pull Ups", "Barbell Rows", "Dumbbell Curls"],
        ["Leg Day"] = ["Squats", "Deadlifts", "Standing Calf Raises"]
    };

    public async Task<DbResult<User>> RegisterUserAsync(User user)
    {
        // Add user without saving
        var userResult = await userRepository.AddAsync(user, saveChanges: false);
        if (!userResult.IsSuccess)
            return DbResult<User>.FromResult(userResult);

        var workouts = new Dictionary<string, Workout>();

        // Add default workouts without saving - use navigation property so EF links them
        foreach (var workoutName in DefaultWorkoutNames)
        {
            var workout = new Workout
            {
                User = user,
                Name = workoutName
            };

            var workoutResult = await exerciseLibraryRepository.AddWorkoutAsync(workout, saveChanges: false);
            if (!workoutResult.IsSuccess) return DbResult<User>.FromResult(workoutResult);
            
            workouts.Add(workoutName, workout);
        }
        
        var exercises = (await exerciseLibraryRepository.GetAllExercisesAsync())
            .Data!
            .ToDictionary(e => e.Name, e => e);

        foreach (var (workoutName, exerciseNames) in DefaultExercisesByWorkout)
        {
            var workout = workouts[workoutName];
            for (var number = 0; number < exerciseNames.Count; number++)
            {
                var exerciseName = exerciseNames[number];
                if (!exercises.TryGetValue(exerciseName, out var exercise))
                    return DbResult<User>.DatabaseError($"Exercise '{exerciseName}' not found in library.");

                var defaultExercise = new WorkoutDefaultExercise
                {
                    Workout = workout,
                    Exercise = exercise,
                    ExerciseNumber = number
                };

                var exerciseResult = await exerciseLibraryRepository.AddWorkoutDefaultExerciseAsync(defaultExercise, saveChanges: false);
                if (!exerciseResult.IsSuccess) return DbResult<User>.FromResult(exerciseResult);
            }
        }

        // Save all changes atomically - EF will generate IDs and link FKs
        var saveResult = await userRepository.SaveChangesAsync();
        return !saveResult.IsSuccess 
            ? DbResult<User>.FromResult(saveResult) 
            : DbResult<User>.Ok(user);
    }
}
