import { ApiResponse } from '../api-response'
import { ApiClient } from '../client'

export type CreateMuscleCategoryRequest = {
  name: string
}

export type CreateMuscleGroupRequest = {
  name: string
  categoryId: string
}

export type MuscleCategory = {
  id: string
  userId: string | null
  name: string
}

export type MuscleGroup = {
  id: string
  userId: string | null
  categoryId: string
  name: string
}

export class MusclesApi {
  constructor(private client: ApiClient) {}

  async createMuscleCategory(payload: CreateMuscleCategoryRequest): Promise<ApiResponse<MuscleCategory>> {
    return await this.client.post<MuscleCategory>('/muscle/categories', payload)
  }

  async createMuscleGroup(payload: CreateMuscleGroupRequest): Promise<ApiResponse<MuscleGroup>> {
    return await this.client.post<MuscleGroup>('/muscle/groups', payload)
  }
}
