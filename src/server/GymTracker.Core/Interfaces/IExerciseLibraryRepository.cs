using GymTracker.Core.Entities;
using GymTracker.Core.Results;

namespace GymTracker.Core.Interfaces
{
    public interface IExerciseLibraryRepository
    {
        // Muscle Categories
        Task<DbResult<MuscleCategory>> GetMuscleCategoryByIdAsync(Guid id);
        Task<DbResult<IEnumerable<MuscleCategory>>> GetMuscleCategoriesByUserIdAsync(Guid? userId);
        Task<DbResult<IEnumerable<MuscleCategory>>> GetAllMuscleCategoriesAsync();
        Task<DbResult> AddMuscleCategoryAsync(MuscleCategory category);
        Task<DbResult> UpdateMuscleCategoryAsync(MuscleCategory category);
        Task<DbResult> DeleteMuscleCategoryAsync(Guid id);

        // Muscle Groups
        Task<DbResult<MuscleGroup>> GetMuscleGroupByIdAsync(Guid id);
        Task<DbResult<IEnumerable<MuscleGroup>>> GetMuscleGroupsByUserIdAsync(Guid? userId);
        Task<DbResult<IEnumerable<MuscleGroup>>> GetMuscleGroupsByCategoryIdAsync(Guid categoryId);
        Task<DbResult<IEnumerable<MuscleGroup>>> GetAllMuscleGroupsAsync();
        Task<DbResult> AddMuscleGroupAsync(MuscleGroup group);
        Task<DbResult> UpdateMuscleGroupAsync(MuscleGroup group);
        Task<DbResult> DeleteMuscleGroupAsync(Guid id);

        // Exercises
        Task<DbResult<Exercise>> GetExerciseByIdAsync(Guid id);
        Task<DbResult<IEnumerable<Exercise>>> GetExercisesByUserIdAsync(Guid? userId);
        Task<DbResult<IEnumerable<Exercise>>> GetExercisesByMuscleGroupIdAsync(Guid muscleGroupId);
        Task<DbResult<IEnumerable<Exercise>>> GetAllExercisesAsync();
        Task<DbResult> AddExerciseAsync(Exercise exercise);
        Task<DbResult> UpdateExerciseAsync(Exercise exercise);
        Task<DbResult> DeleteExerciseAsync(Guid id);

        // Workouts
        Task<DbResult<Workout>> GetWorkoutByIdAsync(Guid id);
        Task<DbResult<IEnumerable<Workout>>> GetWorkoutsByUserIdAsync(Guid userId);
        Task<DbResult> AddWorkoutAsync(Workout workout, bool saveChanges = true);
        Task<DbResult> UpdateWorkoutAsync(Workout workout);
        Task<DbResult> DeleteWorkoutAsync(Guid id);
        Task<DbResult> SaveChangesAsync();
    }
}
