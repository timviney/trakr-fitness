<template>
  <ol class="item-list default-exercises-list">
    <li v-for="(ex, idx) in sortedExercises" :key="ex.id" class="item-card">
      <span class="item-name">{{ ex.name }}</span>
      <span v-if="muscleName(ex.muscleGroupId)" class="item-count">{{ muscleName(ex.muscleGroupId) }}</span>
    </li>
  </ol>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { MuscleGroup } from '../../api/modules/muscles'
import type { DefaultExercise } from '../../api/modules/workouts'
import { PropType } from 'vue'

const props = defineProps({
  exercises: { type: Array as PropType<DefaultExercise[]>, required: true },
  muscleGroups: { type: Array as PropType<MuscleGroup[]>, required: false, default: () => [] },
})

// normalize to objects with { id, name, muscleGroupId, exerciseNumber }
const sortedExercises = computed(() => {
  const normalized = props.exercises.map((d) => ({
    id: d.exercise.id,
    name: d.exercise.name,
    muscleGroupId: d.exercise.muscleGroupId,
    exerciseNumber: d.exerciseNumber,
  }))
  return normalized.sort((a, b) => ((a.exerciseNumber ?? 0) - (b.exerciseNumber ?? 0)))
})

function muscleName(id: string | undefined) {
  if (!id) return ''
  const g = props.muscleGroups.find((m) => m.id === id)
  return g ? g.name : ''
}

</script>

<style scoped>
.default-exercises-list .item-card {
  padding: 8px 12px;
  display: flex;
  gap: 8px;
  align-items: center;
}
.default-exercises-list .item-count {
  font-size: 0.85rem;
  color: var(--trk-text-muted);
  margin-left: auto;
}
</style>