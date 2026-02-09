<template>
  <AppShell>
    <div class="exercises-view">
      <header class="view-header">
        <h1 class="view-title">Exercises & Workouts</h1>
      </header>

      <!-- Segment Toggle -->
      <SegmentToggle v-model="activeTab" :optionsLabels="{ workouts: 'Workouts', exercises: 'Exercises' }" />

      <!-- Loading State -->
      <Loader :loading="loading"/>

      <!-- Error State -->
      <Error :message="error" @retry="loadData" />

      <!-- Workouts Tab (extracted to component) -->
      <WorkoutTab v-if="!loading && !error && activeTab === 'workouts'" 
        :exercise-collection="exerciseCollection"
        @created="workout => exerciseCollection.workouts.push(workout)"
        @updated="updatedWorkout => {
          const idx = exerciseCollection.workouts.findIndex(w => w.id === updatedWorkout.id)
          if (idx >= 0) exerciseCollection.workouts[idx] = updatedWorkout
        }"
        @deleted="workoutId => exerciseCollection.workouts = exerciseCollection.workouts.filter(w => w.id !== workoutId)"
      />

      <!-- Exercises Tab -->
      <template v-if="!loading && !error && activeTab === 'exercises'">
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
          <div v-if="exerciseCollection.muscleCategories.length === 0" class="empty-state">
            <Dumbbell class="empty-icon" />
            <h2 class="empty-title">No muscle categories</h2>
            <p class="empty-description">
              Muscle categories will appear here once configured.
            </p>
          </div>

          <ul v-else class="item-list">
            <li
              v-for="category in exerciseCollection.muscleCategories"
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
              <label>Muscle Group</label>
              <MuscleGroupSelector
                v-model="editSelection"
                :categories="exerciseCollection.muscleCategories"
                :groups="exerciseCollection.muscleGroups"
                :disabled="editProcessing || isEditingDefault"
              />
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
import { ref, computed, onMounted, watch } from 'vue'
import { Dumbbell, Plus, ChevronRight } from 'lucide-vue-next'
import AppShell from '../components/AppShell.vue'
import MuscleGroupSelector from '../components/MuscleGroupSelector.vue'
import { api } from '../api/api'
import WorkoutTab from '../components/WorkoutTab.vue'
import SegmentToggle from '../components/SegmentToggle.vue'
import Loader from '../components/Loader.vue'
import Error from '../components/Error.vue'
import type { Exercise } from '../api/modules/exercises'
import type { MuscleCategory, MuscleGroup } from '../api/modules/muscles'
import { ExerciseCollection } from '../types/ExerciseCollection'

const activeTab = ref<'workouts' | 'exercises'>('exercises')
const loading = ref(true)
const error = ref<string | null>(null)
const creating = ref(false)

const exerciseCollection = ref<ExerciseCollection>({workouts: [], exercises: [], muscleCategories: [], muscleGroups: []})
const showCreateExercise = ref(false)
const newExerciseName = ref('')

// Drill-down navigation state
const selectedCategory = ref<MuscleCategory | null>(null)
const selectedGroup = ref<MuscleGroup | null>(null)

const currentMuscleGroups = computed(() => {
  if (!selectedCategory.value) return []
  return exerciseCollection.value.muscleGroups.filter((g) => g.categoryId === selectedCategory.value!.id)
})

const currentExercises = computed(() => {
  if (!selectedGroup.value) return []
  return exerciseCollection.value.exercises.filter((e) => e.muscleGroupId === selectedGroup.value!.id)
})

