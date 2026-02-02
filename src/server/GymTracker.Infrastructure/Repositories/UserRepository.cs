using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GymTracker.Core.Entities;
using GymTracker.Core.Interfaces;
using GymTracker.Core.Results;
using GymTracker.Infrastructure.Data;

namespace GymTracker.Infrastructure.Repositories
{
    public class UserRepository(GymTrackerDbContext db) : IUserRepository
    {
        public async Task<DbResult> AddAsync(User user)
        {
            var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
            if (existingUser != null)
                return DbResult.DuplicateName($"A user with username '{user.Username}' already exists.");

            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();
            return DbResult.Ok();
        }

        public async Task<DbResult> DeleteAsync(Guid id)
        {
            var user = await db.Users.FindAsync(id);
            if (user is null)
                return DbResult.NotFound($"User with id '{id}' not found.");
            
            db.Users.Remove(user);
            await db.SaveChangesAsync();
            return DbResult.Ok();
        }

        public async Task<DbResult<User>> FindByUsernameAsync(string username)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user is null)
                return DbResult<User>.NotFound($"User with username '{username}' not found.");
            
            return DbResult<User>.Ok(user);
        }

        public async Task<DbResult<User>> GetByIdAsync(Guid id)
        {
            var user = await db.Users.FindAsync(id);
            if (user is null)
                return DbResult<User>.NotFound($"User with id '{id}' not found.");
            
            return DbResult<User>.Ok(user);
        }

        public async Task<DbResult> UpdateAsync(User user)
        {
            var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Username == user.Username && u.Id != user.Id);
            if (existingUser != null)
                return DbResult.DuplicateName($"A user with username '{user.Username}' already exists.");

            db.Users.Update(user);
            await db.SaveChangesAsync();
            return DbResult.Ok();
        }
    }
}


