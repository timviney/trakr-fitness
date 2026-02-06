using GymTracker.Core.Entities;
using GymTracker.Core.Results;

namespace GymTracker.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<DbResult<User>> GetByIdAsync(Guid id);
        Task<DbResult<User>> FindByEmailAsync(string email);
        Task<DbResult<User>> AddAsync(User user, bool saveChanges = true);
        Task<DbResult<User>> UpdateAsync(User user);
        Task<DbResult<User>> DeleteAsync(Guid id);
        Task<DbResult> SaveChangesAsync();
    }
}
