import { ApiResponse } from '../api-response'
import { ApiClient } from '../client'

export type CreateExerciseRequest = {
  name: string
  muscleGroupId: string
}

export type UpdateExerciseRequest = {
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

  async createExercise(payload: CreateExerciseRequest): Promise<ApiResponse<Exercise>> {
    return await this.client.post<Exercise>('/exercises/', payload)
  }

  async getExerciseById(id: string): Promise<ApiResponse<Exercise>> {
    return await this.client.get<Exercise>(`/exercises/${id}`)
  }

  async updateExercise(id: string, payload: UpdateExerciseRequest): Promise<ApiResponse<Exercise>> {
    return await this.client.put<Exercise>(`/exercises/${id}`, payload)
  }

  async deleteExercise(id: string): Promise<ApiResponse<Exercise>> {
    return await this.client.delete<Exercise>(`/exercises/${id}`)
  }
}
