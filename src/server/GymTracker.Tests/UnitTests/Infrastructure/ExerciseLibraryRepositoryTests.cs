using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using GymTracker.Core.Entities;
using GymTracker.Infrastructure.Data;
using GymTracker.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GymTracker.Tests.UnitTests.Infrastructure
{
    [TestFixture]
    public class ExerciseLibraryRepositoryTests
    {
        private GymTrackerDbContext _context;
        private ExerciseLibraryRepository _repository;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var options = new DbContextOptionsBuilder<GymTrackerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GymTrackerDbContext(options);
            _repository = new ExerciseLibraryRepository(_context);
        }

        [TearDown]
        public void Teardown()
        {
            _context.Dispose();
        }

        #region Muscle Categories Tests

        [Test]
        public async Task GetMuscleCategoryByIdAsync_WithValidId_ReturnsCategory()
        {
            // Arrange
            var category = new MuscleCategory
            {
                Id = Guid.NewGuid(),
                Name = "Chest",
                UserId = null
            };
            _context.MuscleCategories.Add(category);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetMuscleCategoryByIdAsync(category.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(category.Id));
            Assert.That(result.Name, Is.EqualTo("Chest"));
        }

        [Test]
        public async Task GetMuscleCategoryByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = await _repository.GetMuscleCategoryByIdAsync(Guid.NewGuid());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetMuscleCategoriesByUserIdAsync_WithNullUserId_ReturnsOnlySystemDefaults()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var systemCategory = new MuscleCategory { Id = Guid.NewGuid(), Name = "System", UserId = null };
            var userCategory = new MuscleCategory { Id = Guid.NewGuid(), Name = "User", UserId = userId };

            _context.MuscleCategories.AddRange(systemCategory, userCategory);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetMuscleCategoriesByUserIdAsync(null);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.First().Name, Is.EqualTo("System"));
        }

        [Test]
        public async Task GetMuscleCategoriesByUserIdAsync_WithUserId_ReturnsUserAndSystemDefaults()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();
            var systemCategory = new MuscleCategory { Id = Guid.NewGuid(), Name = "System", UserId = null };
            var userCategory = new MuscleCategory { Id = Guid.NewGuid(), Name = "User", UserId = userId };
            var anotherUserCategory = new MuscleCategory { Id = Guid.NewGuid(), Name = "Another", UserId = anotherUserId };

            _context.MuscleCategories.AddRange(systemCategory, userCategory, anotherUserCategory);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetMuscleCategoriesByUserIdAsync(userId);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            var names = result.Select(x => x.Name).ToList();
            Assert.That(names, Does.Contain("System"));
            Assert.That(names, Does.Contain("User"));
            Assert.That(names, Does.Not.Contain("Another"));
        }

        [Test]
        public async Task GetAllMuscleCategoriesAsync_ReturnsAllCategories()
        {
            // Arrange
            var categories = new[]
            {
                new MuscleCategory { Id = Guid.NewGuid(), Name = "Category1", UserId = null },
                new MuscleCategory { Id = Guid.NewGuid(), Name = "Category2", UserId = Guid.NewGuid() },
                new MuscleCategory { Id = Guid.NewGuid(), Name = "Category3", UserId = Guid.NewGuid() }
            };

            _context.MuscleCategories.AddRange(categories);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllMuscleCategoriesAsync();

            // Assert
            Assert.That(result, Has.Count.EqualTo(3));
        }

        [Test]
        public async Task AddMuscleCategoryAsync_AddsCategory()
        {
            // Arrange
            var category = new MuscleCategory
            {
                Id = Guid.NewGuid(),
                Name = "Back",
                UserId = null
            };

            // Act
            await _repository.AddMuscleCategoryAsync(category);

            // Assert
            var result = await _context.MuscleCategories.FirstOrDefaultAsync(x => x.Id == category.Id);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Back"));
        }

        [Test]
        public async Task UpdateMuscleCategoryAsync_UpdatesCategory()
        {
            // Arrange
            var category = new MuscleCategory
            {
                Id = Guid.NewGuid(),
                Name = "Legs",
                UserId = null
            };
            _context.MuscleCategories.Add(category);
            await _context.SaveChangesAsync();

            category.Name = "Lower Body";

            // Act
            await _repository.UpdateMuscleCategoryAsync(category);

            // Assert
            var result = await _context.MuscleCategories.FirstOrDefaultAsync(x => x.Id == category.Id);
            Assert.That(result.Name, Is.EqualTo("Lower Body"));
        }

        [Test]
        public async Task DeleteMuscleCategoryAsync_DeletesCategory()
        {
            // Arrange
            var category = new MuscleCategory
            {
                Id = Guid.NewGuid(),
                Name = "Shoulders",
                UserId = null
            };
            _context.MuscleCategories.Add(category);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteMuscleCategoryAsync(category.Id);

            // Assert
            var result = await _context.MuscleCategories.FirstOrDefaultAsync(x => x.Id == category.Id);
            Assert.That(result, Is.Null);
        }

        #endregion

        #region Muscle Groups Tests

        [Test]
        public async Task GetMuscleGroupByIdAsync_WithValidId_ReturnsMuscleGroup()
        {
            // Arrange
            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category", UserId = null };
            var muscleGroup = new MuscleGroup
            {
                Id = Guid.NewGuid(),
                Name = "Upper Chest",
                CategoryId = category.Id,
                UserId = null
            };

            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.Add(muscleGroup);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetMuscleGroupByIdAsync(muscleGroup.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(muscleGroup.Id));
            Assert.That(result.Name, Is.EqualTo("Upper Chest"));
        }

        [Test]
        public async Task GetMuscleGroupsByUserIdAsync_WithNullUserId_ReturnsOnlySystemDefaults()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category", UserId = null };
            var systemGroup = new MuscleGroup
            {
                Id = Guid.NewGuid(),
                Name = "System Group",
                CategoryId = category.Id,
                UserId = null
            };
            var userGroup = new MuscleGroup
            {
                Id = Guid.NewGuid(),
                Name = "User Group",
                CategoryId = category.Id,
                UserId = userId
            };

            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.AddRange(systemGroup, userGroup);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetMuscleGroupsByUserIdAsync(null);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.First().Name, Is.EqualTo("System Group"));
        }

        [Test]
        public async Task GetMuscleGroupsByUserIdAsync_WithUserId_ReturnsUserAndSystemDefaults()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();
            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category", UserId = null };
            var systemGroup = new MuscleGroup
            {
                Id = Guid.NewGuid(),
                Name = "System",
                CategoryId = category.Id,
                UserId = null
            };
            var userGroup = new MuscleGroup
            {
                Id = Guid.NewGuid(),
                Name = "User",
                CategoryId = category.Id,
                UserId = userId
            };
            var anotherUserGroup = new MuscleGroup
            {
                Id = Guid.NewGuid(),
                Name = "Another",
                CategoryId = category.Id,
                UserId = anotherUserId
            };

            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.AddRange(systemGroup, userGroup, anotherUserGroup);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetMuscleGroupsByUserIdAsync(userId);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            var names = result.Select(x => x.Name).ToList();
            Assert.That(names, Does.Contain("System"));
            Assert.That(names, Does.Contain("User"));
            Assert.That(names, Does.Not.Contain("Another"));
        }

        [Test]
        public async Task GetMuscleGroupsByCategoryIdAsync_ReturnsMuscleGroupsByCategory()
        {
            // Arrange
            var category1 = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category1", UserId = null };
            var category2 = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category2", UserId = null };
            var group1 = new MuscleGroup { Id = Guid.NewGuid(), Name = "Group1", CategoryId = category1.Id, UserId = null };
            var group2 = new MuscleGroup { Id = Guid.NewGuid(), Name = "Group2", CategoryId = category1.Id, UserId = null };
            var group3 = new MuscleGroup { Id = Guid.NewGuid(), Name = "Group3", CategoryId = category2.Id, UserId = null };

            _context.MuscleCategories.AddRange(category1, category2);
            _context.MuscleGroups.AddRange(group1, group2, group3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetMuscleGroupsByCategoryIdAsync(category1.Id);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            var names = result.Select(x => x.Name).ToList();
            Assert.That(names, Does.Contain("Group1"));
            Assert.That(names, Does.Contain("Group2"));
        }

        [Test]
        public async Task AddMuscleGroupAsync_AddsMuscleGroup()
        {
            // Arrange
            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category", UserId = null };
            var muscleGroup = new MuscleGroup
            {
                Id = Guid.NewGuid(),
                Name = "New Group",
                CategoryId = category.Id,
                UserId = null
            };

            _context.MuscleCategories.Add(category);
            await _context.SaveChangesAsync();

            // Act
            await _repository.AddMuscleGroupAsync(muscleGroup);

            // Assert
            var result = await _context.MuscleGroups.FirstOrDefaultAsync(x => x.Id == muscleGroup.Id);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("New Group"));
        }

        [Test]
        public async Task UpdateMuscleGroupAsync_UpdatesMuscleGroup()
        {
            // Arrange
            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category", UserId = null };
            var muscleGroup = new MuscleGroup
            {
                Id = Guid.NewGuid(),
                Name = "Old Name",
                CategoryId = category.Id,
                UserId = null
            };

            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.Add(muscleGroup);
            await _context.SaveChangesAsync();

            muscleGroup.Name = "New Name";

            // Act
            await _repository.UpdateMuscleGroupAsync(muscleGroup);

            // Assert
            var result = await _context.MuscleGroups.FirstOrDefaultAsync(x => x.Id == muscleGroup.Id);
            Assert.That(result.Name, Is.EqualTo("New Name"));
        }

        [Test]
        public async Task DeleteMuscleGroupAsync_DeletesMuscleGroup()
        {
            // Arrange
            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category", UserId = null };
            var muscleGroup = new MuscleGroup
            {
                Id = Guid.NewGuid(),
                Name = "Group",
                CategoryId = category.Id,
                UserId = null
            };

            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.Add(muscleGroup);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteMuscleGroupAsync(muscleGroup.Id);

            // Assert
            var result = await _context.MuscleGroups.FirstOrDefaultAsync(x => x.Id == muscleGroup.Id);
            Assert.That(result, Is.Null);
        }

        #endregion

        #region Exercises Tests

        [Test]
        public async Task GetExerciseByIdAsync_WithValidId_ReturnsExercise()
        {
            // Arrange
            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category", UserId = null };
            var muscleGroup = new MuscleGroup { Id = Guid.NewGuid(), Name = "Group", CategoryId = category.Id, UserId = null };
            var exercise = new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Bench Press",
                MuscleGroupId = muscleGroup.Id,
                UserId = null
            };

            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.Add(muscleGroup);
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetExerciseByIdAsync(exercise.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(exercise.Id));
            Assert.That(result.Name, Is.EqualTo("Bench Press"));
        }

        [Test]
        public async Task GetExercisesByUserIdAsync_WithNullUserId_ReturnsOnlySystemDefaults()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category", UserId = null };
            var muscleGroup = new MuscleGroup { Id = Guid.NewGuid(), Name = "Group", CategoryId = category.Id, UserId = null };
            var systemExercise = new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "System Exercise",
                MuscleGroupId = muscleGroup.Id,
                UserId = null
            };
            var userExercise = new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "User Exercise",
                MuscleGroupId = muscleGroup.Id,
                UserId = userId
            };

            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.Add(muscleGroup);
            _context.Exercises.AddRange(systemExercise, userExercise);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetExercisesByUserIdAsync(null);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.First().Name, Is.EqualTo("System Exercise"));
        }

        [Test]
        public async Task GetExercisesByUserIdAsync_WithUserId_ReturnsUserAndSystemDefaults()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();
            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category", UserId = null };
            var muscleGroup = new MuscleGroup { Id = Guid.NewGuid(), Name = "Group", CategoryId = category.Id, UserId = null };
            var systemExercise = new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "System",
                MuscleGroupId = muscleGroup.Id,
                UserId = null
            };
            var userExercise = new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "User",
                MuscleGroupId = muscleGroup.Id,
                UserId = userId
            };
            var anotherUserExercise = new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Another",
                MuscleGroupId = muscleGroup.Id,
                UserId = anotherUserId
            };

            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.Add(muscleGroup);
            _context.Exercises.AddRange(systemExercise, userExercise, anotherUserExercise);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetExercisesByUserIdAsync(userId);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            var names = result.Select(x => x.Name).ToList();
            Assert.That(names, Does.Contain("System"));
            Assert.That(names, Does.Contain("User"));
            Assert.That(names, Does.Not.Contain("Another"));
        }

        [Test]
        public async Task GetExercisesByMuscleGroupIdAsync_ReturnsExercisesByMuscleGroup()
        {
            // Arrange
            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category", UserId = null };
            var muscleGroup1 = new MuscleGroup { Id = Guid.NewGuid(), Name = "Group1", CategoryId = category.Id, UserId = null };
            var muscleGroup2 = new MuscleGroup { Id = Guid.NewGuid(), Name = "Group2", CategoryId = category.Id, UserId = null };
            var exercise1 = new Exercise { Id = Guid.NewGuid(), Name = "Ex1", MuscleGroupId = muscleGroup1.Id, UserId = null };
            var exercise2 = new Exercise { Id = Guid.NewGuid(), Name = "Ex2", MuscleGroupId = muscleGroup1.Id, UserId = null };
            var exercise3 = new Exercise { Id = Guid.NewGuid(), Name = "Ex3", MuscleGroupId = muscleGroup2.Id, UserId = null };

            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.AddRange(muscleGroup1, muscleGroup2);
            _context.Exercises.AddRange(exercise1, exercise2, exercise3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetExercisesByMuscleGroupIdAsync(muscleGroup1.Id);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            var names = result.Select(x => x.Name).ToList();
            Assert.That(names, Does.Contain("Ex1"));
            Assert.That(names, Does.Contain("Ex2"));
        }

        [Test]
        public async Task AddExerciseAsync_AddsExercise()
        {
            // Arrange
            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category", UserId = null };
            var muscleGroup = new MuscleGroup { Id = Guid.NewGuid(), Name = "Group", CategoryId = category.Id, UserId = null };
            var exercise = new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "New Exercise",
                MuscleGroupId = muscleGroup.Id,
                UserId = null
            };

            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.Add(muscleGroup);
            await _context.SaveChangesAsync();

            // Act
            await _repository.AddExerciseAsync(exercise);

            // Assert
            var result = await _context.Exercises.FirstOrDefaultAsync(x => x.Id == exercise.Id);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("New Exercise"));
        }

        [Test]
        public async Task UpdateExerciseAsync_UpdatesExercise()
        {
            // Arrange
            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category", UserId = null };
            var muscleGroup = new MuscleGroup { Id = Guid.NewGuid(), Name = "Group", CategoryId = category.Id, UserId = null };
            var exercise = new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Old Name",
                MuscleGroupId = muscleGroup.Id,
                UserId = null
            };

            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.Add(muscleGroup);
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            exercise.Name = "New Name";

            // Act
            await _repository.UpdateExerciseAsync(exercise);

            // Assert
            var result = await _context.Exercises.FirstOrDefaultAsync(x => x.Id == exercise.Id);
            Assert.That(result.Name, Is.EqualTo("New Name"));
        }

        [Test]
        public async Task DeleteExerciseAsync_DeletesExercise()
        {
            // Arrange
            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category", UserId = null };
            var muscleGroup = new MuscleGroup { Id = Guid.NewGuid(), Name = "Group", CategoryId = category.Id, UserId = null };
            var exercise = new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Exercise",
                MuscleGroupId = muscleGroup.Id,
                UserId = null
            };

            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.Add(muscleGroup);
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteExerciseAsync(exercise.Id);

            // Assert
            var result = await _context.Exercises.FirstOrDefaultAsync(x => x.Id == exercise.Id);
            Assert.That(result, Is.Null);
        }

        #endregion

        #region Workouts Tests

        [Test]
        public async Task GetWorkoutByIdAsync_WithValidId_ReturnsWorkout()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Username = "testuser", PasswordHashed = "hash" };
            var workout = new Workout
            {
                Id = Guid.NewGuid(),
                Name = "Push Day",
                UserId = user.Id
            };

            _context.Users.Add(user);
            _context.Workouts.Add(workout);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetWorkoutByIdAsync(workout.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(workout.Id));
            Assert.That(result.Name, Is.EqualTo("Push Day"));
        }

        [Test]
        public async Task GetWorkoutsByUserIdAsync_ReturnsWorkoutsByUser()
        {
            // Arrange
            var user1 = new User { Id = Guid.NewGuid(), Username = "user1", PasswordHashed = "hash" };
            var user2 = new User { Id = Guid.NewGuid(), Username = "user2", PasswordHashed = "hash" };
            var workout1 = new Workout { Id = Guid.NewGuid(), Name = "Workout1", UserId = user1.Id };
            var workout2 = new Workout { Id = Guid.NewGuid(), Name = "Workout2", UserId = user1.Id };
            var workout3 = new Workout { Id = Guid.NewGuid(), Name = "Workout3", UserId = user2.Id };

            _context.Users.AddRange(user1, user2);
            _context.Workouts.AddRange(workout1, workout2, workout3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetWorkoutsByUserIdAsync(user1.Id);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            var names = result.Select(x => x.Name).ToList();
            Assert.That(names, Does.Contain("Workout1"));
            Assert.That(names, Does.Contain("Workout2"));
        }

        [Test]
        public async Task AddWorkoutAsync_AddsWorkout()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Username = "testuser", PasswordHashed = "hash" };
            var workout = new Workout
            {
                Id = Guid.NewGuid(),
                Name = "New Workout",
                UserId = user.Id
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            await _repository.AddWorkoutAsync(workout);

            // Assert
            var result = await _context.Workouts.FirstOrDefaultAsync(x => x.Id == workout.Id);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("New Workout"));
        }

        [Test]
        public async Task UpdateWorkoutAsync_UpdatesWorkout()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Username = "testuser", PasswordHashed = "hash" };
            var workout = new Workout
            {
                Id = Guid.NewGuid(),
                Name = "Old Name",
                UserId = user.Id
            };

            _context.Users.Add(user);
            _context.Workouts.Add(workout);
            await _context.SaveChangesAsync();

            workout.Name = "New Name";

            // Act
            await _repository.UpdateWorkoutAsync(workout);

            // Assert
            var result = await _context.Workouts.FirstOrDefaultAsync(x => x.Id == workout.Id);
            Assert.That(result.Name, Is.EqualTo("New Name"));
        }

        [Test]
        public async Task DeleteWorkoutAsync_DeletesWorkout()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Username = "testuser", PasswordHashed = "hash" };
            var workout = new Workout
            {
                Id = Guid.NewGuid(),
                Name = "Workout",
                UserId = user.Id
            };

            _context.Users.Add(user);
            _context.Workouts.Add(workout);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteWorkoutAsync(workout.Id);

            // Assert
            var result = await _context.Workouts.FirstOrDefaultAsync(x => x.Id == workout.Id);
            Assert.That(result, Is.Null);
        }

        #endregion

        #region Duplicate Name Tests

        [Test]
        public async Task AddMuscleCategoryAsync_WithDuplicateName_DoesNotAdd()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingCategory = new MuscleCategory
            {
                Id = Guid.NewGuid(),
                Name = "Chest",
                UserId = userId
            };
            _context.MuscleCategories.Add(existingCategory);
            await _context.SaveChangesAsync();

            var duplicateCategory = new MuscleCategory
            {
                Id = Guid.NewGuid(),
                Name = "Chest",
                UserId = userId
            };

            // Act
            await _repository.AddMuscleCategoryAsync(duplicateCategory);

            // Assert
            var result = await _context.MuscleCategories.CountAsync(x => x.Name == "Chest" && x.UserId == userId);
            Assert.That(result, Is.EqualTo(1), "Should not have added duplicate");
        }

        [Test]
        public async Task AddMuscleCategoryAsync_WithDuplicateNameSystemDefaults_DoesNotAdd()
        {
            // Arrange
            var existingCategory = new MuscleCategory
            {
                Id = Guid.NewGuid(),
                Name = "Chest",
                UserId = null
            };
            _context.MuscleCategories.Add(existingCategory);
            await _context.SaveChangesAsync();

            var duplicateCategory = new MuscleCategory
            {
                Id = Guid.NewGuid(),
                Name = "Chest",
                UserId = null
            };

            // Act
            await _repository.AddMuscleCategoryAsync(duplicateCategory);

            // Assert
            var result = await _context.MuscleCategories.CountAsync(x => x.Name == "Chest" && x.UserId == null);
            Assert.That(result, Is.EqualTo(1), "Should not have added duplicate system default");
        }

        [Test]
        public async Task AddMuscleCategoryAsync_WithSameNameDifferentUser_Adds()
        {
            // Arrange
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            var category1 = new MuscleCategory
            {
                Id = Guid.NewGuid(),
                Name = "Chest",
                UserId = userId1
            };
            _context.MuscleCategories.Add(category1);
            await _context.SaveChangesAsync();

            var category2 = new MuscleCategory
            {
                Id = Guid.NewGuid(),
                Name = "Chest",
                UserId = userId2
            };

            // Act
            await _repository.AddMuscleCategoryAsync(category2);

            // Assert
            var result = await _context.MuscleCategories.CountAsync(x => x.Name == "Chest");
            Assert.That(result, Is.EqualTo(2), "Should allow same name for different users");
        }

        [Test]
        public async Task UpdateMuscleCategoryAsync_WithDuplicateName_DoesNotUpdate()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var category1 = new MuscleCategory
            {
                Id = Guid.NewGuid(),
                Name = "Chest",
                UserId = userId
            };
            var category2 = new MuscleCategory
            {
                Id = Guid.NewGuid(),
                Name = "Back",
                UserId = userId
            };
            _context.MuscleCategories.AddRange(category1, category2);
            await _context.SaveChangesAsync();

            category2.Name = "Chest"; // Try to rename to existing name

            // Act
            await _repository.UpdateMuscleCategoryAsync(category2);

            // Assert
            var result = await _context.MuscleCategories.FirstOrDefaultAsync(x => x.Id == category2.Id);
            Assert.That(result.Name, Is.EqualTo("Back"), "Should not have updated to duplicate name");
        }

        [Test]
        public async Task UpdateMuscleCategoryAsync_WithSameName_Updates()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var category = new MuscleCategory
            {
                Id = Guid.NewGuid(),
                Name = "Chest",
                UserId = userId
            };
            _context.MuscleCategories.Add(category);
            await _context.SaveChangesAsync();

            category.Name = "Updated Chest";

            // Act
            await _repository.UpdateMuscleCategoryAsync(category);

            // Assert
            var result = await _context.MuscleCategories.FirstOrDefaultAsync(x => x.Id == category.Id);
            Assert.That(result.Name, Is.EqualTo("Updated Chest"), "Should allow updating the same item");
        }

        [Test]
        public async Task AddMuscleGroupAsync_WithDuplicateName_DoesNotAdd()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category", UserId = null };
            var existingGroup = new MuscleGroup
            {
                Id = Guid.NewGuid(),
                Name = "Upper Chest",
                CategoryId = category.Id,
                UserId = userId
            };
            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.Add(existingGroup);
            await _context.SaveChangesAsync();

            var duplicateGroup = new MuscleGroup
            {
                Id = Guid.NewGuid(),
                Name = "Upper Chest",
                CategoryId = category.Id,
                UserId = userId
            };

            // Act
            await _repository.AddMuscleGroupAsync(duplicateGroup);

            // Assert
            var result = await _context.MuscleGroups.CountAsync(x => x.Name == "Upper Chest" && x.UserId == userId);
            Assert.That(result, Is.EqualTo(1), "Should not have added duplicate");
        }

        [Test]
        public async Task UpdateMuscleGroupAsync_WithDuplicateName_DoesNotUpdate()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category", UserId = null };
            var group1 = new MuscleGroup
            {
                Id = Guid.NewGuid(),
                Name = "Upper Chest",
                CategoryId = category.Id,
                UserId = userId
            };
            var group2 = new MuscleGroup
            {
                Id = Guid.NewGuid(),
                Name = "Lower Chest",
                CategoryId = category.Id,
                UserId = userId
            };
            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.AddRange(group1, group2);
            await _context.SaveChangesAsync();

            group2.Name = "Upper Chest";

            // Act
            await _repository.UpdateMuscleGroupAsync(group2);

            // Assert
            var result = await _context.MuscleGroups.FirstOrDefaultAsync(x => x.Id == group2.Id);
            Assert.That(result.Name, Is.EqualTo("Lower Chest"), "Should not have updated to duplicate name");
        }

        [Test]
        public async Task AddExerciseAsync_WithDuplicateName_DoesNotAdd()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category", UserId = null };
            var muscleGroup = new MuscleGroup
            {
                Id = Guid.NewGuid(),
                Name = "Group",
                CategoryId = category.Id,
                UserId = null
            };
            var existingExercise = new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Bench Press",
                MuscleGroupId = muscleGroup.Id,
                UserId = userId
            };
            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.Add(muscleGroup);
            _context.Exercises.Add(existingExercise);
            await _context.SaveChangesAsync();

            var duplicateExercise = new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Bench Press",
                MuscleGroupId = muscleGroup.Id,
                UserId = userId
            };

            // Act
            await _repository.AddExerciseAsync(duplicateExercise);

            // Assert
            var result = await _context.Exercises.CountAsync(x => x.Name == "Bench Press" && x.UserId == userId);
            Assert.That(result, Is.EqualTo(1), "Should not have added duplicate");
        }

        [Test]
        public async Task UpdateExerciseAsync_WithDuplicateName_DoesNotUpdate()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category", UserId = null };
            var muscleGroup = new MuscleGroup
            {
                Id = Guid.NewGuid(),
                Name = "Group",
                CategoryId = category.Id,
                UserId = null
            };
            var exercise1 = new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Bench Press",
                MuscleGroupId = muscleGroup.Id,
                UserId = userId
            };
            var exercise2 = new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Dumbbell Press",
                MuscleGroupId = muscleGroup.Id,
                UserId = userId
            };
            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.Add(muscleGroup);
            _context.Exercises.AddRange(exercise1, exercise2);
            await _context.SaveChangesAsync();

            exercise2.Name = "Bench Press";

            // Act
            await _repository.UpdateExerciseAsync(exercise2);

            // Assert
            var result = await _context.Exercises.FirstOrDefaultAsync(x => x.Id == exercise2.Id);
            Assert.That(result.Name, Is.EqualTo("Dumbbell Press"), "Should not have updated to duplicate name");
        }

        [Test]
        public async Task AddWorkoutAsync_WithDuplicateName_DoesNotAdd()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Username = "testuser", PasswordHashed = "hash" };
            var existingWorkout = new Workout
            {
                Id = Guid.NewGuid(),
                Name = "Push Day",
                UserId = user.Id
            };
            _context.Users.Add(user);
            _context.Workouts.Add(existingWorkout);
            await _context.SaveChangesAsync();

            var duplicateWorkout = new Workout
            {
                Id = Guid.NewGuid(),
                Name = "Push Day",
                UserId = user.Id
            };

            // Act
            await _repository.AddWorkoutAsync(duplicateWorkout);

            // Assert
            var result = await _context.Workouts.CountAsync(x => x.Name == "Push Day" && x.UserId == user.Id);
            Assert.That(result, Is.EqualTo(1), "Should not have added duplicate");
        }

        [Test]
        public async Task UpdateWorkoutAsync_WithDuplicateName_DoesNotUpdate()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Username = "testuser", PasswordHashed = "hash" };
            var workout1 = new Workout
            {
                Id = Guid.NewGuid(),
                Name = "Push Day",
                UserId = user.Id
            };
            var workout2 = new Workout
            {
                Id = Guid.NewGuid(),
                Name = "Pull Day",
                UserId = user.Id
            };
            _context.Users.Add(user);
            _context.Workouts.AddRange(workout1, workout2);
            await _context.SaveChangesAsync();

            workout2.Name = "Push Day";

            // Act
            await _repository.UpdateWorkoutAsync(workout2);

            // Assert
            var result = await _context.Workouts.FirstOrDefaultAsync(x => x.Id == workout2.Id);
            Assert.That(result.Name, Is.EqualTo("Pull Day"), "Should not have updated to duplicate name");
        }

        #endregion
    }
}
