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
        <ExerciseTab v-if="!loading && !error && activeTab === 'exercises'" 
            :exercise-collection="exerciseCollection"
            @created="exercise => exerciseCollection.exercises.push(exercise)"
            @updated="updatedExercise => {
            const idx = exerciseCollection.exercises.findIndex(e => e.id === updatedExercise.id)
            if (idx >= 0) exerciseCollection.exercises[idx] = updatedExercise
            }"
            @deleted="exerciseId => exerciseCollection.exercises = exerciseCollection.exercises.filter(e => e.id !== exerciseId)"
        />
    </div>
  </AppShell>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import AppShell from '../components/general/AppShell.vue'
import { api } from '../api/api'
import WorkoutTab from '../components/exercises/WorkoutTab.vue'
import ExerciseTab from '../components/exercises/ExerciseTab.vue'
import SegmentToggle from '../components/general/SegmentToggle.vue'
import Loader from '../components/general/Loader.vue'
import Error from '../components/general/Error.vue'
import { ExerciseCollection } from '../types/ExerciseCollection'

const activeTab = ref<'workouts' | 'exercises'>('exercises')
const loading = ref(true)
const error = ref<string | null>(null)

const exerciseCollection = ref<ExerciseCollection>({workouts: [], exercises: [], muscleCategories: [], muscleGroups: []})

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

onMounted(loadData)
</script>

<style scoped>
/* Basic layout */
.exercises-view { display: flex; flex-direction: column; gap: var(--trk-space-4); }
.view-header { text-align: center; }
.view-title { font-family: var(--trk-font-heading); font-size: clamp(1.5rem, 1.25rem + 1.25vw, 2rem); color: var(--trk-text); margin: 0; }
</style>
