using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GymTracker.Core.Entities
{
    public class Session
    {
        public Guid Id { get; set; }
        public Guid WorkoutId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [JsonIgnore]
        public Workout Workout { get; set; } = null!;
        [JsonIgnore]
        public ICollection<SessionExercise> SessionExercises { get; set; } = new List<SessionExercise>();
    }
}
