<template>
    <!-- Workout Tab -->
    <div v-if="exerciseCollection.workouts.length === 0" class="empty-state">
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
        <li v-for="workout in exerciseCollection.workouts" :key="workout.id" class="item-card item-card-clickable"
            @click="openEditWorkout(workout)">
            <span class="item-name">{{ workout.name }}</span>
            <span v-if="!workout.userId" class="item-badge">Default</span>
        </li>
    </ul>

    <button v-if="exerciseCollection.workouts.length > 0" class="fab" @click="showCreateWorkout = true" aria-label="Add workout">
        <Plus />
    </button>

    <!-- Create Workout Modal -->
    <div v-if="showCreateWorkout" class="modal-overlay" @click.self="showCreateWorkout = false">
        <div class="modal">
            <h2 class="modal-title">New Workout</h2>
            <form @submit.prevent="createWorkout">
                <div class="form-field">
                    <label for="workout-name">Name</label>
                    <input id="workout-name" v-model="newWorkoutName" type="text" placeholder="e.g., Push Day"
                        required />
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

    <!-- Edit Workout Modal -->
    <div v-if="editingWorkout" class="modal-overlay" @click.self="closeEditWorkoutModal()">
        <div class="modal">
            <h2 class="modal-title">Edit Workout</h2>
            <form @submit.prevent="updateWorkout">
                <div class="form-field">
                    <label for="edit-workout-name">Name</label>
                    <input id="edit-workout-name" v-model="editWorkoutName" type="text" placeholder="e.g., Push Day"
                        required :disabled="editWorkoutProcessing" />
                </div>

                <div class="form-field">
                    <div class="section-toggle" role="button" tabindex="0"
                        @click="showDefaultExercises = !showDefaultExercises">
                        <span class="section-title">Default Exercises</span>
                        <ChevronDown class="section-icon" :class="{ rotated: showDefaultExercises }" />
                    </div>
                </div>

                <div v-if="showDefaultExercises" class="form-field">
                    <DefaultExercisesList
                            :workout-id="editingWorkout.id"
                            :exercises="editWorkoutDefaultExercises"
                            :exercise-collection="exerciseCollection"
                            :disabled="editWorkoutProcessing"
                            @reorder="onDefaultExercisesReordered"
                        />
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
import { ref, computed } from 'vue'
import { Dumbbell, Plus, ChevronDown } from 'lucide-vue-next'
import DefaultExercisesList from './DefaultExercisesList.vue'
import { api } from '../../api/api'
import type { DefaultExercise, UpdateWorkoutRequest, Workout } from '../../api/modules/workouts'
import { ExerciseCollection } from '../../types/ExerciseCollection'

const props = defineProps<{ exerciseCollection: ExerciseCollection }>()
const emit = defineEmits<{
    (e: 'created', workout: Workout): void
    (e: 'updated', workout: Workout): void
    (e: 'deleted', workoutId: string): void
}>()

const creating = ref(false)
const showCreateWorkout = ref(false)
const newWorkoutName = ref('')

// Editing state
const editingWorkout = ref<Workout | null>(null)
const editWorkoutName = ref('')
const editWorkoutProcessing = ref(false)
const showDefaultExercises = ref(false)
const editWorkoutDefaultExercises = ref<DefaultExercise[]>([])
const changesMade = computed(() => {
    if (!editingWorkout.value) return false
    if (editingWorkout.value.name !== editWorkoutName.value.trim()) return true
    if ((editingWorkout.value.defaultExercises ?? []).length !== editWorkoutDefaultExercises.value.length) return true

    const originalSorted = [...(editingWorkout.value.defaultExercises ?? [])].sort((a, b) => (a.exerciseNumber ?? 0) - (b.exerciseNumber ?? 0))

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
function openEditWorkout(w: Workout) {
    editingWorkout.value = w
    editWorkoutName.value = w.name
    editWorkoutDefaultExercises.value = [...w.defaultExercises].sort((a, b) => (a.exerciseNumber ?? 0) - (b.exerciseNumber ?? 0))
    showCreateWorkout.value = false
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

    editingWorkout.value = null
    editWorkoutDefaultExercises.value = []
    editWorkoutName.value = ''
    showDefaultExercises.value = false
}

async function updateWorkout() {
    if (!editingWorkout.value) return
    if (!editWorkoutName.value.trim()) return

    editWorkoutProcessing.value = true
    try {
        const payload : UpdateWorkoutRequest = { 
            name: editWorkoutName.value.trim(), 
            defaultExercises: editWorkoutDefaultExercises.value
        }
        const res = await api.workouts.updateWorkout(editingWorkout.value.id, payload)
        if (res.isSuccess && res.data) {
            emit('updated', res.data)
            closeEditWorkoutModal(true)
        } else {
            console.error('Failed to update workout:', res.error)
        }
    } catch (e) {
        console.error('Failed to update workout:', e)
    } finally {
        editWorkoutProcessing.value = false
    }
}

async function deleteWorkout() {
    if (!editingWorkout.value) return
    if (!confirm('Delete this workout?')) return

    editWorkoutProcessing.value = true
    try {
        const res = await api.workouts.deleteWorkout(editingWorkout.value.id)
        if (res.isSuccess) {
            emit('deleted', editingWorkout.value.id)
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

async function createWorkout() {
    if (!newWorkoutName.value.trim()) return

    creating.value = true
    try {
        const res = await api.workouts.createWorkout({ name: newWorkoutName.value.trim() })
        if (res.data) {
            emit('created', res.data)
            newWorkoutName.value = ''
            showCreateWorkout.value = false
        }
    } catch (e) {
        console.error('Failed to create workout:', e)
    } finally {
        creating.value = false
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
</style>
