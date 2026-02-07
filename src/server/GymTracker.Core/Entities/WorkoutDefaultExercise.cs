using System.Text.Json.Serialization;

namespace GymTracker.Core.Entities;

public class WorkoutDefaultExercise
{
    public Guid Id { get; set; }
    public Guid? WorkoutId { get; set; }
    public Guid ExerciseId { get; set; }
    public int ExerciseNumber { get; set; } 

    // Navigation properties
    [JsonIgnore]
    public Exercise Exercise { get; set; } = null!;
    [JsonIgnore]
    public Workout Workout { get; set; } = null!;
}