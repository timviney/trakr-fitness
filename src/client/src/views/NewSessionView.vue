<template>
  <AppShell>
    <div class="session-view">

      <!-- SELECTION -->
      <div v-if="viewState === 'selection'">
        <header class="view-header">
          <h1 class="view-title">Start Workout</h1>
        </header>

        <Loader :loading="loading" />
        <AppError :message="error" @retry="loadWorkouts" />

        <div v-if="!loading && !error && workouts.length === 0" class="empty-state">
          <Dumbbell class="empty-icon" />
          <h2 class="empty-title">No workouts yet</h2>
          <p class="empty-description">Create a workout first in the Exercises tab.</p>
        </div>

        <ul v-if="!loading && !error && workouts.length > 0" class="item-list">
          <li v-for="workout in workouts" :key="workout.id" class="item-card item-card-clickable" @click="promptStartWorkout(workout)">
            <span class="item-name">{{ workout.name }}</span>
            <ChevronRight class="item-chevron" />
          </li>
        </ul>

        <!-- Start session confirmation modal -->
        <div v-if="showStartModal" class="modal-overlay" @click.self="cancelStartSession()">
          <div class="modal">
            <h2 class="modal-title">Start Session</h2>
            <div class="modal-content">
              <p class="mb-4">Start a new session for <strong>{{ pendingWorkout?.name }}</strong>?</p>
            </div>
            <div class="modal-actions">
              <button type="button" class="btn btn-secondary" @click="cancelStartSession()">Cancel</button>
              <button type="button" class="btn btn-primary" @click="startSession()">Start Session</button>
            </div>
          </div>
        </div>
      </div>

      <!-- SESSION -->
      <div v-if="viewState === 'session'" class="session-active">
        <header class="session-header">
          <h1 class="session-title">{{ currentWorkout?.name }}</h1>
        </header>

        <Loader :loading="loading" />
        <AppError :message="error" @retry="() => {}" />

        <div v-if="!loading && !error" class="exercise-list">
          <div v-for="(exData, idx) in sessionExercises" 
               :key="exData.sessionExerciseId ?? `local-${exData.exerciseNumber}`" 
               class="exercise-card"
               :class="{ 'exercise-saved': exData.isSaved }"
               @click="openExercise(idx)">

            <div class="exercise-header">
              <h3 class="exercise-title">{{ exData.exercise.name }}</h3>
              <Check v-if="exData.isSaved" class="saved-icon" :size="18" />
            </div>

            <div class="exercise-meta">{{ exData.sets.length }} {{ exData.sets.length === 1 ? 'set' : 'sets' }} • <span :class="getStatusClass(exData)">{{ getStatusText(exData) }}</span></div>

            <div class="exercise-actions" @click.stop>
              <button class="icon-btn" @click="promptReplace(idx)" aria-label="Swap exercise">
                <Repeat :size="18" />
              </button>

              <button class="icon-btn danger" @click="deleteExercise(idx)" aria-label="Delete exercise">
                <Trash2 :size="18" />
              </button>
            </div>

          </div>
        </div>

        <div class="add-exercise-action" style="padding: 0 var(--trk-space-4);">
          <button
            type="button"
            class="btn btn-faded"
            @click="openAddExerciseSelector">
            + Add
          </button>
        </div>

        <!-- Exercise detail modal -->
        <div v-if="showExerciseModal && selectedExerciseIndex !== null" class="modal-overlay" @click.self="closeExerciseModal()">
          <div class="modal">
            <h2 class="modal-title">{{ sessionExercises[selectedExerciseIndex].exercise.name }}</h2>

            <div class="modal-content">
              <ExerciseSetTable
                v-model:sets="sessionExercises[selectedExerciseIndex].sets"
                @add-set="() => addSet(selectedExerciseIndex!)"
                @remove-set="(setIdx) => removeSet(selectedExerciseIndex!, setIdx)"
              />
            </div>

            <div class="modal-actions">
              <button type="button" class="btn btn-secondary" @click="closeExerciseModal()">Close</button>
              <button type="button" class="btn btn-primary" @click="(async () => { await saveExercise(selectedExerciseIndex!); if (sessionExercises[selectedExerciseIndex!].isSaved) closeExerciseModal() })()">Save Exercise</button>
            </div>
          </div>
        </div>

        <div class="session-footer">
          <button class="btn btn-primary btn-finish" @click="finishSession" :disabled="!allExercisesSaved">Finish Session</button>
        </div>

        <ExerciseSelector
          v-if="showExerciseSelector && exerciseCollection"
          :exercise-collection="exerciseCollection"
          @add="handleExerciseSelected"
          @cancel="closeExerciseSelector"
        />
      </div>

    </div>
  </AppShell>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter, onBeforeRouteLeave } from 'vue-router'
