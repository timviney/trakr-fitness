<template>
  <Draggable
    :list="localExercises"
    item-key="id"
    handle=".drag-handle"
    ghost-class="drag-ghost"
    chosen-class="drag-chosen"
    :animation="150"
    :disabled="disabled"
    @start="onDragStart"
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
  <div
    ref="deleteZone"
    class="delete-zone"
    :class="{ active: isDragging }"
  >
    🗑 Drop here to delete
  </div>
  <button type="button" class="btn btn-faded"
    @click="openAddExerciseModal = true">
    + Add
  </button>
  <ExerciseSelector v-if="openAddExerciseModal" 
    :exercise-collection="props.exerciseCollection" 
    @add="exerciseId => addExercise(exerciseId)" 
    @cancel="openAddExerciseModal = false">
  </ExerciseSelector>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import Draggable from 'vuedraggable'
import type { DefaultExercise } from '../../api/modules/workouts'
import { PropType } from 'vue'
import ExerciseSelector from './ExerciseSelector.vue'
import { ExerciseCollection } from '../../types/ExerciseCollection'
import { emptyGuid } from '../../types/Guid'

const props = defineProps({
  workoutId: { type: String as PropType<string>, required: true },
  exercises: { type: Array as PropType<DefaultExercise[]>, required: true },
  exerciseCollection: { type: Object as PropType<ExerciseCollection>, required: false, default: () => ({ exercises: [], muscleGroups: [], workouts: [] }) },
  disabled: { type: Boolean as PropType<boolean>, required: false, default: false },
})

const emit = defineEmits<{
  (e: 'reorder', exercises: DefaultExercise[]): void
}>()

const localExercises = ref<DefaultExercise[]>(props.exercises || [])
const deleteZone = ref<HTMLElement | null>(null)
const isDragging = ref(false)
let draggedExercise: DefaultExercise | null = null
const openAddExerciseModal = ref<boolean>(false);

function onDragStart(evt: any) {
  isDragging.value = true
  draggedExercise = localExercises.value[evt.oldIndex]
}

function onDragEnd(evt: any) {
  isDragging.value = false

  if (draggedExercise === null || !deleteZone.value) {
    draggedExercise = null
    return
  }

  const droppedInDeleteZone = isInDeleteZone(evt)

  if (droppedInDeleteZone) {
    localExercises.value = localExercises.value.filter((ex) => ex.id !== draggedExercise?.id)
  }
  
  localExercises.value = localExercises.value.map((ex, i) => ({
      ...ex,
      exerciseNumber: i
    }))

  emit('reorder', localExercises.value)

  draggedExercise = null
}

function getEventCoordinates(evt: any) {
  const e = evt.originalEvent

  if (!e) return null

  // Mouse
  if (e.clientX !== undefined && e.clientY !== undefined) {
    return { x: e.clientX, y: e.clientY }
  }

  // Touch (mobile)
  if (e.changedTouches && e.changedTouches.length > 0) {
    return {
      x: e.changedTouches[0].clientX,
      y: e.changedTouches[0].clientY
    }
  }

  return null
}

function isInDeleteZone(evt: any) {
  if (!deleteZone.value) return false
  const coords = getEventCoordinates({ originalEvent: evt.originalEvent })
  if (!coords) return false
  const rect = deleteZone.value.getBoundingClientRect()
  return (
    coords.x >= rect.left &&
    coords.x <= rect.right &&
    coords.y >= rect.top &&
    coords.y <= rect.bottom
  )
}

function addExercise(exerciseId: string) {
  const exercise = props.exerciseCollection.exercises.find((ex) => ex.id === exerciseId)
  if (!exercise) return

  let defaultExercise: DefaultExercise = {
    id: emptyGuid,
    exerciseId: exercise.id,
    exerciseNumber: localExercises.value.length + 1,
    exercise: exercise,
    workoutId: props.workoutId
  }

  localExercises.value.push(defaultExercise)
  emit('reorder', localExercises.value)
  openAddExerciseModal.value = false
}

function muscleName(id: string | undefined) {
  if (!id) return ''
  const g = props.exerciseCollection.muscleGroups.find((m) => m.id === id)
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
.drag-ghost { opacity: 0.45; }
.drag-chosen { background: var(--trk-accent-muted); }

.delete-zone {
  margin-top: 0.75rem;
  padding: 0.75rem;
  border: 2px dashed var(--trk-text-muted);
  border-radius: var(--trk-radius-md);
  text-align: center;
}

.delete-zone.active {
  border-color: var(--trk-danger);
  color: var(--trk-danger);
  background: var(--trk-danger-muted);
}
</style>