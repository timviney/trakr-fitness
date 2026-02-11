using Microsoft.EntityFrameworkCore;
using GymTracker.Core.Entities;
using GymTracker.Core.Interfaces;
using GymTracker.Core.Results;
using GymTracker.Infrastructure.Data;

namespace GymTracker.Infrastructure.Repositories
{
    public class ExerciseLibraryRepository(GymTrackerDbContext db) : IExerciseLibraryRepository
    {
        // ===== Muscle Categories =====
        public async Task<DbResult<MuscleCategory>> GetMuscleCategoryByIdAsync(Guid id)
        {
            var category = await db.MuscleCategories
                .Include(mc => mc.User)
                .FirstOrDefaultAsync(mc => mc.Id == id);
            
            if (category is null)
                return DbResult<MuscleCategory>.NotFound($"Muscle category with id '{id}' not found.");
            
            return DbResult<MuscleCategory>.Ok(category);
        }

        public async Task<DbResult<IEnumerable<MuscleCategory>>> GetMuscleCategoriesByUserIdAsync(Guid? userId)
        {
            var categories = await db.MuscleCategories
                .Include(mc => mc.User)
                .Where(mc => mc.UserId == userId || mc.UserId == null)
                .ToListAsync();
            
            return DbResult<IEnumerable<MuscleCategory>>.Ok(categories);
        }

        public async Task<DbResult<IEnumerable<MuscleCategory>>> GetAllMuscleCategoriesAsync()
        {
            var categories = await db.MuscleCategories
                .Include(mc => mc.User)
                .ToListAsync();
            
            return DbResult<IEnumerable<MuscleCategory>>.Ok(categories);
        }

        public async Task<DbResult<MuscleCategory>> AddMuscleCategoryAsync(MuscleCategory category)
        {
            var result = await GetMuscleCategoriesByUserIdAsync(category.UserId);
            if (!result.IsSuccess)
                return DbResult<MuscleCategory>.DatabaseError(result.Message);
            
            var existing = result.Data?.FirstOrDefault(mc => mc.Name == category.Name);
            if (existing is not null)
                return DbResult<MuscleCategory>.DuplicateName($"A muscle category named '{category.Name}' already exists.");
            
            await db.MuscleCategories.AddAsync(category);
            await db.SaveChangesAsync();
            return DbResult<MuscleCategory>.Ok(category);
        }

        public async Task<DbResult<MuscleCategory>> UpdateMuscleCategoryAsync(MuscleCategory category)
        {
            var result = await GetMuscleCategoriesByUserIdAsync(category.UserId);
            if (!result.IsSuccess)
                return DbResult<MuscleCategory>.DatabaseError(result.Message);
            
            var existing = result.Data?.FirstOrDefault(mc => mc.Name == category.Name && mc.Id != category.Id);
            if (existing is not null)
                return DbResult<MuscleCategory>.DuplicateName($"A muscle category named '{category.Name}' already exists.");
            
            db.MuscleCategories.Update(category);
            await db.SaveChangesAsync();
            return DbResult<MuscleCategory>.Ok(category);
        }

        public async Task<DbResult<MuscleCategory>> DeleteMuscleCategoryAsync(Guid id)
        {
            var category = await db.MuscleCategories.FindAsync(id);
            if (category is null)
                return DbResult<MuscleCategory>.NotFound($"Muscle category with id '{id}' not found.");
            
            db.MuscleCategories.Remove(category);
            await db.SaveChangesAsync();
            return DbResult<MuscleCategory>.Ok(category);
        }

        // ===== Muscle Groups =====
        public async Task<DbResult<MuscleGroup>> GetMuscleGroupByIdAsync(Guid id)
        {
            var group = await db.MuscleGroups
                .Include(mg => mg.User)
                .Include(mg => mg.Category)
                .Include(mg => mg.Exercises)
                .FirstOrDefaultAsync(mg => mg.Id == id);
            
            if (group is null)
                return DbResult<MuscleGroup>.NotFound($"Muscle group with id '{id}' not found.");
            
            return DbResult<MuscleGroup>.Ok(group);
        }