function getExerciseCount(groupId: string): number {
  return exerciseCollection.value.exercises.filter((e) => e.muscleGroupId === groupId).length
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

const editSelection = ref({ categoryId: '', groupId: '' })

watch(editSelection, (val) => {
  editCategoryId.value = val.categoryId ?? ''
  editGroupId.value = val.groupId ?? ''
}, { deep: true })

const isEditingDefault = computed(() => {
  return !editingExercise.value?.userId
})

function openEditExercise(ex: Exercise) {
  editingExercise.value = ex
  editName.value = ex.name
  const group = exerciseCollection.value.muscleGroups.find((g) => g.id === ex.muscleGroupId)
  editGroupId.value = ex.muscleGroupId
  editCategoryId.value = group ? group.categoryId : ''
  editSelection.value = { categoryId: editCategoryId.value, groupId: editGroupId.value }
  // ensure create modal is closed
  showCreateExercise.value = false
}



function closeEditModal() {
  editingExercise.value = null
  editName.value = ''
  editCategoryId.value = ''
  editGroupId.value = ''
}

async function updateExercise() {
  if (!editingExercise.value) return
  if (isEditingDefault.value) return
  if (!editName.value.trim() || !editSelection.value.groupId) return

  editProcessing.value = true
  try {
    const res = await api.exercises.updateExercise(editingExercise.value.id, {
      name: editName.value.trim(),
      muscleGroupId: editSelection.value.groupId,
    })
    if (res.isSuccess && res.data) {
      const idx = exerciseCollection.value.exercises.findIndex((e) => e.id === res.data!.id)
      if (idx >= 0) exerciseCollection.value.exercises[idx] = res.data!
      // if moved group, and current selectedGroup doesn't match, navigate to the new group
      if (selectedGroup.value && selectedGroup.value.id !== res.data!.muscleGroupId) {
        const newGroup = exerciseCollection.value.muscleGroups.find((g) => g.id === res.data!.muscleGroupId)
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
      exerciseCollection.value.exercises = exerciseCollection.value.exercises.filter((e) => e.id !== editingExercise.value!.id)
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
    exerciseCollection.value = await api.getExerciseCollection()
  } catch (e) {
    error.value = 'Failed to load data. Please try again.'
    console.error(e)
  } finally {
    loading.value = false
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
      exerciseCollection.value.exercises.push(res.data)
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
  position: relative;
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

/* Section toggle for default exercises */
.section-toggle {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--trk-space-3);
  padding: calc(var(--trk-space-2) - 2px) calc(var(--trk-space-3) - 2px);
  background: transparent;
  border-radius: var(--trk-radius-sm);
  cursor: pointer;
}
.section-toggle:focus {
  outline: 2px solid rgba(0,0,0,0.06);
}
.section-title {
  font-weight: 600;
  color: var(--trk-text);
}
.section-icon {
  width: 18px;
  height: 18px;
  color: var(--trk-text-muted);
  transition: transform 150ms ease;
}
.section-icon.rotated {
  transform: rotate(180deg);
}

.add-default-row {
  display: flex;
  gap: var(--trk-space-2);
  align-items: center;
}
.add-default-row select {
  padding: var(--trk-space-2);
  border-radius: var(--trk-radius-sm);
  border: 1px solid var(--trk-surface-border);
  background: var(--trk-surface);
  color: var(--trk-text);
}

.add-default-panel {
  display: flex;
  gap: var(--trk-space-2);
  align-items: center;
  width: 100%;
}
.add-default-actions {
  display: flex;
  gap: var(--trk-space-2);
}

@media (max-width: 520px) {
  .add-default-panel {
    flex-direction: column;
    align-items: stretch;
  }
  .add-default-actions {
    display: flex;
    gap: var(--trk-space-2);
    flex-direction: row;
  }
  .add-default-actions .btn {
    flex: 1 1 0;
  }
}

.add-candidates {
  width: 100%;
}
.add-candidates .item-list {
  max-height: 160px;
  overflow-y: auto;
  padding: 0;
}
.add-candidate {
  padding: 8px 10px;
  border-radius: 6px;
}
.add-candidate.selected {
  background: var(--trk-accent);
  color: var(--trk-bg);
  border-color: var(--trk-accent);
}

/* Overlay panel for candidate selection */
.add-candidates-overlay {
  position: absolute;
  right: var(--trk-space-4);
  top: calc(52px);
  z-index: 300;
  width: calc(100% - var(--trk-space-8));
  max-width: 420px;
  display: flex;
  justify-content: center;
}
.add-candidates-panel {
  width: 100%;
  background: var(--trk-surface);
  border: 1px solid var(--trk-surface-border);
  border-radius: var(--trk-radius-md);
  box-shadow: var(--trk-shadow);
  padding: var(--trk-space-3);
}
.add-candidates-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--trk-space-2);
  margin-bottom: var(--trk-space-2);
}
.add-candidates-body {
  display: flex;
  flex-direction: column;
  gap: var(--trk-space-2);
}
.add-candidates-footer {
  display: flex;
  gap: var(--trk-space-2);
  margin-top: var(--trk-space-2);
}

/* faded button variant to match list styling */
.btn-faded {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: var(--trk-space-2);
  border-radius: var(--trk-radius-md);
  border: 1px solid var(--trk-surface-border);
  background: transparent;
  color: var(--trk-text);
  padding: 0.65rem 0.75rem;
  font-weight: 800;
  cursor: pointer;
  transition: background 150ms ease, color 150ms ease, border-color 150ms ease, box-shadow 120ms ease;
}
.btn-faded:hover {
  background: var(--trk-accent);
  color: var(--trk-text-dark);
  border-color: var(--trk-accent);
  box-shadow: 0 6px 18px rgba(0,0,0,0.35);
}
.btn-faded:active {
  transform: translateY(1px) scale(0.995);
}
.btn-faded:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

@media (max-width: 520px) {
  .add-candidates-overlay {
    position: fixed;
    inset: 0;
    padding: calc(var(--trk-space-4) + env(safe-area-inset-top, 0)) var(--trk-space-4) var(--trk-space-4);
    background: rgba(0,0,0,0.35);
    display: flex;
    align-items: flex-end;
    justify-content: center;
  }
  .add-candidates-panel {
    max-width: 100%;
    border-radius: var(--trk-radius-lg) var(--trk-radius-lg) 0 0;
    padding-bottom: calc(var(--trk-space-6) + env(safe-area-inset-bottom, 0));
  }
}

@media (max-width: 520px) {
  .add-candidates .item-list {
    max-height: 220px;
  }
}

/* Clickable Item Cards */
.item-card-clickable {
  cursor: pointer;
  transition: background 150ms ease, border-color 150ms ease;
}

.item-card-clickable:hover {
  background: var(--trk-surface-hover);
  border-color: var(--trk-accent-muted);
}

.item-card-clickable:active {
  transform: scale(0.98);
}

.item-card-clickable.selected {
  transform: scale(0.98);
  background: var(--trk-accent);
  color: var(--trk-text-dark);
}

.item-card-clickable.selected .item-name {
  color: var(--trk-text-dark);
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
