import { ApiResponse } from '../api-response'
import { ApiClient } from '../client'

export type CreateMuscleCategoryRequest = {
  name: string
}

export type UpdateMuscleCategoryRequest = {
  name: string
}

export type CreateMuscleGroupRequest = {
  name: string
  categoryId: string
}

export type UpdateMuscleGroupRequest = {
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

  // Muscle Category endpoints
  async getMuscleCategories(): Promise<ApiResponse<MuscleCategory[]>> {
    return await this.client.get<MuscleCategory[]>('/muscle/categories')
  }

  async getMuscleCategoryById(id: string): Promise<ApiResponse<MuscleCategory>> {
    return await this.client.get<MuscleCategory>(`/muscle/categories/${id}`)
  }

  async createMuscleCategory(payload: CreateMuscleCategoryRequest): Promise<ApiResponse<MuscleCategory>> {
    return await this.client.post<MuscleCategory>('/muscle/categories', payload)
  }

  async updateMuscleCategory(id: string, payload: UpdateMuscleCategoryRequest): Promise<ApiResponse<MuscleCategory>> {
    return await this.client.put<MuscleCategory>(`/muscle/categories/${id}`, payload)
  }

  async deleteMuscleCategory(id: string): Promise<ApiResponse<MuscleCategory>> {
    return await this.client.delete<MuscleCategory>(`/muscle/categories/${id}`)
  }

  // Muscle Group endpoints
  async getMuscleGroups(): Promise<ApiResponse<MuscleGroup[]>> {
    return await this.client.get<MuscleGroup[]>('/muscle/groups')
  }

  async getMuscleGroupById(id: string): Promise<ApiResponse<MuscleGroup>> {
    return await this.client.get<MuscleGroup>(`/muscle/groups/${id}`)
  }

  async createMuscleGroup(payload: CreateMuscleGroupRequest): Promise<ApiResponse<MuscleGroup>> {
    return await this.client.post<MuscleGroup>('/muscle/groups', payload)
  }

  async updateMuscleGroup(id: string, payload: UpdateMuscleGroupRequest): Promise<ApiResponse<MuscleGroup>> {
    return await this.client.put<MuscleGroup>(`/muscle/groups/${id}`, payload)
  }

  async deleteMuscleGroup(id: string): Promise<ApiResponse<MuscleCategory>> {
    return await this.client.delete<MuscleCategory>(`/muscle/groups/${id}`)
  }
}
