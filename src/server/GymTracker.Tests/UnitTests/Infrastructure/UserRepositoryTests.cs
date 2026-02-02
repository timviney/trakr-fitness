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
    public class UserRepositoryTests
    {
        private GymTrackerDbContext _context = null!;
        private UserRepository _repository = null!;
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
                Email = "test-user",
                PasswordHashed = "hashed-password",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(user.Id);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Id.ShouldBe(user.Id);
            result.Data.Email.ShouldBe("test-user");
        }

        [Test]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _repository.GetByIdAsync(Guid.NewGuid());

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Status.ShouldBe(DbResultStatus.NotFound);
        }

        [Test]
        public async Task FindByEmailAsync_WithValidEmail_ReturnsUser()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "unique-user",
                PasswordHashed = "hashed-password",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.FindByEmailAsync("unique-user");

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Email.ShouldBe("unique-user");
            result.Data.Id.ShouldBe(user.Id);
        }

        [Test]
        public async Task FindByEmailAsync_WithInvalidEmail_ReturnsNotFound()
        {
            // Act
            var result = await _repository.FindByEmailAsync("non-existent-user");

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Status.ShouldBe(DbResultStatus.NotFound);
        }

        [Test]
        public async Task FindByEmailAsync_IsCaseSensitive()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test-user",
                PasswordHashed = "hashed-password",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var resultCorrectCase = await _repository.FindByEmailAsync("test-user");
            var resultWrongCase = await _repository.FindByEmailAsync("TEST-USER");

            // Assert
            resultCorrectCase.IsSuccess.ShouldBeTrue();
            resultWrongCase.IsSuccess.ShouldBeFalse();
        }

        [Test]
        public async Task AddAsync_AddsUserToDatabase()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = "new-user",
                PasswordHashed = "hashed-password",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _repository.AddAsync(user);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var dbUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            dbUser.ShouldNotBeNull();
            dbUser.Email.ShouldBe("new-user");
        }

        [Test]
        public async Task AddAsync_SetsCreatedAtToCurrentTime()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "new-user",
                PasswordHashed = "hashed-password",
                CreatedAt = DateTime.UtcNow
            };

            var beforeAdd = DateTime.UtcNow.AddSeconds(-1);

            // Act
            await _repository.AddAsync(user);

            var afterAdd = DateTime.UtcNow.AddSeconds(1);

            // Assert
            var result = await _context.Users.FirstOrDefaultAsync(x => x.Id == user.Id);
            result.ShouldNotBeNull();
            result.CreatedAt.ShouldBeGreaterThanOrEqualTo(beforeAdd);
            result.CreatedAt.ShouldBeLessThanOrEqualTo(afterAdd);
        }

        [Test]
        public async Task UpdateAsync_UpdatesUserInDatabase()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "original-user",
                PasswordHashed = "hashed-password",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            user.Email = "updated-user";
            user.PasswordHashed = "new-hash";

            // Act
            var result = await _repository.UpdateAsync(user);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var dbUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == user.Id);
            dbUser.ShouldNotBeNull();
            dbUser.Email.ShouldBe("updated-user");
            dbUser.PasswordHashed.ShouldBe("new-hash");
        }

        [Test]
        public async Task DeleteAsync_RemovesUserFromDatabase()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "user-to-delete",
                PasswordHashed = "hashed-password",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteAsync(user.Id);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var dbUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == user.Id);
            dbUser.ShouldBeNull();
        }

        [Test]
        public async Task DeleteAsync_WithNonExistentId_ReturnsNotFound()
        {
            // Act
            var result = await _repository.DeleteAsync(Guid.NewGuid());

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Status.ShouldBe(DbResultStatus.NotFound);
        }

        [Test]
        public async Task AddAsync_WithDuplicateEmail_ReturnsDuplicateName()
        {
            // Arrange
            var user1 = new User
            {
                Id = Guid.NewGuid(),
                Email = "duplicate-user",
                PasswordHashed = "hash1",
                CreatedAt = DateTime.UtcNow
            };

            var user2 = new User
            {
                Id = Guid.NewGuid(),
                Email = "duplicate-user",
                PasswordHashed = "hash2",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user1);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.AddAsync(user2);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Status.ShouldBe(DbResultStatus.DuplicateName);
        }
    }
}
