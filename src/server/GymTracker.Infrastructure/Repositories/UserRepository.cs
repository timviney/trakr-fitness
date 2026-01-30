using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GymTracker.Core.Entities;
using GymTracker.Core.Interfaces;
using GymTracker.Infrastructure.Data;

namespace GymTracker.Infrastructure.Repositories
{
    public class UserRepository(GymTrackerDbContext db) : IUserRepository
    {
        public async Task AddAsync(User user)
        {
            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await db.Users.FindAsync(id);
            if (user is null) return;
            db.Users.Remove(user);
            await db.SaveChangesAsync();
        }

        public async Task<User?> FindByUsernameAsync(string username)
        {
            return await db.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await db.Users.FindAsync(id);
        }

        public async Task UpdateAsync(User user)
        {
            db.Users.Update(user);
            await db.SaveChangesAsync();
        }
    }
}
