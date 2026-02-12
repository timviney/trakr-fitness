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
               :ref="el => setOverflowButtonRef(el as HTMLElement | null, idx)"
               @click="openCardMenu(idx)">

            <div class="exercise-header">
              <div class="exercise-title-group">
                <h3 class="exercise-title">{{ exData.exercise.name }}</h3>
              </div>

              <div class="muscle-name">{{ muscleName(exData.exercise.muscleGroupId) }}</div>
            </div>

            <div class="exercise-footer">
              <div class="status-area">
                <span class="status-pill" :class="getStatusClass(exData)">{{ getStatusText(exData) }}</span>
              </div>

              <div class="sets-area">
                <div class="exercise-meta">{{ exData.sets.length }} {{ exData.sets.length === 1 ? 'set' : 'sets' }}</div>
              </div>
            </div>

          </div>
        </div>

        <div class="add-exercise-action" style="padding: 0;">
          <button
            type="button"
            class="btn btn-secondary"
            @click="openAddExerciseSelector">
            + Add
          </button>
        </div>

        <!-- Floating overflow menu (full-screen popup) -->
        <Teleport to="body">
          <div v-if="openMenuIndex !== null" class="overflow-backdrop" @click="closeOverflowMenu">
            <div class="overflow-menu" @click.stop>
              <div class="overflow-panel">
                <header class="overflow-header">
                  <h3 class="overflow-title">{{ sessionExercises[openMenuIndex!].exercise.name }}</h3>
                </header>

                <div class="overflow-actions">
                  <button class="btn btn-primary overflow-action" @click="handleStart">
                    <Play class="icon"/>
                    <span>Start exercise</span>
                  </button>

                  <button class="btn btn-secondary overflow-action" @click="handleSwap">
                    <Repeat class="icon"/>
                    <span>Swap exercise</span>
                  </button>

                  <button class="btn btn-danger overflow-action" @click="handleDelete">
                    <Trash2 class="icon"/>
                    <span>Delete exercise</span>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </Teleport>

        <!-- Exercise detail modal -->
        <div v-if="showExerciseModal && selectedExerciseIndex !== null" class="modal-overlay">
          <!-- use the fullscreen modal variant so the sets table can expand -->
          <div class="modal modal-fullscreen">
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
import { ref, computed, onMounted, nextTick } from 'vue'
import { useRouter, onBeforeRouteLeave } from 'vue-router'
import { Dumbbell, ChevronRight, Repeat, Trash2, Play } from 'lucide-vue-next' 

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

// contextual menu state (overflow)
const openMenuIndex = ref<number | null>(null)
const overflowMenuStyle = ref<Record<string, string>>({})
const overflowButtonRefs = ref<Record<number, HTMLElement>>({})

function setOverflowButtonRef(el: HTMLElement | null, idx: number) {
  if (el) {
    overflowButtonRefs.value[idx] = el
  } else {
    delete overflowButtonRefs.value[idx]
  }
}

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
  // close any open overflow menu when opening modal
  openMenuIndex.value = null
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
    ex.sets = []
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
  // close menu and open selector in replace mode
  openMenuIndex.value = null
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
  // close any open menu immediately
  openMenuIndex.value = null

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

// Overflow (floating) menu controls
function openOverflow(index: number) {
  const button = overflowButtonRefs.value[index]

  openMenuIndex.value = index

  nextTick(() => {
    // fallback: fixed position in top-left area so menu is always visible
    if (!button) {
      overflowMenuStyle.value = {
        position: 'fixed',
        top: '120px',
        left: '16px'
      }
      return
    }

    const rect = button.getBoundingClientRect()

    overflowMenuStyle.value = {
      position: 'fixed',
      top: `${rect.bottom + 8}px`,
      left: `${Math.min(window.innerWidth - 188, rect.left)}px`
    }
  })
}

function closeOverflowMenu() {
  openMenuIndex.value = null
}

function handleSwap() {
  if (openMenuIndex.value !== null) {
    promptReplace(openMenuIndex.value)
    closeOverflowMenu()
  }
}

function handleDelete() {
  if (openMenuIndex.value !== null) {
    deleteExercise(openMenuIndex.value)
    closeOverflowMenu()
  }
}

function handleStart() {
  if (openMenuIndex.value !== null) {
    openExercise(openMenuIndex.value)
    closeOverflowMenu()
  }
}

function openCardMenu(index: number) {
  // primary action changed: clicking card now opens contextual popup
  openOverflow(index)
}

// UI helpers for exercise card status
function getStatusText(exData: SessionExerciseData): string {
  if (exData.isSaved) return 'Saved'
  return 'Not started'
}

function getStatusClass(exData: SessionExerciseData): string {
  if (exData.isSaved) return 'status-saved'
  return 'status-not-started'
}

