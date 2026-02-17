import type { SessionHistoryItemResponse } from '../api/modules/sessions'
import type { ExerciseCollection } from '../types/ExerciseCollection'
import type { FlatExerciseRow } from '../types/Session'

/**
 * Transform session history (server response) + exercise collection into flat rows
 * suitable for charts/tables. Falls back to sensible defaults when metadata
 * is missing in the collection.
 */
export function transformToFlatRows(
  sessions: SessionHistoryItemResponse[],
  collection: ExerciseCollection
): FlatExerciseRow[] {
  const exercisesById = new Map(collection.exercises.map(e => [e.id, e]))
  const muscleGroupsById = new Map(collection.muscleGroups.map(g => [g.id, g]))
  const muscleCategoriesById = new Map(collection.muscleCategories.map(c => [c.id, c]))
  const workoutsById = new Map(collection.workouts.map(w => [w.id, w]))

  const flatRows: FlatExerciseRow[] = []

  sessions.forEach(session => {
    const workoutName = session.workout?.name ?? workoutsById.get(session.workoutId)?.name ?? 'Manual Session'
    const date = new Date(session.createdAt)

    session.sessionExercises.forEach(se => {
      const exerciseMeta = exercisesById.get(se.exerciseId)
      const muscleGroup = exerciseMeta ? muscleGroupsById.get(exerciseMeta.muscleGroupId) : undefined
      const muscleCategory = muscleGroup ? muscleCategoriesById.get(muscleGroup.categoryId) : undefined

      flatRows.push({
        sessionId: session.id,
        date,
        workoutName,
        exerciseName: se.exerciseName ?? exerciseMeta?.name ?? 'Unknown Exercise',
        muscleGroupName: muscleGroup?.name ?? 'Other',
        muscleCategoryName: muscleCategory?.name ?? 'Other',
        sets: se.sets.map(s => ({ weight: s.weight, reps: s.reps }))
      })
    })
  })

  flatRows.sort((a, b) => b.date.getTime() - a.date.getTime())

  return flatRows
}
