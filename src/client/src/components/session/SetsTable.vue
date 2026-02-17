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

        <div class="pill-scroller">
          <button
            v-for="group in availableMuscleGroups"
            :key="group"
            @click="selectedMuscleGroup = selectedMuscleGroup === group ? null : group"
            :class="['filter-pill', { active: selectedMuscleGroup === group }]"
          >
            {{ group }}
          </button>
        </div>
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
          <th>Date</th>
          <th>Workout</th>
          <th>Muscle Group</th>
          <th>Category</th>
          <th>Exercise</th>
          <th>Sets</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(row, ridx) in table.getRowModel().rows" :key="ridx">
          <td>{{ row.original.date.toLocaleDateString() }}</td>
          <td>{{ row.original.workoutName }}</td>
          <td>{{ row.original.muscleGroupName }}</td>
          <td>{{ row.original.muscleCategoryName }}</td>
          <td>{{ row.original.exerciseName }}</td>
          <td>
            <div class="sets-row">
              <div v-for="(s, i) in row.original.sets" :key="i" class="set-pill">
                <span class="set-count">{{ i + 1 }}</span>
                <div class="set-data">
                  <span class="val">{{ s.weight }}</span>
                  <span class="sep">×</span>
                  <span class="val">{{ s.reps }}</span>
                </div>
              </div>
            </div>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import type { FlatExerciseRow } from '../../types/Session'

// TanStack Table imports
import { createColumnHelper, getCoreRowModel } from '@tanstack/table-core'
import { useVueTable } from '@tanstack/vue-table'

const props = defineProps<{
  history: FlatExerciseRow[]
}>()

const searchQuery = ref('')
const selectedMuscleGroup = ref<string | null>(null)

const availableMuscleGroups = computed(() => {
  const groups = new Set(props.history.map(x => x.muscleGroupName))
  return Array.from(groups).sort()
})

const filteredHistory = computed(() => {
  return props.history.filter(item => {
    const matchesSearch = !searchQuery.value ||
      item.exerciseName.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      item.workoutName.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchesGroup = !selectedMuscleGroup.value || item.muscleGroupName === selectedMuscleGroup.value
    return matchesSearch && matchesGroup
  })
})

// Define minimal columns (we'll render rows using row.original for simplicity)
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
/* Reuse existing list/table styles */
.trkr-table { width: 100%; border-collapse: collapse; background: var(--trk-surface); border-radius: var(--trk-radius-md); overflow: hidden; }
.trkr-table thead th { text-align: left; padding: 0.75rem 1rem; border-bottom: 1px solid var(--trk-surface-border); font-weight: 700; font-size: 0.85rem; }
.trkr-table tbody td { padding: 0.75rem 1rem; border-bottom: 1px dashed var(--trk-surface-border); vertical-align: top; }

.history-header { padding-bottom: var(--trk-space-4); }
.filter-section { display: flex; flex-direction: column; gap: var(--trk-space-2); }
.search-container { margin-bottom: 0; }
.pill-scroller { display: flex; gap: var(--trk-space-2); overflow-x: auto; padding: var(--trk-space-2) 0; }
.filter-pill { white-space: nowrap; padding: 0.35rem 0.65rem; border-radius: 999px; background: var(--trk-surface-inner); border: 1px solid var(--trk-surface-border); color: var(--trk-text-muted); font-size: 0.85rem; font-weight: 600; cursor: pointer; }
.filter-pill.active { background: var(--trk-accent-ring); border-color: var(--trk-accent); color: var(--trk-accent); }

/* reuse Set pill styles from history view */
.sets-row { display: flex; flex-wrap: wrap; gap: 0.5rem; }
.set-pill { display: flex; align-items: center; background: var(--trk-surface-inner); border: 1px solid var(--trk-surface-border); border-radius: var(--trk-radius-md); overflow: hidden; }
.set-count { background: var(--trk-surface-border); color: var(--trk-text-muted); font-size: 0.65rem; font-weight: 800; padding: 0.35rem 0.4rem; min-width: 20px; text-align: center; }
.set-data { padding: 0.25rem 0.5rem; display: flex; align-items: baseline; gap: 4px; }
.val { font-weight: 700; font-size: 0.95rem; color: var(--trk-text); }
.sep { color: var(--trk-accent-muted); font-size: 0.8rem; margin: 0 4px; }
</style>