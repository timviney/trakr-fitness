<template>
  <AppShell>
    <div class="stats-view">
      <header class="view-header">
        <h1 class="view-title">Your Stats</h1>
      </header>

      <div class="stats-landing">
        <Loader :loading="historyLoading" />

        <div v-if="!historyLoading">
          <div class="charts-placeholder single-chart">
            <div class="stat-card" style="flex:1; display:flex; flex-direction:column; gap:var(--trk-space-3);">
              <div class="stats-controls">
                <div class="exercise-select">
                  <button class="btn btn-secondary" @click="showExerciseSelector = !showExerciseSelector" :aria-pressed="showExerciseSelector" aria-haspopup="dialog">
                    {{ selectedExerciseName }}
                  </button>
                </div>

                <SegmentToggle v-model="selectedMetric" :optionsLabels="metricOptions" />
              </div>

              <div class="chart-container full-bleed">
                <ExerciseStatsChart :seriesData="seriesData" :metricLabel="metricLabel" :loading="historyLoading" />
              </div>
            </div>

            <ExerciseSelector
              v-if="showExerciseSelector && exerciseCollection.exercises"
              v-model="exerciseSelection"
              :exerciseCollection="exerciseCollection"
              :allow-subcategory-selection="false"
              :keep-history="true"
              @cancel="showExerciseSelector = false"
              @add="onExerciseAdd"
            />
          </div>
        </div>
      </div>
    </div>
  </AppShell>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import AppShell from '../components/general/AppShell.vue'
import Loader from '../components/general/Loader.vue'
import SegmentToggle from '../components/general/SegmentToggle.vue'
import ExerciseSelector from '../components/exercises/ExerciseSelector.vue'
import ExerciseStatsChart from '../components/stats/ExerciseStatsChart.vue'
import { api } from '../api/api'
import { useStatsStore } from '../stores/stats'
import { computeExerciseTimeSeries } from '../stores/statsHelpers'
import { storeToRefs } from 'pinia'
import type { ExerciseCollection } from '../types/ExerciseCollection'

const statsStore = useStatsStore()
const { fetchSessionHistory } = statsStore
const { sessionHistory, historyLoading } = storeToRefs(statsStore)

const exerciseCollection = ref<ExerciseCollection>({ workouts: [], exercises: [], muscleCategories: [], muscleGroups: [] })
const exerciseSelection = ref<{ categoryId?: string; groupId?: string; exerciseId?: string } | null>(null)
const showExerciseSelector = ref(false)

const selectedMetric = ref<'maxWeight' | 'maxReps' | 'totalReps' | 'totalVolume'>('maxWeight')
const metricOptions = { maxWeight: 'Max Weight', maxReps: 'Max Reps', totalReps: 'Total Reps', totalVolume: 'Total Volume' }

const selectedExerciseId = computed(() => exerciseSelection.value?.exerciseId ?? null)
const selectedExerciseName = computed(() => {
  if (!selectedExerciseId.value) return 'Select exercise'
  const ex = exerciseCollection.value.exercises.find(e => e.id === selectedExerciseId.value)
  return ex?.name ?? 'Selected exercise'
})

const metricLabel = computed(() => {
  switch (selectedMetric.value) {
    case 'maxWeight': return 'Weight'
    case 'maxReps': return 'Reps'
    case 'totalReps': return 'Reps'
    case 'totalVolume': return 'Volume'
  }
})

const seriesData = computed(() => {
  if (!selectedExerciseId.value) return [] as Array<[number, number]>
  return computeExerciseTimeSeries(sessionHistory.value, selectedExerciseId.value, selectedMetric.value, true)
})

function onExerciseAdd(id: string) { showExerciseSelector.value = false }

async function loadExerciseCollection() {
  try { exerciseCollection.value = await api.getExerciseCollection() } catch (err) { console.error('Failed to load exercise collection', err) }
}

onMounted(() => {
  fetchSessionHistory()
  loadExerciseCollection()
})
</script>

<style scoped>
.stats-view {
  display: flex;
  flex-direction: column;
  gap: var(--trk-space-6);
}

.view-header {
  text-align: center;
}

.view-title {
  font-family: var(--trk-font-heading);
  font-size: clamp(1.75rem, 1.5rem + 1.25vw, 2.25rem);
  color: var(--trk-text);
  margin: 0;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding: var(--trk-space-8) var(--trk-space-4);
  margin-top: var(--trk-space-8);
}

.empty-icon {
  width: 64px;
  height: 64px;
  color: var(--trk-text-muted);
  margin-bottom: var(--trk-space-4);
  opacity: 0.5;
}

.empty-title {
  font-family: var(--trk-font-heading);
  font-size: 1.5rem;
  color: var(--trk-text);
  margin: 0 0 var(--trk-space-2);
}

.empty-description {
  color: var(--trk-text-muted);
  font-size: 0.9375rem;
  line-height: 1.6;
  max-width: 280px;
  margin: 0 0 var(--trk-space-6);
}

.link {
  color: var(--trk-accent);
  text-decoration: none;
}

.link:hover {
  text-decoration: underline;
}

.muted { color: var(--trk-text-muted); margin-bottom: var(--trk-space-4); }
.charts-placeholder { display: flex; flex-direction: column; gap: var(--trk-space-4); width: 100%; }
.charts-placeholder.single-chart { padding: 0; }
.stat-card { padding: var(--trk-space-3); background: var(--trk-surface); border-radius: var(--trk-radius-md); border: 1px solid var(--trk-surface-border); width: 100%; box-sizing: border-box; }
.stats-landing { padding: var(--trk-space-3); }

/* controls stacked on mobile, row on wider screens */
.stats-controls { display: flex; flex-direction: column; align-items: stretch; gap: var(--trk-space-3); }
@media (min-width: 720px) {
  .stats-controls { flex-direction: row; align-items: center; justify-content: space-between; }
}

.exercise-select { display: flex; gap: var(--trk-space-2); align-items: center; }
.exercise-select .btn { min-width: 140px; }
.chart-container { width: 100%; }

</style>