        public async Task<DbResult<IEnumerable<MuscleGroup>>> GetMuscleGroupsByUserIdAsync(Guid? userId)
        {
            var groups = await db.MuscleGroups
                .Include(mg => mg.User)
                .Include(mg => mg.Category)
                .Include(mg => mg.Exercises)
                .Where(mg => mg.UserId == userId || mg.UserId == null)
                .ToListAsync();
            
            return DbResult<IEnumerable<MuscleGroup>>.Ok(groups);
        }

        public async Task<DbResult<IEnumerable<MuscleGroup>>> GetMuscleGroupsByCategoryIdAsync(Guid categoryId)
        {
            var groups = await db.MuscleGroups
                .Include(mg => mg.User)
                .Include(mg => mg.Category)
                .Include(mg => mg.Exercises)
                .Where(mg => mg.CategoryId == categoryId)
                .ToListAsync();
            
            return DbResult<IEnumerable<MuscleGroup>>.Ok(groups);
        }

        public async Task<DbResult<IEnumerable<MuscleGroup>>> GetAllMuscleGroupsAsync()
        {
            var groups = await db.MuscleGroups
                .Include(mg => mg.User)
                .Include(mg => mg.Category)
                .Include(mg => mg.Exercises)
                .ToListAsync();
            
            return DbResult<IEnumerable<MuscleGroup>>.Ok(groups);
        }

        public async Task<DbResult<MuscleGroup>> AddMuscleGroupAsync(MuscleGroup group)
        {
            var result = await GetMuscleGroupsByUserIdAsync(group.UserId);
            if (!result.IsSuccess)
                return DbResult<MuscleGroup>.DatabaseError(result.Message);
            
            var existing = result.Data?.FirstOrDefault(mg => mg.Name == group.Name);
            if (existing is not null)
                return DbResult<MuscleGroup>.DuplicateName($"A muscle group named '{group.Name}' already exists.");
            
            await db.MuscleGroups.AddAsync(group);
            await db.SaveChangesAsync();
            return DbResult<MuscleGroup>.Ok(group);
        }

        public async Task<DbResult<MuscleGroup>> UpdateMuscleGroupAsync(MuscleGroup group)
        {
            var result = await GetMuscleGroupsByUserIdAsync(group.UserId);
            if (!result.IsSuccess)
                return DbResult<MuscleGroup>.DatabaseError(result.Message);
            
            var existing = result.Data?.FirstOrDefault(mg => mg.Name == group.Name && mg.Id != group.Id);
            if (existing is not null)
                return DbResult<MuscleGroup>.DuplicateName($"A muscle group named '{group.Name}' already exists.");
            
            db.MuscleGroups.Update(group);
            await db.SaveChangesAsync();
            return DbResult<MuscleGroup>.Ok(group);
        }

        public async Task<DbResult<MuscleGroup>> DeleteMuscleGroupAsync(Guid id)
        {
            var group = await db.MuscleGroups.FindAsync(id);
            if (group is null)
                return DbResult<MuscleGroup>.NotFound($"Muscle group with id '{id}' not found.");
            
            db.MuscleGroups.Remove(group);
            await db.SaveChangesAsync();
            return DbResult<MuscleGroup>.Ok(group);
        }

        // ===== Exercises =====
        public async Task<DbResult<Exercise>> GetExerciseByIdAsync(Guid id)
        {
            var exercise = await db.Exercises
                .Include(e => e.User)
                .Include(e => e.MuscleGroup)
                .Include(e => e.SessionExercises)
                .FirstOrDefaultAsync(e => e.Id == id);
            
            if (exercise is null)
                return DbResult<Exercise>.NotFound($"Exercise with id '{id}' not found.");
            
            return DbResult<Exercise>.Ok(exercise);
        }

