import { ApiClient } from './client'
import { AuthApi } from './modules/auth'
import { ExercisesApi } from './modules/exercises'
import { WorkoutsApi } from './modules/workouts'
import { MusclesApi } from './modules/muscles'
import { SessionsApi } from './modules/sessions'
import { ExerciseCollection } from '../types/ExerciseCollection'

class Api {
  readonly auth: AuthApi
  readonly exercises: ExercisesApi
  readonly workouts: WorkoutsApi
  readonly muscles: MusclesApi
  readonly sessions: SessionsApi

  constructor(client: ApiClient) {
    this.auth = new AuthApi(client)
    this.exercises = new ExercisesApi(client)
    this.workouts = new WorkoutsApi(client)
    this.muscles = new MusclesApi(client)
    this.sessions = new SessionsApi(client)
  }

  getExerciseCollection(): Promise<ExerciseCollection> {
    return Promise.all([
      this.workouts.getWorkouts(),
      this.exercises.getExercises(),
      this.muscles.getMuscleCategories(),
      this.muscles.getMuscleGroups(),
    ]).then(([workouts, exercises, muscleCategories, muscleGroups]) => {
      let result: ExerciseCollection = {
        workouts: workouts.data!,
        exercises: exercises.data!,
        muscleCategories: muscleCategories.data!,
        muscleGroups: muscleGroups.data!,
      };

      if (!workouts.isSuccess || !exercises.isSuccess || !muscleCategories.isSuccess || !muscleGroups.isSuccess) {
        throw new Error('Failed to fetch exercise collection: ' +`${JSON.stringify(result, null, 2)}`)
      }

      return result
    });
  }
}

export const api = new Api(new ApiClient())
