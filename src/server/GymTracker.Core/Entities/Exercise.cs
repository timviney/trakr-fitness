using System;
using System.Collections.Generic;

namespace GymTracker.Core.Entities
{
    public class Exercise
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public Guid MuscleGroupId { get; set; }
        public string Name { get; set; } = null!;

        // Navigation properties
        public User? User { get; set; }
        public MuscleGroup MuscleGroup { get; set; } = null!;
        public ICollection<SessionExercise> SessionExercises { get; set; } = new List<SessionExercise>();
    }
}
