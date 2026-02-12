<template>
  <div class="tracker-view">
    <div class="sets-grid sets-header">
      <div class="col-warmup">Warmup</div>
      <div class="col-kg">kg</div>
      <div class="col-reps">reps</div>
      <div class="col-done">Done</div>
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
          
          <button class="del-btn" @click="emit('remove-set', idx)">
            <X :size="14" />
          </button>
        </div>
      </div>

      <button class="sets-grid add-set-row" @click="emit('add-set')">
        <div class="col-warmup"><Plus :size="18" /></div>
        <div class="col-kg col-reps" style="grid-column: span 2;">Add New Set</div>
        <div class="col-done"></div>
      </button>
    </div>

    <Transition name="slide-up">
      <div v-if="picker.isOpen" class="picker-overlay" @click.self="closePicker">
        <div class="picker-sheet">
          <div class="picker-header">
            <span class="picker-title">Set {{ picker.mode }}</span>
            <button class="btn-done" @click="closePicker">Done</button>
          </div>
          <div class="picker-wheels">
            
            <div class="wheel-col">
              <div class="wheel-scroll" ref="integerScroll">
                <div class="wheel-spacer"></div>
                <button 
                  v-for="i in picker.integers" 
                  :key="i"
                  class="wheel-item"
                  :class="{ 'is-selected': picker.tempInt === i }"
                  @click="setPickerInt(i)"
                >
                  {{ i }}
                </button>
                <div class="wheel-spacer"></div>
              </div>
            </div>

            <div class="wheel-col" v-if="picker.mode === 'weight'">
              <div class="wheel-scroll">
                <div class="wheel-spacer"></div>
                <button 
                  class="wheel-item" 
                  :class="{ 'is-selected': picker.tempDec === 0 }"
                  @click="setPickerDec(0)"
                >
                  .0
                </button>
                <button 
                  class="wheel-item" 
                  :class="{ 'is-selected': picker.tempDec === 0.5 }"
                  @click="setPickerDec(0.5)"
                >
                  .5
                </button>
                <div class="wheel-spacer"></div>
              </div>
            </div>

            <div class="wheel-highlight"></div>
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, nextTick } from 'vue'
import { Plus, X, Check } from 'lucide-vue-next'
import type { SetData } from '../../types/Session' // Keep your type import

const props = defineProps<{
  sets: SetData[]
}>()

const emit = defineEmits<{
  (e: 'update:sets', sets: SetData[]): void
  (e: 'add-set'): void
  (e: 'remove-set', index: number): void
}>()

// --- State Management ---

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

// --- Picker Logic ---

const picker = reactive({
  isOpen: false,
  mode: 'weight' as 'weight' | 'reps',
  targetIndex: -1,
  integers: [] as number[],
  tempInt: 0,
  tempDec: 0
})

function openPicker(index: number, mode: 'weight' | 'reps') {
  picker.mode = mode
  picker.targetIndex = index
  
  // Setup logic for weight vs reps
  const currentVal = mode === 'weight' ? props.sets[index].weight : props.sets[index].reps
  picker.tempInt = Math.floor(currentVal || 0)
  picker.tempDec = (currentVal || 0) % 1
  
  // Generate ranges
  if (mode === 'weight') {
    picker.integers = Array.from({ length: 300 }, (_, i) => i) // 0 to 299kg
  } else {
    picker.integers = Array.from({ length: 100 }, (_, i) => i) // 0 to 99 reps
  }

  picker.isOpen = true
  
  // Auto-scroll to current value (simple implementation)
  nextTick(() => {
    const el = document.querySelector('.wheel-item.is-selected')
    el?.scrollIntoView({ block: 'center', behavior: 'smooth' })
  })
}

function setPickerInt(val: number) {
  picker.tempInt = val
  saveValue()
}

function setPickerDec(val: number) {
  picker.tempDec = val
  saveValue()
}

