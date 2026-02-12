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
export type CreateSessionExerciseRequest = {
  exerciseId: string
  exerciseNumber: number
  sets: CreateSetRequest[]
}

export type UpdateSessionExerciseRequest = {
  exerciseNumber?: number
  exerciseId?: string
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
  async getSessions(): Promise<ApiResponse<Session[]>> {
    return await this.client.get<Session[]>('/sessions/')
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

  async createSessionExercise(sessionId: string, payload: CreateSessionExerciseRequest): Promise<ApiResponse<SessionExercise>> {
    return await this.client.post<SessionExercise>(`/sessions/${sessionId}/exercises/`, payload)
  }

  async getSessionExerciseById(sessionId: string, sessionExerciseId: string): Promise<ApiResponse<SessionExercise>> {
    return await this.client.get<SessionExercise>(`/sessions/${sessionId}/exercises/${sessionExerciseId}`)
  }

  async updateSessionExercise(sessionId: string, sessionExerciseId: string, payload: UpdateSessionExerciseRequest): Promise<ApiResponse<SessionExercise>> {
    return await this.client.put<SessionExercise>(`/sessions/${sessionId}/exercises/${sessionExerciseId}`, payload)
  }

  async deleteSessionExercise(sessionId: string, sessionExerciseId: string): Promise<ApiResponse<SessionExercise>> {
    return await this.client.delete<SessionExercise>(`/sessions/${sessionId}/exercises/${sessionExerciseId}`)
  }

  // Set endpoints
  async getSets(sessionId: string, sessionExerciseId: string): Promise<ApiResponse<Set[]>> {
    return await this.client.get<Set[]>(`/sessions/${sessionId}/exercises/${sessionExerciseId}/sets/`)
  }

  async createSet(sessionId: string, sessionExerciseId: string, payload: CreateSetRequest): Promise<ApiResponse<Set>> {
    return await this.client.post<Set>(`/sessions/${sessionId}/exercises/${sessionExerciseId}/sets/`, payload)
  }

  async getSetById(sessionId: string, sessionExerciseId: string, setId: string): Promise<ApiResponse<Set>> {
    return await this.client.get<Set>(`/sessions/${sessionId}/exercises/${sessionExerciseId}/sets/${setId}`)
  }

  async updateSet(sessionId: string, sessionExerciseId: string, setId: string, payload: UpdateSetRequest): Promise<ApiResponse<Set>> {
    return await this.client.put<Set>(`/sessions/${sessionId}/exercises/${sessionExerciseId}/sets/${setId}`, payload)
  }

  async deleteSet(sessionId: string, sessionExerciseId: string, setId: string): Promise<ApiResponse<Set>> {
    return await this.client.delete<Set>(`/sessions/${sessionId}/exercises/${sessionExerciseId}/sets/${setId}`)
  }
}
