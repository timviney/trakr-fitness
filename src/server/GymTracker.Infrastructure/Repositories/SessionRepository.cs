using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GymTracker.Core.Entities;
using GymTracker.Core.Interfaces;
using GymTracker.Infrastructure.Data;

namespace GymTracker.Infrastructure.Repositories
{
    public class SessionRepository(GymTrackerDbContext db) : ISessionRepository
    {
        // ===== Sessions =====
        public async Task<Session?> GetSessionByIdAsync(Guid id)
        {
            return await db.Sessions
                .Include(s => s.Workout)
                .Include(s => s.SessionExercises)
                    .ThenInclude(se => se.Exercise)
                .Include(s => s.SessionExercises)
                    .ThenInclude(se => se.Sets)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Session>> GetSessionsByWorkoutIdAsync(Guid workoutId)
        {
            return await db.Sessions
                .Include(s => s.Workout)
                .Include(s => s.SessionExercises)
                    .ThenInclude(se => se.Exercise)
                .Include(s => s.SessionExercises)
                    .ThenInclude(se => se.Sets)
                .Where(s => s.WorkoutId == workoutId)
                .ToListAsync();
        }

        public async Task AddSessionAsync(Session session)
        {
            await db.Sessions.AddAsync(session);
            await db.SaveChangesAsync();
        }

        public async Task UpdateSessionAsync(Session session)
        {
            db.Sessions.Update(session);
            await db.SaveChangesAsync();
        }

        public async Task DeleteSessionAsync(Guid id)
        {
            var session = await db.Sessions.FindAsync(id);
            if (session is null) return;
            db.Sessions.Remove(session);
            await db.SaveChangesAsync();
        }

        // ===== Session Exercises =====
        public async Task<SessionExercise?> GetSessionExerciseByIdAsync(Guid id)
        {
            return await db.SessionExercises
                .Include(se => se.Session)
                .Include(se => se.Exercise)
                .Include(se => se.Sets)
                .FirstOrDefaultAsync(se => se.Id == id);
        }

        public async Task<IEnumerable<SessionExercise>> GetSessionExercisesBySessionIdAsync(Guid sessionId)
        {
            return await db.SessionExercises
                .Include(se => se.Session)
                .Include(se => se.Exercise)
                .Include(se => se.Sets)
                .Where(se => se.SessionId == sessionId)
                .OrderBy(se => se.ExerciseNumber)
                .ToListAsync();
        }

        public async Task AddSessionExerciseAsync(SessionExercise sessionExercise)
        {
            await db.SessionExercises.AddAsync(sessionExercise);
            await db.SaveChangesAsync();
        }

        public async Task UpdateSessionExerciseAsync(SessionExercise sessionExercise)
        {
            db.SessionExercises.Update(sessionExercise);
            await db.SaveChangesAsync();
        }

        public async Task DeleteSessionExerciseAsync(Guid id)
        {
            var sessionExercise = await db.SessionExercises.FindAsync(id);
            if (sessionExercise is null) return;
            db.SessionExercises.Remove(sessionExercise);
            await db.SaveChangesAsync();
        }

        // ===== Sets =====
        public async Task<Set?> GetSetByIdAsync(Guid id)
        {
            return await db.Sets
                .Include(s => s.SessionExercise)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Set>> GetSetsBySessionExerciseIdAsync(Guid sessionExerciseId)
        {
            return await db.Sets
                .Include(s => s.SessionExercise)
                .Where(s => s.SessionExerciseId == sessionExerciseId)
                .OrderBy(s => s.SetNumber)
                .ToListAsync();
        }

        public async Task AddSetAsync(Set set)
        {
            await db.Sets.AddAsync(set);
            await db.SaveChangesAsync();
        }

        public async Task UpdateSetAsync(Set set)
        {
            db.Sets.Update(set);
            await db.SaveChangesAsync();
        }

        public async Task DeleteSetAsync(Guid id)
        {
            var set = await db.Sets.FindAsync(id);
            if (set is null) return;
            db.Sets.Remove(set);
            await db.SaveChangesAsync();
        }
    }
}
