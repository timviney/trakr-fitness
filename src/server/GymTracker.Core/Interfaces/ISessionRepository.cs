using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GymTracker.Core.Entities;

namespace GymTracker.Core.Interfaces
{
    public interface ISessionRepository
    {
        // Sessions
        Task<Session?> GetSessionByIdAsync(Guid id);
        Task<IEnumerable<Session>> GetSessionsByWorkoutIdAsync(Guid workoutId);
        Task AddSessionAsync(Session session);
        Task UpdateSessionAsync(Session session);
        Task DeleteSessionAsync(Guid id);

        // Session Exercises
        Task<SessionExercise?> GetSessionExerciseByIdAsync(Guid id);
        Task<IEnumerable<SessionExercise>> GetSessionExercisesBySessionIdAsync(Guid sessionId);
        Task AddSessionExerciseAsync(SessionExercise sessionExercise);
        Task UpdateSessionExerciseAsync(SessionExercise sessionExercise);
        Task DeleteSessionExerciseAsync(Guid id);

        // Sets
        Task<Set?> GetSetByIdAsync(Guid id);
        Task<IEnumerable<Set>> GetSetsBySessionExerciseIdAsync(Guid sessionExerciseId);
        Task AddSetAsync(Set set);
        Task UpdateSetAsync(Set set);
        Task DeleteSetAsync(Guid id);
    }
}
