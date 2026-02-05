<template>
  <AppShell>
    <div class="session-view">
      <header class="view-header">
        <h1 class="view-title">Record Session</h1>
      </header>

      <div class="placeholder-state">
        <Play class="placeholder-icon" />
        <h2 class="placeholder-title">Coming Soon</h2>
        <p class="placeholder-description">
          Session recording will let you log your sets, reps, and weights in real-time. First,
          make sure you have some
          <router-link to="/exercises" class="link">workouts created</router-link>.
        </p>
      </div>
    </div>
  </AppShell>
</template>

<script setup lang="ts">
import { Play } from 'lucide-vue-next'
import AppShell from '../components/AppShell.vue'
</script>

<style scoped>
.session-view {
  display: flex;
  flex-direction: column;
  gap: var(--trk-space-6);
}

.view-header {
  text-align: center;
}

.view-title {
  font-family: var(--trk-font-heading);
  font-size: clamp(1.75rem, 1.5rem + 1.25vw, 2.25rem);
  color: var(--trk-text);
  margin: 0;
}

.placeholder-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding: var(--trk-space-8) var(--trk-space-4);
  margin-top: var(--trk-space-6);
}

.placeholder-icon {
  width: 64px;
  height: 64px;
  color: var(--trk-accent);
  margin-bottom: var(--trk-space-4);
  opacity: 0.7;
}

.placeholder-title {
  font-family: var(--trk-font-heading);
  font-size: 1.5rem;
  color: var(--trk-text);
  margin: 0 0 var(--trk-space-2);
}

.placeholder-description {
  color: var(--trk-text-muted);
  font-size: 0.9375rem;
  line-height: 1.6;
  max-width: 280px;
  margin: 0;
}

.link {
  color: var(--trk-accent);
  text-decoration: none;
}

.link:hover {
  text-decoration: underline;
}
</style>
