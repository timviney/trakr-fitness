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
  isCompleted: boolean
}

export enum SessionStatus {
  NotStarted = 'not_started',
  InProgress = 'in_progress',
  Completed = 'completed'
}

export const getStatus = (s: SessionExerciseData): SessionStatus =>
  s.isCompleted
    ? SessionStatus.Completed
    : s.sets.length > 0
      ? SessionStatus.InProgress
      : SessionStatus.NotStarted


