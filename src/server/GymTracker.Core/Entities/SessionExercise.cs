using System;
using System.Collections.Generic;

namespace GymTracker.Core.Entities
{
    public class SessionExercise
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public Guid ExerciseId { get; set; }
        public int ExerciseNumber { get; set; }

        // Navigation properties
        public Session Session { get; set; } = null!;
        public Exercise Exercise { get; set; } = null!;
        public ICollection<Set> Sets { get; set; } = new List<Set>();
    }
}
