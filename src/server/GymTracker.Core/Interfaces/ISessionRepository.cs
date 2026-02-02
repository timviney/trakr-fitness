using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GymTracker.Core.Entities;
using GymTracker.Core.Results;

namespace GymTracker.Core.Interfaces
{
    public interface ISessionRepository
    {
        // Sessions
        Task<DbResult<Session>> GetSessionByIdAsync(Guid id);
        Task<DbResult<IEnumerable<Session>>> GetSessionsByWorkoutIdAsync(Guid workoutId);
        Task<DbResult> AddSessionAsync(Session session);
        Task<DbResult> UpdateSessionAsync(Session session);
        Task<DbResult> DeleteSessionAsync(Guid id);

        // Session Exercises
        Task<DbResult<SessionExercise>> GetSessionExerciseByIdAsync(Guid id);
        Task<DbResult<IEnumerable<SessionExercise>>> GetSessionExercisesBySessionIdAsync(Guid sessionId);
        Task<DbResult> AddSessionExerciseAsync(SessionExercise sessionExercise);
        Task<DbResult> UpdateSessionExerciseAsync(SessionExercise sessionExercise);
        Task<DbResult> DeleteSessionExerciseAsync(Guid id);

        // Sets
        Task<DbResult<Set>> GetSetByIdAsync(Guid id);
        Task<DbResult<IEnumerable<Set>>> GetSetsBySessionExerciseIdAsync(Guid sessionExerciseId);
        Task<DbResult> AddSetAsync(Set set);
        Task<DbResult> UpdateSetAsync(Set set);
        Task<DbResult> DeleteSetAsync(Guid id);
    }

}
