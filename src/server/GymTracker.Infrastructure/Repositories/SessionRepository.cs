using Microsoft.EntityFrameworkCore;
using GymTracker.Core.Entities;
using GymTracker.Core.Interfaces;
using GymTracker.Core.Results;
using GymTracker.Infrastructure.Data;

namespace GymTracker.Infrastructure.Repositories
{
    public class SessionRepository(GymTrackerDbContext db) : ISessionRepository
    {
        // ===== Sessions =====
        public async Task<DbResult<Session>> GetSessionByIdAsync(Guid id)
        {
            var session = await db.Sessions
                .Include(s => s.Workout)
                .Include(s => s.SessionExercises)
                    .ThenInclude(se => se.Exercise)
                .Include(s => s.SessionExercises)
                    .ThenInclude(se => se.Sets)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (session is null)
                return DbResult<Session>.NotFound($"Session with id '{id}' not found.");
            
            return DbResult<Session>.Ok(session);
        }

        public async Task<DbResult<IEnumerable<Session>>> GetSessionsByWorkoutIdAsync(Guid workoutId)
        {
            var sessions = await db.Sessions
                .Include(s => s.Workout)
                .Include(s => s.SessionExercises)
                    .ThenInclude(se => se.Exercise)
                .Include(s => s.SessionExercises)
                    .ThenInclude(se => se.Sets)
                .Where(s => s.WorkoutId == workoutId)
                .ToListAsync();
            
            return DbResult<IEnumerable<Session>>.Ok(sessions);
        }

        public async Task<DbResult<IEnumerable<Session>>> GetSessionHistoryAsync(Guid userId, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 1;

            var query = db.Sessions
                .Include(s => s.Workout)
                .Include(s => s.SessionExercises)
                    .ThenInclude(se => se.Exercise)
                .Include(s => s.SessionExercises)
                    .ThenInclude(se => se.Sets)
                .Where(s => s.Workout.UserId == userId)
                .OrderByDescending(s => s.CreatedAt);

            var sessions = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return DbResult<IEnumerable<Session>>.Ok(sessions);
        }

        public async Task<DbResult<IEnumerable<Session>>> GetSessionHistoryByWorkoutIdAsync(Guid userId, Guid workoutId, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 1;

            var query = db.Sessions
                .Include(s => s.Workout)
                .Include(s => s.SessionExercises)
                    .ThenInclude(se => se.Exercise)
                .Include(s => s.SessionExercises)
                    .ThenInclude(se => se.Sets)
                .Where(s => s.Workout.UserId == userId && s.WorkoutId == workoutId)
                .OrderByDescending(s => s.CreatedAt);

            var sessions = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return DbResult<IEnumerable<Session>>.Ok(sessions);
        }

        public async Task<DbResult<Session>> AddSessionAsync(Session session)
        {
            await db.Sessions.AddAsync(session);
            await db.SaveChangesAsync();
            return DbResult<Session>.Ok(session);
        }

        public async Task<DbResult<Session>> UpdateSessionAsync(Session session)
        {
            db.Sessions.Update(session);
            await db.SaveChangesAsync();
            return DbResult<Session>.Ok(session);
        }

        public async Task<DbResult<Session>> DeleteSessionAsync(Guid id)
        {
            var session = await db.Sessions.FindAsync(id);
            if (session is null)
                return DbResult<Session>.NotFound($"Session with id '{id}' not found.");
            
            db.Sessions.Remove(session);
            await db.SaveChangesAsync();
            return DbResult<Session>.Ok(session);
        }

        // ===== Session Exercises =====
        public async Task<DbResult<SessionExercise>> GetSessionExerciseByIdAsync(Guid id)
        {
            var sessionExercise = await db.SessionExercises
                .Include(se => se.Session)
                .Include(se => se.Exercise)
                .Include(se => se.Sets)
                .FirstOrDefaultAsync(se => se.Id == id);
            
            if (sessionExercise is null)
                return DbResult<SessionExercise>.NotFound($"Session exercise with id '{id}' not found.");
            
            return DbResult<SessionExercise>.Ok(sessionExercise);
        }

        public async Task<DbResult<IEnumerable<SessionExercise>>> GetSessionExercisesBySessionIdAsync(Guid sessionId)
        {
            var sessionExercises = await db.SessionExercises
                .Include(se => se.Session)
                .Include(se => se.Exercise)
                .Include(se => se.Sets)
                .Where(se => se.SessionId == sessionId)
                .OrderBy(se => se.ExerciseNumber)
                .ToListAsync();
            
            return DbResult<IEnumerable<SessionExercise>>.Ok(sessionExercises);
        }

        public async Task<DbResult<SessionExercise>> AddSessionExerciseAsync(SessionExercise sessionExercise)
        {
            await db.SessionExercises.AddAsync(sessionExercise);
            await db.SaveChangesAsync();
            return DbResult<SessionExercise>.Ok(sessionExercise);
        }

        public async Task<DbResult<SessionExercise>> UpdateSessionExerciseAsync(SessionExercise sessionExercise, bool saveChanges = true)
        {
            db.SessionExercises.Update(sessionExercise);
            if (saveChanges) await db.SaveChangesAsync();
            return DbResult<SessionExercise>.Ok(sessionExercise);
        }

        public async Task<DbResult<SessionExercise>> DeleteSessionExerciseAsync(Guid id)
        {
            var sessionExercise = await db.SessionExercises.FindAsync(id);
            if (sessionExercise is null)
                return DbResult<SessionExercise>.NotFound($"Session exercise with id '{id}' not found.");
            
            db.SessionExercises.Remove(sessionExercise);
            await db.SaveChangesAsync();
            return DbResult<SessionExercise>.Ok(sessionExercise);
        }

        // ===== Sets =====
        public async Task<DbResult<Set>> GetSetByIdAsync(Guid id)
        {
            var set = await db.Sets
                .Include(s => s.SessionExercise)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (set is null)
                return DbResult<Set>.NotFound($"Set with id '{id}' not found.");
            
            return DbResult<Set>.Ok(set);
        }

        public async Task<DbResult<IEnumerable<Set>>> GetSetsBySessionExerciseIdAsync(Guid sessionExerciseId)
        {
            var sets = await db.Sets
                .Include(s => s.SessionExercise)
                .Where(s => s.SessionExerciseId == sessionExerciseId)
                .OrderBy(s => s.SetNumber)
                .ToListAsync();
            
            return DbResult<IEnumerable<Set>>.Ok(sets);
        }

        public async Task<DbResult<Set>> AddSetAsync(Set set)
        {
            await db.Sets.AddAsync(set);
            await db.SaveChangesAsync();
            return DbResult<Set>.Ok(set);
        }

        public async Task<DbResult<Set>> UpdateSetAsync(Set set)
        {
            db.Sets.Update(set);
            await db.SaveChangesAsync();
            return DbResult<Set>.Ok(set);
        }

        public async Task<DbResult<Set>> DeleteSetAsync(Guid id, bool saveChanges = true)
        {
            var set = await db.Sets.FindAsync(id);
            if (set is null)
                return DbResult<Set>.NotFound($"Set with id '{id}' not found.");
            
            db.Sets.Remove(set);
            if (saveChanges) await db.SaveChangesAsync();
            return DbResult<Set>.Ok(set);
        }
        
        
        public async Task<DbResult> SaveChangesAsync()
        {
            await db.SaveChangesAsync();
            return DbResult.Ok();
        }
    }
}
