using System.Text.Json.Serialization;

namespace GymTracker.Core.Entities
{
    public class MuscleGroup
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = null!;

        // Navigation properties
        [JsonIgnore]
        public User? User { get; set; }
        [JsonIgnore]
        public MuscleCategory Category { get; set; } = null!;
        [JsonIgnore]
        public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
    }
}
