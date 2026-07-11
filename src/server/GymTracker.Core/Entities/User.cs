using System.Text.Json.Serialization;

namespace GymTracker.Core.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string PasswordHashed { get; set; } = null!;
        public string? RefreshTokenHash { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }

        // Navigation properties
        [JsonIgnore]
        public ICollection<Workout> Workouts { get; set; } = new List<Workout>();
        [JsonIgnore]
        public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
        [JsonIgnore]
        public ICollection<MuscleGroup> MuscleGroups { get; set; } = new List<MuscleGroup>();
        [JsonIgnore]
        public ICollection<MuscleCategory> MuscleCategories { get; set; } = new List<MuscleCategory>();
    }
}
