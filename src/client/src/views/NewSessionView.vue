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

        <!-- Unfinished local drafts -->
        <div v-if="!loading && !error && sessionStore.draftList.length > 0" class="drafts-section">
          <h3 class="section-subtitle">Unfinished sessions</h3>
          <ul class="item-list">
            <li v-for="d in sessionStore.draftList" :key="d.session.id" class="item-card item-card-clickable" @click="openDraftModal(d.session.id)">
              <div class="item-left">
                <span class="item-name">{{ d.workout?.name ?? d.session.workoutId }}</span>
                <div class="item-sub">{{ new Date(d.persistedAt).toLocaleString() }}</div>
              </div>
            </li>
          </ul>
        </div>

        <div v-if="!loading && !error && workouts.length === 0" class="empty-state">
          <Dumbbell class="empty-icon" />
          <h2 class="empty-title">No workouts yet</h2>
          <p class="empty-description">Create a workout first in the Exercises tab.</p>
        </div>

<h3 class="section-subtitle">New session:</h3>
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

        <!-- Draft actions modal (teleported, same style as overflow) -->
        <Teleport to="body">
          <div v-if="showDraftModal" class="overflow-backdrop" @click.self="closeDraftModal()">
            <div class="overflow-menu" @click.stop>
              <div class="overflow-panel">
                <header class="overflow-header">
                  <h3 class="overflow-title">{{ selectedDraft?.workout?.name ?? (selectedDraft?.session?.workoutId ?? 'Unfinished session') }}</h3>
                </header>

                <div class="overflow-actions">
                  <button class="btn btn-primary overflow-action" @click="() => { if (selectedDraftId) resumeDraft(selectedDraftId); }">
                    Continue Editing
                  </button>

                  <button class="btn btn-danger overflow-action" @click="() => { if (selectedDraftId) deleteDraft(selectedDraftId); }">
                    Delete Changes
                  </button>

                  <button class="btn btn-secondary overflow-action" @click="closeDraftModal()">Cancel</button>
                </div>
              </div>
            </div>
          </div>
        </Teleport>
      </div>

      <!-- SESSION -->
      <div v-if="viewState === 'session'" class="session-active">
        <header class="session-header">
          <h1 class="session-title">{{ sessionStore.currentWorkout?.name }}</h1>
        </header>

        <Loader :loading="loading" />
        <AppError :message="error" @retry="() => {}" />

        <div v-if="!loading && !error" class="exercise-list">
          <div v-for="(exData, idx) in sessionStore.sessionExercises" 
               :key="exData.sessionExerciseId ?? `local-${exData.exerciseNumber}`" 
               class="exercise-card"
               :class="{ 'exercise-completed': exData.isCompleted }"
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
                <div class="exercise-meta">{{ exData.sets.filter(s => s.completed).length }} {{ exData.sets.filter(s => s.completed).length === 1 ? 'set' : 'sets' }}</div>
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
                  <h3 class="overflow-title">{{ sessionStore.sessionExercises[openMenuIndex!].exercise.name }}</h3>
                </header>

                <div class="overflow-actions">
                  <button class="btn btn-primary overflow-action" @click="handleOpenExercise">
                    <Play class="icon"/>
                    <span>{{ getOpenExerciseSetsText(sessionStore.sessionExercises[openMenuIndex!]) }}</span>
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
        <div v-if="showExerciseModal && selectedExerciseIndex !== null" class="modal-overlay fullscreen">
          <!-- use the fullscreen modal variant so the sets table can expand -->
          <div class="modal modal-fullscreen">
            <h2 class="modal-title">{{ sessionStore.sessionExercises[selectedExerciseIndex].exercise.name }}</h2>

            <div class="modal-content">
              <ExerciseSetTable
                v-model:sets="sessionStore.sessionExercises[selectedExerciseIndex].sets"
                @add-set="() => addSet(selectedExerciseIndex!)"
                @remove-set="(setIdx) => removeSet(selectedExerciseIndex!, setIdx)"
                @update:set="(set) => updateSet(set)"
              />
            </div>

            <div class="modal-actions">
              <button type="button" class="btn btn-secondary" @click="closeExerciseModal()">Close</button>
              <button type="button" class="btn btn-primary" @click="(async () => { sessionStore.sessionExercises[selectedExerciseIndex!].isCompleted = true; closeExerciseModal() })()">Complete Exercise</button>
            </div>
          </div>
        </div>

        <div class="session-footer">
          <button class="btn btn-primary btn-finish" @click="finishSession()" :disabled="sessionStore.hasUncompletedExercises">Finish Session</button>
        </div>

        <ExerciseSelector
          v-if="showExerciseSelector && exerciseCollection"
          :exercise-collection="exerciseCollection"
          :modelValue="exerciseSelectorModel"
          @add="handleExerciseSelected"
          @cancel="closeExerciseSelector"
        />
      </div>

    </div>
  </AppShell>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, nextTick } from 'vue'
