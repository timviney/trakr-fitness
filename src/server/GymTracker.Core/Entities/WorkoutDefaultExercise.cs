using System.Text.Json.Serialization;

namespace GymTracker.Core.Entities;

public class WorkoutDefaultExercise
{
    public Guid Id { get; set; }
    public Guid WorkoutId { get; set; }
    public Guid ExerciseId { get; set; }
    public int ExerciseNumber { get; set; } 

    // Navigation properties
    [JsonIgnore]
    public Workout Workout { get; set; } = null!;
    // We include Exercise in the JSON response because it contains the name 
    public Exercise Exercise { get; set; } = null!;
}