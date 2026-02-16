import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { Session } from '../api/modules/sessions'
import type { Workout } from '../api/modules/workouts'
import type { Exercise } from '../api/modules/exercises'
import type { SessionExerciseData, SetData } from '../types/Session'

const SESSION_DRAFTS_KEY = 'session_drafts_v1'

type DraftPayload = {
  version: number
  session: Session
  workout: Workout | null
  exercises: SessionExerciseData[]
  persistedAt: string
}

export const useSessionStore = defineStore('session', () => {
  // State
  const drafts = ref<Record<string, DraftPayload>>({})
  const activeSession = ref<Session | null>(null)
  const currentWorkout = ref<Workout | null>(null)
  const sessionExercises = ref<SessionExerciseData[]>([])
  const lastPersistedAt = ref<string | null>(null)
  const draftVersion = 1

  // Getters
  const draftExists = computed(() => Object.keys(drafts.value).length > 0)
  const draftList = computed(() => Object.keys(drafts.value).map(id => ({ id, ...drafts.value[id] })))

  const hasUncompletedExercises = computed(() =>
    sessionExercises.value.some(ex => !ex.isCompleted)
  )

  const activeSessionId = computed(() => activeSession.value?.id ?? null)
  const exerciseCount = computed(() => sessionExercises.value.length)

  // Helpers
  function writeDraftsToStorage() {
    try {
      localStorage.setItem(SESSION_DRAFTS_KEY, JSON.stringify(drafts.value))
    } catch (e) {
      console.error('Failed to persist session drafts', e)
    }
  }

  function persistActiveDraft() {
    if (!activeSession.value) return
    const payload: DraftPayload = {
      version: draftVersion,
      session: activeSession.value,
      workout: currentWorkout.value,
      exercises: sessionExercises.value,
      persistedAt: new Date().toISOString()
    }
    drafts.value[activeSession.value.id] = payload
    lastPersistedAt.value = payload.persistedAt
    writeDraftsToStorage()
  }

  function initialize() {
    try {
      const raw = localStorage.getItem(SESSION_DRAFTS_KEY)
      if (raw) {
        const parsed = JSON.parse(raw) as Record<string, DraftPayload>
        drafts.value = parsed || {}
      }


    } catch (e) {
      console.warn('Ignored invalid session drafts in storage', e)
      try { localStorage.removeItem(SESSION_DRAFTS_KEY) } catch {}
      drafts.value = {}
    }
  }

  function getDraft(sessionId: string) {
    return drafts.value[sessionId] ?? null
  }

  function loadDraft(sessionId: string) {
    const d = getDraft(sessionId)
    if (!d) return false
    activeSession.value = d.session
    currentWorkout.value = d.workout
    sessionExercises.value = d.exercises
    lastPersistedAt.value = d.persistedAt
    return true
  }

  function clearDraft(sessionId?: string) {
    if (sessionId) {
      delete drafts.value[sessionId]
      // if currently active session matches, clear active
      if (activeSession.value?.id === sessionId) {
        activeSession.value = null
        currentWorkout.value = null
        sessionExercises.value = []
        lastPersistedAt.value = null
      }
      writeDraftsToStorage()
      return
    }

    // clear all
    drafts.value = {}
    activeSession.value = null
    currentWorkout.value = null
    sessionExercises.value = []
    lastPersistedAt.value = null
    try { localStorage.removeItem(SESSION_DRAFTS_KEY) } catch {}
  }

  // State setters
  function setActiveSession(session: Session, workout?: Workout | null, exercises?: SessionExerciseData[] | null) {
    activeSession.value = session
    if (workout) currentWorkout.value = workout
    if (exercises) sessionExercises.value = exercises
    persistActiveDraft()
  }

  function setExercises(exercises: SessionExerciseData[]) {
    sessionExercises.value = exercises
    // renumber exerciseNumber to keep consistency
    sessionExercises.value.forEach((e, i) => (e.exerciseNumber = i))
    // persist under active session id if present
    persistActiveDraft()
  }

  // Mutations
  function addExercise(exercise: Exercise) {
    const newEx: SessionExerciseData = {
      exercise,
      exerciseNumber: sessionExercises.value.length,
      sets: [],
      isCompleted: false
    }
    sessionExercises.value.push(newEx)
    persistActiveDraft()
    return sessionExercises.value.length - 1
  }

  function replaceExercise(index: number, exercise: Exercise) {
    const ex = sessionExercises.value[index]
    if (!ex) return
    ex.exercise = exercise
    persistActiveDraft()
  }

  function deleteExercise(index: number) {
    sessionExercises.value.splice(index, 1)
    sessionExercises.value.forEach((e, i) => (e.exerciseNumber = i))
    persistActiveDraft()
  }

  // Sets
  function addSet(exerciseIndex: number, set?: Partial<SetData>) {
    const ex = sessionExercises.value[exerciseIndex]
    if (!ex) return
    const newSet: SetData = {
      tempId: `temp-${Date.now()}-${Math.random()}`,
      setNumber: ex.sets.length,
      weight: set?.weight ?? 0,
      reps: set?.reps ?? 0,
      warmUp: set?.warmUp ?? false,
      completed: set?.completed ?? false,
      id: set?.id
    }
    ex.sets.push(newSet)
    persistActiveDraft()
  }

  function removeSet(exerciseIndex: number, setIndex: number) {
    const ex = sessionExercises.value[exerciseIndex]
    if (!ex) return
    ex.sets.splice(setIndex, 1)
    ex.sets.forEach((s, i) => (s.setNumber = i))
    persistActiveDraft()
  }

  function updateSet(exerciseIndex: number, setIndex: number, updates: Partial<SetData>) {
    const ex = sessionExercises.value[exerciseIndex]
    if (!ex) return
    const s = ex.sets[setIndex]
    if (!s) return
    Object.assign(s, updates)
    persistActiveDraft()
  }

  // Mark as completed (server-synced)
  function markExerciseCompleted(exerciseIndex: number, sessionExerciseId?: string, sets?: SetData[]) {
    const ex = sessionExercises.value[exerciseIndex]
    if (!ex) return
    ex.isCompleted = true
    if (sessionExerciseId) ex.sessionExerciseId = sessionExerciseId
    if (sets) ex.sets = sets
    persistActiveDraft()
  }

  return {
    // state
    drafts,
    activeSession,
    currentWorkout,
    sessionExercises,
    lastPersistedAt,
    // getters
    draftExists,
    draftList,
    hasUncompletedExercises,
    activeSessionId,
    exerciseCount,
    // actions
    initialize,
    getDraft,
    loadDraft,
    listDrafts: draftList,
    persistActiveDraft,
    clearDraft,
    clearAllDrafts: () => clearDraft(),
    setActiveSession,
    setExercises,
    // mutations
    addExercise,
    replaceExercise,
    deleteExercise,
    addSet,
    removeSet,
    updateSet,
    markExerciseCompleted
  }
})