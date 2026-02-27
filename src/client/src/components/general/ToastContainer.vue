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
  gap: 8px;
  pointer-events: none;
}

.toast {
  display: flex;
  align-items: center;
  pointer-events: auto;
  padding: 0.75rem 1rem;
  border-radius: var(--trk-radius-md);
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  font-weight: 600;
  color: var(--trk-text);
  background: var(--trk-surface);
}

.toast-success { border-left: 4px solid var(--trk-success-bg); }
.toast-info    { border-left: 4px solid var(--trk-accent); }
.toast-warning { border-left: 4px solid var(--trk-warning-bg); }
.toast-error   { border-left: 4px solid var(--trk-danger-bg); }

.toast-message { flex: 1; }
.toast-action,
.toast-close {
  background: none;
  border: none;
  margin-left: 0.5rem;
  font-size: 0.9rem;
  cursor: pointer;
  color: inherit;
}

.toast-enter-active, .toast-leave-active {
  transition: all 0.2s ease;
}
.toast-enter-from, .toast-leave-to {
  opacity: 0;
  transform: translateY(-8px) scale(0.97);
}
</style>