import { useRouter, useRoute, onBeforeRouteLeave } from 'vue-router' 
import { Dumbbell, ChevronRight, Repeat, Trash2, Play } from 'lucide-vue-next' 

import AppShell from '../components/general/AppShell.vue'
import Loader from '../components/general/Loader.vue'
import AppError from '../components/general/Error.vue'
import ExerciseSetTable from '../components/session/ExerciseSetTable.vue'
import ExerciseSelector from '../components/exercises/ExerciseSelector.vue'

import { api } from '../api/api'
import type { Workout } from '../api/modules/workouts'
import type { Exercise } from '../api/modules/exercises'
import { getStatus, SessionStatus, SetData, type SessionExerciseData } from '../types/Session'
import { ExerciseCollection } from '../types/ExerciseCollection'
import { useSessionStore } from '../stores/session'
import { useStatsStore } from '../stores/stats' 

const router = useRouter()
const route = useRoute()
const sessionStore = useSessionStore()
const statsStore = useStatsStore()

const viewState = ref<'selection' | 'session'>('selection')
const loading = ref(false)
const error = ref<string | null>(null)

// selection / start modal state
const workouts = ref<Workout[]>([])
const pendingWorkout = ref<Workout | null>(null)
const showStartModal = ref(false)

// draft modal state (unfinished sessions)
const showDraftModal = ref(false)
const selectedDraftId = ref<string | null>(null)
const selectedDraft = computed(() => selectedDraftId.value ? sessionStore.getDraft(selectedDraftId.value) : null)

// active session state (moved to session store)
// use `sessionStore.activeSession`, `sessionStore.currentWorkout`, `sessionStore.sessionExercises`
const exerciseLookup = ref<Map<string, Exercise>>(new Map())
const exerciseCollection = ref<ExerciseCollection | null>(null)

// selector state
const showExerciseSelector = ref(false)
const exerciseSelectorMode = ref<'add' | 'replace'>('add')
const replaceTargetIndex = ref<number | null>(null)

