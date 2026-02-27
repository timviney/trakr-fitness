<template>
  <div class="toast-container">
    <transition-group name="toast" tag="div">
      <div
        v-for="t in toasts"
        :key="t.id"
        :class="['toast', `toast-${t.type}`]"
      >
        <span class="toast-message">{{ t.message }}</span>
        <button
          v-if="t.action"
          class="toast-action"
          @click="t.action.handler()"
        >{{ t.action.text }}</button>
        <button class="toast-close" @click="store.remove(t.id)">×</button>
      </div>
    </transition-group>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useToastStore } from '../../stores/toastStore'

const store = useToastStore()
const toasts = computed(() => store.toasts)
</script>

<style scoped>
.toast-container {
  position: fixed;
  top: env(safe-area-inset-top, 16px);
  right: 16px;
  z-index: 10000;
  display: flex;
  flex-direction: column;
  gap: var(--trk-space-3);
  pointer-events: none;
}

.toast {
  display: flex;
  align-items: center;
  pointer-events: auto;
  width: 100%;
  max-width: 400px;
  padding: var(--trk-space-3) var(--trk-space-4);
  border-radius: var(--trk-radius-lg);
  box-shadow: var(--trk-shadow);
  font-weight: 600;
  font-size: clamp(var(--trk-font-sm), 1rem, var(--trk-font-compact)); /* slightly larger on small screens */
  color: var(--trk-text);
  background: var(--trk-surface);
}

.toast-success { border-left: 4px solid var(--trk-success-bg); }
.toast-info    { border-left: 4px solid var(--trk-accent); }
.toast-warning { border-left: 4px solid var(--trk-warning-bg); }
.toast-error   { border-left: 4px solid var(--trk-danger-bg); }

.toast-message {
  flex: 1;
  line-height: 1.3;
}
.toast-action,
.toast-close {
  background: none;
  border: none;
  margin-left: var(--trk-space-2);
  font-size: var(--trk-font-sm);
  cursor: pointer;
  color: inherit;
}
.toast-action {
  color: var(--trk-accent);
  font-weight: 500;
}
.toast-action:hover {
  text-decoration: underline;
}
.toast-close {
  font-weight: 700;
  opacity: 0.7;
  font-size: 1.25rem;      /* larger X on mobile */
  line-height: 1;
}
.toast-close:hover {
  opacity: 1;
}

.toast-enter-active, .toast-leave-active {
  transition: all 0.2s ease;
}
.toast-enter-from, .toast-leave-to {
  opacity: 0;
  /* slide up when appearing from bottom; slide down when leaving */
  transform: translateY(8px) scale(0.97);
}
</style>
