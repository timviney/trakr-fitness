using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GymTracker.Core.Entities;

namespace GymTracker.Core.Interfaces
{
    public interface IExerciseLibraryRepository
    {
        // Muscle Categories
        Task<MuscleCategory?> GetMuscleCategoryByIdAsync(Guid id);
        Task<IEnumerable<MuscleCategory>> GetMuscleCategoriesByUserIdAsync(Guid? userId);
        Task<IEnumerable<MuscleCategory>> GetAllMuscleCategoriesAsync();
        Task AddMuscleCategoryAsync(MuscleCategory category);
        Task UpdateMuscleCategoryAsync(MuscleCategory category);
        Task DeleteMuscleCategoryAsync(Guid id);

        // Muscle Groups
        Task<MuscleGroup?> GetMuscleGroupByIdAsync(Guid id);
        Task<IEnumerable<MuscleGroup>> GetMuscleGroupsByUserIdAsync(Guid? userId);
        Task<IEnumerable<MuscleGroup>> GetMuscleGroupsByCategoryIdAsync(Guid categoryId);
        Task<IEnumerable<MuscleGroup>> GetAllMuscleGroupsAsync();
        Task AddMuscleGroupAsync(MuscleGroup group);
        Task UpdateMuscleGroupAsync(MuscleGroup group);
        Task DeleteMuscleGroupAsync(Guid id);

        // Exercises
        Task<Exercise?> GetExerciseByIdAsync(Guid id);
        Task<IEnumerable<Exercise>> GetExercisesByUserIdAsync(Guid? userId);
        Task<IEnumerable<Exercise>> GetExercisesByMuscleGroupIdAsync(Guid muscleGroupId);
        Task<IEnumerable<Exercise>> GetAllExercisesAsync();
        Task AddExerciseAsync(Exercise exercise);
        Task UpdateExerciseAsync(Exercise exercise);
        Task DeleteExerciseAsync(Guid id);

        // Workouts
        Task<Workout?> GetWorkoutByIdAsync(Guid id);
        Task<IEnumerable<Workout>> GetWorkoutsByUserIdAsync(Guid userId);
        Task AddWorkoutAsync(Workout workout);
        Task UpdateWorkoutAsync(Workout workout);
        Task DeleteWorkoutAsync(Guid id);
    }
}
