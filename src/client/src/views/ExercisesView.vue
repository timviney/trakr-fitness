<template>
  <AppShell>
    <div class="exercises-view">
      <header class="view-header">
        <h1 class="view-title">Exercises & Workouts</h1>
      </header>

      <!-- Segment Toggle -->
      <div class="segment-toggle">
        <button
          class="segment-btn"
          :class="{ active: activeTab === 'workouts' }"
          @click="activeTab = 'workouts'"
        >
          Workouts
        </button>
        <button
          class="segment-btn"
          :class="{ active: activeTab === 'exercises' }"
          @click="activeTab = 'exercises'"
        >
          Exercises
        </button>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="loading-state">
        <Loader2 class="loading-spinner" />
        <span>Loading...</span>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="error-state">
        <p>{{ error }}</p>
        <button class="btn btn-primary" @click="loadData">Retry</button>
      </div>

      <!-- Workouts Tab -->
      <template v-else-if="activeTab === 'workouts'">
        <div v-if="workouts.length === 0" class="empty-state">
          <Dumbbell class="empty-icon" />
          <h2 class="empty-title">No workouts yet</h2>
          <p class="empty-description">
            Create your first workout template to organise your exercises.
          </p>
          <button class="btn btn-primary" @click="showCreateWorkout = true">
            <Plus class="btn-icon" />
            Create Workout
          </button>
        </div>

        <ul v-else class="item-list">
          <li v-for="workout in workouts" :key="workout.id" class="item-card">
            <span class="item-name">{{ workout.name }}</span>
          </li>
        </ul>

        <button
          v-if="workouts.length > 0"
          class="fab"
          @click="showCreateWorkout = true"
          aria-label="Add workout"
        >
          <Plus />
        </button>
      </template>

      <!-- Exercises Tab -->
      <template v-else-if="activeTab === 'exercises'">
        <div v-if="exercises.length === 0" class="empty-state">
          <Dumbbell class="empty-icon" />
          <h2 class="empty-title">No exercises yet</h2>
          <p class="empty-description">
            Add exercises to track during your workout sessions.
          </p>
          <button class="btn btn-primary" @click="showCreateExercise = true">
            <Plus class="btn-icon" />
            Add Exercise
          </button>
        </div>

        <ul v-else class="item-list">
          <li v-for="exercise in exercises" :key="exercise.id" class="item-card">
            <span class="item-name">{{ exercise.name }}</span>
            <span v-if="!exercise.userId" class="item-badge">Default</span>
          </li>
        </ul>

        <button
          v-if="exercises.length > 0"
          class="fab"
          @click="showCreateExercise = true"
          aria-label="Add exercise"
        >
          <Plus />
        </button>
      </template>

      <!-- Create Workout Modal -->
      <div v-if="showCreateWorkout" class="modal-overlay" @click.self="showCreateWorkout = false">
        <div class="modal">
          <h2 class="modal-title">New Workout</h2>
          <form @submit.prevent="createWorkout">
            <div class="form-field">
              <label for="workout-name">Name</label>
              <input
                id="workout-name"
                v-model="newWorkoutName"
                type="text"
                placeholder="e.g., Push Day"
                required
              />
            </div>
            <div class="modal-actions">
              <button type="button" class="btn btn-secondary" @click="showCreateWorkout = false">
                Cancel
              </button>
              <button type="submit" class="btn btn-primary" :disabled="creating">
                {{ creating ? 'Creating...' : 'Create' }}
              </button>
            </div>
          </form>
        </div>
      </div>

      <!-- Create Exercise Modal (placeholder - needs muscle group selection) -->
      <div v-if="showCreateExercise" class="modal-overlay" @click.self="showCreateExercise = false">
        <div class="modal">
          <h2 class="modal-title">New Exercise</h2>
          <p class="modal-note">
            Exercise creation requires muscle group selection. This feature will be available once
            muscle group APIs are implemented.
          </p>
          <div class="modal-actions">
            <button class="btn btn-primary" @click="showCreateExercise = false">Got it</button>
          </div>
        </div>
      </div>
    </div>
  </AppShell>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Dumbbell, Plus, Loader2 } from 'lucide-vue-next'
import AppShell from '../components/AppShell.vue'
import { api } from '../api/api'
import type { Workout } from '../api/modules/workouts'
import type { Exercise } from '../api/modules/exercises'

const activeTab = ref<'workouts' | 'exercises'>('workouts')
const loading = ref(true)
const error = ref<string | null>(null)
const creating = ref(false)

const workouts = ref<Workout[]>([])
const exercises = ref<Exercise[]>([])

const showCreateWorkout = ref(false)
const showCreateExercise = ref(false)
const newWorkoutName = ref('')