import { Dumbbell, ChevronRight, Check, Repeat, Trash2 } from 'lucide-vue-next' 

import AppShell from '../components/general/AppShell.vue'
import Loader from '../components/general/Loader.vue'
import AppError from '../components/general/Error.vue'
import ExerciseSetTable from '../components/session/ExerciseSetTable.vue'
import ExerciseSelector from '../components/exercises/ExerciseSelector.vue'

import { api } from '../api/api'
import type { Workout } from '../api/modules/workouts'
import type { Exercise } from '../api/modules/exercises'
import type { Session } from '../api/modules/sessions'
import type { SetData, SessionExerciseData } from '../types/Session'
import { ExerciseCollection } from '../types/ExerciseCollection'

const router = useRouter()

const viewState = ref<'selection' | 'session'>('selection')
const loading = ref(false)
const error = ref<string | null>(null)
const savingExercise = ref<number | null>(null)

// selection / start modal state
const workouts = ref<Workout[]>([])
const pendingWorkout = ref<Workout | null>(null)
const showStartModal = ref(false)

// active session state
const currentSession = ref<Session | null>(null)
const currentWorkout = ref<Workout | null>(null)
const sessionExercises = ref<SessionExerciseData[]>([])
const exerciseLookup = ref<Map<string, Exercise>>(new Map())
const exerciseCollection = ref<ExerciseCollection | null>(null)

// selector state
const showExerciseSelector = ref(false)
const exerciseSelectorMode = ref<'add' | 'replace'>('add')
const replaceTargetIndex = ref<number | null>(null)

// exercise modal
const selectedExerciseIndex = ref<number | null>(null)
const showExerciseModal = ref(false)

onMounted(async () => {
  await loadWorkouts()
})

onBeforeRouteLeave((to, from, next) => {
  if (viewState.value === 'session' && hasUnsavedData.value) {
    if (confirm('You have unsaved data. Are you sure you want to leave?')) {
      next()
    } else {
      next(false)
    }
  } else {
    next()
  }
})

// --- Start session modal handlers
function promptStartWorkout(workout: Workout) {
  pendingWorkout.value = workout
  showStartModal.value = true
}

function cancelStartSession() {
  pendingWorkout.value = null
  showStartModal.value = false
}

// move previous selectWorkout logic here (starts session and builds exercises)
async function startSession() {
  const workout = pendingWorkout.value
  if (!workout) return
  showStartModal.value = false

  loading.value = true
  error.value = null

  try {
    const sessionRes = await api.sessions.createSession(workout.id)
    if (!sessionRes.isSuccess || !sessionRes.data) throw new Error('Failed to create session')
    currentSession.value = sessionRes.data

    const workoutRes = await api.workouts.getWorkoutById(workout.id)
    if (!workoutRes.isSuccess || !workoutRes.data) throw new Error('Failed to load workout details')
    currentWorkout.value = workoutRes.data

    sessionExercises.value = (workoutRes.data.defaultExercises || [])
      .sort((a, b) => a.exerciseNumber - b.exerciseNumber)
      .map(defEx => {
        const exercise = exerciseLookup.value.get(defEx.exerciseId)
        return {
          exercise: exercise || { id: defEx.exerciseId, name: 'Unknown Exercise', muscleGroupId: '', userId: null },
          exerciseNumber: defEx.exerciseNumber,
          sets: [],
          isSaved: false
        } as SessionExerciseData
      })

    viewState.value = 'session'
  } catch (e) {
    error.value = 'Failed to start session. Please try again.'
    console.error('startSession error:', e)
  } finally {
    loading.value = false
    pendingWorkout.value = null
  }
}

// --- Exercise modal handlers
function openExercise(index: number) {
  selectedExerciseIndex.value = index
  showExerciseModal.value = true
}

function closeExerciseModal() {
  const idx = selectedExerciseIndex.value
  if (idx === null) {
    showExerciseModal.value = false
    return
  }

  const ex = sessionExercises.value[idx]
  // warn if there are unsaved sets
  if (ex.sets.length > 0 && !ex.isSaved) {
    if (!confirm('You have unsaved changes for this exercise. Discard and close?')) return
  }

  selectedExerciseIndex.value = null
  showExerciseModal.value = false
}

