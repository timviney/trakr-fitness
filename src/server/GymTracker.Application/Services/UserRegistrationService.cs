using GymTracker.Core.Entities;
using GymTracker.Core.Interfaces;
using GymTracker.Core.Results;

namespace GymTracker.Application.Services;

public class UserRegistrationService(
    IUserRepository userRepository,
    IExerciseLibraryRepository exerciseLibraryRepository) : IUserRegistrationService
{
    private static readonly string[] DefaultWorkoutNames = ["Push", "Pull", "Legs"];

    public async Task<DbResult<User>> RegisterUserAsync(User user)
    {
        // Add user without saving
        var userResult = await userRepository.AddAsync(user, saveChanges: false);
        if (!userResult.IsSuccess)
            return DbResult<User>.FromResult(userResult);

        // Add default workouts without saving - use navigation property so EF links them
        foreach (var workoutName in DefaultWorkoutNames)
        {
            var workout = new Workout
            {
                User = user,
                Name = workoutName
            };

            var workoutResult = await exerciseLibraryRepository.AddWorkoutAsync(workout, saveChanges: false);
            if (!workoutResult.IsSuccess)
                return DbResult<User>.FromResult(workoutResult);
        }

        // Save all changes atomically - EF will generate IDs and link FKs
        var saveResult = await userRepository.SaveChangesAsync();
        if (!saveResult.IsSuccess)
            return DbResult<User>.FromResult(saveResult);

        return DbResult<User>.Ok(user);
    }
}
