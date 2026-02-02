using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GymTracker.Core.Entities
{
    public class Exercise
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public Guid MuscleGroupId { get; set; }
        public string Name { get; set; } = null!;

        // Navigation properties
        [JsonIgnore]
        public User? User { get; set; }
        [JsonIgnore]
        public MuscleGroup MuscleGroup { get; set; } = null!;
        [JsonIgnore]
        public ICollection<SessionExercise> SessionExercises { get; set; } = new List<SessionExercise>();
    }
}
