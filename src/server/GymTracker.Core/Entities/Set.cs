using System;

namespace GymTracker.Core.Entities
{
    public class Set
    {
        public Guid Id { get; set; }
        public Guid SessionExerciseId { get; set; }
        public int SetNumber { get; set; }
        public double Weight { get; set; }
        public int Reps { get; set; }

        // Navigation properties
        public SessionExercise SessionExercise { get; set; } = null!;
    }
}
