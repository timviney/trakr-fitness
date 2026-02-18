<template>
  <Transition name="slide-up">
    <div v-if="isOpen" class="picker-overlay" @click.self="handleClose">
      <div class="picker-sheet">
        <div class="picker-header">
          <span class="picker-title">Set {{ mode }}</span>
          <button class="btn-done" @click="handleClose">Done</button>
        </div>

        <div class="picker-wheels">
          <div class="wheel-col">
            <div class="wheel-scroll" ref="integerScroll" @scroll="onIntegerScroll">
              <div class="wheel-spacer"></div>
              <button
                v-for="i in integers"
                :key="i"
                class="wheel-item"
                :class="{ 'is-selected': tempInt === i }"
                @click="selectInt(i)"
              >
                {{ i }}
              </button>
              <div class="wheel-spacer"></div>
            </div>
          </div>

          <div class="wheel-col" v-if="mode === 'weight'">
            <div class="wheel-scroll" ref="decimalScroll" @scroll="onDecimalScroll">
              <div class="wheel-spacer"></div>
              <button
                class="wheel-item"
                :class="{ 'is-selected': tempDec === 0 }"
                @click="selectDec(0)"
              >
                .0
              </button>
              <button
                class="wheel-item"
                :class="{ 'is-selected': tempDec === 0.5 }"
                @click="selectDec(0.5)"
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
</template>

<script setup lang="ts">
import { ref, nextTick } from 'vue'

const emit = defineEmits<{
  (e: 'weight', val: number): void
  (e: 'reps', val: number): void
  (e: 'close'): void
}>()

const isOpen = ref(false)
const mode = ref<'weight' | 'reps'>('weight')
const integers = ref<number[]>([])

const tempInt = ref<number>(0)
const tempDec = ref<number>(0)
const integerScroll = ref<HTMLElement | null>(null)
const decimalScroll = ref<HTMLElement | null>(null)
let intScrollTimeout: number | null = null
let decScrollTimeout: number | null = null

function syncFromValue(v: number) {
  tempInt.value = Math.floor(v || 0)
  tempDec.value = (v || 0) % 1
}

function open(m: 'weight' | 'reps', initial = 0) {
  mode.value = m
  integers.value = m === 'weight'
    ? Array.from({ length: 300 }, (_, i) => i)
    : Array.from({ length: 100 }, (_, i) => i)

  syncFromValue(initial)
  isOpen.value = true

  // ensure highlighted item is visible after render
  nextTick(() => {
    const el = integerScroll.value?.querySelector('.wheel-item.is-selected') as HTMLElement | null
    el?.scrollIntoView({ block: 'center', behavior: 'smooth' })
  })
}

function handleClose() {
  isOpen.value = false
  // clear any pending timeouts
  if (intScrollTimeout) window.clearTimeout(intScrollTimeout)
  if (decScrollTimeout) window.clearTimeout(decScrollTimeout)
  emit('close')
}

function selectInt(i: number) {
  // scroll the clicked item into the center and select it
  const items = integerScroll.value?.querySelectorAll('.wheel-item') ?? []
  for (const el of Array.from(items) as HTMLElement[]) {
    const txt = el.textContent?.trim() ?? ''
    if (Number(txt) === i) {
      el.scrollIntoView({ block: 'center', behavior: 'smooth' })
      break
    }
  }
  tempInt.value = i
  emitChange()
}

function selectDec(d: number) {
  // scroll the clicked decimal item into view (text shows ".0" or ".5")
  const items = decimalScroll.value?.querySelectorAll('.wheel-item') ?? []
  for (const el of Array.from(items) as HTMLElement[]) {
    const txt = (el.textContent?.trim() ?? '')
    const parsed = Number(txt.replace(/^\./, '0.'))
    if (parsed === d) {
      el.scrollIntoView({ block: 'center', behavior: 'smooth' })
      break
    }
  }
  tempDec.value = d
  emitChange()
}

function emitChange() {
  const finalVal = tempInt.value + (mode.value === 'weight' ? tempDec.value : 0)
  if (mode.value === 'weight') emit('weight', finalVal)
  else emit('reps', finalVal)
}

function findCenteredValue(container: HTMLElement | null): number | null {
  if (!container) return null
  const rect = container.getBoundingClientRect()
  const centerY = rect.top + rect.height / 2
  const items = Array.from(container.querySelectorAll('.wheel-item')) as HTMLElement[]
  if (!items.length) return null
  let closest: { el: HTMLElement; d: number } | null = null
  for (const el of items) {
    const r = el.getBoundingClientRect()
    const itemCenter = r.top + r.height / 2
    const d = Math.abs(itemCenter - centerY)
    if (!closest || d < closest.d) closest = { el, d }
  }
  if (!closest) return null
  const txt = closest.el.textContent?.trim() ?? ''
  if (!txt) return null
  // parse numeric (handles ".0" and ".5" too)
  const num = Number(txt.replace(/^\./, '0.'))
  return Number.isFinite(num) ? num : null
}

function onIntegerScroll() {
  if (intScrollTimeout) window.clearTimeout(intScrollTimeout)
  intScrollTimeout = window.setTimeout(() => {
    const val = findCenteredValue(integerScroll.value)
    if (val !== null && tempInt.value !== val) {
      tempInt.value = val
      emitChange()
    }
  }, 100)
}

function onDecimalScroll() {
  if (decScrollTimeout) window.clearTimeout(decScrollTimeout)
  decScrollTimeout = window.setTimeout(() => {
    const val = findCenteredValue(decimalScroll.value)
    if (val !== null && tempDec.value !== val) {
      tempDec.value = val
      emitChange()
    }
  }, 100)
}

// expose the open method for parent to call via ref
defineExpose({ open })
</script>

<style scoped>
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
.slide-up-enter-active, .slide-up-leave-active { transition: transform 0.1s ease; }
.slide-up-enter-from, .slide-up-leave-to { transform: translateY(100%); }
</style>
