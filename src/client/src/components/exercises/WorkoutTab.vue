<template>
    <!-- Workout Tab -->
    <div v-if="exerciseCollection.workouts.length === 0" class="empty-state">
        <Dumbbell class="empty-icon" />
        <h2 class="empty-title">No workouts yet</h2>
        <p class="empty-description">
            Create your first workout template to organise your exercises.
        </p>
        <button class="btn btn-primary" @click="showEditingWorkout = true">
            <Plus class="btn-icon" />
            Create Workout
        </button>
    </div>

    <ul v-else class="item-list">
        <li v-for="workout in exerciseCollection.workouts" :key="workout.id" class="item-card item-card-clickable"
            @click="openUpdateWorkout(workout)">
            <span class="item-name">{{ workout.name }}</span>
            <span v-if="!workout.userId" class="item-badge">Default</span>
        </li>
    </ul>

    <button v-if="exerciseCollection.workouts.length > 0" class="fab" @click="showEditingWorkout = true" aria-label="Add workout">
        <Plus />
    </button>

    <!-- Edit Workout Modal -->
    <div v-if="showEditingWorkout" class="modal-overlay" @click.self="closeEditWorkoutModal()">
        <div class="modal">
            <h2 class="modal-title">{{ updatingWorkout ? 'Edit' : 'Create' }} Workout</h2>
            <form @submit.prevent="saveWorkout">
                <div class="modal-content">
                    <div class="form-field">
                        <label for="edit-workout-name">Name</label>
                        <input id="edit-workout-name" v-model="editWorkoutName" type="text" placeholder="e.g., Push Day"
                            required :disabled="editWorkoutProcessing" />
                    </div>

                    <div class="form-field">
                        <SectionToggle v-model="showDefaultExercises">
                            Default Exercises
                        </SectionToggle>
                    </div>

                    <div v-if="showDefaultExercises" class="form-field">
                        <DefaultExercisesList
                                :workout-id="updatingWorkout?.id || emptyGuid"
                                :default-exercises="editWorkoutDefaultExercises"
                                :exercise-collection="exerciseCollection"
                                :disabled="editWorkoutProcessing"
                                @reorder="onDefaultExercisesReordered"
                            />
                    </div>
                </div>

                <div class="modal-actions">
                    <button type="button" class="btn btn-secondary" @click="closeEditWorkoutModal()"
                        :disabled="editWorkoutProcessing">Cancel</button>
                    <button type="button" class="btn btn-danger" @click="deleteWorkout"
                        :disabled="editWorkoutProcessing">Delete</button>
                    <button type="submit" class="btn btn-primary" :disabled="editWorkoutProcessing">Save</button>
                </div>
            </form>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { Dumbbell, Plus } from 'lucide-vue-next'
import DefaultExercisesList from './DefaultExercisesList.vue'
import { api } from '../../api/api'
import type { DefaultExercise, UpdateWorkoutRequest, Workout } from '../../api/modules/workouts'
import { ExerciseCollection } from '../../types/ExerciseCollection'
import { emptyGuid } from '../../types/Guid'
import SectionToggle from './SectionToggle.vue'

const props = defineProps<{ exerciseCollection: ExerciseCollection }>()
const emit = defineEmits<{
    (e: 'created', workout: Workout): void
    (e: 'updated', workout: Workout): void
    (e: 'deleted', workoutId: string): void
}>()

// Editing state
const showEditingWorkout = ref<boolean>(false)
const updatingWorkout = ref<Workout | null>(null)
const editWorkoutName = ref('')

