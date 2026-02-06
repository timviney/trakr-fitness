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
        <!-- Breadcrumb Navigation -->
        <nav v-if="selectedCategory || selectedGroup" class="breadcrumb">
          <button class="breadcrumb-link" @click="navigateToCategories">
            Categories
          </button>
          <template v-if="selectedCategory">
            <ChevronRight class="breadcrumb-sep" />
            <button
              class="breadcrumb-link"
              :class="{ current: !selectedGroup }"
              @click="navigateToCategory(selectedCategory)"
            >
              {{ selectedCategory.name }}
            </button>
          </template>
          <template v-if="selectedGroup">
            <ChevronRight class="breadcrumb-sep" />
            <span class="breadcrumb-current">{{ selectedGroup.name }}</span>
          </template>
        </nav>

        <!-- Categories List -->
        <template v-if="!selectedCategory">
          <div v-if="muscleCategories.length === 0" class="empty-state">
            <Dumbbell class="empty-icon" />
            <h2 class="empty-title">No muscle categories</h2>
            <p class="empty-description">
              Muscle categories will appear here once configured.
            </p>
          </div>

          <ul v-else class="item-list">
            <li
              v-for="category in muscleCategories"
              :key="category.id"
              class="item-card item-card-clickable"
              @click="navigateToCategory(category)"
            >
              <span class="item-name">{{ category.name }}</span>
              <ChevronRight class="item-chevron" />
            </li>
          </ul>
        </template>

        <!-- Muscle Groups List -->
        <template v-else-if="selectedCategory && !selectedGroup">
          <div v-if="currentMuscleGroups.length === 0" class="empty-state">
            <Dumbbell class="empty-icon" />
            <h2 class="empty-title">No muscle groups</h2>
            <p class="empty-description">
              No muscle groups in {{ selectedCategory.name }}.
            </p>
          </div>

          <ul v-else class="item-list">
            <li
              v-for="group in currentMuscleGroups"
              :key="group.id"
              class="item-card item-card-clickable"
              @click="navigateToGroup(group)"
            >
              <span class="item-name">{{ group.name }}</span>
              <span class="item-count">{{ getExerciseCount(group.id) }}</span>
              <ChevronRight class="item-chevron" />
            </li>
          </ul>
        </template>

        <!-- Exercises List -->
        <template v-else-if="selectedGroup">
          <div v-if="currentExercises.length === 0" class="empty-state">
            <Dumbbell class="empty-icon" />
            <h2 class="empty-title">No exercises yet</h2>
            <p class="empty-description">
              Add your first {{ selectedGroup.name }} exercise.
            </p>
            <button class="btn btn-primary" @click="showCreateExercise = true">
              <Plus class="btn-icon" />
              Add Exercise
            </button>
          </div>

          <ul v-else class="item-list">
            <li
              v-for="exercise in currentExercises"
              :key="exercise.id"
              class="item-card item-card-clickable"
              @click="openEditExercise(exercise)"
            >
              <span class="item-name">{{ exercise.name }}</span>
              <span v-if="!exercise.userId" class="item-badge">Default</span>
            </li>
          </ul>

          <button
            v-if="currentExercises.length > 0"
            class="fab"
            @click="showCreateExercise = true"
            aria-label="Add exercise"
          >
            <Plus />
          </button>
        </template>
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

      <!-- Create Exercise Modal -->
      <div v-if="showCreateExercise" class="modal-overlay" @click.self="closeExerciseModal">
        <div class="modal">
          <h2 class="modal-title">New {{ selectedGroup?.name }} Exercise</h2>
          <form @submit.prevent="createExercise">
            <div class="form-field">
              <label for="exercise-name">Name</label>
              <input
                id="exercise-name"
                v-model="newExerciseName"
                type="text"
                placeholder="e.g., Bench Press"
                required
              />
            </div>
            <div class="modal-actions">
              <button type="button" class="btn btn-secondary" @click="closeExerciseModal">
                Cancel
              </button>
              <button type="submit" class="btn btn-primary" :disabled="creating">
                {{ creating ? 'Creating...' : 'Create' }}
              </button>
            </div>
          </form>
        </div>
      </div>

      <!-- Edit Exercise Modal -->
      <div v-if="editingExercise" class="modal-overlay" @click.self="closeEditModal">
        <div class="modal">
          <h2 class="modal-title">{{ isEditingDefault ? "Default Exercise" : "Edit Exercise" }}</h2>
          <form @submit.prevent="updateExercise">
            <div class="form-field">
              <label for="edit-exercise-name">Name</label>
              <input
                id="edit-exercise-name"
                v-model="editName"
                type="text"
                placeholder="e.g., Bench Press"
                required
                :disabled="editProcessing || isEditingDefault"
              />
            </div>

            <div class="form-field">
              <label for="edit-category">Category</label>
              <select id="edit-category" v-model="editCategoryId" @change="onEditCategoryChange" :disabled="editProcessing || isEditingDefault">
                <option value="" disabled>Select a category</option>
                <option v-for="c in muscleCategories" :key="c.id" :value="c.id">{{ c.name }}</option>
              </select>
            </div>

            <div class="form-field">
              <label for="edit-group">Muscle Group</label>
              <select id="edit-group" v-model="editGroupId" :disabled="!editCategoryId || editProcessing || isEditingDefault">
                <option value="" disabled>Select a group</option>
                <option v-for="g in editFilteredGroups" :key="g.id" :value="g.id">{{ g.name }}</option>
              </select>
            </div>

            <div class="modal-actions">
              <button type="button" class="btn btn-secondary" @click="closeEditModal" :disabled="editProcessing">Cancel</button>
              <button type="button" class="btn btn-danger" @click="deleteExercise" v-if="!isEditingDefault" :disabled="editProcessing">Delete</button>
              <button type="submit" class="btn btn-primary" v-if="!isEditingDefault" :disabled="editProcessing">Save</button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </AppShell>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { Dumbbell, Plus, Loader2, ChevronRight } from 'lucide-vue-next'