const exerciseSelectorModel = computed(() => {
  if (exerciseSelectorMode.value === 'replace' && replaceTargetIndex.value !== null) {
    const ex = sessionStore.sessionExercises[replaceTargetIndex.value]?.exercise
    if (!ex) return null
    const groupId = ex.muscleGroupId || undefined
    const categoryId = exerciseCollection.value?.muscleGroups?.find(g => g.id === groupId)?.categoryId || undefined
    return { categoryId, groupId, exerciseId: ex.id }
  }
  return null
})

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

  // initialize session store (do not auto-restore drafts)
  sessionStore.initialize()

  const sessionId = route.params.id as string | undefined
  if (sessionId && sessionId !== 'new') {
    await loadSession(sessionId)
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

function openDraftModal(sessionId: string) {
  selectedDraftId.value = sessionId
  showDraftModal.value = true
}

function closeDraftModal() {
  showDraftModal.value = false
  selectedDraftId.value = null
}

function resumeDraft(sessionId: string) {
  if (!sessionId) return
  const ok = sessionStore.loadDraft(sessionId)
  if (!ok) {
    error.value = 'Failed to load local draft.'
    return
  }
  showDraftModal.value = false
  selectedDraftId.value = null
  viewState.value = 'session'
  router.replace(`/session/${sessionId}`)
}

function deleteDraft(sessionId: string) {
  if (!confirm('Delete this unfinished session draft?')) return
  sessionStore.clearDraft(sessionId)
  // if the draft modal was open for this id, close it
  if (selectedDraftId.value === sessionId) closeDraftModal()
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
    const newSession = sessionRes.data

    const workoutRes = await api.workouts.getWorkoutById(workout.id)
    if (!workoutRes.isSuccess || !workoutRes.data) throw new Error('Failed to load workout details')

    const mapped = (workoutRes.data.defaultExercises || [])
      .sort((a, b) => a.exerciseNumber - b.exerciseNumber)
      .map(defEx => {
        const exercise = exerciseLookup.value.get(defEx.exerciseId)
        return {
          exercise: exercise || { id: defEx.exerciseId, name: 'Unknown Exercise', muscleGroupId: '', userId: null },
          exerciseNumber: defEx.exerciseNumber,
          sets: [],
          isCompleted: false
        } as SessionExerciseData
      })

    // write into session store (store persists draft)
    sessionStore.setActiveSession(newSession, workoutRes.data, mapped)

    // prime session history (cached for 10s)
    await statsStore.fetchSessionHistory().catch(() => {})

    viewState.value = 'session'

    // update URL from /session/new --> /session/{id}
    router.replace(`/session/${newSession.id}`)
  } catch (e) {
    error.value = 'Failed to start session. Please try again.'
    console.error('startSession error:', e)
  } finally {
    loading.value = false
    pendingWorkout.value = null
  }
}

// --- Exercise modal handlers
async function openExercise(index: number) {
  // close any open overflow menu when opening modal
  openMenuIndex.value = null
  selectedExerciseIndex.value = index
  sessionStore.sessionExercises[index].isCompleted = false

  if (!sessionStore.sessionExercises[index].sets || sessionStore.sessionExercises[index].sets.length === 0) {
    // prefill sets from most recent history (use stats store). only if there are no sets yet.
    try {
      await statsStore.fetchSessionHistory().catch(() => {})
      const exId = sessionStore.sessionExercises[index].exercise.id
      const sessions = (statsStore.sessionHistory || [])
        .slice()
        .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())

      const recentSession = sessions.find(s => s.sessionExercises.some(se => se.exerciseId === exId))
      if (recentSession) {
        const se = recentSession.sessionExercises.find(se => se.exerciseId === exId)
        if (se && (!sessionStore.sessionExercises[index].sets || sessionStore.sessionExercises[index].sets.length === 0)) {
          // include warm-up sets and ensure ascending order by setNumber so set 0 comes first
          const sortedSets = [...(se.sets || [])].sort((a, b) => (a.setNumber ?? 0) - (b.setNumber ?? 0))
          const mappedSets = sortedSets.map((s, i) => ({
            tempId: `hist-${Date.now()}-${i}`,
            setNumber: i,
            weight: s.weight,
            reps: s.reps,
            warmUp: s.warmUp,
            completed: false
          }))
          sessionStore.sessionExercises[index].sets = mappedSets
          sessionStore.persistActiveDraft()
        }
      }
    } catch (err) {
      console.warn('prefill history failed', err)
    }
  }

  showExerciseModal.value = true
}