async function loadData() {
  loading.value = true
  error.value = null

  try {
    const [workoutsRes, exercisesRes] = await Promise.all([
      api.workouts.getWorkouts(),
      api.exercises.getExercises(),
    ])

    if (workoutsRes.data) {
      workouts.value = workoutsRes.data
    }
    if (exercisesRes.data) {
      exercises.value = exercisesRes.data
    }
  } catch (e) {
    error.value = 'Failed to load data. Please try again.'
    console.error(e)
  } finally {
    loading.value = false
  }
}

async function createWorkout() {
  if (!newWorkoutName.value.trim()) return

  creating.value = true
  try {
    const res = await api.workouts.createWorkout({ name: newWorkoutName.value.trim() })
    if (res.data) {
      workouts.value.push(res.data)
      newWorkoutName.value = ''
      showCreateWorkout.value = false
    }
  } catch (e) {
    console.error('Failed to create workout:', e)
  } finally {
    creating.value = false
  }
}

onMounted(loadData)
</script>

<style scoped>
.exercises-view {
  display: flex;
  flex-direction: column;
  gap: var(--trk-space-4);
}

.view-header {
  text-align: center;
}

.view-title {
  font-family: var(--trk-font-heading);
  font-size: clamp(1.5rem, 1.25rem + 1.25vw, 2rem);
  color: var(--trk-text);
  margin: 0;
}

/* Segment Toggle */
.segment-toggle {
  display: flex;
  background: var(--trk-surface);
  border-radius: var(--trk-radius-md);
  padding: 4px;
  gap: 4px;
}

.segment-btn {
  flex: 1;
  padding: var(--trk-space-2) var(--trk-space-3);
  border: none;
  border-radius: calc(var(--trk-radius-md) - 2px);
  background: transparent;
  color: var(--trk-text-muted);
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 150ms ease;
}

.segment-btn.active {
  background: var(--trk-accent);
  color: var(--trk-bg);
}

/* Loading */
.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--trk-space-3);
  padding: var(--trk-space-8);
  color: var(--trk-text-muted);
}

.loading-spinner {
  width: 32px;
  height: 32px;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

/* Error */
.error-state {
  text-align: center;
  padding: var(--trk-space-6);
  color: var(--trk-text-muted);
}

/* Empty State */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding: var(--trk-space-6) var(--trk-space-4);
}

.empty-icon {
  width: 48px;
  height: 48px;
  color: var(--trk-text-muted);
  opacity: 0.5;
  margin-bottom: var(--trk-space-3);
}

.empty-title {
  font-family: var(--trk-font-heading);
  font-size: 1.25rem;
  color: var(--trk-text);
  margin: 0 0 var(--trk-space-2);
}

.empty-description {
  color: var(--trk-text-muted);
  font-size: 0.875rem;
  line-height: 1.5;
  max-width: 260px;
  margin: 0 0 var(--trk-space-4);
}

/* Item List */
.item-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: var(--trk-space-2);
}

.item-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--trk-space-4);
  background: var(--trk-surface);
  border-radius: var(--trk-radius-md);
  border: 1px solid var(--trk-surface-border);
}

.item-name {
  color: var(--trk-text);
  font-weight: 500;
}

.item-badge {
  font-size: 0.75rem;
  padding: 2px 8px;
  background: var(--trk-accent-muted);
  color: var(--trk-bg);
  border-radius: 999px;
  font-weight: 500;
}

/* FAB */
.fab {
  position: fixed;
  bottom: calc(56px + env(safe-area-inset-bottom, 0) + var(--trk-space-4));
  right: var(--trk-space-4);
  width: 56px;
  height: 56px;
  border-radius: 50%;
  background: var(--trk-accent);
  color: var(--trk-bg);
  border: none;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: var(--trk-shadow);
  cursor: pointer;
  transition: transform 150ms ease;
}

.fab:active {
  transform: scale(0.95);
}

.fab svg {
  width: 24px;
  height: 24px;
}

/* Modal */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.7);
  display: flex;
  align-items: flex-end;
  justify-content: center;
  z-index: 200;
  padding: var(--trk-space-4);
}

.modal {
  width: 100%;
  max-width: 400px;
  background: var(--trk-surface);
  border-radius: var(--trk-radius-lg) var(--trk-radius-lg) 0 0;
  padding: var(--trk-space-6);
  padding-bottom: calc(var(--trk-space-6) + env(safe-area-inset-bottom, 0));
}

.modal-title {
  font-family: var(--trk-font-heading);
  font-size: 1.5rem;
  color: var(--trk-text);
  margin: 0 0 var(--trk-space-4);
}

.modal-note {
  color: var(--trk-text-muted);
  font-size: 0.875rem;
  line-height: 1.5;
  margin: 0 0 var(--trk-space-4);
}

.modal-actions {
  display: flex;
  gap: var(--trk-space-3);
  margin-top: var(--trk-space-4);
}

.modal-actions .btn {
  flex: 1;
}

.btn-icon {
  width: 18px;
  height: 18px;
  margin-right: 6px;
}

.btn-secondary {
  background: transparent;
  border: 1px solid var(--trk-surface-border);
  color: var(--trk-text);
}
</style>