import AppShell from '../components/AppShell.vue'
import { api } from '../api/api'
import type { Workout } from '../api/modules/workouts'
import type { Exercise } from '../api/modules/exercises'
import type { MuscleCategory, MuscleGroup } from '../api/modules/muscles'

const activeTab = ref<'workouts' | 'exercises'>('workouts')
const loading = ref(true)
const error = ref<string | null>(null)
const creating = ref(false)

const workouts = ref<Workout[]>([])
const exercises = ref<Exercise[]>([])
const muscleCategories = ref<MuscleCategory[]>([])
const muscleGroups = ref<MuscleGroup[]>([])

const showCreateWorkout = ref(false)
const showCreateExercise = ref(false)
const newWorkoutName = ref('')
const newExerciseName = ref('')

// Drill-down navigation state
const selectedCategory = ref<MuscleCategory | null>(null)
const selectedGroup = ref<MuscleGroup | null>(null)

const currentMuscleGroups = computed(() => {
  if (!selectedCategory.value) return []
  return muscleGroups.value.filter((g) => g.categoryId === selectedCategory.value!.id)
})

const currentExercises = computed(() => {
  if (!selectedGroup.value) return []
  return exercises.value.filter((e) => e.muscleGroupId === selectedGroup.value!.id)
})

function getExerciseCount(groupId: string): number {
  return exercises.value.filter((e) => e.muscleGroupId === groupId).length
}

function navigateToCategories() {
  selectedCategory.value = null
  selectedGroup.value = null
}

function navigateToCategory(category: MuscleCategory) {
  selectedCategory.value = category
  selectedGroup.value = null
}

function navigateToGroup(group: MuscleGroup) {
  selectedGroup.value = group
}

function closeExerciseModal() {
  showCreateExercise.value = false
  newExerciseName.value = ''
}

// Editing state
const editingExercise = ref<Exercise | null>(null)
const editName = ref('')
const editCategoryId = ref('')
const editGroupId = ref('')
const editProcessing = ref(false)

const editFilteredGroups = computed(() => {
  if (!editCategoryId.value) return []
  return muscleGroups.value.filter((g) => g.categoryId === editCategoryId.value)
})

const isEditingDefault = computed(() => {
  return !editingExercise.value?.userId
})

function openEditExercise(ex: Exercise) {
  editingExercise.value = ex
  editName.value = ex.name
  const group = muscleGroups.value.find((g) => g.id === ex.muscleGroupId)
  editGroupId.value = ex.muscleGroupId
  editCategoryId.value = group ? group.categoryId : ''
  // ensure create modal is closed
  showCreateExercise.value = false
}

function closeEditModal() {
  editingExercise.value = null
  editName.value = ''
  editCategoryId.value = ''
  editGroupId.value = ''
}

function onEditCategoryChange() {
  editGroupId.value = ''
}

