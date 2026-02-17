<template>
  <AppShell>
    <div class="history-view">
      <header class="view-header">
        <h1 class="view-title">Session History</h1>
      </header>

      <Loader :loading="historyLoading" />

      <div v-if="!historyLoading && sessionHistory.length === 0" class="empty-state">
        <h2 class="empty-title">No sessions recorded yet</h2>
        <p class="empty-description">
          Record your first session from the
          <router-link to="/session/new" class="link">Record Session</router-link>
          page.
        </p>
      </div>

      <ul v-else class="item-list">
        <li v-for="s in sessionHistory" :key="s.id" class="item-card item-card-clickable">
          <div>
            <div class="item-name">{{ s.workout?.name ?? 'Workout' }}</div>
            <div class="item-meta">{{ new Date(s.createdAt).toLocaleString() }}</div>
          </div>
          <div class="item-extra">{{ s.sessionExercises.length }} exercises</div>
        </li>
      </ul>
    </div>
  </AppShell>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { onBeforeRouteUpdate } from 'vue-router'
import Loader from '../components/general/Loader.vue'
import AppShell from '../components/general/AppShell.vue'
import { useStatsStore } from '../stores/stats'

const statsStore = useStatsStore()
const { fetchSessionHistory, sessionHistory, historyLoading } = statsStore

async function fetch() {
  await fetchSessionHistory()
}

onMounted(fetch)
onBeforeRouteUpdate((to, from, next) => {
  fetch().then(() => next())
})
</script>

<style scoped>
.history-view { display: flex; flex-direction: column; gap: var(--trk-space-4); }
.item-meta { font-size: 0.875rem; color: var(--trk-text-muted); }
.item-extra { font-weight: 700; color: var(--trk-text); }
</style>
