import { Exercise } from "../api/modules/exercises"
import { MuscleCategory, MuscleGroup } from "../api/modules/muscles"
import { Workout } from "../api/modules/workouts"

export type ExerciseCollection = {
    workouts: Workout[]
    exercises: Exercise[]
    muscleCategories: MuscleCategory[]
    muscleGroups: MuscleGroup[]
}