<template>
  <div class="tracker-view">
    <div class="sets-grid sets-header">
      <div class="col-warmup">Warmup</div>
      <div class="col-kg">kg</div>
      <div class="col-reps">reps</div>
      <div class="col-done">Done</div>
      <div class="col-del"></div>
    </div>

    <div class="sets-body">
      <div v-if="sets.length === 0" class="empty-state">
        <p class="empty-description">No sets recorded yet.</p>
      </div>

      <div 
        v-for="(set, idx) in sets" 
        :key="set.tempId || idx" 
        class="sets-grid set-row"
        :class="{ 'is-complete': set.completed }"
      >
        <div class="col-warmup">
          <button 
            class="warmup-btn" 
            :class="{ 'is-active': set.warmUp }"
            @click="toggleWarmup(idx)"
          >
            W
          </button>
        </div>

        <div class="col-kg">
          <div class="input-pill">
            <button class="pill-btn" @click="openPicker(idx, 'weight')">
              <span class="pill-value">{{ set.weight }}</span>
            </button>
          </div>
        </div>

        <div class="col-reps">
          <div class="input-pill">
            <button class="pill-btn" @click="openPicker(idx, 'reps')">
              <span class="pill-value">{{ set.reps }}</span>
            </button>
          </div>
        </div>

        <div class="col-done">
          <button 
            class="done-btn" 
            :class="{ 'is-active': set.completed }"
            @click="toggleCompleted(idx)"
          >
            <Check v-if="set.completed" :size="20" stroke-width="3" />
          </button>
        </div>

        <div class="col-del">
          <button class="del-col-btn" @click="confirmRemove(idx)" aria-label="Delete set">
            <Trash2 :size="20" />
          </button>
        </div>
      </div>

      <button class="sets-grid add-set-row" @click="emit('add-set')">
        <div class="col-warmup"><Plus :size="18" /></div>
        <div class="col-kg col-reps col-done" style="grid-column: span 3;">Add New Set</div>
        <div class="col-del"></div>
      </button>
    </div>

    <WeightRepPicker
      ref="pickerRef"
      @weight="onWeightChange"
      @reps="onRepsChange"
      @close="onPickerClose"
    />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import WeightRepPicker from './WeightRepPicker.vue'
import { Plus, Trash2, Check } from 'lucide-vue-next' 
import type { SetData } from '../../types/Session'

const props = defineProps<{
  sets: SetData[]
}>()

const emit = defineEmits<{
  (e: 'update:set', set: SetData): void
  (e: 'add-set'): void
  (e: 'remove-set', index: number): void
}>()

// --- State Management ---

function updateSet(index: number) {
  emit('update:set', props.sets[index])
}

function toggleWarmup(index: number) {
  props.sets[index].warmUp = !props.sets[index].warmUp
  updateSet(index)
}

function toggleCompleted(index: number) {
  props.sets[index].completed = !props.sets[index].completed
  updateSet(index)
}

// --- Picker ---

const pickerRef = ref<any>(null)
const selectedIndex = ref(-1)

function openPicker(index: number, mode: 'weight' | 'reps') {
  selectedIndex.value = index
  const initial = mode === 'weight'
    ? props.sets[index].weight ?? 0
    : props.sets[index].reps ?? 0

  // delegate open + initial value to the child picker
  pickerRef.value?.open(mode, initial)
}

function onWeightChange(val: number) {
  if (selectedIndex.value === -1) return
  props.sets[selectedIndex.value].weight = val
  updateSet(selectedIndex.value)
}

function onRepsChange(val: number) {
  if (selectedIndex.value === -1) return
  props.sets[selectedIndex.value].reps = val
  updateSet(selectedIndex.value)
}

function onPickerClose() {
  // reset selection when picker closes
  selectedIndex.value = -1
}

function confirmRemove(index: number) {
  if (!confirm('Delete this set?')) return
  emit('remove-set', index)
} 
</script>