// --- Exercise selector (add / replace)
function openAddExerciseSelector() {
  exerciseSelectorMode.value = 'add'
  replaceTargetIndex.value = null
  showExerciseSelector.value = true
}

function closeExerciseSelector() {
  showExerciseSelector.value = false
  replaceTargetIndex.value = null
}

function promptReplace(index: number) {
  exerciseSelectorMode.value = 'replace'
  replaceTargetIndex.value = index
  showExerciseSelector.value = true
}

function handleExerciseSelected(exerciseId: string) {
  if (!exerciseCollection.value) return

  const exercise = exerciseCollection.value.exercises.find(e => e.id === exerciseId)
  if (!exercise) return

  if (exerciseSelectorMode.value === 'add') {
    sessionExercises.value.push({
      exercise,
      exerciseNumber: sessionExercises.value.length,
      sets: [],
      isSaved: false
    })
  }

  if (exerciseSelectorMode.value === 'replace' && replaceTargetIndex.value !== null) {
    replaceExercise(replaceTargetIndex.value, exercise)
  }

  showExerciseSelector.value = false
}

async function replaceExercise(index: number, newExercise: Exercise) {
  const exData = sessionExercises.value[index]
  exData.exercise = newExercise

  if (
    exData.isSaved &&
    exData.sessionExerciseId &&
    currentSession.value
  ) {
    try {
      await api.sessions.updateSessionExercise(
        currentSession.value.id,
        exData.sessionExerciseId,
        { exerciseId: newExercise.id }
      )
    } catch (e) {
      console.error('replaceExercise error:', e)
      error.value = 'Failed to replace exercise. Please try again.'
    }
  }
}

async function deleteExercise(index: number) {
  const exData = sessionExercises.value[index]

  const confirmed = confirm(
    exData.sets.length > 0
      ? 'This exercise has sets attached. Delete it?'
      : 'Delete this exercise?'
  )

  if (!confirmed) return

  if (
    exData.isSaved &&
    exData.sessionExerciseId &&
    currentSession.value
  ) {
    try {
      await api.sessions.deleteSessionExercise(currentSession.value.id, exData.sessionExerciseId)
    } catch (e) {
      console.error('deleteExercise error:', e)
      error.value = 'Failed to delete exercise. Please try again.'
      return
    }
  }

  // ALWAYS delete locally after API (or immediately if unsaved)
  sessionExercises.value.splice(index, 1)
  sessionExercises.value.forEach((e, i) => {
    e.exerciseNumber = i
  })
}

// UI helpers for exercise card status
function getStatusText(exData: SessionExerciseData): string {
  if (exData.isSaved) return 'Saved'
  if (exData.sets.length === 0) return 'Not started'
  return 'In progress'
}

function getStatusClass(exData: SessionExerciseData): string {
  if (exData.isSaved) return 'status-saved'
  if (exData.sets.length === 0) return 'status-not-started'
  return 'status-in-progress'
}

async function loadWorkouts() {
  loading.value = true
  error.value = null

  try {
    const collection = await api.getExerciseCollection()
    exerciseCollection.value = collection
    workouts.value = collection.workouts || []
    exerciseLookup.value = new Map((collection.exercises || []).map(ex => [ex.id, ex]))
  } catch (e) {
    error.value = 'Failed to load workouts. Please try again.'
    console.error('loadWorkouts error:', e)
  } finally {
    loading.value = false
  }
}

function addSet(exerciseIndex: number) {
  const exercise = sessionExercises.value[exerciseIndex]
  const newSet: SetData = {
    tempId: `temp-${Date.now()}-${Math.random()}`,
    // use 0-indexed setNumber
    setNumber: exercise.sets.length,
    weight: 0,
    reps: 0,
    warmUp: false,
    completed: false
  }
  exercise.sets.push(newSet)
}

function removeSet(exerciseIndex: number, setIndex: number) {
  const exercise = sessionExercises.value[exerciseIndex]
  exercise.sets.splice(setIndex, 1)
  exercise.sets.forEach((s, i) => (s.setNumber = i))
}

