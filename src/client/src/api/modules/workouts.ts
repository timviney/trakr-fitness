import { ApiResponse } from '../api-response'
import { ApiClient } from '../client'
import { Exercise } from './exercises'

export type CreateWorkoutRequest = {
  name: string
}

export type UpdateWorkoutRequest = {
  name: string
}

export type DefaultExercise = {
  id: string
  workoutId: string
  exerciseId: string
  exerciseNumber: number
  exercise: Exercise
}

export type Workout = {
  id: string
  userId: string | null
  name: string
  defaultExercises: DefaultExercise[]
}

export class WorkoutsApi {
  constructor(private client: ApiClient) {}

  async getWorkouts(): Promise<ApiResponse<Workout[]>> {
    return await this.client.get<Workout[]>('/workouts/')
  }

  async createWorkout(payload: CreateWorkoutRequest): Promise<ApiResponse<Workout>> {
    return await this.client.post<Workout>('/workouts/', payload)
  }

  async getWorkoutById(id: string): Promise<ApiResponse<Workout>> {
    return await this.client.get<Workout>(`/workouts/${id}`)
  }

  async updateWorkout(id: string, payload: UpdateWorkoutRequest): Promise<ApiResponse<Workout>> {
    return await this.client.put<Workout>(`/workouts/${id}`, payload)
  }

  async deleteWorkout(id: string): Promise<ApiResponse<Workout>> {
    return await this.client.delete<Workout>(`/workouts/${id}`)
  }
}
