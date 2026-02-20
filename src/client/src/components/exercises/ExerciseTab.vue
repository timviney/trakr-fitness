<template>
    <!-- Exercises Tab -->
        <!-- Breadcrumb Navigation -->
        <nav v-if="selectedCategory || selectedGroup" class="breadcrumb">
            <button class="breadcrumb-link" @click="navigateToCategories">
                Categories
            </button>
            <template v-if="selectedCategory">
                <ChevronRight class="breadcrumb-sep" />
                <button class="breadcrumb-link" :class="{ current: !selectedGroup }"
                    @click="navigateToCategory(selectedCategory)">
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
                <li v-for="category in exerciseCollection.muscleCategories" :key="category.id"
                    class="item-card item-card-clickable" @click="navigateToCategory(category)">
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
                <li v-for="group in currentMuscleGroups" :key="group.id" class="item-card item-card-clickable"
                    @click="navigateToGroup(group)">
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
                <li v-for="exercise in currentExercises" :key="exercise.id" class="item-card item-card-clickable"
                    @click="openEditExercise(exercise)">
                    <span class="item-name">{{ exercise.name }}</span>
                    <span v-if="!exercise.userId" class="item-badge">Default</span>
                </li>
            </ul>

            <button v-if="currentExercises.length > 0" class="fab" @click="showCreateExercise = true"
                aria-label="Add exercise">
                <Plus />
            </button>
        </template>

    <!-- Create Exercise Modal -->
    <div v-if="showCreateExercise" class="modal-overlay" @click.self="closeExerciseModal">
        <div class="modal">
            <h2 class="modal-title">New {{ selectedGroup?.name }} Exercise</h2>
            <form @submit.prevent="createExercise">
                <div class="form-field">
                    <label for="exercise-name">Name</label>
                    <input id="exercise-name" v-model="newExerciseName" type="text" placeholder="e.g., Bench Press"
                        required />
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
                    <input id="edit-exercise-name" v-model="editName" type="text" placeholder="e.g., Bench Press"
                        required :disabled="editProcessing || isEditingDefault" />
                </div>

                <div class="form-field">
                    <label>Muscle Group</label>
                    <MuscleGroupSelector v-model="editSelection" :categories="exerciseCollection.muscleCategories"
                        :groups="exerciseCollection.muscleGroups" :disabled="editProcessing || isEditingDefault" />
                </div>

                <div class="modal-actions">
                    <button type="button" class="btn btn-secondary" @click="closeEditModal"
                        :disabled="editProcessing">Cancel</button>
                    <button type="button" class="btn btn-danger" @click="deleteExercise" v-if="!isEditingDefault"
                        :disabled="editProcessing">Delete</button>
                    <button type="submit" class="btn btn-primary" v-if="!isEditingDefault"
                        :disabled="editProcessing">Save</button>
                </div>
            </form>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { Dumbbell, Plus, ChevronRight } from 'lucide-vue-next'
import MuscleGroupSelector from './MuscleGroupSelector.vue'
import { api } from '../../api/api'
import type { Exercise } from '../../api/modules/exercises'
import type { MuscleCategory, MuscleGroup } from '../../api/modules/muscles'
import { ExerciseCollection } from '../../types/ExerciseCollection'

const props = defineProps<{ exerciseCollection: ExerciseCollection }>()
const emit = defineEmits<{
    (e: 'created', exercise: Exercise): void
    (e: 'updated', exercise: Exercise): void
    (e: 'deleted', exerciseId: string): void
}>()

const creating = ref(false)

const showCreateExercise = ref(false)
const newExerciseName = ref('')

// Editing state (moved early so overlayOpen can reference it safely)
const editingExercise = ref<Exercise | null>(null)
const editName = ref('')
const editCategoryId = ref('')
const editGroupId = ref('')
const editProcessing = ref(false)

const editSelection = ref({ categoryId: '', groupId: '' })

// disable background scroll when either create or edit modal is open
const overlayOpen = computed(() => showCreateExercise.value || !!editingExercise.value)
watch(overlayOpen, (open) => {
    if (open) document.body.classList.add('no-scroll')
    else document.body.classList.remove('no-scroll')
})

// Drill-down navigation state
const selectedCategory = ref<MuscleCategory | null>(null)
const selectedGroup = ref<MuscleGroup | null>(null)

const currentMuscleGroups = computed(() => {
    if (!selectedCategory.value) return []
    return props.exerciseCollection.muscleGroups.filter((g) => g.categoryId === selectedCategory.value!.id)
})

const currentExercises = computed(() => {
    if (!selectedGroup.value) return []
    return props.exerciseCollection.exercises.filter((e) => e.muscleGroupId === selectedGroup.value!.id)
})

function getExerciseCount(groupId: string): number {
    return props.exerciseCollection.exercises.filter((e) => e.muscleGroupId === groupId).length
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
    const group = props.exerciseCollection.muscleGroups.find((g) => g.id === ex.muscleGroupId)
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
            emit('updated', res.data)
            // if moved group, and current selectedGroup doesn't match, navigate to the new group
            if (selectedGroup.value && selectedGroup.value.id !== res.data!.muscleGroupId) {
                const newGroup = props.exerciseCollection.muscleGroups.find((g) => g.id === res.data!.muscleGroupId)
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
            emit('deleted', editingExercise.value.id)
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

async function createExercise() {
    if (!newExerciseName.value.trim() || !selectedGroup.value) return

    creating.value = true
    try {
        const res = await api.exercises.createExercise({
            name: newExerciseName.value.trim(),
            muscleGroupId: selectedGroup.value.id,
        })
        if (res.data) {
            emit('created', res.data)
            closeExerciseModal()
        }
    } catch (e) {
        console.error('Failed to create exercise:', e)
    } finally {
        creating.value = false
    }
}

</script>

<style scoped>
/* Breadcrumb (view-specific) */
.breadcrumb { display: flex; align-items: center; gap: var(--trk-space-1); padding: var(--trk-space-2) 0; flex-wrap: wrap; }
.breadcrumb-link { background: none; border: none; padding: var(--trk-space-1) var(--trk-space-2); color: var(--trk-accent); font-size: 0.875rem; font-weight: 500; cursor: pointer; border-radius: var(--trk-radius-sm); transition: background 150ms ease; }
.breadcrumb-link:hover { background: var(--trk-surface); }
.breadcrumb-link.current { color: var(--trk-text); cursor: default; }
.breadcrumb-link.current:hover { background: transparent; }
.breadcrumb-sep { width: 16px; height: 16px; color: var(--trk-text-muted); flex-shrink: 0; }
.breadcrumb-current { color: var(--trk-text); font-size: 0.875rem; font-weight: 500; padding: var(--trk-space-1) var(--trk-space-2); }

</style>
