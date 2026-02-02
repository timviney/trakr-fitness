using GymTracker.Core.Entities;
using GymTracker.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Api.Setup;

public static class DataSeeder
{
    public static async Task SeedDefaultDataAsync(GymTrackerDbContext context)
    {
        // Check if data already exists
        if (await context.Users.AnyAsync())
            return;

        // Create test user
        var ph = new PasswordHasher<User>();
        var testUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "timmyv",
            PasswordHashed = ph.HashPassword(null!, "password"),
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(testUser);

        // Create system default muscle categories (UserId = null)
        var pushCategoryId = Guid.NewGuid();
        var pullCategoryId = Guid.NewGuid();
        var legsCategoryId = Guid.NewGuid();

        var muscleCategories = new[]
        {
            new MuscleCategory { Id = pushCategoryId, UserId = null, Name = "Push" },
            new MuscleCategory { Id = pullCategoryId, UserId = null, Name = "Pull" },
            new MuscleCategory { Id = legsCategoryId, UserId = null, Name = "Legs" }
        };

        context.MuscleCategories.AddRange(muscleCategories);

        // Create system default muscle groups (UserId = null)
        var pushMuscleGroups = new[]
        {
            new MuscleGroup { Id = Guid.NewGuid(), UserId = null, CategoryId = pushCategoryId, Name = "Chest" },
            new MuscleGroup { Id = Guid.NewGuid(), UserId = null, CategoryId = pushCategoryId, Name = "Shoulders" },
            new MuscleGroup { Id = Guid.NewGuid(), UserId = null, CategoryId = pushCategoryId, Name = "Triceps" }
        };

        var pullMuscleGroups = new[]
        {
            new MuscleGroup { Id = Guid.NewGuid(), UserId = null, CategoryId = pullCategoryId, Name = "Back" },
            new MuscleGroup { Id = Guid.NewGuid(), UserId = null, CategoryId = pullCategoryId, Name = "Biceps" },
            new MuscleGroup { Id = Guid.NewGuid(), UserId = null, CategoryId = pullCategoryId, Name = "Rear Delts" }
        };

        var legsMuscleGroups = new[]
        {
            new MuscleGroup { Id = Guid.NewGuid(), UserId = null, CategoryId = legsCategoryId, Name = "Quadriceps" },
            new MuscleGroup { Id = Guid.NewGuid(), UserId = null, CategoryId = legsCategoryId, Name = "Hamstrings" },
            new MuscleGroup { Id = Guid.NewGuid(), UserId = null, CategoryId = legsCategoryId, Name = "Calves" }
        };

        context.MuscleGroups.AddRange(pushMuscleGroups);
        context.MuscleGroups.AddRange(pullMuscleGroups);
        context.MuscleGroups.AddRange(legsMuscleGroups);

        // Create exercises for each muscle group
        var exercises = new List<Exercise>();

        // Push exercises
        exercises.AddRange(new[]
        {
            new Exercise { Id = Guid.NewGuid(), UserId = null, MuscleGroupId = pushMuscleGroups[0].Id, Name = "Bench Press" },
            new Exercise { Id = Guid.NewGuid(), UserId = null, MuscleGroupId = pushMuscleGroups[0].Id, Name = "Dumbbell Flys" },
            new Exercise { Id = Guid.NewGuid(), UserId = null, MuscleGroupId = pushMuscleGroups[1].Id, Name = "Overhead Press" },
            new Exercise { Id = Guid.NewGuid(), UserId = null, MuscleGroupId = pushMuscleGroups[1].Id, Name = "Lateral Raises" },
            new Exercise { Id = Guid.NewGuid(), UserId = null, MuscleGroupId = pushMuscleGroups[2].Id, Name = "Tricep Dips" },
            new Exercise { Id = Guid.NewGuid(), UserId = null, MuscleGroupId = pushMuscleGroups[2].Id, Name = "Skull Crushers" }
        });

        // Pull exercises
        exercises.AddRange(new[]
        {
            new Exercise { Id = Guid.NewGuid(), UserId = null, MuscleGroupId = pullMuscleGroups[0].Id, Name = "Pull Ups" },
            new Exercise { Id = Guid.NewGuid(), UserId = null, MuscleGroupId = pullMuscleGroups[0].Id, Name = "Barbell Rows" },
            new Exercise { Id = Guid.NewGuid(), UserId = null, MuscleGroupId = pullMuscleGroups[1].Id, Name = "Barbell Curls" },
            new Exercise { Id = Guid.NewGuid(), UserId = null, MuscleGroupId = pullMuscleGroups[1].Id, Name = "Dumbbell Curls" },
            new Exercise { Id = Guid.NewGuid(), UserId = null, MuscleGroupId = pullMuscleGroups[2].Id, Name = "Face Pulls" },
            new Exercise { Id = Guid.NewGuid(), UserId = null, MuscleGroupId = pullMuscleGroups[2].Id, Name = "Reverse Flys" }
        });

        // Legs exercises
        exercises.AddRange(new[]
        {
            new Exercise { Id = Guid.NewGuid(), UserId = null, MuscleGroupId = legsMuscleGroups[0].Id, Name = "Squats" },
            new Exercise { Id = Guid.NewGuid(), UserId = null, MuscleGroupId = legsMuscleGroups[0].Id, Name = "Leg Press" },
            new Exercise { Id = Guid.NewGuid(), UserId = null, MuscleGroupId = legsMuscleGroups[1].Id, Name = "Deadlifts" },
            new Exercise { Id = Guid.NewGuid(), UserId = null, MuscleGroupId = legsMuscleGroups[1].Id, Name = "Leg Curls" },
            new Exercise { Id = Guid.NewGuid(), UserId = null, MuscleGroupId = legsMuscleGroups[2].Id, Name = "Standing Calf Raises" },
            new Exercise { Id = Guid.NewGuid(), UserId = null, MuscleGroupId = legsMuscleGroups[2].Id, Name = "Seated Calf Raises" }
        });

        context.Exercises.AddRange(exercises);

        // Create system default workouts for each user
        var pushWorkout = new Workout { Id = Guid.NewGuid(), UserId = testUser.Id, Name = "Push" };
        var pullWorkout = new Workout { Id = Guid.NewGuid(), UserId = testUser.Id, Name = "Pull" };
        var legsWorkout = new Workout { Id = Guid.NewGuid(), UserId = testUser.Id, Name = "Legs" };

        context.Workouts.AddRange(pushWorkout, pullWorkout, legsWorkout);

        // Save all changes
        await context.SaveChangesAsync();
    }
}
