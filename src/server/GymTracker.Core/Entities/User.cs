using System;
using System.Collections.Generic;

namespace GymTracker.Core.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string PasswordHashed { get; set; } = null!;

        // Navigation properties
        public ICollection<Workout> Workouts { get; set; } = new List<Workout>();
        public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
        public ICollection<MuscleGroup> MuscleGroups { get; set; } = new List<MuscleGroup>();
        public ICollection<MuscleCategory> MuscleCategories { get; set; } = new List<MuscleCategory>();
    }
}
