import { ApiResponse } from '../api-response'
import { ApiClient } from '../client'

export type CreateExerciseRequest = {
  name: string
  muscleGroupId: string
}

export type Exercise = {
  id: string
  userId: string | null
  muscleGroupId: string
  name: string
}

export class ExercisesApi {
  constructor(private client: ApiClient) {}

  getExercises() {
    return this.client.get<ApiResponse<Exercise[]>>('/exercises/')
  }

  createExercise(payload: CreateExerciseRequest) {
    return this.client.post<ApiResponse<Exercise>>('/exercises/', payload)
  }
}