// prevent body scroll when the edit/create workout modal is visible
watch(showEditingWorkout, (open) => {
    if (open) document.body.classList.add('no-scroll')
    else document.body.classList.remove('no-scroll')
})
const editWorkoutProcessing = ref(false)
const showDefaultExercises = ref(false)
const editWorkoutDefaultExercises = ref<DefaultExercise[]>([])
const changesMade = computed(() => {
    if (!showEditingWorkout.value) return false
    if (updatingWorkout.value?.name !== editWorkoutName.value.trim()) return true
    if ((updatingWorkout.value?.defaultExercises ?? []).length !== editWorkoutDefaultExercises.value.length) return true

    const originalSorted = [...(updatingWorkout.value?.defaultExercises ?? [])].sort((a, b) => (a.exerciseNumber ?? 0) - (b.exerciseNumber ?? 0))

    for (let i = 0; i < originalSorted.length; i++) {
        const original = originalSorted[i]
        const edited = editWorkoutDefaultExercises.value[i]
        if (!edited) return true
        if (original.exerciseId !== edited.exerciseId || original.exerciseNumber !== edited.exerciseNumber) {
            return true
        }
    }
    return false
})

// --- ensure we open edit state in a deterministic order
function openUpdateWorkout(w: Workout) {
    updatingWorkout.value = w
    editWorkoutName.value = w.name
    editWorkoutDefaultExercises.value = [...w.defaultExercises].sort((a, b) => (a.exerciseNumber ?? 0) - (b.exerciseNumber ?? 0))
    showEditingWorkout.value = true
}

function onDefaultExercisesReordered(reordered: DefaultExercise[]) {
    // accept the reordered list from the child (exerciseNumber already recalculated)
    editWorkoutDefaultExercises.value = reordered
}

function closeEditWorkoutModal(saved = false) {
    if (!saved && changesMade.value) {
        if (!confirm('You have unsaved changes. Discard them and close?')) {
            return
        }
    }

    updatingWorkout.value = null
    editWorkoutDefaultExercises.value = []
    editWorkoutName.value = ''
    showDefaultExercises.value = false
    showEditingWorkout.value = false
}

async function saveWorkout() {
    editWorkoutProcessing.value = true
    try {
        const payload : UpdateWorkoutRequest = { 
            name: editWorkoutName.value.trim(), 
            defaultExercises: editWorkoutDefaultExercises.value
        }
        const res = updatingWorkout.value
         ? await api.workouts.updateWorkout(updatingWorkout.value.id, payload)
         : await api.workouts.createWorkout(payload)
        if (res.isSuccess && res.data) {
            updatingWorkout.value
                ? emit('updated', res.data)
                : emit('created', res.data)

            closeEditWorkoutModal(true)
        } else {
            console.error('Failed to update/create workout:', res.error)
        }
        
    } catch (e) {
        console.error('Failed to update/create workout:', e)
    } finally {
        editWorkoutProcessing.value = false
    }
}

async function deleteWorkout() {
    if (!updatingWorkout.value) return
    if (!confirm('Delete this workout?')) return

    editWorkoutProcessing.value = true
    try {
        const res = await api.workouts.deleteWorkout(updatingWorkout.value.id)
        if (res.isSuccess) {
            emit('deleted', updatingWorkout.value.id)
            closeEditWorkoutModal()
        } else {
            console.error('Failed to delete workout:', res.error)
        }
    } catch (e) {
        console.error('Failed to delete workout:', e)
    } finally {
        editWorkoutProcessing.value = false
    }
}

</script>

<style scoped>
/* Section toggle for default exercises (component-specific) */
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

.modal {
  position: relative;
}

/* Make the edit-workout modal content scrollable when it grows too tall. */
.modal form {
  display: flex;
  flex-direction: column;
  /* ensure modal never exceeds viewport (account for overlay padding + modal chrome) */
  max-height: calc(100vh - 120px);
}

.modal-content {
  overflow: auto;
  -webkit-overflow-scrolling: touch;
  /* leave room for title + actions + modal padding */
  max-height: calc(100vh - 260px);
  padding-right: 6px; /* prevents content from being obscured by scrollbar */
  margin-bottom: var(--trk-space-4);
}

/* Keep action buttons visible while the content scrolls */
.modal-actions {
  flex: 0 0 auto;
  position: sticky;
  bottom: 0;
  display: flex;
  gap: var(--trk-space-3);
  padding-top: var(--trk-space-4);
  background: linear-gradient(var(--trk-surface), rgba(255,255,255,0));
  /* ensure buttons remain tappable above safe-area */
  padding-bottom: env(safe-area-inset-bottom, 0);
}
</style>
