using System.Text.Json.Serialization;

namespace GymTracker.Core.Entities
{
    public class Workout
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = null!;

        // Navigation properties
        [JsonIgnore]
        public User User { get; set; } = null!;
        [JsonIgnore]
        public ICollection<Session> Sessions { get; set; } = new List<Session>();

        // We are specifically not ignoring this to save ever having to get exercises in isolation
        public ICollection<WorkoutDefaultExercise> DefaultExercises { get; set; } = new List<WorkoutDefaultExercise>();
    }
}
