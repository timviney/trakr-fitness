using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GymTracker.Core.Entities
{
    public class SessionExercise
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public Guid ExerciseId { get; set; }
        public int ExerciseNumber { get; set; }

        // Navigation properties
        [JsonIgnore]
        public Session Session { get; set; } = null!;
        [JsonIgnore]
        public Exercise Exercise { get; set; } = null!;
        [JsonIgnore]
        public ICollection<Set> Sets { get; set; } = new List<Set>();
    }
}