async function updateExercise() {
  if (!editingExercise.value) return
  if (isEditingDefault.value) return
  if (!editName.value.trim() || !editGroupId.value) return

  editProcessing.value = true
  try {
    const res = await api.exercises.updateExercise(editingExercise.value.id, {
      name: editName.value.trim(),
      muscleGroupId: editGroupId.value,
    })
    if (res.isSuccess && res.data) {
      const idx = exercises.value.findIndex((e) => e.id === res.data!.id)
      if (idx >= 0) exercises.value[idx] = res.data!
      // if moved group, and current selectedGroup doesn't match, navigate to the new group
      if (selectedGroup.value && selectedGroup.value.id !== res.data!.muscleGroupId) {
        const newGroup = muscleGroups.value.find((g) => g.id === res.data!.muscleGroupId)
        if (newGroup) selectedGroup.value = newGroup
      }
      closeEditModal()
    } else {
      console.error('Failed to update exercise:', res.error)
    }
  } catch (e) {
    console.error('Failed to update exercise:', e)
  } finally {
    editProcessing.value = false
  }
}

async function deleteExercise() {
  if (!editingExercise.value) return
  if (isEditingDefault.value) return
  if (!confirm('Delete this exercise?')) return

  editProcessing.value = true
  try {
    const res = await api.exercises.deleteExercise(editingExercise.value.id)
    if (res.isSuccess) {
      exercises.value = exercises.value.filter((e) => e.id !== editingExercise.value!.id)
      closeEditModal()
    } else {
      console.error('Failed to delete exercise:', res.error)
    }
  } catch (e) {
    console.error('Failed to delete exercise:', e)
  } finally {
    editProcessing.value = false
  }
}

async function loadData() {
  loading.value = true
  error.value = null

  try {
    const [workoutsRes, exercisesRes, categoriesRes, groupsRes] = await Promise.all([
      api.workouts.getWorkouts(),
      api.exercises.getExercises(),
      api.muscles.getMuscleCategories(),
      api.muscles.getMuscleGroups(),
    ])

    if (workoutsRes.data) {
      workouts.value = workoutsRes.data
    }
    if (exercisesRes.data) {
      exercises.value = exercisesRes.data
    }
    if (categoriesRes.data) {
      muscleCategories.value = categoriesRes.data
    }
    if (groupsRes.data) {
      muscleGroups.value = groupsRes.data
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

async function createExercise() {
  if (!newExerciseName.value.trim() || !selectedGroup.value) return

  creating.value = true
  try {
    const res = await api.exercises.createExercise({
      name: newExerciseName.value.trim(),
      muscleGroupId: selectedGroup.value.id,
    })
    if (res.data) {
      exercises.value.push(res.data)
      closeExerciseModal()
    }
  } catch (e) {
    console.error('Failed to create exercise:', e)
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

.btn-danger {
  background: #ef4444;
  color: white;
  border: none;
  padding: 8px 12px;
  border-radius: 6px;
}

/* Breadcrumb Navigation */
.breadcrumb {
  display: flex;
  align-items: center;
  gap: var(--trk-space-1);
  padding: var(--trk-space-2) 0;
  flex-wrap: wrap;
}

.breadcrumb-link {
  background: none;
  border: none;
  padding: var(--trk-space-1) var(--trk-space-2);
  color: var(--trk-accent);
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  border-radius: var(--trk-radius-sm);
  transition: background 150ms ease;
}

.breadcrumb-link:hover {
  background: var(--trk-surface);
}

.breadcrumb-link.current {
  color: var(--trk-text);
  cursor: default;
}

.breadcrumb-link.current:hover {
  background: transparent;
}

.breadcrumb-sep {
  width: 16px;
  height: 16px;
  color: var(--trk-text-muted);
  flex-shrink: 0;
}

.breadcrumb-current {
  color: var(--trk-text);
  font-size: 0.875rem;
  font-weight: 500;
  padding: var(--trk-space-1) var(--trk-space-2);
}

/* Clickable Item Cards */
.item-card-clickable {
  cursor: pointer;
  transition: background 150ms ease, border-color 150ms ease;
}

.item-card-clickable:hover {
  background: var(--trk-surface-hover, rgba(255, 255, 255, 0.05));
  border-color: var(--trk-accent-muted);
}

.item-card-clickable:active {
  transform: scale(0.99);
}

.item-chevron {
  width: 20px;
  height: 20px;
  color: var(--trk-text-muted);
  flex-shrink: 0;
}

.item-count {
  font-size: 0.75rem;
  color: var(--trk-text-muted);
  margin-left: auto;
  margin-right: var(--trk-space-2);
}
</style>
