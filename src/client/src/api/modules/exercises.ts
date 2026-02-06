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

  async getExercises() : Promise<ApiResponse<Exercise[]>> {
    return await this.client.get<Exercise[]>('/exercises/')
  }

  async createExercise(payload: CreateExerciseRequest) : Promise<ApiResponse<Exercise>> {
    return await this.client.post<Exercise>('/exercises/', payload)
  }
}
