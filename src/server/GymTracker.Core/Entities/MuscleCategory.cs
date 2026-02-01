using System;

namespace GymTracker.Core.Entities
{
    public class MuscleCategory
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string Name { get; set; } = null!;

        // Navigation properties
        public User? User { get; set; }
        public ICollection<MuscleGroup> MuscleGroups { get; set; } = new List<MuscleGroup>();
    }
}
