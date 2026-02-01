using System;
using System.Collections.Generic;

namespace GymTracker.Core.Entities
{
    public class MuscleGroup
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = null!;

        // Navigation properties
        public User? User { get; set; }
        public MuscleCategory Category { get; set; } = null!;
        public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
    }
}
