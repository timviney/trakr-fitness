using AutoFixture;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using GymTracker.Infrastructure.Data;
using GymTracker.Infrastructure.Repositories;
using GymTracker.Core.Entities;

namespace GymTracker.Tests.UnitTests.Infrastructure;

[TestFixture]
public class UserRepositoryTests
{
    private Fixture _fixture = null!;
    private GymTrackerDbContext _context = null!;
    private UserRepository _repo = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        var options = new DbContextOptionsBuilder<GymTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new GymTrackerDbContext(options);
        _repo = new UserRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task AddAndGetUser_Works()
    {
        var user = _fixture.Build<User>()
            .With(u => u.PasswordHashed, "hashed")
            .With(u => u.Username, "alice")
            .Create();

        await _repo.AddAsync(user);

        var fetched = await _repo.GetByIdAsync(user.Id);
        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched!.Username, Is.EqualTo("alice"));
        Assert.That(fetched.PasswordHashed, Is.EqualTo("hashed"));
    }

    [Test]
    public async Task FindByUsername_ReturnsCorrectUser()
    {
        var user = _fixture.Build<User>()
            .With(u => u.PasswordHashed, "hashed")
            .With(u => u.Username, "bob")
            .Create();

        await _repo.AddAsync(user);

        var fetched = await _repo.FindByUsernameAsync("bob");
        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched!.Id, Is.EqualTo(user.Id));
    }

    [Test]
    public async Task FindByUsername_ReturnsNull_WhenMissing()
    {
        var fetched = await _repo.FindByUsernameAsync("doesnotexist");
        Assert.That(fetched, Is.Null);
    }

    [Test]
    public async Task UpdateUser_UpdatesFields()
    {
        var user = _fixture.Build<User>()
            .With(u => u.PasswordHashed, "old")
            .With(u => u.Username, "charlie")
            .Create();

        await _repo.AddAsync(user);

        // modify fields
        user.PasswordHashed = "newhash";
        user.Username = "charlie2";

        await _repo.UpdateAsync(user);

        var fetched = await _repo.GetByIdAsync(user.Id);
        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched!.PasswordHashed, Is.EqualTo("newhash"));
        Assert.That(fetched.Username, Is.EqualTo("charlie2"));
    }

    [Test]
    public async Task Delete_RemovesUser()
    {
        var user = _fixture.Build<User>()
            .With(u => u.PasswordHashed, "hashed")
            .With(u => u.Username, "delta")
            .Create();

        await _repo.AddAsync(user);

        await _repo.DeleteAsync(user.Id);

        var fetched = await _repo.GetByIdAsync(user.Id);
        Assert.That(fetched, Is.Null);
    }
}