function muscleName(muscleGroupId?: string | null) {
  if (!muscleGroupId || !exerciseCollection.value) return ''
  const g = exerciseCollection.value.muscleGroups?.find(m => m.id === muscleGroupId)
  return g ? g.name : ''
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

<style>
.session-view { display: flex; flex-direction: column; gap: var(--trk-space-4); padding-bottom: calc(80px + env(safe-area-inset-bottom, 0)); }
.view-header, .session-header { text-align: center; margin-bottom: var(--trk-space-4); }
 .view-title, .session-title { font-family: var(--trk-font-heading); font-size: clamp(1.75rem, 1.5rem + 1.25vw, 2.25rem); color: var(--trk-text); margin: 0; }
.exercise-list { display: flex; flex-direction: column; gap: var(--trk-space-3); }

/* Mobile-first card */
.exercise-card {
  background: var(--trk-surface);
  border-radius: var(--trk-radius-md);
  padding: 8px; /* reduced for slimmer cards */
  display: flex;
  flex-direction: column;
  gap: 6px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.04);
  border: 1px solid var(--trk-surface-border);
  cursor: pointer;
  transition: transform 0.1s ease, background 0.1s ease;
  -webkit-tap-highlight-color: transparent; 
} 

.exercise-meta { font-size: 0.9rem; font-weight: 600; color: var(--trk-text-muted); opacity: 0.95; }
.sets-area .exercise-meta { color: var(--trk-accent); font-weight: 700; }
.exercise-card:hover {
  transform: scale(0.99);
  background: var(--trk-accent-muted); 
}
.exercise-card.exercise-saved { border-left: 4px solid var(--trk-success); background: var(--trk-surface); }

.exercise-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.muscle-name {
  font-size: 0.85rem;
  color: var(--trk-text-muted);
  opacity: 0.95;
  white-space: nowrap;
  text-overflow: ellipsis;
  overflow: hidden;
  max-width: 50%;
  text-align: right;
}

.exercise-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 6px;
}

.status-area { flex: 1; display:flex; align-items:center; justify-content:flex-start; }
.sets-area { flex: 0 0 auto; display:flex; align-items:center; justify-content:flex-end; }

.exercise-title-group {
  display: flex;
  flex-direction: column;
  gap: 2px;
  flex: 1;
  min-width: 0;
}

.exercise-title { font-size: 1.12rem; font-weight: 700; letter-spacing: -0.01em; margin: 0; color: var(--trk-text); }

.exercise-actions { display: flex; gap: 12px; }

/* Status pill styles */
.status-pill { font-size: 0.9rem; padding: 0px 0px;  font-weight: 500;  display: inline-flex;  align-items: center;  justify-content: center; margin: 0; }
.status-saved { color: var(--trk-accent); }
.status-not-started { color: var(--trk-text-muted); }

/* Teleported overflow menu (full-screen popup, app-themed) */
.overflow-backdrop {
  position: fixed;
  inset: 0;
  z-index: 9999;
  background: rgba(0,0,0,0.36); /* subtle dim */
  display: flex;
  align-items: flex-end; /* slide-up feel on mobile */
  justify-content: center;
  padding: 20px;
  backdrop-filter: blur(6px);
}

.overflow-menu {
  position: relative;
  width: 100%;
  max-width: 720px;
  display: flex;
  align-items: center;
  justify-content: center;
  pointer-events: none;
  animation: menuFadeIn 160ms cubic-bezier(.2,.9,.2,1);
}

.overflow-panel {
  pointer-events: auto;
  width: 100%;
  background: var(--trk-surface);
  border-radius: 16px;
  padding: 18px;
  box-shadow: 0 28px 60px rgba(2,6,23,0.45);
  border: 1px solid var(--trk-surface-border);
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.overflow-header { display:flex; align-items:center; justify-content:space-between; gap:12px; }
.overflow-title { margin:0; font-size:1.1rem; font-weight:800; color:var(--trk-text); }

.overflow-actions { display:flex; flex-direction:column; gap:12px; margin-top:6px;}
.overflow-action { display:flex; text-align:right; align-items: center; gap:12px; padding:12px 14px; font-weight:800; font-size:1rem; border-radius:12px; justify-content:flex-start; }
.overflow-action svg { flex-shrink:0; }
.overflow-action span { flex:1; text-align:center; margin-right:18px; }
.overflow-action.icon { justify-content:center; padding:12px; }

@keyframes menuFadeIn { from { opacity: 0; transform: translateY(-4px) scale(0.98); } to { opacity: 1; transform: translateY(0) scale(1); } } .item-chevron { display: none; } .save-exercise-btn { width: 100%; margin-top: var(--trk-space-3); } .add-exercise-action .btn-secondary { margin-top: 20px; width: 100%; padding: 0.8rem 0; font-weight: 800; }
.session-footer { position: fixed; bottom: calc(56px + env(safe-area-inset-bottom, 0)); left: 0; right: 0; padding: var(--trk-space-4); background: var(--trk-bg); border-top: 1px solid var(--trk-surface-border); z-index: 10; }
.btn-finish { width: 100%; max-width: 400px; margin: 0 auto; display: flex; }
</style>
