using System;
using System.Text.Json.Serialization;

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
        [JsonIgnore]
        public SessionExercise SessionExercise { get; set; } = null!;
    }
}
