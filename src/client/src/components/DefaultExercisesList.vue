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
import type { Exercise } from '../api/modules/exercises'
import type { MuscleGroup } from '../api/modules/muscles'
import { PropType } from 'vue'

type MaybeNumbered = Exercise & { exerciseNumber?: number }

const props = defineProps({
  exercises: { type: Array as PropType<MaybeNumbered[]>, required: true },
  muscleGroups: { type: Array as PropType<MuscleGroup[]>, required: false, default: () => [] },
})

const sortedExercises = computed(() => {
  return [...props.exercises].sort((a, b) => ( (a.exerciseNumber ?? 0) - (b.exerciseNumber ?? 0) ))
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

<style scoped>
.default-exercises-list .item-card {
  padding: 8px 12px;
  display: flex;
  gap: 8px;
  align-items: center;
}
.default-exercises-list .item-order {
  font-weight: 600;
  color: var(--trk-text-muted);
  width: 28px;
  text-align: right;
}
.default-exercises-list .item-count {
  font-size: 0.85rem;
  color: var(--trk-text-muted);
  margin-left: auto;
}
</style>
