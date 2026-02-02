using System;
using System.Threading.Tasks;
using AutoFixture;
using GymTracker.Core.Entities;
using GymTracker.Core.Results;
using GymTracker.Infrastructure.Data;
using GymTracker.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GymTracker.Tests.UnitTests.Infrastructure
{
    [TestFixture]
    public class UserRepositoryTests
    {
        private GymTrackerDbContext _context;
        private UserRepository _repository;
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
            _repository = new UserRepository(_context);
        }

        [TearDown]
        public void Teardown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task GetByIdAsync_WithValidId_ReturnsUser()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                PasswordHashed = "hashedpassword",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(user.Id);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data!.Id, Is.EqualTo(user.Id));
            Assert.That(result.Data.Username, Is.EqualTo("testuser"));
        }

        [Test]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _repository.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo(DbResultStatus.NotFound));
        }

        [Test]
        public async Task FindByUsernameAsync_WithValidUsername_ReturnsUser()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "uniqueuser",
                PasswordHashed = "hashedpassword",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.FindByUsernameAsync("uniqueuser");

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data!.Username, Is.EqualTo("uniqueuser"));
            Assert.That(result.Data.Id, Is.EqualTo(user.Id));
        }

        [Test]
        public async Task FindByUsernameAsync_WithInvalidUsername_ReturnsNotFound()
        {
            // Act
            var result = await _repository.FindByUsernameAsync("nonexistentuser");

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo(DbResultStatus.NotFound));
        }

        [Test]
        public async Task FindByUsernameAsync_IsCaseSensitive()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "TestUser",
                PasswordHashed = "hashedpassword",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var resultCorrectCase = await _repository.FindByUsernameAsync("TestUser");
            var resultWrongCase = await _repository.FindByUsernameAsync("testuser");

            // Assert
            Assert.That(resultCorrectCase.IsSuccess, Is.True);
            Assert.That(resultWrongCase.IsSuccess, Is.False);
        }

        [Test]
        public async Task AddAsync_AddsUserToDatabase()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Username = "newuser",
                PasswordHashed = "hashedpassword",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _repository.AddAsync(user);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            var dbUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            Assert.That(dbUser, Is.Not.Null);
            Assert.That(dbUser!.Username, Is.EqualTo("newuser"));
        }

        [Test]
        public async Task AddAsync_SetsCreatedAtToCurrentTime()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "newuser",
                PasswordHashed = "hashedpassword",
                CreatedAt = DateTime.UtcNow
            };

            var beforeAdd = DateTime.UtcNow.AddSeconds(-1);

            // Act
            await _repository.AddAsync(user);

            var afterAdd = DateTime.UtcNow.AddSeconds(1);

            // Assert
            var result = await _context.Users.FirstOrDefaultAsync(x => x.Id == user.Id);
            Assert.That(result.CreatedAt, Is.GreaterThanOrEqualTo(beforeAdd));
            Assert.That(result.CreatedAt, Is.LessThanOrEqualTo(afterAdd));
        }

        [Test]
        public async Task UpdateAsync_UpdatesUserInDatabase()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "originaluser",
                PasswordHashed = "hashedpassword",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            user.Username = "updateduser";
            user.PasswordHashed = "newhash";

            // Act
            var result = await _repository.UpdateAsync(user);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            var dbUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == user.Id);
            Assert.That(dbUser.Username, Is.EqualTo("updateduser"));
            Assert.That(dbUser.PasswordHashed, Is.EqualTo("newhash"));
        }

        [Test]
        public async Task DeleteAsync_RemovesUserFromDatabase()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "usertoDelete",
                PasswordHashed = "hashedpassword",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteAsync(user.Id);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            var dbUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == user.Id);
            Assert.That(dbUser, Is.Null);
        }

        [Test]
        public async Task DeleteAsync_WithNonExistentId_ReturnsNotFound()
        {
            // Act
            var result = await _repository.DeleteAsync(Guid.NewGuid());

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo(DbResultStatus.NotFound));
        }

        [Test]
        public async Task AddAsync_WithDuplicateUsername_ReturnsDuplicateName()
        {
            // Arrange
            var user1 = new User
            {
                Id = Guid.NewGuid(),
                Username = "duplicateuser",
                PasswordHashed = "hash1",
                CreatedAt = DateTime.UtcNow
            };

            var user2 = new User
            {
                Id = Guid.NewGuid(),
                Username = "duplicateuser",
                PasswordHashed = "hash2",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user1);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.AddAsync(user2);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo(DbResultStatus.DuplicateName));
        }
    }
}
