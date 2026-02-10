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
                    <DefaultExercisesList :exercises="editWorkoutDefaultExercises" :muscle-groups="exerciseCollection.muscleGroups" />
                    <div v-if="editingWorkout" class="form-field">
                        <button type="button" class="btn btn-faded" v-if="!addDefaultVisible"
                            @click="addDefaultVisible = true">
                            + Add
                        </button>

                        <div v-if="addDefaultVisible" class="add-candidates-overlay"
                            @click.self="addDefaultVisible = false">
                            <div class="add-candidates-panel" @click.stop>
                                <div class="add-candidates-header">
                                    <strong>Select exercise</strong>
                                </div>
                                <div class="add-candidates-body">
                                    <MuscleGroupSelector v-model="addDefaultSelection" 
                                        :categories="exerciseCollection.muscleCategories"
                                        :groups="exerciseCollection.muscleGroups" />
                                    <div class="add-candidates">
                                        <ul class="item-list">
                                            <li v-for="ex in addCandidates" :key="ex.id"
                                                class="item-card item-card-clickable add-candidate"
                                                :class="{ selected: selectedNewDefaultExerciseId === ex.id }"
                                                @click="selectCandidate(ex.id)">
                                                <span class="item-name">{{ ex.name }}</span>
                                            </li>
                                        </ul>
                                    </div>
                                </div>
                                <div class="add-candidates-footer">
                                    <button type="button" class="btn btn-secondary"
                                        @click="cancelAddDefault">Cancel</button>
                                    <button type="button" class="btn btn-primary" @click="addDefaultExercise"
                                        :disabled="!canAddDefault">Add</button>
                                </div>
                            </div>
                        </div>
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
import { Dumbbell, Plus, ChevronDown } from 'lucide-vue-next'
import DefaultExercisesList from './DefaultExercisesList.vue'
import MuscleGroupSelector from './MuscleGroupSelector.vue'
import { api } from '../../api/api'
import type { DefaultExercise, UpdateWorkoutRequest, Workout } from '../../api/modules/workouts'
import type { Exercise } from '../../api/modules/exercises'
import { ExerciseCollection } from '../../types/ExerciseCollection'
import { emptyGuid } from '../../types/Guid'

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
    if (editingWorkout.value.defaultExercises.length !== editWorkoutDefaultExercises.value.length) return true
    // Check if exercise IDs or order have changed
    for (let i = 0; i < editingWorkout.value.defaultExercises.length; i++) {
        const original = editingWorkout.value.defaultExercises[i]
        const edited = editWorkoutDefaultExercises.value[i]
        if (original.exerciseId !== edited.exerciseId || original.exerciseNumber !== edited.exerciseNumber) {
            return true
        }
    }
    return false
})

// Add-default UI state
const addDefaultVisible = ref(false)
const addDefaultSelection = ref({ categoryId: '', groupId: '' })
const selectedNewDefaultExerciseId = ref('')

const addCandidates = computed(() => {
    if (!addDefaultSelection.value.groupId) return [] as Exercise[]
    return props.exerciseCollection.exercises.filter((e) => e.muscleGroupId === addDefaultSelection.value.groupId)
})

watch(addDefaultSelection, () => {
    selectedNewDefaultExerciseId.value = ''
})

const canAddDefault = computed(() => {
    return !!editingWorkout.value && !!selectedNewDefaultExerciseId.value
})

function selectCandidate(id: string) {
    selectedNewDefaultExerciseId.value = id
}

function cancelAddDefault() {
    addDefaultVisible.value = false
    selectedNewDefaultExerciseId.value = ''
    addDefaultSelection.value = { categoryId: '', groupId: '' }
}

async function addDefaultExercise() {
    if (!editWorkoutDefaultExercises.value || !selectedNewDefaultExerciseId.value) return
    editWorkoutDefaultExercises.value.push({
        id: emptyGuid, // will be set by backend
        exerciseId: selectedNewDefaultExerciseId.value,
        workoutId: editingWorkout.value!.id,
        exerciseNumber: editWorkoutDefaultExercises.value.length + 1,
        exercise: props.exerciseCollection.exercises.find(e => e.id === selectedNewDefaultExerciseId.value)!,
    })
    addDefaultVisible.value = false
    selectedNewDefaultExerciseId.value = ''
    addDefaultSelection.value = { categoryId: '', groupId: '' }
}

function openEditWorkout(w: Workout) {
    editingWorkout.value = w
    editWorkoutName.value = w.name
    editWorkoutDefaultExercises.value = [...w.defaultExercises]
    showCreateWorkout.value = false
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

.section-toggle:focus { outline: 2px solid rgba(0, 0, 0, 0.06); }
.section-title { font-weight: 600; color: var(--trk-text); }
.section-icon { width: 18px; height: 18px; color: var(--trk-text-muted); transition: transform 150ms ease; }
.section-icon.rotated { transform: rotate(180deg); }

/* Add-candidates list and overlay (component-specific) */
.add-candidates { width: 100%; }
.add-candidates .item-list { max-height: 160px; overflow-y: auto; padding: 0; }
.add-candidate { padding: 8px 10px; border-radius: 6px; }
.add-candidate.selected { background: var(--trk-accent); color: var(--trk-bg); border-color: var(--trk-accent); }

.add-candidates-overlay { position: absolute; right: var(--trk-space-4); top: calc(52px); z-index: 300; width: calc(100% - var(--trk-space-8)); max-width: 420px; display: flex; justify-content: center; }
.add-candidates-panel { width: 100%; background: var(--trk-surface); border: 1px solid var(--trk-surface-border); border-radius: var(--trk-radius-md); box-shadow: var(--trk-shadow); padding: var(--trk-space-3); }
.add-candidates-header { display: flex; justify-content: space-between; align-items: center; gap: var(--trk-space-2); margin-bottom: var(--trk-space-2); }
.add-candidates-body { display: flex; flex-direction: column; gap: var(--trk-space-2); }
.add-candidates-footer { display: flex; gap: var(--trk-space-2); margin-top: var(--trk-space-2); }

/* faded button variant to match list styling */
.btn-faded { display: inline-flex; align-items: center; justify-content: center; gap: var(--trk-space-2); border-radius: var(--trk-radius-md); border: 1px solid var(--trk-surface-border); background: transparent; color: var(--trk-text); padding: 0.65rem 0.75rem; font-weight: 800; cursor: pointer; transition: background 150ms ease, color 150ms ease, border-color 150ms ease, box-shadow 120ms ease; }
.btn-faded:hover { background: var(--trk-accent); color: var(--trk-text-dark); border-color: var(--trk-accent); box-shadow: 0 6px 18px rgba(0, 0, 0, 0.35); }
.btn-faded:active { transform: translateY(1px) scale(0.995); }
.btn-faded:disabled { opacity: 0.6; cursor: not-allowed; }

@media (max-width: 520px) {
    .add-candidates-overlay { position: fixed; inset: 0; padding: calc(var(--trk-space-4) + env(safe-area-inset-top, 0)) var(--trk-space-4) var(--trk-space-4); background: rgba(0, 0, 0, 0.35); display: flex; align-items: flex-end; justify-content: center; }
    .add-candidates-panel { max-width: 100%; border-radius: var(--trk-radius-lg) var(--trk-radius-lg) 0 0; padding-bottom: calc(var(--trk-space-6) + env(safe-area-inset-bottom, 0)); }
    .add-candidates .item-list { max-height: 220px; }
}

</style>
