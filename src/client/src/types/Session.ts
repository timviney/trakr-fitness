import type { Exercise } from '../api/modules/exercises'

export type SetData = {
  id?: string
  tempId: string
  setNumber: number
  weight: number
  reps: number
  warmUp: boolean
  completed: boolean
}

export type SessionExerciseData = {
  sessionExerciseId?: string
  exercise: Exercise
  exerciseNumber: number
  sets: SetData[]
  isSaved: boolean
}
