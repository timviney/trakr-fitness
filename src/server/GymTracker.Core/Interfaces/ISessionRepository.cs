using GymTracker.Core.Entities;
using GymTracker.Core.Results;

namespace GymTracker.Core.Interfaces
{
    public interface ISessionRepository
    {
        // Sessions
        Task<DbResult<Session>> GetSessionByIdAsync(Guid id);
        Task<DbResult<IEnumerable<Session>>> GetSessionsByWorkoutIdAsync(Guid workoutId);
        Task<DbResult<Session>> AddSessionAsync(Session session);
        Task<DbResult<Session>> UpdateSessionAsync(Session session);
        Task<DbResult<Session>> DeleteSessionAsync(Guid id);

        // Session Exercises
        Task<DbResult<SessionExercise>> GetSessionExerciseByIdAsync(Guid id);
        Task<DbResult<IEnumerable<SessionExercise>>> GetSessionExercisesBySessionIdAsync(Guid sessionId);
        Task<DbResult<SessionExercise>> AddSessionExerciseAsync(SessionExercise sessionExercise);
        Task<DbResult<SessionExercise>> UpdateSessionExerciseAsync(SessionExercise sessionExercise);
        Task<DbResult<SessionExercise>> DeleteSessionExerciseAsync(Guid id);

        // Sets
        Task<DbResult<Set>> GetSetByIdAsync(Guid id);
        Task<DbResult<IEnumerable<Set>>> GetSetsBySessionExerciseIdAsync(Guid sessionExerciseId);
        Task<DbResult<Set>> AddSetAsync(Set set);
        Task<DbResult<Set>> UpdateSetAsync(Set set);
        Task<DbResult<Set>> DeleteSetAsync(Guid id);
    }

}
