<template>
  <AppShell>
    <div class="stats-view">
      <header class="view-header">
        <h1 class="view-title">Your Stats</h1>
      </header>

      <div class="empty-state">
        <BarChart2 class="empty-icon" />
        <h2 class="empty-title">No workout data yet</h2>
        <p class="empty-description">
          Start by adding exercises in the
          <router-link to="/exercises" class="link">Exercises</router-link>
          tab, or record your first workout session.
        </p>
        <router-link to="/session/new" class="btn btn-primary">
          Record a Session
        </router-link>
      </div>
    </div>
  </AppShell>
</template>

<script setup lang="ts">
import { BarChart2 } from 'lucide-vue-next'
import AppShell from '../components/AppShell.vue'
</script>

<style scoped>
.stats-view {
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

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding: var(--trk-space-8) var(--trk-space-4);
  margin-top: var(--trk-space-8);
}

.empty-icon {
  width: 64px;
  height: 64px;
  color: var(--trk-text-muted);
  margin-bottom: var(--trk-space-4);
  opacity: 0.5;
}

.empty-title {
  font-family: var(--trk-font-heading);
  font-size: 1.5rem;
  color: var(--trk-text);
  margin: 0 0 var(--trk-space-2);
}

.empty-description {
  color: var(--trk-text-muted);
  font-size: 0.9375rem;
  line-height: 1.6;
  max-width: 280px;
  margin: 0 0 var(--trk-space-6);
}

.link {
  color: var(--trk-accent);
  text-decoration: none;
}

.link:hover {
  text-decoration: underline;
}
</style>
