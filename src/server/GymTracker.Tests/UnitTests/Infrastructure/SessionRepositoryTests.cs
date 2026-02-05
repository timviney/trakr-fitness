using AutoFixture;
using GymTracker.Core.Entities;
using GymTracker.Core.Results;
using GymTracker.Infrastructure.Data;
using GymTracker.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shouldly;

namespace GymTracker.Tests.UnitTests.Infrastructure
{
    [TestFixture]
    public class SessionRepositoryTests
    {
        private GymTrackerDbContext _context = null!;
        private SessionRepository _repository = null!;
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
            _repository = new SessionRepository(_context);
        }

        [TearDown]
        public void Teardown()
        {
            _context.Dispose();
        }

        private async Task<(User, Workout, Session)> SetupWorkoutWithSessionAsync()
        {
            var user = new User { Id = Guid.NewGuid(), Email = "test-user", PasswordHashed = "hash" };
            var workout = new Workout { Id = Guid.NewGuid(), Name = "Test Workout", UserId = user.Id };
            var session = new Session { Id = Guid.NewGuid(), WorkoutId = workout.Id, CreatedAt = DateTime.UtcNow };

            _context.Users.Add(user);
            _context.Workouts.Add(workout);
            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return (user, workout, session);
        }

        private async Task<(MuscleCategory, MuscleGroup, Exercise)> SetupExerciseAsync()
        {
            var category = new MuscleCategory { Id = Guid.NewGuid(), Name = "Category", UserId = null };
            var group = new MuscleGroup { Id = Guid.NewGuid(), Name = "Group", CategoryId = category.Id, UserId = null };
            var exercise = new Exercise { Id = Guid.NewGuid(), Name = "Exercise", MuscleGroupId = group.Id, UserId = null };

            _context.MuscleCategories.Add(category);
            _context.MuscleGroups.Add(group);
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            return (category, group, exercise);
        }

        #region Sessions Tests

        [Test]
        public async Task GetSessionByIdAsync_WithValidId_ReturnsSession()
        {
            // Arrange
            var (_, _, session) = await SetupWorkoutWithSessionAsync();

            // Act
            var result = await _repository.GetSessionByIdAsync(session.Id);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Id.ShouldBe(session.Id);
            result.Data.WorkoutId.ShouldBe(session.WorkoutId);
        }

