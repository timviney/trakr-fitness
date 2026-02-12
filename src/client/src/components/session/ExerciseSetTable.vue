<template>
  <div class="sets-table">
    <div class="sets-header">
      <div class="col-warmup">W</div>
      <div class="col-weight">Weight (kg)</div>
      <div class="col-reps">Reps</div>
      <div class="col-done">✓</div>
      <div class="col-actions"></div>
    </div>

    <div v-if="sets.length === 0" class="sets-empty">
      <p>No sets yet. Add your first set below.</p>
    </div>

    <div v-for="(set, idx) in sets" :key="set.tempId" class="set-row">
      <div class="col-warmup">
        <button
          type="button"
          class="warmup-toggle"
          :class="{ active: set.warmUp }"
          @click="toggleWarmup(idx)"
          :aria-pressed="set.warmUp"
          aria-label="Toggle warmup"
        >
          {{ set.warmUp ? 'W' : '' }}
        </button>
      </div>

      <div class="col-weight">
        <input
          type="number"
          v-model.number="set.weight"
          min="0"
          step="0.5"
          @input="updateSets"
          class="number-input"
          placeholder="0"
        />
      </div>

      <div class="col-reps">
        <input
          type="number"
          v-model.number="set.reps"
          min="0"
          @input="updateSets"
          class="number-input"
          placeholder="0"
        />
      </div>

      <div class="col-done">
        <button
          type="button"
          class="done-toggle"
          :class="{ active: set.completed }"
          @click="toggleCompleted(idx)"
          aria-label="Toggle completed"
        >
          <Check v-if="set.completed" :size="16" />
        </button>
      </div>

      <div class="col-actions">
        <button type="button" class="btn-icon-sm btn-danger-icon" @click="emit('remove-set', idx)" aria-label="Remove set">
          <X :size="16" />
        </button>
      </div>
    </div>

    <button type="button" class="btn btn-faded add-set-btn" @click="emit('add-set')">
      <Plus :size="18" />
      Add Set
    </button>
  </div>
</template>

<script setup lang="ts">
import { Plus, X, Check } from 'lucide-vue-next'
import type { SetData } from '../../types/Session'

const props = defineProps<{
  sets: SetData[]
}>()

const emit = defineEmits<{
  (e: 'update:sets', sets: SetData[]): void
  (e: 'add-set'): void
  (e: 'remove-set', index: number): void
}>()

function updateSets() {
  emit('update:sets', [...props.sets])
}

function toggleWarmup(index: number) {
  props.sets[index].warmUp = !props.sets[index].warmUp
  updateSets()
}

function toggleCompleted(index: number) {
  props.sets[index].completed = !props.sets[index].completed
  updateSets()
}
</script>

<style scoped>
.sets-table { display: flex; flex-direction: column; gap: var(--trk-space-2); }

.sets-header {
  display: grid;
  grid-template-columns: 40px 1fr 1fr 40px 40px;
  gap: var(--trk-space-2);
  padding: var(--trk-space-2) var(--trk-space-3);
  color: var(--trk-text-muted);
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.sets-empty {
  padding: var(--trk-space-4);
  text-align: center;
  color: var(--trk-text-muted);
  font-size: 0.875rem;
  background: var(--trk-surface-inner);
  border-radius: var(--trk-radius-md);
}

.set-row {
  display: grid;
  grid-template-columns: 40px 1fr 1fr 40px 40px;
  gap: var(--trk-space-2);
  padding: var(--trk-space-2) var(--trk-space-3);
  background: var(--trk-surface-inner);
  border-radius: var(--trk-radius-md);
  align-items: center;
}

.col-warmup, .col-done { display: flex; align-items: center; justify-content: center; }

.checkbox-input { cursor: pointer; width: 18px; height: 18px; accent-color: var(--trk-accent); }

.number-input {
  width: 100%;
  padding: 0.5rem;
  border-radius: 6px;
  border: 1px solid var(--trk-surface-border);
  background: var(--trk-input-bg);
  color: var(--trk-text);
  font-size: 0.875rem;
  text-align: center;
  font-family: inherit;
}

.number-input:focus {
  outline: none;
  border-color: var(--trk-accent);
  box-shadow: 0 0 0 2px var(--trk-accent-ring);
}

/* hide default number input spinners for a cleaner UI */
.number-input::-webkit-outer-spin-button,
.number-input::-webkit-inner-spin-button { -webkit-appearance: none; margin: 0; }
.number-input[type=number] { -moz-appearance: textfield; appearance: textfield; }

.number-input::placeholder { color: var(--trk-text-muted); opacity: 0.5; }

.col-actions { display: flex; justify-content: center; }

.btn-icon-sm {
  padding: 6px; border: none; background: transparent; color: var(--trk-danger); cursor: pointer; border-radius: 4px; display: flex; align-items: center; justify-content: center; transition: background 150ms ease;
}

.btn-icon-sm:hover { background: var(--trk-danger-muted); }
.btn-icon-sm:active { transform: scale(0.95); }

.add-set-btn { width: 100%; margin-top: var(--trk-space-2); }
</style>
