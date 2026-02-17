<template>
  <div :class="['add-candidates-overlay']">
      <div class="add-candidates-panel" @click.stop>
          <div class="add-candidates-header">
              <strong>Select exercise</strong>
          </div>
          <div class="add-candidates-body">
              <MuscleGroupSelector v-model="muscleSelection" 
                  :categories="exerciseCollection.muscleCategories"
                  :groups="exerciseCollection.muscleGroups" />
              <div class="add-candidates">
                  <ul class="item-list">
                      <li v-for="ex in addCandidates" :key="ex.id"
                          class="item-card item-card-clickable add-candidate"
                          :class="{ selected: selectedNewExerciseId === ex.id }"
                          @click="selectCandidate(ex.id)">
                          <span class="item-name">{{ ex.name }}</span>
                      </li>
                  </ul>
              </div>
          </div>
          <div v-if="!props.allowSubcategorySelection" class="add-candidates-footer">
              <button type="button" class="btn btn-secondary"
                  @click="cancelAdd">Cancel</button>
              <button type="button" class="btn btn-primary" @click="addExercise"
                  :disabled="!canAdd">Select</button>
          </div>
          <div v-else class="add-candidates-footer">
              <button type="button" class="btn btn-secondary"
                  @click="cancelAdd">Ok</button>
          </div>
      </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { Exercise } from '../../api/modules/exercises'
import { ExerciseCollection } from '../../types/ExerciseCollection';
import MuscleGroupSelector from './MuscleGroupSelector.vue'

const props = defineProps<{
    exerciseCollection: ExerciseCollection; 
    modelValue?: { categoryId?: string; groupId?: string; exerciseId?: string } | null;
    allowSubcategorySelection?: boolean;
    keepHistory?: boolean;
}>()
const emit = defineEmits<{
    (e: 'add', exerciseId: string): void
    (e: 'cancel'): void
    (e: 'update:modelValue', value: { categoryId?: string; groupId?: string; exerciseId?: string } | null): void
}>()

// Add- UI state
const muscleSelection = ref({ categoryId: props.modelValue?.categoryId ?? '', groupId: props.modelValue?.groupId ?? '' })
const selectedNewExerciseId = ref(props.modelValue?.exerciseId ?? '')

const addCandidates = computed(() => {
    if (!muscleSelection.value.groupId) return [] as Exercise[]
    return props.exerciseCollection.exercises.filter((e) => e.muscleGroupId === muscleSelection.value.groupId)
})

// keep internal state in sync with external v-model and emit changes when used as a filter (modelValue bound)
watch(() => props.modelValue, (v) => {
    // prefer explicit category/group from modelValue; if missing, derive from exerciseId using the collection
    let newCategory = v?.categoryId ?? ''
    let newGroup = v?.groupId ?? ''

    const newMuscle = { categoryId: newCategory, groupId: newGroup }
    // avoid recreating the same object (prevents retriggering the other watcher)
    if (newMuscle.categoryId !== muscleSelection.value.categoryId || newMuscle.groupId !== muscleSelection.value.groupId) {
        muscleSelection.value = newMuscle
    }

    const newSelectedId = v?.exerciseId ?? ''
    if (newSelectedId !== selectedNewExerciseId.value) {
        selectedNewExerciseId.value = newSelectedId
    }
})

watch(muscleSelection, (val) => {
    // if parent already has the same values, do nothing (prevents echo loop)
    const parentModel = props.modelValue ?? null
    const parentCategory = parentModel?.categoryId ?? ''
    const parentGroup = parentModel?.groupId ?? ''
    if (val.categoryId === parentCategory && val.groupId === parentGroup) return

    selectedNewExerciseId.value = ''
    if (props.modelValue !== undefined) {
        // if nothing selected, emit null so parent clears filter
        if (!val.categoryId && !val.groupId) {
            emit('update:modelValue', null)
        } else {
            emit('update:modelValue', { categoryId: val.categoryId || undefined, groupId: val.groupId || undefined, exerciseId: undefined })
        }
    }
})

const canAdd = computed(() => {
    return props.allowSubcategorySelection || !!selectedNewExerciseId.value
})

function selectCandidate(id: string) {
    selectedNewExerciseId.value = id
    if (props.modelValue !== undefined) {
        emit('update:modelValue', { categoryId: muscleSelection.value.categoryId || undefined, groupId: muscleSelection.value.groupId || undefined, exerciseId: id })
    }
} 

function cancelAdd() {
    if (!props.keepHistory) {
        selectedNewExerciseId.value = ''
        muscleSelection.value = { categoryId: '', groupId: '' }
    }

    emit('cancel')
}

async function addExercise() {
    if (!selectedNewExerciseId.value) return

    emit('add', selectedNewExerciseId.value)

    if (!props.keepHistory) {
        selectedNewExerciseId.value = ''
        muscleSelection.value = { categoryId: '', groupId: '' }
    }
}

</script>

<style scoped>
.add-candidates { width: 100%; }
.add-candidates .item-list { max-height: 160px; overflow-y: auto; padding: 0; }
.add-candidate { padding: 8px 10px; border-radius: 6px; }
.add-candidate.selected { background: var(--trk-accent); color: var(--trk-bg); border-color: var(--trk-accent); }

.add-candidates-overlay { position: absolute; right: var(--trk-space-4); top: calc(52px); z-index: 300; width: calc(100% - var(--trk-space-8)); max-width: 420px; display: flex; justify-content: center; }
.add-candidates-panel { width: 100%; background: var(--trk-surface); border: 1px solid var(--trk-surface-border); border-radius: var(--trk-radius-md); box-shadow: var(--trk-shadow); padding: var(--trk-space-3); }
.add-candidates-header { display: flex; justify-content: space-between; align-items: center; gap: var(--trk-space-2); margin-bottom: var(--trk-space-2); }
.add-candidates-body { display: flex; flex-direction: column; gap: var(--trk-space-2); }
.add-candidates-footer { display: flex; gap: var(--trk-space-2); margin-top: var(--trk-space-2); }

@media (max-width: 520px) {
    .add-candidates-overlay { position: fixed; inset: 0; padding: calc(var(--trk-space-4) + env(safe-area-inset-top, 0)) var(--trk-space-4) var(--trk-space-4); background: rgba(0, 0, 0, 0.35); display: flex; align-items: flex-end; justify-content: center; }
    .add-candidates-panel { max-width: 100%; border-radius: var(--trk-radius-lg) var(--trk-radius-lg) 0 0; padding-bottom: calc(var(--trk-space-6) + env(safe-area-inset-bottom, 0)); }
    .add-candidates .item-list { max-height: 220px; }
}
</style>