        public async Task<DbResult<IEnumerable<Exercise>>> GetExercisesByUserIdAsync(Guid? userId)
        {
            var exercises = await db.Exercises
                .Include(e => e.User)
                .Include(e => e.MuscleGroup)
                .Include(e => e.SessionExercises)
                .Where(e => e.UserId == userId || e.UserId == null)
                .ToListAsync();
            
            return DbResult<IEnumerable<Exercise>>.Ok(exercises);
        }

        public async Task<DbResult<IEnumerable<Exercise>>> GetExercisesByMuscleGroupIdAsync(Guid muscleGroupId)
        {
            var exercises = await db.Exercises
                .Include(e => e.User)
                .Include(e => e.MuscleGroup)
                .Include(e => e.SessionExercises)
                .Where(e => e.MuscleGroupId == muscleGroupId)
                .ToListAsync();
            
            return DbResult<IEnumerable<Exercise>>.Ok(exercises);
        }

        public async Task<DbResult<IEnumerable<Exercise>>> GetAllExercisesAsync()
        {
            var exercises = await db.Exercises
                .Include(e => e.User)
                .Include(e => e.MuscleGroup)
                .Include(e => e.SessionExercises)
                .ToListAsync();
            
            return DbResult<IEnumerable<Exercise>>.Ok(exercises);
        }

        public async Task<DbResult<Exercise>> AddExerciseAsync(Exercise exercise)
        {
            var result = await GetExercisesByUserIdAsync(exercise.UserId);
            if (!result.IsSuccess)
                return DbResult<Exercise>.DatabaseError(result.Message);
            
            var existing = result.Data?.FirstOrDefault(e => e.Name == exercise.Name);
            if (existing is not null)
                return DbResult<Exercise>.DuplicateName($"An exercise named '{exercise.Name}' already exists.");
            
            await db.Exercises.AddAsync(exercise);
            await db.SaveChangesAsync();
            return DbResult<Exercise>.Ok(exercise);
        }

        public async Task<DbResult<Exercise>> UpdateExerciseAsync(Exercise exercise)
        {
            var result = await GetExercisesByUserIdAsync(exercise.UserId);
            if (!result.IsSuccess)
                return DbResult<Exercise>.DatabaseError(result.Message);
            
            var existing = result.Data?.FirstOrDefault(e => e.Name == exercise.Name && e.Id != exercise.Id);
            if (existing is not null)
                return DbResult<Exercise>.DuplicateName($"An exercise named '{exercise.Name}' already exists.");
            
            db.Exercises.Update(exercise);
            await db.SaveChangesAsync();
            return DbResult<Exercise>.Ok(exercise);
        }

        public async Task<DbResult<Exercise>> DeleteExerciseAsync(Guid id)
        {
            var exercise = await db.Exercises.FindAsync(id);
            if (exercise is null)
                return DbResult<Exercise>.NotFound($"Exercise with id '{id}' not found.");
            
            db.Exercises.Remove(exercise);
            await db.SaveChangesAsync();
            return DbResult<Exercise>.Ok(exercise);
        }

        // ===== Workouts =====
        public async Task<DbResult<Workout>> GetWorkoutByIdAsync(Guid id)
        {
            var workout = await db.Workouts
                .Include(w => w.User)
                .Include(w => w.Sessions)
                    .ThenInclude(s => s.SessionExercises)
                        .ThenInclude(se => se.Exercise)
                .Include(w => w.DefaultExercises)
                    .ThenInclude(de => de.Exercise)
                .FirstOrDefaultAsync(w => w.Id == id);
            
            if (workout is null)
                return DbResult<Workout>.NotFound($"Workout with id '{id}' not found.");
            
            return DbResult<Workout>.Ok(workout);
        }