<style scoped>
/* Grid Layout shared by Header, Row, and Add Button */
.sets-grid {
  display: grid;
   grid-template-columns: 50px 0.9fr 0.3fr 1fr 32px; /* warmup / kg / reps / done / delete */
  align-items: center;
}

.tracker-view {
  width: 100%;
  background: var(--trk-bg);
  -webkit-tap-highlight-color: transparent;
  /* ensure table content respects mobile safe areas */
  padding-top: env(safe-area-inset-top, 0);
  padding-bottom: env(safe-area-inset-bottom, 0);
}

.sets-header {
  height: 40px;
  padding: 0 16px;
  border-bottom: 1px solid var(--trk-surface-border);
  color: var(--trk-text-muted);
  font-size: 0.7rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

.set-row {
  height: 72px;
  padding: 0 16px;
  border-bottom: 1px solid var(--trk-surface-border);
  background: var(--trk-bg); /* Match app background */
}

.set-row.is-complete {
  background: rgba(26, 58, 46, 0.1);
}

.col-kg { text-align: center; align-items: center; justify-content: center;}
.col-reps { text-align: center; align-items: center; justify-content: center; }

/* 1. Ghost Warmup Styles */
.warmup-btn {
  width: 26px;
  height: 26px;
  background: transparent;
  border: none;
  font-family: 'Inter', sans-serif;
  font-weight: 900;
  font-size: 1.2rem;
  color: var(--trk-text-muted);
  opacity: 0.15;
  transition: all 0.2s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.warmup-btn.is-active {
  opacity: 1;
  color: var(--trk-accent);
  transform: scale(1.1);
  text-shadow: 0 0 12px rgba(250, 204, 21, 0.3);
}

/* 2. Horizontal Pill */
.input-pill {
  display: inline-flex; 
  justify-content: center;
  align-items: center;
  background: var(--trk-surface-inner);
  border: 1px solid var(--trk-surface-border);
  border-radius: var(--trk-radius-md);
  padding: 2px 2px;
  width: 40px;
}

.pill-btn {
  background: transparent;
  border: none;
  display: flex;
  align-items: baseline; /* Align kg/reps with bottom of number */
  gap: 4px;
  padding: 8px 10px;
  cursor: pointer;
}

.pill-value {
  font-size: 1.2rem;
  font-weight: 700;
  color: var(--trk-text);
}

.pill-unit {
  font-size: 0.75rem;
  color: var(--trk-text-muted);
  font-weight: 500;
}

.pill-divider {
  width: 1px;
  height: 18px;
  background: var(--trk-surface-border);
}

/* 3. Success & Delete Action */
.col-done {
  position: relative;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.done-btn {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  background: var(--trk-surface-inner);
  border: 2px solid var(--trk-surface-border);
  color: transparent;
  transition: all 0.15s;
}

.done-btn.is-active {
  background: var(--trk-success-bg);
  border-color: var(--trk-success-text);
  color: var(--trk-success-text);
}

.col-del { display: flex; align-items: center; justify-content: center; }
.del-col-btn {
  width: 58px;
  height: 28px;
  border-radius: 6px;
  background: transparent; /* boxless */
  border: none;
  color: var(--trk-danger);
  display: inline-flex;
  align-items: center;
  justify-content: right;
  opacity: 0.9;
  padding-right: 0%;
}


/* 4. Inline Add Set Row */
.add-set-row {
  width: 100%;
  height: 60px;
  padding: 0 16px;
  background: transparent;
  border: none;
  border-bottom: 1px dashed var(--trk-surface-border);
  color: var(--trk-text-muted);
  font-weight: 600;
  cursor: pointer;
  text-align: left;
}

.add-set-row:active {
  background: var(--trk-surface-inner);
  color: var(--trk-accent);
}

/* Transitions */
.slide-up-enter-active, .slide-up-leave-active { transition: transform 0.3s ease; }
.slide-up-enter-from, .slide-up-leave-to { transform: translateY(100%); }
</style>