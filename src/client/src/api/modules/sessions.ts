import { ApiResponse } from '../api-response'
import { ApiClient } from '../client'

// Session types
export type Session = {
  id: string
  workoutId: string
  createdAt: string
}

export type SessionExercise = {
  id: string
  sessionId: string
  exerciseId: string
  exerciseNumber: number
}

export type Set = {
  id: string
  sessionExerciseId: string
  setNumber: number
  weight: number
  reps: number
  warmUp: boolean
}

// Request types
export type SessionExerciseRequest = {
  exerciseId: string
  exerciseNumber: number
  sets: CreateSetRequest[]
}

export type CreateSetRequest = {
  setNumber: number
  weight: number
  reps: number
  warmUp: boolean
}

export type UpdateSetRequest = {
  setNumber: number
  weight: number
  reps: number
  warmUp: boolean
}

export class SessionsApi {
  constructor(private client: ApiClient) {}

  // Session endpoints
  async getSessions(workoutId: string): Promise<ApiResponse<Session[]>> {
    return await this.client.get<Session[]>(`/workouts/${workoutId}/sessions/`)
  }

  async createSession(workoutId: string): Promise<ApiResponse<Session>> {
    // server expects POST /workouts/{workoutId}/sessions
    return await this.client.post<Session>(`/workouts/${workoutId}/sessions`, {})
  }

  async getSessionById(sessionId: string): Promise<ApiResponse<Session>> {
    return await this.client.get<Session>(`/sessions/${sessionId}`)
  }

  async deleteSession(sessionId: string): Promise<ApiResponse<void>> {
    return await this.client.delete<void>(`/sessions/${sessionId}`)
  }

  // Session Exercise endpoints
  async getSessionExercises(sessionId: string): Promise<ApiResponse<SessionExercise[]>> {
    return await this.client.get<SessionExercise[]>(`/sessions/${sessionId}/exercises/`)
  }

  async createSessionExercise(sessionId: string, payload: SessionExerciseRequest): Promise<ApiResponse<SessionExercise>> {
    return await this.client.post<SessionExercise>(`/sessions/${sessionId}/exercises/`, payload)
  }

  async getSessionExerciseById(id: string): Promise<ApiResponse<SessionExercise>> {
    return await this.client.get<SessionExercise>(`/session-exercises/${id}`)
  }

  async updateSessionExercise(id: string, payload: SessionExerciseRequest): Promise<ApiResponse<SessionExercise>> {
    return await this.client.put<SessionExercise>(`/session-exercises/${id}`, payload)
  }

  async deleteSessionExercise(id: string): Promise<ApiResponse<SessionExercise>> {
    return await this.client.delete<SessionExercise>(`/session-exercises/${id}`)
  }

  // Set endpoints
  async getSets(sessionExerciseId: string): Promise<ApiResponse<Set[]>> {
    return await this.client.get<Set[]>(`/session-exercises/${sessionExerciseId}/sets/`)
  }

  async createSet(sessionExerciseId: string, payload: CreateSetRequest): Promise<ApiResponse<Set>> {
    return await this.client.post<Set>(`/session-exercises/${sessionExerciseId}/sets/`, payload)
  }

  async getSetById(id: string): Promise<ApiResponse<Set>> {
    return await this.client.get<Set>(`/sets/${id}`)
  }

  async updateSet(id: string, payload: UpdateSetRequest): Promise<ApiResponse<Set>> {
    return await this.client.put<Set>(`/sets/${id}`, payload)
  }

  async deleteSet(id: string): Promise<ApiResponse<Set>> {
    return await this.client.delete<Set>(`/sets/${id}`)
  }
}
