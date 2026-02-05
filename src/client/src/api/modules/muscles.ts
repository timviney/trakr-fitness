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

  createMuscleCategory(payload: CreateMuscleCategoryRequest) {
    return this.client.post<ApiResponse<MuscleCategory>>('/muscle/categories', payload)
  }

  createMuscleGroup(payload: CreateMuscleGroupRequest) {
    return this.client.post<ApiResponse<MuscleGroup>>('/muscle/groups', payload)
  }
}
