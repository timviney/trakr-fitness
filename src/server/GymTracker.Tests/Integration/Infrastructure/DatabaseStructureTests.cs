using AutoFixture;
using GymTracker.Core.Entities;
using GymTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shouldly;

namespace GymTracker.Tests.Integration.Infrastructure
{
    [TestFixture]
    [Category("Integration")]
    public class DatabaseStructureTests
    {
        private GymTrackerDbContext _context = null!;
        private Fixture _fixture = null!;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var options = new DbContextOptionsBuilder<GymTrackerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GymTrackerDbContext(options);
        }

        [TearDown]
        public void Teardown()
        {
            _context.Dispose();
        }

        #region User Table Tests

        [Test]
        public async Task CanInsertUser()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test-user",
                PasswordHashed = "hashed-password",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            result.ShouldNotBeNull();
            result.Email.ShouldBe("test-user");
        }

        [Test]
        public Task UserIdIsRequired()
        {
            // Arrange
            var user = new User
            {
                Email = "test-user",
                PasswordHashed = "hashed-password",
                CreatedAt = DateTime.UtcNow
            };

            // Act & Assert
            return Should.NotThrowAsync(async () =>
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }).ContinueWith(_ =>
            {
                user.Id.ShouldNotBe(Guid.Empty);
            });
        }

        #endregion

        #region Muscle Categories Tests

        [Test]
        public async Task CanInsertMuscleCategoryWithNullUserId()
        {
            // Arrange
            var category = new MuscleCategory
            {
                Id = Guid.NewGuid(),
                Name = "System Category",
                UserId = null
            };

            // Act
            _context.MuscleCategories.Add(category);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.MuscleCategories.FirstOrDefaultAsync(c => c.Id == category.Id);
            result.ShouldNotBeNull();
            result.UserId.ShouldBeNull();
        }

        [Test]
        public async Task CanInsertMuscleCategoryWithUserId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = "test-user",
                PasswordHashed = "hash",
                CreatedAt = DateTime.UtcNow
            };

            var category = new MuscleCategory
            {
                Id = Guid.NewGuid(),
                Name = "User Category",
                UserId = userId
            };

            _context.Users.Add(user);
            _context.MuscleCategories.Add(category);

            // Act
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.MuscleCategories
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == category.Id);
            result.ShouldNotBeNull();
            result.UserId.ShouldBe(userId);
            result.User.ShouldNotBeNull();
        }

        #endregion

        #region Muscle Groups Tests

        [Test]
        public async Task CanInsertMuscleGroupWithCategoryReference()
        {
            // Arrange
            var category = new MuscleCategory
            {
                Id = Guid.NewGuid(),
                Name = "Category",
                UserId = null
            };

            var group = new MuscleGroup
            {
                Id = Guid.NewGuid(),
                Name = "Upper Chest",
                CategoryId = category.Id,
                UserId = null
            };

            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.Add(group);

            // Act
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.MuscleGroups
                .Include(g => g.Category)
                .FirstOrDefaultAsync(g => g.Id == group.Id);
            result.ShouldNotBeNull();
            result.CategoryId.ShouldBe(category.Id);
            result.Category.Name.ShouldBe("Category");
        }

        [Test]
        public async Task CanInsertMuscleGroupWithAndWithoutUserId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = "test-user",
                PasswordHashed = "hash",
                CreatedAt = DateTime.UtcNow
            };

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

            _context.Users.Add(user);
            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.AddRange(systemGroup, userGroup);

            // Act
            await _context.SaveChangesAsync();

            // Assert
            var systemResult = await _context.MuscleGroups.FirstOrDefaultAsync(g => g.Id == systemGroup.Id);
            var userResult = await _context.MuscleGroups.FirstOrDefaultAsync(g => g.Id == userGroup.Id);

            systemResult.ShouldNotBeNull();
            userResult.ShouldNotBeNull();
            systemResult.UserId.ShouldBeNull();
            userResult.UserId.ShouldBe(userId);
        }

        #endregion

        #region Exercises Tests

        [Test]
        public async Task CanInsertExerciseWithMuscleGroupReference()
        {
            // Arrange
            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category", UserId = null };
            var group = new MuscleGroup { Id = Guid.NewGuid(), Name = "Group", CategoryId = category.Id, UserId = null };
            var exercise = new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Bench Press",
                MuscleGroupId = group.Id,
                UserId = null
            };

            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.Add(group);
            _context.Exercises.Add(exercise);

            // Act
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Exercises
                .Include(e => e.MuscleGroup)
                .FirstOrDefaultAsync(e => e.Id == exercise.Id);
            result.ShouldNotBeNull();
            result.MuscleGroupId.ShouldBe(group.Id);
            result.MuscleGroup.Name.ShouldBe("Group");
        }

        [Test]
        public async Task SystemExercisesAndUserExercisesCanCoexist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = "test-user", PasswordHashed = "hash", CreatedAt = DateTime.UtcNow };
            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category", UserId = null };
            var group = new MuscleGroup { Id = Guid.NewGuid(), Name = "Group", CategoryId = category.Id, UserId = null };

            var systemExercise = new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "System Exercise",
                MuscleGroupId = group.Id,
                UserId = null
            };

            var userExercise = new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "User Exercise",
                MuscleGroupId = group.Id,
                UserId = userId
            };

            _context.Users.Add(user);
            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.Add(group);
            _context.Exercises.AddRange(systemExercise, userExercise);

            // Act
            await _context.SaveChangesAsync();

            // Assert
            var systemResult = await _context.Exercises.FirstOrDefaultAsync(e => e.Id == systemExercise.Id);
            var userResult = await _context.Exercises.FirstOrDefaultAsync(e => e.Id == userExercise.Id);

            systemResult.ShouldNotBeNull();
            userResult.ShouldNotBeNull();
            systemResult.UserId.ShouldBeNull();
            userResult.UserId.ShouldBe(userId);
        }

        #endregion

        #region Workouts Tests

        [Test]
        public async Task CanInsertWorkoutWithUserReference()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Email = "test-user", PasswordHashed = "hash", CreatedAt = DateTime.UtcNow };
            var workout = new Workout { Id = Guid.NewGuid(), Name = "Push Day", UserId = user.Id };

            _context.Users.Add(user);
            _context.Workouts.Add(workout);

            // Act
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Workouts
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.Id == workout.Id);
            result.ShouldNotBeNull();
            result.UserId.ShouldBe(user.Id);
            result.User.Email.ShouldBe("test-user");
        }

        #endregion

        #region Sessions Tests

        [Test]
        public async Task CanInsertSessionWithWorkoutReference()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Email = "test-user", PasswordHashed = "hash", CreatedAt = DateTime.UtcNow };
            var workout = new Workout { Id = Guid.NewGuid(), Name = "Workout", UserId = user.Id };
            var session = new Session { Id = Guid.NewGuid(), WorkoutId = workout.Id, CreatedAt = DateTime.UtcNow };

            _context.Users.Add(user);
            _context.Workouts.Add(workout);
            _context.Sessions.Add(session);

            // Act
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Sessions
                .Include(s => s.Workout)
                .FirstOrDefaultAsync(s => s.Id == session.Id);
            result.ShouldNotBeNull();
            result.WorkoutId.ShouldBe(workout.Id);
        }

        #endregion

        #region Session Exercises Tests

        [Test]
        public async Task CanInsertSessionExerciseWithReferences()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Email = "test-user", PasswordHashed = "hash", CreatedAt = DateTime.UtcNow };
            var workout = new Workout { Id = Guid.NewGuid(), Name = "Workout", UserId = user.Id };
            var session = new Session { Id = Guid.NewGuid(), WorkoutId = workout.Id, CreatedAt = DateTime.UtcNow };
            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category", UserId = null };
            var group = new MuscleGroup { Id = Guid.NewGuid(), Name = "Group", CategoryId = category.Id, UserId = null };
            var exercise = new Exercise { Id = Guid.NewGuid(), Name = "Exercise", MuscleGroupId = group.Id, UserId = null };
            var sessionExercise = new SessionExercise
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                ExerciseId = exercise.Id,
                ExerciseNumber = 1
            };

            _context.Users.Add(user);
            _context.Workouts.Add(workout);
            _context.Sessions.Add(session);
            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.Add(group);
            _context.Exercises.Add(exercise);
            _context.SessionExercises.Add(sessionExercise);

            // Act
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.SessionExercises
                .Include(se => se.Session)
                .Include(se => se.Exercise)
                .FirstOrDefaultAsync(se => se.Id == sessionExercise.Id);
            result.ShouldNotBeNull();
            result.SessionId.ShouldBe(session.Id);
            result.ExerciseId.ShouldBe(exercise.Id);
            result.ExerciseNumber.ShouldBe(1);
        }

        #endregion

        #region Sets Tests

        [Test]
        public async Task CanInsertSetWithSessionExerciseReference()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Email = "test-user", PasswordHashed = "hash", CreatedAt = DateTime.UtcNow };
            var workout = new Workout { Id = Guid.NewGuid(), Name = "Workout", UserId = user.Id };
            var session = new Session { Id = Guid.NewGuid(), WorkoutId = workout.Id, CreatedAt = DateTime.UtcNow };
            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category", UserId = null };
            var group = new MuscleGroup { Id = Guid.NewGuid(), Name = "Group", CategoryId = category.Id, UserId = null };
            var exercise = new Exercise { Id = Guid.NewGuid(), Name = "Exercise", MuscleGroupId = group.Id, UserId = null };
            var sessionExercise = new SessionExercise
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                ExerciseId = exercise.Id,
                ExerciseNumber = 1
            };
            var set = new Set
            {
                Id = Guid.NewGuid(),
                SessionExerciseId = sessionExercise.Id,
                SetNumber = 1,
                Weight = 100.0,
                Reps = 10
            };

            _context.Users.Add(user);
            _context.Workouts.Add(workout);
            _context.Sessions.Add(session);
            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.Add(group);
            _context.Exercises.Add(exercise);
            _context.SessionExercises.Add(sessionExercise);
            _context.Sets.Add(set);

            // Act
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Sets
                .Include(s => s.SessionExercise)
                .FirstOrDefaultAsync(s => s.Id == set.Id);
            result.ShouldNotBeNull();
            result.SessionExerciseId.ShouldBe(sessionExercise.Id);
            result.Weight.ShouldBe(100.0);
            result.Reps.ShouldBe(10);
        }

        #endregion

        #region Cascade Delete Tests

        [Test]
        public async Task DeletingUserCascadesDeleteToWorkouts()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Email = "test-user", PasswordHashed = "hash", CreatedAt = DateTime.UtcNow };
            var workout = new Workout { Id = Guid.NewGuid(), Name = "Workout", UserId = user.Id };

            _context.Users.Add(user);
            _context.Workouts.Add(workout);
            await _context.SaveChangesAsync();

            // Act
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            // Assert
            var workoutResult = await _context.Workouts.FirstOrDefaultAsync(w => w.Id == workout.Id);
            workoutResult.ShouldBeNull();
        }

        [Test]
        public async Task DeletingWorkoutCascadesDeleteToSessions()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Email = "test-user", PasswordHashed = "hash", CreatedAt = DateTime.UtcNow };
            var workout = new Workout { Id = Guid.NewGuid(), Name = "Workout", UserId = user.Id };
            var session = new Session { Id = Guid.NewGuid(), WorkoutId = workout.Id };

            _context.Users.Add(user);
            _context.Workouts.Add(workout);
            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            // Act
            _context.Workouts.Remove(workout);
            await _context.SaveChangesAsync();

            // Assert
            var sessionResult = await _context.Sessions.FirstOrDefaultAsync(s => s.Id == session.Id);
            sessionResult.ShouldBeNull();
        }

        [Test]
        public async Task DeletingSessionCascadesDeleteToSessionExercises()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Email = "test-user", PasswordHashed = "hash", CreatedAt = DateTime.UtcNow };
            var workout = new Workout { Id = Guid.NewGuid(), Name = "Workout", UserId = user.Id };
            var session = new Session { Id = Guid.NewGuid(), WorkoutId = workout.Id };
            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category", UserId = null };
            var group = new MuscleGroup { Id = Guid.NewGuid(), Name = "Group", CategoryId = category.Id, UserId = null };
            var exercise = new Exercise { Id = Guid.NewGuid(), Name = "Exercise", MuscleGroupId = group.Id, UserId = null };
            var sessionExercise = new SessionExercise { Id = Guid.NewGuid(), SessionId = session.Id, ExerciseId = exercise.Id, ExerciseNumber = 1 };

            _context.Users.Add(user);
            _context.Workouts.Add(workout);
            _context.Sessions.Add(session);
            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.Add(group);
            _context.Exercises.Add(exercise);
            _context.SessionExercises.Add(sessionExercise);
            await _context.SaveChangesAsync();

            // Act
            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.SessionExercises.FirstOrDefaultAsync(se => se.Id == sessionExercise.Id);
            result.ShouldBeNull();
        }

        [Test]
        public async Task DeletingSessionExerciseCascadesDeleteToSets()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Email = "test-user", PasswordHashed = "hash", CreatedAt = DateTime.UtcNow };
            var workout = new Workout { Id = Guid.NewGuid(), Name = "Workout", UserId = user.Id };
            var session = new Session { Id = Guid.NewGuid(), WorkoutId = workout.Id };
            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category", UserId = null };
            var group = new MuscleGroup { Id = Guid.NewGuid(), Name = "Group", CategoryId = category.Id, UserId = null };
            var exercise = new Exercise { Id = Guid.NewGuid(), Name = "Exercise", MuscleGroupId = group.Id, UserId = null };
            var sessionExercise = new SessionExercise { Id = Guid.NewGuid(), SessionId = session.Id, ExerciseId = exercise.Id, ExerciseNumber = 1 };
            var set = new Set { Id = Guid.NewGuid(), SessionExerciseId = sessionExercise.Id, SetNumber = 1, Weight = 100, Reps = 10 };

            _context.Users.Add(user);
            _context.Workouts.Add(workout);
            _context.Sessions.Add(session);
            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.Add(group);
            _context.Exercises.Add(exercise);
            _context.SessionExercises.Add(sessionExercise);
            _context.Sets.Add(set);
            await _context.SaveChangesAsync();

            // Act
            _context.SessionExercises.Remove(sessionExercise);
            await _context.SaveChangesAsync();

            // Assert
            var setResult = await _context.Sets.FirstOrDefaultAsync(s => s.Id == set.Id);
            setResult.ShouldBeNull();
        }

        #endregion

        #region Full Workflow Tests

        [Test]
        public async Task CompleteWorkflowFromUserToSet()
        {
            // Create a complete workout session with exercises and sets
            
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Email = "complete-user", PasswordHashed = "hash", CreatedAt = DateTime.UtcNow };
            var workout = new Workout { Id = Guid.NewGuid(), Name = "Full Body", UserId = user.Id };
            var session = new Session { Id = Guid.NewGuid(), WorkoutId = workout.Id, CreatedAt = DateTime.UtcNow };

            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Chest", UserId = null };
            var muscleGroup = new MuscleGroup { Id = Guid.NewGuid(), Name = "Upper Chest", CategoryId = category.Id, UserId = null };
            var exercise1 = new Exercise { Id = Guid.NewGuid(), Name = "Bench Press", MuscleGroupId = muscleGroup.Id, UserId = null };
            var exercise2 = new Exercise { Id = Guid.NewGuid(), Name = "Incline Press", MuscleGroupId = muscleGroup.Id, UserId = null };

            var sessionExercise1 = new SessionExercise { Id = Guid.NewGuid(), SessionId = session.Id, ExerciseId = exercise1.Id, ExerciseNumber = 1 };
            var sessionExercise2 = new SessionExercise { Id = Guid.NewGuid(), SessionId = session.Id, ExerciseId = exercise2.Id, ExerciseNumber = 2 };

            var set1 = new Set { Id = Guid.NewGuid(), SessionExerciseId = sessionExercise1.Id, SetNumber = 1, Weight = 100, Reps = 10 };
            var set2 = new Set { Id = Guid.NewGuid(), SessionExerciseId = sessionExercise1.Id, SetNumber = 2, Weight = 95, Reps = 12 };
            var set3 = new Set { Id = Guid.NewGuid(), SessionExerciseId = sessionExercise2.Id, SetNumber = 1, Weight = 80, Reps = 12 };

            _context.Users.Add(user);
            _context.Workouts.Add(workout);
            _context.Sessions.Add(session);
            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.Add(muscleGroup);
            _context.Exercises.AddRange(exercise1, exercise2);
            _context.SessionExercises.AddRange(sessionExercise1, sessionExercise2);
            _context.Sets.AddRange(set1, set2, set3);

            // Act
            await _context.SaveChangesAsync();

            // Assert
            var retrievedSession = await _context.Sessions
                .Include(s => s.Workout)
                    .ThenInclude(w => w.User)
                .Include(s => s.SessionExercises)
                    .ThenInclude(se => se.Exercise)
                        .ThenInclude(e => e.MuscleGroup)
                            .ThenInclude(mg => mg.Category)
                .Include(s => s.SessionExercises)
                    .ThenInclude(se => se.Sets)
                .FirstOrDefaultAsync(s => s.Id == session.Id);

            retrievedSession.ShouldNotBeNull();
            retrievedSession.Workout.User.Email.ShouldBe("complete-user");
            retrievedSession.SessionExercises.Count.ShouldBe(2);

            var firstExercise = retrievedSession.SessionExercises.First(se => se.ExerciseNumber == 1);
            firstExercise.Exercise.Name.ShouldBe("Bench Press");
            firstExercise.Sets.Count.ShouldBe(2);

            var secondExercise = retrievedSession.SessionExercises.First(se => se.ExerciseNumber == 2);
            secondExercise.Exercise.Name.ShouldBe("Incline Press");
            secondExercise.Sets.Count.ShouldBe(1);
        }

        #endregion
    }
}
