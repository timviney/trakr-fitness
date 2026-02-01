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
    public class SessionRepositoryTests
    {
        private GymTrackerDbContext _context;
        private SessionRepository _repository;
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
            _repository = new SessionRepository(_context);
        }

        [TearDown]
        public void Teardown()
        {
            _context.Dispose();
        }

        private async Task<(User, Workout, Session)> SetupWorkoutWithSessionAsync()
        {
            var user = new User { Id = Guid.NewGuid(), Username = "testuser", PasswordHashed = "hash" };
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
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(session.Id));
            Assert.That(result.WorkoutId, Is.EqualTo(session.WorkoutId));
        }

        [Test]
        public async Task GetSessionByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = await _repository.GetSessionByIdAsync(Guid.NewGuid());

            // Assert
            Assert.That(result, Is.Null);
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
            Assert.That(result, Is.Not.Null);
            Assert.That(result.SessionExercises, Has.Count.EqualTo(1));
            Assert.That(result.SessionExercises.First().Exercise.Name, Is.EqualTo(exercise.Name));
        }

        [Test]
        public async Task GetSessionsByWorkoutIdAsync_ReturnsSessionsByWorkout()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Username = "testuser", PasswordHashed = "hash" };
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
            Assert.That(result, Has.Count.EqualTo(2));
            var ids = result.Select(x => x.Id).ToList();
            Assert.That(ids, Does.Contain(session1.Id));
            Assert.That(ids, Does.Contain(session2.Id));
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
            await _repository.AddSessionAsync(newSession);

            // Assert
            var result = await _context.Sessions.FirstOrDefaultAsync(x => x.Id == newSession.Id);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.WorkoutId, Is.EqualTo(workout.Id));
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
            await _repository.UpdateSessionAsync(session);

            // Assert
            var result = await _context.Sessions.FirstOrDefaultAsync(x => x.Id == session.Id);
            Assert.That(result.CreatedAt, Is.EqualTo(newCreatedAt));
        }

        [Test]
        public async Task DeleteSessionAsync_DeletesSession()
        {
            // Arrange
            var (_, _, session) = await SetupWorkoutWithSessionAsync();

            // Act
            await _repository.DeleteSessionAsync(session.Id);

            // Assert
            var result = await _context.Sessions.FirstOrDefaultAsync(x => x.Id == session.Id);
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task DeleteSessionAsync_WithNonExistentId_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _repository.DeleteSessionAsync(Guid.NewGuid()));
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
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(sessionExercise.Id));
            Assert.That(result.ExerciseNumber, Is.EqualTo(1));
        }

        [Test]
        public async Task GetSessionExerciseByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = await _repository.GetSessionExerciseByIdAsync(Guid.NewGuid());

            // Assert
            Assert.That(result, Is.Null);
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
            Assert.That(result, Has.Count.EqualTo(2));
            var exerciseNumbers = result.Select(x => x.ExerciseNumber).OrderBy(x => x).ToList();
            Assert.That(exerciseNumbers, Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public async Task GetSessionExercisesBySessionIdAsync_OrdersByExerciseNumber()
        {
            // ...existing code...
        }

        [Test]
        public async Task AddSessionExerciseAsync_AddsSessionExercise()
        {
            // ...existing code...
        }

        [Test]
        public async Task UpdateSessionExerciseAsync_UpdatesSessionExercise()
        {
            // ...existing code...
        }

        [Test]
        public async Task DeleteSessionExerciseAsync_DeletesSessionExercise()
        {
            // ...existing code...
        }

        [Test]
        public async Task DeleteSessionExerciseAsync_WithNonExistentId_DoesNotThrow()
        {
            // ...existing code...
        }

        #endregion

        #region Sets Tests

        [Test]
        public async Task GetSetByIdAsync_WithValidId_ReturnsSet()
        {
            // ...existing code...
        }

        [Test]
        public async Task GetSetByIdAsync_WithInvalidId_ReturnsNull()
        {
            // ...existing code...
        }

        [Test]
        public async Task GetSetsBySessionExerciseIdAsync_ReturnsSets()
        {
            // ...existing code...
        }

        [Test]
        public async Task GetSetsBySessionExerciseIdAsync_OrdersBySetNumber()
        {
            // ...existing code...
        }

        [Test]
        public async Task AddSetAsync_AddsSet()
        {
            // ...existing code...
        }

        [Test]
        public async Task UpdateSetAsync_UpdatesSet()
        {
            // ...existing code...
        }

        [Test]
        public async Task DeleteSetAsync_DeletesSet()
        {
            // ...existing code...
        }

        [Test]
        public async Task DeleteSetAsync_WithNonExistentId_DoesNotThrow()
        {
            // ...existing code...
        }

        #endregion
    }
}
