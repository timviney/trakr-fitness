<template>
  <Draggable
    :list="localExercises"
    item-key="id"
    handle=".drag-handle"
    ghost-class="drag-ghost"
    chosen-class="drag-chosen"
    :animation="150"
    :disabled="disabled"
    @end="onDragEnd"
  >
    <template #item="{ element }">
      <li class="item-card default-exercise-item" :data-id="element.id">
        <span class="drag-handle" role="button" aria-label="Reorder exercise" tabindex="0">⋮⋮</span>
        <span class="item-name">{{ element.exercise.name }}</span>
        <span v-if="muscleName(element.exercise.muscleGroupId)" class="item-count">{{ muscleName(element.exercise.muscleGroupId) }}</span>
      </li>
    </template>
  </Draggable>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import Draggable from 'vuedraggable'
import type { MuscleGroup } from '../../api/modules/muscles'
import type { DefaultExercise } from '../../api/modules/workouts'
import { PropType } from 'vue'

const props = defineProps({
  exercises: { type: Array as PropType<DefaultExercise[]>, required: true },
  muscleGroups: { type: Array as PropType<MuscleGroup[]>, required: false, default: () => [] },
  disabled: { type: Boolean as PropType<boolean>, required: false, default: false },
})

const emit = defineEmits<{
  (e: 'reorder', exercises: DefaultExercise[]): void
}>()

const localExercises = ref<DefaultExercise[]>([])

watch(
  () => props.exercises,
  (val) => {
    localExercises.value = [...val].sort((a, b) => (a.exerciseNumber ?? 0) - (b.exerciseNumber ?? 0))
  },
  { immediate: true }
)

function onDragEnd() {
  const reordered = localExercises.value.map((ex, i) => ({ ...ex, exerciseNumber: i + 1 }))
  localExercises.value = reordered
  emit('reorder', reordered)
}

function muscleName(id: string | undefined) {
  if (!id) return ''
  const g = props.muscleGroups.find((m) => m.id === id)
  return g ? g.name : ''
}
</script>

<style scoped>
.default-exercise-item {
  display: flex;
  align-items: center;
  gap: var(--trk-space-3);
  padding: 0.75rem 0.85rem;
  border-radius: var(--trk-radius-md);
  background: var(--trk-surface);
  margin-bottom: 0.1rem;
}
.default-exercise-item .item-count {
  font-size: 0.85rem;
  color: var(--trk-text-muted);
  margin-left: auto;
}
.drag-handle {
  cursor: grab;
  color: var(--trk-text-muted);
  user-select: none;
  font-weight: 700;
}
.exercise-info { display: flex; flex-direction: column; gap: 2px; }
.item-sub { font-size: 0.85rem; color: var(--trk-text-muted); }
.drag-ghost { opacity: 0.45; }
.drag-chosen { background: var(--trk-accent-muted); }
</style>