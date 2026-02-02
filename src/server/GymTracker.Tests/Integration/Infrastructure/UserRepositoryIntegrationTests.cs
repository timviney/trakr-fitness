using AutoFixture;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using GymTracker.Infrastructure.Data;
using GymTracker.Infrastructure.Repositories;
using GymTracker.Core.Entities;
using GymTracker.Core.Results;

namespace GymTracker.Tests.Integration.Infrastructure;

[TestFixture]
[Category("Integration")]
public class UserRepositoryIntegrationTests
{
    private Fixture _fixture = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [Test]
    public async Task AddDuplicateUsername_Throws()
    {
        // Use SQLite in-memory which enforces uniqueness constraints
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        try
        {
            var options = new DbContextOptionsBuilder<GymTrackerDbContext>()
                .UseSqlite(connection)
                .Options;

            await using var sqliteContext = new GymTrackerDbContext(options);
            // Ensure schema is created so unique index is applied
            await sqliteContext.Database.EnsureCreatedAsync();

            var sqliteRepo = new UserRepository(sqliteContext);

            var user1 = _fixture.Build<User>()
                .OmitAutoProperties()
                .With(u => u.PasswordHashed, "h1")
                .With(u => u.Username, "dup")
                .Create();

            var user2 = _fixture.Build<User>()
                .OmitAutoProperties()
                .With(u => u.PasswordHashed, "h2")
                .With(u => u.Username, "dup")
                .Create();

            await sqliteRepo.AddAsync(user1);

            // adding second user with same Username should fail and return DuplicateName status
            var result = await sqliteRepo.AddAsync(user2);
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo(DbResultStatus.DuplicateName));
        }
        finally
        {
            await connection.CloseAsync();
            connection.Dispose();
        }
    }
}
