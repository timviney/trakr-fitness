using System;
using System.Collections.Generic;

namespace GymTracker.Core.Entities
{
    public class Session
    {
        public Guid Id { get; set; }
        public Guid WorkoutId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Workout Workout { get; set; } = null!;
        public ICollection<SessionExercise> SessionExercises { get; set; } = new List<SessionExercise>();
    }
}
