<template>
  <div class="add-candidates-overlay">
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
          <div class="add-candidates-footer">
              <button type="button" class="btn btn-secondary"
                  @click="cancelAdd">Cancel</button>
              <button type="button" class="btn btn-primary" @click="addExercise"
                  :disabled="!canAdd">Add</button>
          </div>
      </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { Exercise } from '../../api/modules/exercises'
import { ExerciseCollection } from '../../types/ExerciseCollection';
import MuscleGroupSelector from './MuscleGroupSelector.vue'

const props = defineProps<{ exerciseCollection: ExerciseCollection }>()
const emit = defineEmits<{
    (e: 'add', exerciseId: string): void
    (e: 'cancel'): void
}>()

// Add- UI state
const muscleSelection = ref({ categoryId: '', groupId: '' })
const selectedNewExerciseId = ref('')

const addCandidates = computed(() => {
    if (!muscleSelection.value.groupId) return [] as Exercise[]
    return props.exerciseCollection.exercises.filter((e) => e.muscleGroupId === muscleSelection.value.groupId)
})

watch(muscleSelection, () => {
    selectedNewExerciseId.value = ''
})

const canAdd = computed(() => {
    return !!selectedNewExerciseId.value
})

function selectCandidate(id: string) {
    selectedNewExerciseId.value = id
}

function cancelAdd() {
    selectedNewExerciseId.value = ''
    muscleSelection.value = { categoryId: '', groupId: '' }

    emit('cancel')
}

async function addExercise() {
    if (!selectedNewExerciseId.value) return

    emit('add', selectedNewExerciseId.value)

    selectedNewExerciseId.value = ''
    muscleSelection.value = { categoryId: '', groupId: '' }
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