function closeExerciseModal() {
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
    sessionStore.addExercise(exercise)
  }

  if (exerciseSelectorMode.value === 'replace' && replaceTargetIndex.value !== null) {
    replaceExercise(replaceTargetIndex.value, exercise)
  }

  showExerciseSelector.value = false
}

async function replaceExercise(index: number, newExercise: Exercise) {
  sessionStore.replaceExercise(index, newExercise)
}

async function deleteExercise(index: number) {
  // close any open menu immediately
  openMenuIndex.value = null

  const exData = sessionStore.sessionExercises[index]

  const confirmed = confirm(
    exData.sets.length > 0
      ? 'This exercise has sets attached. Delete it?'
      : 'Delete this exercise?'
  )

  if (!confirmed) return

  sessionStore.deleteExercise(index)
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

function handleOpenExercise() {
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
  const status = getStatus(exData)
  switch (status) {
    case SessionStatus.NotStarted: return 'Not started'
    case SessionStatus.InProgress: return 'In progress'
    case SessionStatus.Completed: return 'Completed'
    default: return ''
  }
}

// UI helpers for exercise card status
function getOpenExerciseSetsText(exData: SessionExerciseData): string {
  const status = getStatus(exData)
  switch (status) {
    case SessionStatus.NotStarted: return 'Start exercise'
    case SessionStatus.InProgress: return 'Continue exercise'
    case SessionStatus.Completed: return 'Edit sets'
    default: return ''
  }
}

function getStatusClass(exData: SessionExerciseData): string {
  const status = getStatus(exData)
  switch (status) {
    case SessionStatus.NotStarted: return 'status-not-started'
    case SessionStatus.InProgress: return 'status-in-progress'
    case SessionStatus.Completed: return 'status-completed'
    default: return ''
  }
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

// Load an existing session by id (deep-link support for /session/:id)
async function loadSession(sessionId: string) {
  loading.value = true
  error.value = null

  try {
    if (sessionStore.draftList.some(d => d.session.id === sessionId)) {
      // if a local draft exists for this session id, load from the draft instead of API
      const ok = sessionStore.loadDraft(sessionId)
      if (!ok) {
        error.value = 'Failed to load local draft.'
        return
      }
      viewState.value = 'session'
      return
    }

    const sessionRes = await api.sessions.getSessionById(sessionId)
    if (!sessionRes.isSuccess || !sessionRes.data) throw new Error('Failed to load session')

    const workoutRes = await api.workouts.getWorkoutById(sessionRes.data.workoutId)
    if (!workoutRes.isSuccess || !workoutRes.data) throw new Error('Failed to load workout details')

    const sessExRes = await api.sessions.getSessionExercises(sessionId)
    if (!sessExRes.isSuccess || !sessExRes.data) throw new Error('Failed to load session exercises')

    const mapped = sessExRes.data
      .sort((a, b) => a.exerciseNumber - b.exerciseNumber)
      .map(se => ({
        exercise: exerciseLookup.value.get(se.exerciseId) || { id: se.exerciseId, name: 'Unknown Exercise', muscleGroupId: '', userId: null },
        exerciseNumber: se.exerciseNumber,
        sets: [],
        isCompleted: true,
        sessionExerciseId: se.id
      } as SessionExerciseData))

    // populate store first (sets fetched below)
    sessionStore.setActiveSession(sessionRes.data, workoutRes.data, mapped)

    // fetch sets for each exercise in parallel and update store
    await Promise.all(sessionStore.sessionExercises.map(async (exData, i) => {
      const seId = exData.sessionExerciseId!
      const setsRes = await api.sessions.getSets(seId)
      if (setsRes.isSuccess && setsRes.data) {
        const mappedSets = setsRes.data.map(s => ({
          id: s.id,
          tempId: s.id,
          setNumber: s.setNumber,
          weight: s.weight,
          reps: s.reps,
          warmUp: s.warmUp,
          completed: true
        }))
        sessionStore.markExerciseCompleted(i, seId, mappedSets)
      }
    }))

    viewState.value = 'session'
  } catch (e) {
    error.value = 'Failed to load session. Please try again.'
    console.error('loadSession error:', e)
    router.replace('/session/new')
  } finally {
    loading.value = false
  }
}

function addSet(exerciseIndex: number) {
  const ex = sessionStore.sessionExercises[exerciseIndex]
  if (!ex) {
    sessionStore.addSet(exerciseIndex)
    return
  }

  const last = ex.sets && ex.sets.length > 0 ? ex.sets[ex.sets.length - 1] : null
  if (last) {
    sessionStore.addSet(exerciseIndex, { weight: last.weight ?? 0, reps: last.reps ?? 0, warmUp: last.warmUp ?? false })
  } else {
    sessionStore.addSet(exerciseIndex)
  }
} 

function removeSet(exerciseIndex: number, setIndex: number) {
  sessionStore.removeSet(exerciseIndex, setIndex)
}

function updateSet(set: SetData){
  if (selectedExerciseIndex.value === null) return
  sessionStore.updateSet(selectedExerciseIndex.value, set.setNumber, set)
}

async function finishSession() {
  loading.value = true
  error.value = null

  const active = sessionStore.activeSession
  if (!active) {
    error.value = 'No active session to finish.'
    loading.value = false
    return
  }

  try {
    const sessionId = active.id

    // persist each session exercise (create or update). server may return sets as part of the response — prefer that.
    await Promise.all(sessionStore.sessionExercises.map(async (exData, exIndex) => {
      const payload = {
        exerciseId: exData.exercise.id,
        exerciseNumber: exData.exerciseNumber,
        sets: (exData.sets || []).map(s => ({
          id: s.id,
          setNumber: s.setNumber,
          weight: s.weight,
          reps: s.reps,
          warmUp: s.warmUp
        }))
      }

      if (exData.sessionExerciseId) {
        const upd = await api.sessions.updateSessionExercise(exData.sessionExerciseId, payload)
        if (!upd.isSuccess || !upd.data) throw new Error('Failed to update session exercise')
      } else {
        const created = await api.sessions.createSessionExercise(sessionId, payload)
        if (!created.isSuccess || !created.data) throw new Error('Failed to create session exercise')
      }
    }))

    // clear persisted draft for the active session only after successful API save
    const activeId = sessionStore.activeSessionId
    if (activeId) sessionStore.clearDraft(activeId)

    router.push('/stats')
  } catch (e) {
    error.value = 'Failed to save session. Please try again.'
    console.error('finishSession error:', e)
  } finally {
    loading.value = false
  }
}

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
.exercise-card.exercise-completed { border-left: 4px solid var(--trk-success-bg); background: var(--trk-surface); }

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
.status-completed { 
  background-color: var(--trk-success-bg);
  border: 1px solid var(--trk-success-border);
  color: var(--trk-success-text);
  padding: 0.1rem 0.4rem;
  border-radius: var(--trk-radius-md);
 }
.status-in-progress { color: var(--trk-accent); }
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

@keyframes menuFadeIn { from { opacity: 0; transform: translateY(-4px) scale(0.98); } to { opacity: 1; transform: translateY(0) scale(1); } } .item-chevron { display: none; } .add-exercise-action .btn-secondary { margin-top: 20px; width: 100%; padding: 0.8rem 0; font-weight: 800; }
.session-footer { position: fixed; bottom: calc(56px + env(safe-area-inset-bottom, 0)); left: 0; right: 0; padding: var(--trk-space-4); background: var(--trk-bg); border-top: 1px solid var(--trk-surface-border); z-index: 10; align-items: center; justify-content: center; display: flex; }
.btn-finish { width: 100%; max-width: 400px; margin: 0 auto; display: flex; align-items: center; justify-content: center;}

.item-sub {
  font-size: 0.85rem;
  color: var(--trk-text-muted);
}
</style>
