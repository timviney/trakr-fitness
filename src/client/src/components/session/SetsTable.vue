<template>
  <div class="history-page history-table">
    <header class="history-header">
      <div class="filter-section">
        <div class="form-field search-container">
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Search exercises..."
            aria-label="Search"
          />
        </div>

        <div class="filter-actions">
          <button type="button" class="btn" @click="showFilterOverlay = !showFilterOverlay" :aria-pressed="showFilterOverlay" aria-label="Open filters">
            <FunnelPlus class="btn-icon" />
          </button>
        </div>

        <ExerciseSelector
          v-if="showFilterOverlay && exerciseCollection"
          v-model="exerciseSelection"
          :exerciseCollection="exerciseCollection"
          :allow-subcategory-selection="true"
          :keep-history="true"
          @cancel="showFilterOverlay = false"
        />
      </div>
    </header>

    <div v-if="filteredHistory.length === 0" class="empty-state">
      <div class="empty-icon">📂</div>
      <h2 class="empty-title">No rows</h2>
      <p class="empty-description">Try adjusting filters.</p>
    </div>

    <table v-else class="trkr-table">
      <thead>
        <tr>
          <th>Exercise</th>
          <th>Sets</th>
          <th>Muscle Group</th>
          <th>Date</th>
          <th>Workout</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(row, ridx) in table.getRowModel().rows" :key="ridx">
          <td>{{ row.original.exerciseName }}</td>
          <td>
            <div class="sets-row">
              <div v-for="(s, i) in row.original.sets" :key="i" class="set-pill">
                <div class="set-data">
                  <span class="val">{{ s.weight }}</span>
                  <span class="sep">×</span>
                  <span class="val">{{ s.reps }}</span>
                </div>
              </div>
            </div>
          </td>
          <td>{{ row.original.muscleGroupName }}</td>
          <td>{{ row.original.date.toLocaleDateString() }}</td>
          <td>{{ row.original.workoutName }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import type { FlatExerciseRow } from '../../types/Session'
import ExerciseSelector from '../exercises/ExerciseSelector.vue'
import { FunnelPlus } from 'lucide-vue-next'
import type { ExerciseCollection } from '../../types/ExerciseCollection' 

// TanStack Table imports
import { createColumnHelper, getCoreRowModel } from '@tanstack/table-core'
import { useVueTable } from '@tanstack/vue-table'

const props = defineProps<{
  history: FlatExerciseRow[]
  exerciseCollection: ExerciseCollection
}>()

const searchQuery = ref('')
const selectedMuscleGroup = ref<string | null>(null)
const exerciseSelection = ref<{ categoryId?: string; groupId?: string; exerciseId?: string } | null>(null)
const showFilterOverlay = ref(false)

watch(exerciseSelection, (val) => {
  if (val?.groupId && props.exerciseCollection) {
    const g = props.exerciseCollection.muscleGroups.find(x => x.id === val.groupId)
    selectedMuscleGroup.value = g ? g.name : null
  } else {
    selectedMuscleGroup.value = null
  }
})

const filteredHistory = computed(() => {
  const q = searchQuery.value.trim().toLowerCase()
  return props.history.filter(item => {
    const matchesSearch = !q ||
      item.exerciseName.toLowerCase().includes(q) ||
      item.workoutName.toLowerCase().includes(q)

    let matchesFilter = true

    if (exerciseSelection.value?.exerciseId) {
      const ex = props.exerciseCollection?.exercises.find(e => e.id === exerciseSelection.value!.exerciseId)
      matchesFilter = ex ? item.exerciseName === ex.name : false
    } else if (exerciseSelection.value?.groupId) {
      const group = props.exerciseCollection?.muscleGroups.find(g => g.id === exerciseSelection.value!.groupId)
      matchesFilter = group ? item.muscleGroupName === group.name : false
    } else if (exerciseSelection.value?.categoryId) {
      const cat = props.exerciseCollection?.muscleCategories.find(c => c.id === exerciseSelection.value!.categoryId)
      matchesFilter = cat ? item.muscleCategoryName === cat.name : false
    } else {
      matchesFilter = !selectedMuscleGroup.value || item.muscleGroupName === selectedMuscleGroup.value
    }

    return matchesSearch && matchesFilter
  })
})

// Define minimal columns 
const columnHelper = createColumnHelper<FlatExerciseRow>()
const columns = [
  columnHelper.accessor('date', { header: 'Date' }),
  columnHelper.accessor('workoutName', { header: 'Workout' }),
  columnHelper.accessor('muscleGroupName', { header: 'Muscle Group' }),
  columnHelper.accessor('muscleCategoryName', { header: 'Category' }),
  columnHelper.accessor('exerciseName', { header: 'Exercise' }),
  columnHelper.accessor('sets', { header: 'Sets' })
]

const table = useVueTable({
  data: filteredHistory,
  columns,
  getCoreRowModel: getCoreRowModel()
})
</script>

<style scoped>
.trkr-table { width: 100%; border-collapse: collapse; background: var(--trk-surface); border-radius: var(--trk-radius-md); overflow: hidden; }
.trkr-table thead th { text-align: left; padding: var(--trk-space-2) var(--trk-space-3); border-bottom: 1px solid var(--trk-surface-border); font-weight: 700; font-size: var(--trk-font-sm); }
.trkr-table tbody td { padding: calc(var(--trk-space-2) + 0.1rem) var(--trk-space-3); border-bottom: 1px dashed var(--trk-surface-border); vertical-align: top; }

.history-header { padding-bottom: var(--trk-space-4); }
.filter-section { display: flex; flex-direction: column; gap: var(--trk-space-2); }
.search-container { margin-bottom: 0; } 
.filter-actions { display: flex; gap: var(--trk-space-2); }
.filter-actions .btn { background: var(--trk-accent); padding: 0.3rem 0.45rem; }
.btn-icon {  color: var(--trk-text-dark);  width: 18px; height: 18px; }
.pill-scroller { display: flex; gap: var(--trk-space-2); overflow-x: auto; padding: var(--trk-space-2) 0; }
.filter-pill { white-space: nowrap; padding: 0.35rem 0.65rem; border-radius: 999px; background: var(--trk-surface-inner); border: 1px solid var(--trk-surface-border); color: var(--trk-text-muted); font-size: var(--trk-font-sm); font-weight: 600; cursor: pointer; }
.filter-pill.active { background: var(--trk-accent-ring); border-color: var(--trk-accent); color: var(--trk-accent); }

.sets-row { display: flex; flex-wrap: wrap; gap: 0.5rem; }
.set-pill { display: flex; align-items: center; background: var(--trk-surface-inner); border: 1px solid var(--trk-surface-border); border-radius: var(--trk-radius-md); overflow: hidden; }
.set-count { background: var(--trk-surface-border); color: var(--trk-text-muted); font-size: var(--trk-font-xxs); font-weight: 800; padding: 0.35rem 0.4rem; min-width: 20px; text-align: center; }
.set-data { padding: 0.25rem 0.5rem; display: flex; align-items: baseline; gap: 4px; }
.val { font-weight: 700; font-size: var(--trk-font-compact); color: var(--trk-text); }
.sep { color: var(--trk-accent-muted); font-size: var(--trk-font-xs); margin: 0 4px; }
</style>