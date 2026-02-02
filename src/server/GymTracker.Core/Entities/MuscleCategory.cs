using System.Text.Json.Serialization;

namespace GymTracker.Core.Entities
{
    public class MuscleCategory
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string Name { get; set; } = null!;

        // Navigation properties
        [JsonIgnore]
        public User? User { get; set; }
        [JsonIgnore]
        public ICollection<MuscleGroup> MuscleGroups { get; set; } = new List<MuscleGroup>();
    }
}
