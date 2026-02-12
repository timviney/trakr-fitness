<template>
  <div class="sets-table">
    <div class="sets-header">
      <div class="col-warmup">W</div>
      <div class="col-weight">Weight (kg)</div>
      <div class="col-reps">Reps</div>
      <div class="col-done">✓</div>
      <div class="col-actions"></div>
    </div>

    <div class="sets-body">
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
    </div>

    <button type="button" class="btn btn-secondary add-set-btn" @click="emit('add-set')">
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
.sets-table { display: flex; flex-direction: column; gap: 12px; /* increased from var(--trk-space-2) */ height: 100%; min-height: 220px; }

/* Body area becomes scrollable so header + add button stay visible */
.sets-body { flex: 1 1 auto; overflow: auto; -webkit-overflow-scrolling: touch; padding-right: 6px; display: flex; flex-direction: column; gap: 10px; /* increased from var(--trk-space-2) */ }

.sets-header {
  display: grid;
  grid-template-columns: 40px 1fr 1fr 40px 40px;
  gap: 12px; 
  padding: 10px 16px;
  color: var(--trk-text-muted);
  font-size: 0.6875rem; 
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.sets-empty {
  padding: var(--trk-space-6);
  text-align: center;
  color: var(--trk-text-muted);
  font-size: 0.875rem;
  line-height: 1.5; /* add line height */
  background: var(--trk-surface-inner);
  border-radius: var(--trk-radius-md);
}

.set-row {
  display: grid;
  grid-template-columns: 40px 1fr 1fr 40px 40px;
  gap: 12px; /* increased from var(--trk-space-2) */
  padding: 12px 16px; /* increased from var(--trk-space-2) var(--trk-space-3) */
  background: var(--trk-surface-inner);
  border-radius: var(--trk-radius-md);
  align-items: center;
  border: 1px solid transparent;
  transition: all 150ms cubic-bezier(0.4, 0, 0.2, 1);
}

.set-row:hover {
  border-color: var(--trk-surface-border);
  background: var(--trk-surface-hover);
}

.col-warmup, .col-done { display: flex; align-items: center; justify-content: center; }

.checkbox-input { cursor: pointer; width: 18px; height: 18px; accent-color: var(--trk-accent); }

.number-input {
  width: 100%;
  padding: 10px 12px; /* increased from 0.5rem */
  border-radius: 8px; /* increased from 6px */
  border: 1px solid var(--trk-surface-border);
  background: var(--trk-surface); /* changed from var(--trk-input-bg) for better contrast */
  color: var(--trk-text);
  font-size: 0.9375rem; /* 15px - increased from 0.875rem */
  text-align: center;
  font-family: inherit;
  transition: all 150ms cubic-bezier(0.4, 0, 0.2, 1);
}

.number-input:hover:not(:focus) {
  border-color: var(--trk-accent-muted);
}

.number-input:focus {
  outline: none;
  border-color: var(--trk-accent);
  box-shadow: 0 0 0 3px var(--trk-accent-ring);
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

.warmup-toggle {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  border: 1px solid var(--trk-surface-border);
  background: transparent;
  color: var(--trk-text-muted);
  font-size: 0.75rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 150ms cubic-bezier(0.4, 0, 0.2, 1);
  display: flex;
  align-items: center;
  justify-content: center;
}

.warmup-toggle:hover {
  background: var(--trk-surface-hover);
  border-color: var(--trk-accent-muted);
}

.warmup-toggle.active {
  background: var(--trk-accent);
  color: var(--trk-text-dark);
  border-color: var(--trk-accent);
  transform: scale(1.05);
}

.warmup-toggle:active {
  transform: scale(0.95);
}

.done-toggle {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  border: 1px solid var(--trk-surface-border);
  background: transparent;
  color: var(--trk-text-muted);
  cursor: pointer;
  transition: all 150ms cubic-bezier(0.4, 0, 0.2, 1);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0;
}

.done-toggle:hover {
  background: var(--trk-surface-hover);
  border-color: var(--trk-success-border);
}

.done-toggle.active {
  background: var(--trk-success-bg);
  color: var(--trk-success-text);
  border-color: var(--trk-success-border);
}

.done-toggle:active {
  transform: scale(0.95);
}
.btn-icon-sm:hover { background: var(--trk-danger-muted); }
.btn-icon-sm:active { transform: scale(0.95); }

.add-set-btn { 
  width: 100%; 
  margin-top: 8px; /* reduced from var(--trk-space-2) since gap increased */
  font-size: 0.9375rem; /* match input size */
}
</style>