async function saveExercise(exerciseIndex: number) {
  const exData = sessionExercises.value[exerciseIndex]
  if (exData.sets.length === 0) {
    alert('Please add at least one set before saving.')
    return
  }
  if (!currentSession.value) {
    error.value = 'No active session'
    return
  }

  savingExercise.value = exerciseIndex
  try {
    const payload = {
      exerciseId: exData.exercise.id,
      exerciseNumber: exData.exerciseNumber,
      sets: exData.sets.map(s => ({ setNumber: s.setNumber, weight: s.weight, reps: s.reps, warmUp: s.warmUp }))
    }

    if (exData.isSaved && exData.sessionExerciseId) {
      const res = await api.sessions.updateSessionExercise(currentSession.value.id, exData.sessionExerciseId, { exerciseNumber: exData.exerciseNumber })
      if (res.isSuccess) {
        console.log('Exercise updated')
      }
    } else {
      const res = await api.sessions.createSessionExercise(currentSession.value.id, payload)
      if (!res.isSuccess || !res.data) throw new Error('Failed to save exercise')
      exData.sessionExerciseId = res.data.id
    }

    exData.isSaved = true
  } catch (e) {
    error.value = 'Failed to save exercise. Please try again.'
    console.error('saveExercise error:', e)
  } finally {
    savingExercise.value = null
  }
}

function finishSession() {
  if (!allExercisesSaved.value) {
    alert('Please save all exercises before finishing the session.')
    return
  }
  router.push('/stats')
}

const hasUnsavedData = computed(() => sessionExercises.value.some(ex => ex.sets.length > 0 && !ex.isSaved))
const allExercisesSaved = computed(() => {
  const exercisesWithSets = sessionExercises.value.filter(ex => ex.sets.length > 0)
  return exercisesWithSets.length > 0 && exercisesWithSets.every(ex => ex.isSaved)
})
</script>

<style scoped>
.session-view { display: flex; flex-direction: column; gap: var(--trk-space-4); padding-bottom: calc(80px + env(safe-area-inset-bottom, 0)); }
.view-header, .session-header { text-align: center; margin-bottom: var(--trk-space-4); }
 .view-title, .session-title { font-family: var(--trk-font-heading); font-size: clamp(1.75rem, 1.5rem + 1.25vw, 2.25rem); color: var(--trk-text); margin: 0; }
.exercise-list { display: flex; flex-direction: column; gap: var(--trk-space-3); }

/* Mobile-first card */
.exercise-card {
  background: var(--trk-surface);
  border-radius: var(--trk-radius-md);
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 10px;
  box-shadow: var(--trk-shadow-sm);
  border: 1px solid var(--trk-surface-border);
  cursor: pointer;
  transition: transform 0.1s ease, background 0.1s ease;
}
.exercise-card:active {
  transform: scale(0.99);
  background: var(--trk-surface-hover);
}
.exercise-card.exercise-saved { border-color: var(--trk-success-border); background: var(--trk-success-bg); }

.exercise-header { display: flex; justify-content: space-between; align-items: center; }
.exercise-title { font-size: 1rem; font-weight: 600; margin: 0; color: var(--trk-text); }

.exercise-meta { font-size: 0.85rem; color: var(--trk-text-muted); }

/* Icon button system */
.icon-btn {
  background: var(--trk-surface-alt);
  border: none;
  padding: 8px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--trk-text-muted);
  cursor: pointer;
  transition: background 0.15s ease, color 0.15s ease;
  min-width: 40px;
  min-height: 40px;
}
.icon-btn:active { background: var(--trk-surface-hover); }
.icon-btn:hover { color: var(--trk-text); }
.icon-btn.danger:hover, .icon-btn.danger:active { background: var(--trk-danger-bg); color: var(--trk-danger-text); }

.exercise-actions { display: flex; gap: 12px; }

/* Saved check — subtle and not competing with actions */
.saved-icon { color: var(--trk-success); display: inline-flex; align-items: center; justify-content: center; }

/* Slight stacking margin for visual separation */
.exercise-card + .exercise-card { margin-top: 12px; }

/* keep chevron styling available but hidden on cards to reduce clutter */
.item-chevron { display: none; }


.save-exercise-btn { width: 100%; margin-top: var(--trk-space-3); }

.add-exercise-action .btn-faded { margin-top: 20px; width: 100%; }

.session-footer { position: fixed; bottom: calc(56px + env(safe-area-inset-bottom, 0)); left: 0; right: 0; padding: var(--trk-space-4); background: var(--trk-bg); border-top: 1px solid var(--trk-surface-border); z-index: 10; }
.btn-finish { width: 100%; max-width: 400px; margin: 0 auto; display: flex; }
</style>
