<template>
  <div class="app-shell">
    <main class="app-content">
      <slot />
    </main>
    <BottomNav />
  </div>
</template>

<script setup lang="ts">
import BottomNav from './BottomNav.vue'
</script>

<style scoped>
.app-shell {
  min-height: 100dvh;
  display: flex;
  flex-direction: column;
  background: var(--trk-bg);
}

.app-content {
  flex: 1;
  padding: var(--trk-space-4);
  padding-bottom: calc(56px + env(safe-area-inset-bottom, 0) + var(--trk-space-4));
  overflow-y: auto;
}
</style>
