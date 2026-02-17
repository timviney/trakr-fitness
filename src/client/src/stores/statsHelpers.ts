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

  const excludeWarmups = true

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
        sets: (excludeWarmups? se.sets.filter(s => !s.warmUp) : se.sets).map(s => ({ weight: s.weight, reps: s.reps }))
      })
    })
  })

  flatRows.sort((a, b) => b.date.getTime() - a.date.getTime())

  return flatRows
}

/**
 * Helper types + 1RM estimators (Epley + Brzycki) and a reliability-weighted
 * aggregator to produce a single 1RM estimate per calendar day.
 */
type SetEntry = { weight: number; reps: number }

function estimate1RM_Epley(weight: number, reps: number): number {
  return weight * (1 + reps / 30)
}

function estimate1RM_Brzycki(weight: number, reps: number): number {
  if (reps >= 37) return weight; // avoid division issues
  return weight * (36 / (37 - reps))
}

function estimateBest1RM(sets: SetEntry[]): number {
  let bestEpley = 0
  let bestBrzycki = 0

  for (const set of sets) {
    if (set.reps <= 0) continue

    // Skip very high rep sets (too unreliable)
    if (set.reps > 15) continue

    const epley = estimate1RM_Epley(set.weight, set.reps)
    const brzycki = estimate1RM_Brzycki(set.weight, set.reps)

    bestBrzycki = Math.max(bestBrzycki, brzycki)
    bestEpley = Math.max(bestEpley, epley)
  }

  return Math.round((bestEpley + bestBrzycki)) / 2.0 // nearest .5
}

/**
 * Compute a time-series for a single exercise from session history.
 * - groups sessions by local calendar date
 * - excludes warm-up sets by default
 * - returns Highcharts-ready points: [timestampMs, value]
 */
export function computeExerciseTimeSeries(
  sessions: SessionHistoryItemResponse[],
  exerciseId: string | undefined | null,
  metric: 'maxWeight' | 'maxReps' | 'totalReps' | 'totalVolume' | 'oneRepMax',
  excludeWarmups = true
): Array<[number, number]> {
  if (!exerciseId) return []

  const byDate = new Map<number, { maxWeight: number; maxReps: number; totalReps: number; totalVolume: number; sets: SetEntry[] }>()

  sessions.forEach(sess => {
    const dt = new Date(sess.createdAt)
    const dayStart = new Date(dt.getFullYear(), dt.getMonth(), dt.getDate()).getTime()

    sess.sessionExercises.forEach(se => {
      if (se.exerciseId !== exerciseId) return

      se.sets.forEach(s => {
        if (excludeWarmups && s.warmUp) return

        const cur = byDate.get(dayStart) ?? { maxWeight: 0, maxReps: 0, totalReps: 0, totalVolume: 0, sets: [] }
        if (s.weight > cur.maxWeight) cur.maxWeight = s.weight
        if (s.reps > cur.maxReps) cur.maxReps = s.reps
        cur.totalReps += s.reps
        cur.totalVolume += (s.weight * s.reps)
        cur.sets.push({ weight: s.weight, reps: s.reps })
        byDate.set(dayStart, cur)
      })
    })
  })

  const points: Array<[number, number]> = Array.from(byDate.entries())
    .map(([ts, vals]) => {
      switch (metric) {
        case 'maxWeight': return [ts, vals.maxWeight]
        case 'maxReps': return [ts, vals.maxReps]
        case 'totalReps': return [ts, vals.totalReps]
        case 'totalVolume': return [ts, vals.totalVolume]
        case 'oneRepMax': return [ts, estimateBest1RM(vals.sets)]
      }
    })
    .filter(Boolean) as Array<[number, number]>

  points.sort((a, b) => a[0] - b[0])
  return points
}

