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
        Task<DbResult<MuscleCategory>> AddMuscleCategoryAsync(MuscleCategory category);
        Task<DbResult<MuscleCategory>> UpdateMuscleCategoryAsync(MuscleCategory category);
        Task<DbResult<MuscleCategory>> DeleteMuscleCategoryAsync(Guid id);

        // Muscle Groups
        Task<DbResult<MuscleGroup>> GetMuscleGroupByIdAsync(Guid id);
        Task<DbResult<IEnumerable<MuscleGroup>>> GetMuscleGroupsByUserIdAsync(Guid? userId);
        Task<DbResult<IEnumerable<MuscleGroup>>> GetMuscleGroupsByCategoryIdAsync(Guid categoryId);
        Task<DbResult<IEnumerable<MuscleGroup>>> GetAllMuscleGroupsAsync();
        Task<DbResult<MuscleGroup>> AddMuscleGroupAsync(MuscleGroup group);
        Task<DbResult<MuscleGroup>> UpdateMuscleGroupAsync(MuscleGroup group);
        Task<DbResult<MuscleGroup>> DeleteMuscleGroupAsync(Guid id);

        // Exercises
        Task<DbResult<Exercise>> GetExerciseByIdAsync(Guid id);
        Task<DbResult<IEnumerable<Exercise>>> GetExercisesByUserIdAsync(Guid? userId);
        Task<DbResult<IEnumerable<Exercise>>> GetExercisesByMuscleGroupIdAsync(Guid muscleGroupId);
        Task<DbResult<IEnumerable<Exercise>>> GetAllExercisesAsync();
        Task<DbResult<Exercise>> AddExerciseAsync(Exercise exercise);
        Task<DbResult<Exercise>> UpdateExerciseAsync(Exercise exercise);
        Task<DbResult<Exercise>> DeleteExerciseAsync(Guid id);

        // Workouts
        Task<DbResult<Workout>> GetWorkoutByIdAsync(Guid id);
        Task<DbResult<IEnumerable<Workout>>> GetWorkoutsByUserIdAsync(Guid userId);
        Task<DbResult<Workout>> AddWorkoutAsync(Workout workout, bool saveChanges = true);
        Task<DbResult<Workout>> UpdateWorkoutAsync(Workout workout);
        Task<DbResult<Workout>> DeleteWorkoutAsync(Guid id);
        Task<DbResult> SaveChangesAsync();
    }
}
