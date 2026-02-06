using GymTracker.Core.Entities;
using GymTracker.Core.Results;

namespace GymTracker.Core.Interfaces;

public interface IUserRegistrationService
{
    /// <summary>
    /// Registers a new user and creates default workouts (Push, Pull, Legs) atomically.
    /// </summary>
    /// <param name="user">The user to register (must have Email and PasswordHashed set)</param>
    /// <returns>DbResult containing the new user's ID on success</returns>
    Task<DbResult<User>> RegisterUserAsync(User user);
}
