<template>
  <AppShell>
    <div class="stats-view">
      <header class="view-header">
        <h1 class="view-title">Your Stats</h1>
      </header>

      <div class="stats-landing">
        <Loader :loading="historyLoading" />

        <div v-if="!historyLoading">
          <p class="muted">Session history is fetched each time you enter this view. Charts will be added later.</p>

          <div class="charts-placeholder">
            <div class="stat-card">Total sessions: <strong>{{ sessionHistory.length }}</strong></div>
          </div>
        </div>
      </div>
    </div>
  </AppShell>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { BarChart2 } from 'lucide-vue-next'
import AppShell from '../components/general/AppShell.vue'
import Loader from '../components/general/Loader.vue'
import { useStatsStore } from '../stores/stats'

const statsStore = useStatsStore()
const { fetchSessionHistory, sessionHistory, historyLoading } = statsStore

onMounted(() => {
  fetchSessionHistory()
})
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

.muted { color: var(--trk-text-muted); margin-bottom: var(--trk-space-4); }
.charts-placeholder { display: flex; gap: var(--trk-space-4); }
.stat-card { padding: var(--trk-space-3); background: var(--trk-surface); border-radius: var(--trk-radius-md); border: 1px solid var(--trk-surface-border); }
.stats-landing { padding: var(--trk-space-4); }

</style>