        public async Task<DbResult<IEnumerable<Workout>>> GetWorkoutsByUserIdAsync(Guid userId)
        {
            var workouts = await db.Workouts
                .Include(w => w.User)
                .Include(w => w.Sessions)
                    .ThenInclude(s => s.SessionExercises)
                        .ThenInclude(se => se.Exercise)
                .Include(w => w.DefaultExercises)
                    .ThenInclude(de => de.Exercise)
                .Where(w => w.UserId == userId)
                .ToListAsync();
            
            return DbResult<IEnumerable<Workout>>.Ok(workouts);
        }

        public async Task<DbResult<Workout>> AddWorkoutAsync(Workout workout, bool saveChanges = true)
        {
            try
            {
                var result = await GetWorkoutsByUserIdAsync(workout.UserId);
                if (!result.IsSuccess)
                    return DbResult<Workout>.DatabaseError(result.Message);
            
                var existing = result.Data?.FirstOrDefault(w => w.Name == workout.Name);
                if (existing is not null)
                    return DbResult<Workout>.DuplicateName($"A workout named '{workout.Name}' already exists.");

                await db.Workouts.AddAsync(workout);
                if (saveChanges)
                    await db.SaveChangesAsync();
                return DbResult<Workout>.Ok(workout);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<DbResult> SaveChangesAsync()
        {
            await db.SaveChangesAsync();
            return DbResult.Ok();
        }

        public async Task<DbResult<Workout>> UpdateWorkoutAsync(Workout workout)
        {
            try
            {
                var result = await GetWorkoutsByUserIdAsync(workout.UserId);
                if (!result.IsSuccess)
                    return DbResult<Workout>.DatabaseError(result.Message);
                
                var existing = result.Data?.FirstOrDefault(w => w.Name == workout.Name && w.Id != workout.Id);
                if (existing is not null)
                    return DbResult<Workout>.DuplicateName($"A workout named '{workout.Name}' already exists.");

                db.Workouts.Update(workout);
                await db.SaveChangesAsync();
                return DbResult<Workout>.Ok(workout);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<DbResult<Workout>> DeleteWorkoutAsync(Guid id)
        {
            var workout = await db.Workouts.FindAsync(id);
            if (workout is null)
                return DbResult<Workout>.NotFound($"Workout with id '{id}' not found.");
            
            db.Workouts.Remove(workout);
            await db.SaveChangesAsync();
            return DbResult<Workout>.Ok(workout);
        }
        
        public async Task<DbResult<WorkoutDefaultExercise>> AddWorkoutDefaultExerciseAsync(WorkoutDefaultExercise defaultExercise, bool saveChanges = true)
        {
            await db.WorkoutDefaultExercises.AddAsync(defaultExercise);
            if (saveChanges) await db.SaveChangesAsync();
            return DbResult<WorkoutDefaultExercise>.Ok(defaultExercise);
        }
        
        public async Task<DbResult<WorkoutDefaultExercise>> UpdateWorkoutDefaultExerciseAsync(WorkoutDefaultExercise defaultExercise)
        {
            var workoutDefaultExercise = await db.WorkoutDefaultExercises.FindAsync(defaultExercise.Id);
            if (workoutDefaultExercise is null)
                return DbResult<WorkoutDefaultExercise>.NotFound($"Workout with id '{defaultExercise.Id}' not found.");
            
            db.WorkoutDefaultExercises.Update(defaultExercise);
            await db.SaveChangesAsync();
            return DbResult<WorkoutDefaultExercise>.Ok(defaultExercise);
        }
        
        public async Task<DbResult<WorkoutDefaultExercise>> DeleteWorkoutDefaultExerciseAsync(WorkoutDefaultExercise defaultExercise)
        {
            var workoutDefaultExercise = await db.WorkoutDefaultExercises.FindAsync(defaultExercise.Id);
            if (workoutDefaultExercise is null)
                return DbResult<WorkoutDefaultExercise>.NotFound($"Workout with id '{defaultExercise.Id}' not found.");
            
            db.WorkoutDefaultExercises.Remove(defaultExercise);
            await db.SaveChangesAsync();
            return DbResult<WorkoutDefaultExercise>.Ok(defaultExercise);
        }
    }
}

