import { ApiResponse } from '../api-response'
import { ApiClient } from '../client'

export type CreateWorkoutRequest = {
  name: string
}

export type UpdateWorkoutRequest = {
  name: string
}

export type Workout = {
  id: string
  userId: string
  name: string
}

export class WorkoutsApi {
  constructor(private client: ApiClient) {}

  getWorkouts() {
    return this.client.get<ApiResponse<Workout[]>>('/workouts/')
  }

  createWorkout(payload: CreateWorkoutRequest) {
    return this.client.post<ApiResponse<Workout>>('/workouts/', payload)
  }

  updateWorkout(id: string, payload: UpdateWorkoutRequest) {
    return this.client.put<ApiResponse<void>>(`/workouts/${id}`, payload)
  }
}