function saveValue() {
  if (picker.targetIndex === -1) return
  const finalVal = picker.tempInt + (picker.mode === 'weight' ? picker.tempDec : 0)
  
  if (picker.mode === 'weight') {
    props.sets[picker.targetIndex].weight = finalVal
  } else {
    props.sets[picker.targetIndex].reps = finalVal
  }
  updateSets()
}

function closePicker() {
  picker.isOpen = false
}
</script>

<style scoped>
/* Grid Layout shared by Header, Row, and Add Button */
.sets-grid {
  display: grid;
  grid-template-columns: 70px 1fr 1fr 70px; /* Fixed side columns, fluid center */
  align-items: center;
}

.tracker-view {
  width: 100%;
  background: var(--trk-bg);
  -webkit-tap-highlight-color: transparent;
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

.col-kg { text-align: center; align-items: center; }
.col-reps { text-align: center; align-items: center; }

/* 1. Ghost Warmup Styles */
.warmup-btn {
  width: 40px;
  height: 40px;
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
  width: 44px;
  height: 44px;
  border-radius: 12px;
  background: var(--trk-surface-inner);
  border: 2px solid var(--trk-surface-border);
  color: transparent;
  transition: all 0.2s;
}

.done-btn.is-active {
  background: var(--trk-success-bg);
  border-color: var(--trk-success-text);
  color: var(--trk-success-text);
}

.del-btn {
  position: absolute;
  top: 0;
  right: -8px;
  background: transparent;
  border: none;
  color: var(--trk-danger);
  opacity: 0;
  padding: 8px;
}

.set-row:hover .del-btn {
  opacity: 0.4;
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

/* --- Picker Overlay (Bottom Sheet) --- */
.picker-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.6);
  backdrop-filter: blur(2px);
  z-index: 999;
  display: flex;
  align-items: flex-end;
}

.picker-sheet {
  width: 100%;
  background: var(--trk-surface);
  border-radius: 20px 20px 0 0;
  box-shadow: 0 -4px 30px rgba(0,0,0,0.5);
  padding-bottom: env(safe-area-inset-bottom);
  border-top: 1px solid var(--trk-surface-border);
}

.picker-header {
  display: flex;
  justify-content: space-between;
  padding: 16px 20px;
  border-bottom: 1px solid var(--trk-surface-border);
}

.picker-title { font-weight: 600; color: var(--trk-text); }
.btn-done, .btn-text { background: none; border: none; color: var(--trk-accent); font-weight: 700; font-size: 1rem; cursor: pointer; }

/* The Wheel Area */
.picker-wheels {
  position: relative;
  height: 220px;
  display: flex;
  justify-content: center;
  overflow: hidden;
}

/* The Highlight Bar (Visual Only) */
.wheel-highlight {
  position: absolute;
  top: 50%;
  left: 16px;
  right: 16px;
  height: 48px;
  transform: translateY(-50%);
  background: var(--trk-surface-hover);
  border-radius: 8px;
  pointer-events: none;
  z-index: 0;
}

.wheel-col {
  flex: 1;
  max-width: 100px;
  height: 100%;
  z-index: 1; /* Above highlight */
}

.wheel-scroll {
  height: 100%;
  overflow-y: scroll;
  scroll-snap-type: y mandatory;
  scrollbar-width: none; /* Firefox */
}
.wheel-scroll::-webkit-scrollbar { display: none; }

.wheel-spacer { height: 86px; /* (220 - 48)/2 */ }

.wheel-item {
  height: 48px;
  width: 100%;
  scroll-snap-align: center;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.25rem;
  color: var(--trk-text-muted);
  background: transparent;
  border: none;
  font-weight: 500;
  transition: color 0.2s, transform 0.2s;
  cursor: pointer;
}

.wheel-item.is-selected {
  color: var(--trk-text);
  font-weight: 700;
  font-size: 1.5rem;
}

/* Transitions */
.slide-up-enter-active, .slide-up-leave-active { transition: transform 0.3s ease; }
.slide-up-enter-from, .slide-up-leave-to { transform: translateY(100%); }
</style>