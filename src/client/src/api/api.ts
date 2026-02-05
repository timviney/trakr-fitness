import { ApiClient } from './client'
import { AuthApi } from './modules/auth'
import { ExercisesApi } from './modules/exercises'
import { WorkoutsApi } from './modules/workouts'
import { MusclesApi } from './modules/muscles'

class Api {
  readonly auth: AuthApi
  readonly exercises: ExercisesApi
  readonly workouts: WorkoutsApi
  readonly muscles: MusclesApi

  constructor(client: ApiClient) {
    this.auth = new AuthApi(client)
    this.exercises = new ExercisesApi(client)
    this.workouts = new WorkoutsApi(client)
    this.muscles = new MusclesApi(client)
  }
}

export const api = new Api(new ApiClient())