        [Test]
        public async Task GetSessionByIdAsync_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _repository.GetSessionByIdAsync(Guid.NewGuid());

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Status.ShouldBe(DbResultStatus.NotFound);
        }

        [Test]
        public async Task GetSessionByIdAsync_IncludesSessionExercises()
        {
            // Arrange
            var (_, _, session) = await SetupWorkoutWithSessionAsync();
            var (_, _, exercise) = await SetupExerciseAsync();

            var sessionExercise = new SessionExercise
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                ExerciseId = exercise.Id,
                ExerciseNumber = 1
            };

            _context.SessionExercises.Add(sessionExercise);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetSessionByIdAsync(session.Id);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.SessionExercises.Count.ShouldBe(1);
            result.Data.SessionExercises.First().Exercise.Name.ShouldBe(exercise.Name);
        }

        [Test]
        public async Task GetSessionsByWorkoutIdAsync_ReturnsSessionsByWorkout()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Email = "test-user", PasswordHashed = "hash" };
            var workout1 = new Workout { Id = Guid.NewGuid(), Name = "Workout1", UserId = user.Id };
            var workout2 = new Workout { Id = Guid.NewGuid(), Name = "Workout2", UserId = user.Id };

            _context.Users.Add(user);
            _context.Workouts.AddRange(workout1, workout2);
            await _context.SaveChangesAsync();

            var session1 = new Session { Id = Guid.NewGuid(), WorkoutId = workout1.Id };
            var session2 = new Session { Id = Guid.NewGuid(), WorkoutId = workout1.Id };
            var session3 = new Session { Id = Guid.NewGuid(), WorkoutId = workout2.Id };

            _context.Sessions.AddRange(session1, session2, session3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetSessionsByWorkoutIdAsync(workout1.Id);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            var sessions = result.Data.ToList();
            sessions.Count.ShouldBe(2);
            var ids = sessions.Select(x => x.Id).ToList();
            ids.ShouldContain(session1.Id);
            ids.ShouldContain(session2.Id);
        }

        [Test]
        public async Task AddSessionAsync_AddsSession()
        {
            // Arrange
            var (_, workout, _) = await SetupWorkoutWithSessionAsync();

            var newSession = new Session
            {
                Id = Guid.NewGuid(),
                WorkoutId = workout.Id,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _repository.AddSessionAsync(newSession);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var dbSession = await _context.Sessions.FirstOrDefaultAsync(x => x.Id == newSession.Id);
            dbSession.ShouldNotBeNull();
            dbSession.WorkoutId.ShouldBe(workout.Id);
        }

        [Test]
        public async Task UpdateSessionAsync_UpdatesSession()
        {
            // Arrange
            var (_, _, session) = await SetupWorkoutWithSessionAsync();
            var originalCreatedAt = session.CreatedAt;
            var newCreatedAt = originalCreatedAt.AddHours(1);

            session.CreatedAt = newCreatedAt;

            // Act
            var result = await _repository.UpdateSessionAsync(session);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var dbSession = await _context.Sessions.FirstOrDefaultAsync(x => x.Id == session.Id);
            dbSession.ShouldNotBeNull();
            dbSession.CreatedAt.ShouldBe(newCreatedAt);
        }

        [Test]
        public async Task DeleteSessionAsync_DeletesSession()
        {
            // Arrange
            var (_, _, session) = await SetupWorkoutWithSessionAsync();

            // Act
            var result = await _repository.DeleteSessionAsync(session.Id);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var dbSession = await _context.Sessions.FirstOrDefaultAsync(x => x.Id == session.Id);
            dbSession.ShouldBeNull();
        }

        [Test]
        public async Task DeleteSessionAsync_WithNonExistentId_ReturnsNotFound()
        {
            // Act
            var result = await _repository.DeleteSessionAsync(Guid.NewGuid());

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Status.ShouldBe(DbResultStatus.NotFound);
        }

        #endregion

        #region Session Exercises Tests

        [Test]
        public async Task GetSessionExerciseByIdAsync_WithValidId_ReturnsSessionExercise()
        {
            // Arrange
            var (_, _, session) = await SetupWorkoutWithSessionAsync();
            var (_, _, exercise) = await SetupExerciseAsync();

            var sessionExercise = new SessionExercise
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                ExerciseId = exercise.Id,
                ExerciseNumber = 1
            };

            _context.SessionExercises.Add(sessionExercise);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetSessionExerciseByIdAsync(sessionExercise.Id);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Id.ShouldBe(sessionExercise.Id);
            result.Data.ExerciseNumber.ShouldBe(1);
        }

        [Test]
        public async Task GetSessionExerciseByIdAsync_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _repository.GetSessionExerciseByIdAsync(Guid.NewGuid());

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Status.ShouldBe(DbResultStatus.NotFound);
        }

        [Test]
        public async Task GetSessionExercisesBySessionIdAsync_ReturnsSessionExercisesBySession()
        {
            // Arrange
            var (_, _, session) = await SetupWorkoutWithSessionAsync();
            var (_, _, exercise1) = await SetupExerciseAsync();
            var (_, _, exercise2) = await SetupExerciseAsync();

            var sessionExercise1 = new SessionExercise
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                ExerciseId = exercise1.Id,
                ExerciseNumber = 1
            };
            var sessionExercise2 = new SessionExercise
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                ExerciseId = exercise2.Id,
                ExerciseNumber = 2
            };

            _context.SessionExercises.AddRange(sessionExercise1, sessionExercise2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetSessionExercisesBySessionIdAsync(session.Id);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            var sessionExercises = result.Data.ToList();
            sessionExercises.Count.ShouldBe(2);
            var exerciseNumbers = sessionExercises.Select(x => x.ExerciseNumber).OrderBy(x => x).ToList();
            exerciseNumbers.ShouldBe(new[] { 1, 2 });
        }

        [Test]
        public async Task AddSessionExerciseAsync_AddsSessionExercise()
        {
            // Arrange
            var (_, _, session) = await SetupWorkoutWithSessionAsync();
            var (_, _, exercise) = await SetupExerciseAsync();

            var sessionExercise = new SessionExercise
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                ExerciseId = exercise.Id,
                ExerciseNumber = 1
            };

            // Act
            var result = await _repository.AddSessionExerciseAsync(sessionExercise);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var dbSessionExercise = await _context.SessionExercises.FirstOrDefaultAsync(x => x.Id == sessionExercise.Id);
            dbSessionExercise.ShouldNotBeNull();
        }

        [Test]
        public async Task UpdateSessionExerciseAsync_UpdatesSessionExercise()
        {
            // Arrange
            var (_, _, session) = await SetupWorkoutWithSessionAsync();
            var (_, _, exercise) = await SetupExerciseAsync();

            var sessionExercise = new SessionExercise
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                ExerciseId = exercise.Id,
                ExerciseNumber = 1
            };

            _context.SessionExercises.Add(sessionExercise);
            await _context.SaveChangesAsync();

            sessionExercise.ExerciseNumber = 5;

            // Act
            var result = await _repository.UpdateSessionExerciseAsync(sessionExercise);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var dbSessionExercise = await _context.SessionExercises.FirstOrDefaultAsync(x => x.Id == sessionExercise.Id);
            dbSessionExercise.ShouldNotBeNull();
            dbSessionExercise.ExerciseNumber.ShouldBe(5);
        }

        [Test]
        public async Task DeleteSessionExerciseAsync_DeletesSessionExercise()
        {
            // Arrange
            var (_, _, session) = await SetupWorkoutWithSessionAsync();
            var (_, _, exercise) = await SetupExerciseAsync();

            var sessionExercise = new SessionExercise
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                ExerciseId = exercise.Id,
                ExerciseNumber = 1
            };

            _context.SessionExercises.Add(sessionExercise);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteSessionExerciseAsync(sessionExercise.Id);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var dbSessionExercise = await _context.SessionExercises.FirstOrDefaultAsync(x => x.Id == sessionExercise.Id);
            dbSessionExercise.ShouldBeNull();
        }

        [Test]
        public async Task DeleteSessionExerciseAsync_WithNonExistentId_ReturnsNotFound()
        {
            // Act
            var result = await _repository.DeleteSessionExerciseAsync(Guid.NewGuid());

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Status.ShouldBe(DbResultStatus.NotFound);
        }

        #endregion

        #region Sets Tests

        [Test]
        public async Task GetSetByIdAsync_WithValidId_ReturnsSet()
        {
            // Arrange
            var (_, _, session) = await SetupWorkoutWithSessionAsync();
            var (_, _, exercise) = await SetupExerciseAsync();

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
                Reps = 10,
                Weight = 100,
                WarmUp = true
            };

            _context.SessionExercises.Add(sessionExercise);
            _context.Sets.Add(set);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetSetByIdAsync(set.Id);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Id.ShouldBe(set.Id);
            result.Data.SetNumber.ShouldBe(1);
            result.Data.WarmUp.ShouldBeTrue();
        }

        [Test]
        public async Task GetSetByIdAsync_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _repository.GetSetByIdAsync(Guid.NewGuid());

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Status.ShouldBe(DbResultStatus.NotFound);
        }

        [Test]
        public async Task GetSetsBySessionExerciseIdAsync_ReturnsSets()
        {
            // Arrange
            var (_, _, session) = await SetupWorkoutWithSessionAsync();
            var (_, _, exercise) = await SetupExerciseAsync();

            var sessionExercise = new SessionExercise
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                ExerciseId = exercise.Id,
                ExerciseNumber = 1
            };

            var set1 = new Set { Id = Guid.NewGuid(), SessionExerciseId = sessionExercise.Id, SetNumber = 1, Reps = 10, Weight = 100, WarmUp = true };
            var set2 = new Set { Id = Guid.NewGuid(), SessionExerciseId = sessionExercise.Id, SetNumber = 2, Reps = 8, Weight = 110, WarmUp = false };

            _context.SessionExercises.Add(sessionExercise);
            _context.Sets.AddRange(set1, set2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetSetsBySessionExerciseIdAsync(sessionExercise.Id);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.Count().ShouldBe(2);
        }

        [Test]
        public async Task GetSetsBySessionExerciseIdAsync_OrdersBySetNumber()
        {
            // Arrange
            var (_, _, session) = await SetupWorkoutWithSessionAsync();
            var (_, _, exercise) = await SetupExerciseAsync();

            var sessionExercise = new SessionExercise
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                ExerciseId = exercise.Id,
                ExerciseNumber = 1
            };

            var set2 = new Set { Id = Guid.NewGuid(), SessionExerciseId = sessionExercise.Id, SetNumber = 2, Reps = 8, Weight = 110, WarmUp = true };
            var set1 = new Set { Id = Guid.NewGuid(), SessionExerciseId = sessionExercise.Id, SetNumber = 1, Reps = 10, Weight = 100, WarmUp = false };
            var set3 = new Set { Id = Guid.NewGuid(), SessionExerciseId = sessionExercise.Id, SetNumber = 3, Reps = 6, Weight = 120, WarmUp = true };

            _context.SessionExercises.Add(sessionExercise);
            _context.Sets.AddRange(set2, set1, set3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetSetsBySessionExerciseIdAsync(sessionExercise.Id);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            var sets = result.Data!;
            var setNumbers = sets.Select(x => x.SetNumber).ToList();
            setNumbers.ShouldBe(new[] { 1, 2, 3 });
        }

        [Test]
        public async Task AddSetAsync_AddsSet()
        {
            // Arrange
            var (_, _, session) = await SetupWorkoutWithSessionAsync();
            var (_, _, exercise) = await SetupExerciseAsync();

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
                Reps = 10,
                Weight = 100,
                WarmUp = true
            };

            _context.SessionExercises.Add(sessionExercise);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.AddSetAsync(set);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var dbSet = await _context.Sets.FirstOrDefaultAsync(x => x.Id == set.Id);
            dbSet.ShouldNotBeNull();
            dbSet!.WarmUp.ShouldBeTrue();
        }

        [Test]
        public async Task UpdateSetAsync_UpdatesSet()
        {
            // Arrange
            var (_, _, session) = await SetupWorkoutWithSessionAsync();
            var (_, _, exercise) = await SetupExerciseAsync();

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
                Reps = 10,
                Weight = 100,
                WarmUp = false
            };

            _context.SessionExercises.Add(sessionExercise);
            _context.Sets.Add(set);
            await _context.SaveChangesAsync();

            set.Reps = 12;
            set.Weight = 120;
            set.WarmUp = true;

            // Act
            var result = await _repository.UpdateSetAsync(set);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var dbSet = await _context.Sets.FirstOrDefaultAsync(x => x.Id == set.Id);
            dbSet.ShouldNotBeNull();
            dbSet.Reps.ShouldBe(12);
            dbSet.Weight.ShouldBe(120);
            dbSet.WarmUp.ShouldBeTrue();
        }

        [Test]
        public async Task DeleteSetAsync_DeletesSet()
        {
            // Arrange
            var (_, _, session) = await SetupWorkoutWithSessionAsync();
            var (_, _, exercise) = await SetupExerciseAsync();

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
                Reps = 10,
                Weight = 100,
                WarmUp = true
            };

            _context.SessionExercises.Add(sessionExercise);
            _context.Sets.Add(set);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteSetAsync(set.Id);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var dbSet = await _context.Sets.FirstOrDefaultAsync(x => x.Id == set.Id);
            dbSet.ShouldBeNull();
        }

        [Test]
        public async Task DeleteSetAsync_WithNonExistentId_ReturnsNotFound()
        {
            // Act
            var result = await _repository.DeleteSetAsync(Guid.NewGuid());

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Status.ShouldBe(DbResultStatus.NotFound);
        }

        #endregion
    }
}
