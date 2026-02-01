using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GymTracker.Core.Entities;
using GymTracker.Core.Interfaces;
using GymTracker.Infrastructure.Data;

namespace GymTracker.Infrastructure.Repositories
{
    public class ExerciseLibraryRepository(GymTrackerDbContext db) : IExerciseLibraryRepository
    {
        // ===== Muscle Categories =====
        public async Task<MuscleCategory?> GetMuscleCategoryByIdAsync(Guid id)
        {
            return await db.MuscleCategories
                .Include(mc => mc.User)
                .FirstOrDefaultAsync(mc => mc.Id == id);
        }

        public async Task<IEnumerable<MuscleCategory>> GetMuscleCategoriesByUserIdAsync(Guid? userId)
        {
            return await db.MuscleCategories
                .Include(mc => mc.User)
                .Where(mc => mc.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<MuscleCategory>> GetAllMuscleCategoriesAsync()
        {
            return await db.MuscleCategories
                .Include(mc => mc.User)
                .ToListAsync();
        }

        public async Task AddMuscleCategoryAsync(MuscleCategory category)
        {
            await db.MuscleCategories.AddAsync(category);
            await db.SaveChangesAsync();
        }

        public async Task UpdateMuscleCategoryAsync(MuscleCategory category)
        {
            db.MuscleCategories.Update(category);
            await db.SaveChangesAsync();
        }

        public async Task DeleteMuscleCategoryAsync(Guid id)
        {
            var category = await db.MuscleCategories.FindAsync(id);
            if (category is null) return;
            db.MuscleCategories.Remove(category);
            await db.SaveChangesAsync();
        }

        // ===== Muscle Groups =====
        public async Task<MuscleGroup?> GetMuscleGroupByIdAsync(Guid id)
        {
            return await db.MuscleGroups
                .Include(mg => mg.User)
                .Include(mg => mg.Category)
                .Include(mg => mg.Exercises)
                .FirstOrDefaultAsync(mg => mg.Id == id);
        }

        public async Task<IEnumerable<MuscleGroup>> GetMuscleGroupsByUserIdAsync(Guid? userId)
        {
            return await db.MuscleGroups
                .Include(mg => mg.User)
                .Include(mg => mg.Category)
                .Include(mg => mg.Exercises)
                .Where(mg => mg.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<MuscleGroup>> GetMuscleGroupsByCategoryIdAsync(Guid categoryId)
        {
            return await db.MuscleGroups
                .Include(mg => mg.User)
                .Include(mg => mg.Category)
                .Include(mg => mg.Exercises)
                .Where(mg => mg.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<MuscleGroup>> GetAllMuscleGroupsAsync()
        {
            return await db.MuscleGroups
                .Include(mg => mg.User)
                .Include(mg => mg.Category)
                .Include(mg => mg.Exercises)
                .ToListAsync();
        }

        public async Task AddMuscleGroupAsync(MuscleGroup group)
        {
            await db.MuscleGroups.AddAsync(group);
            await db.SaveChangesAsync();
        }

        public async Task UpdateMuscleGroupAsync(MuscleGroup group)
        {
            db.MuscleGroups.Update(group);
            await db.SaveChangesAsync();
        }

        public async Task DeleteMuscleGroupAsync(Guid id)
        {
            var group = await db.MuscleGroups.FindAsync(id);
            if (group is null) return;
            db.MuscleGroups.Remove(group);
            await db.SaveChangesAsync();
        }

        // ===== Exercises =====
        public async Task<Exercise?> GetExerciseByIdAsync(Guid id)
        {
            return await db.Exercises
                .Include(e => e.User)
                .Include(e => e.MuscleGroup)
                .Include(e => e.SessionExercises)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Exercise>> GetExercisesByUserIdAsync(Guid? userId)
        {
            return await db.Exercises
                .Include(e => e.User)
                .Include(e => e.MuscleGroup)
                .Include(e => e.SessionExercises)
                .Where(e => e.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Exercise>> GetExercisesByMuscleGroupIdAsync(Guid muscleGroupId)
        {
            return await db.Exercises
                .Include(e => e.User)
                .Include(e => e.MuscleGroup)
                .Include(e => e.SessionExercises)
                .Where(e => e.MuscleGroupId == muscleGroupId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Exercise>> GetAllExercisesAsync()
        {
            return await db.Exercises
                .Include(e => e.User)
                .Include(e => e.MuscleGroup)
                .Include(e => e.SessionExercises)
                .ToListAsync();
        }

        public async Task AddExerciseAsync(Exercise exercise)
        {
            await db.Exercises.AddAsync(exercise);
            await db.SaveChangesAsync();
        }

        public async Task UpdateExerciseAsync(Exercise exercise)
        {
            db.Exercises.Update(exercise);
            await db.SaveChangesAsync();
        }

        public async Task DeleteExerciseAsync(Guid id)
        {
            var exercise = await db.Exercises.FindAsync(id);
            if (exercise is null) return;
            db.Exercises.Remove(exercise);
            await db.SaveChangesAsync();
        }

        // ===== Workouts =====
        public async Task<Workout?> GetWorkoutByIdAsync(Guid id)
        {
            return await db.Workouts
                .Include(w => w.User)
                .Include(w => w.Sessions)
                    .ThenInclude(s => s.SessionExercises)
                        .ThenInclude(se => se.Exercise)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<IEnumerable<Workout>> GetWorkoutsByUserIdAsync(Guid userId)
        {
            return await db.Workouts
                .Include(w => w.User)
                .Include(w => w.Sessions)
                    .ThenInclude(s => s.SessionExercises)
                        .ThenInclude(se => se.Exercise)
                .Where(w => w.UserId == userId)
                .ToListAsync();
        }

        public async Task AddWorkoutAsync(Workout workout)
        {
            await db.Workouts.AddAsync(workout);
            await db.SaveChangesAsync();
        }

        public async Task UpdateWorkoutAsync(Workout workout)
        {
            db.Workouts.Update(workout);
            await db.SaveChangesAsync();
        }

        public async Task DeleteWorkoutAsync(Guid id)
        {
            var workout = await db.Workouts.FindAsync(id);
            if (workout is null) return;
            db.Workouts.Remove(workout);
            await db.SaveChangesAsync();
        }
    }
}
