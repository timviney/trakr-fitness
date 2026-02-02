using GymTracker.Core.Entities;
using GymTracker.Core.Results;

namespace GymTracker.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<DbResult<User>> GetByIdAsync(Guid id);
        Task<DbResult<User>> FindByEmailAsync(string email);
        Task<DbResult> AddAsync(User user);
        Task<DbResult> UpdateAsync(User user);
        Task<DbResult> DeleteAsync(Guid id);
    }
}
