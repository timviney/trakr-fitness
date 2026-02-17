<template>
  <AppShell>
    <div class="history-view">
      <header class="view-header">
        <h1 class="view-title">Session History</h1>
        <SegmentToggle
          v-model="selectedSegment"
          :optionsLabels="{ sessions: 'Workout Sessions', sets: 'Sets' }"
        />
      </header>

      <Loader :loading="historyLoading" />

      <div v-if="selectedSegment === 'sessions'">
        <div v-if="!historyLoading && sessionHistory.length === 0" class="empty-state">
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

        <ul v-else class="item-list">
          <li v-for="(s, idx) in sessionHistory" :key="s.id" class="item-card item-card-clickable" @click="openSessionMenu(idx)">
            <div>
              <div class="item-name">{{ s.workout?.name ?? 'Workout' }}</div>
              <div class="item-meta">{{ new Date(s.createdAt).toLocaleString() }}</div>
            </div>
            <div class="item-extra">{{ s.sessionExercises.length }} exercises</div>
          </li>
        </ul>

        <!-- Floating overflow menu (full-screen popup) -->
        <Teleport to="body">
          <div v-if="selectedSegment === 'sessions' && openMenuIndex !== null" class="overflow-backdrop" @click="closeOverflowMenu">
            <div class="overflow-menu" @click.stop>
              <div class="overflow-panel">
                <header class="overflow-header">
                  <h3 class="overflow-title">{{ sessionHistory[openMenuIndex].workout?.name ?? 'Session' }}</h3>
                </header>

                <div class="overflow-actions">
                  <button class="btn btn-primary overflow-action" @click="editSession">
                    <Edit3 class="icon" />
                    <span>Edit Session</span>
                  </button>

                  <button class="btn btn-danger overflow-action" @click="deleteSession">
                    <Trash2 class="icon" />
                    <span>Delete Session</span>
                  </button>

                </div>
              </div>
            </div>
          </div>
        </Teleport>
      </div>

      <div v-else>
        <SetsTable :history="flatRows" :exercise-collection="exerciseCollection"/>
      </div>
    </div>
  </AppShell>
</template>

<script setup lang="ts">
import { onMounted, ref, computed, watch } from 'vue'
import { onBeforeRouteUpdate, useRouter } from 'vue-router'
import { BarChart2, Trash2, Edit3 } from 'lucide-vue-next'
import Loader from '../components/general/Loader.vue'
import AppShell from '../components/general/AppShell.vue'
import SegmentToggle from '../components/general/SegmentToggle.vue'
import SetsTable from '../components/session/SetsTable.vue'
import { useStatsStore } from '../stores/stats'
import { api } from '../api/api'
import { storeToRefs } from 'pinia'
import { transformToFlatRows } from '../stores/statsHelpers'
import type { ExerciseCollection } from '../types/ExerciseCollection'

const router = useRouter()
const statsStore = useStatsStore()
const { fetchSessionHistory } = statsStore
const { sessionHistory, historyLoading } = storeToRefs(statsStore)

const openMenuIndex = ref<number | null>(null)
const selectedSegment = ref<'sessions' | 'sets'>('sessions')
const exerciseCollection = ref<ExerciseCollection>({ workouts: [], exercises: [], muscleCategories: [], muscleGroups: [] })

const flatRows = computed(() => transformToFlatRows(sessionHistory.value, exerciseCollection.value))

async function fetch() {
  await fetchSessionHistory()
}

async function loadExerciseCollection() {
  try {
    const collection = await api.getExerciseCollection()
    exerciseCollection.value = collection
  } catch (err) {
    console.error('Failed to load exercise collection', err)
  }
}

watch(selectedSegment, (val) => {
  if (val === 'sets') fetch()
})
function openSessionMenu(idx: number) {
  openMenuIndex.value = idx
}

function closeOverflowMenu() {
  openMenuIndex.value = null
}

function editSession() {
  if (openMenuIndex.value === null) return
  const id = sessionHistory.value[openMenuIndex.value].id
  closeOverflowMenu()
  router.push(`/session/${id}`)
}

async function deleteSession() {
  if (openMenuIndex.value === null) return
  const id = sessionHistory.value[openMenuIndex.value].id
  if (!confirm('Delete this session?')) return

  try {
    const resp = await api.sessions.deleteSession(id)
    if (!resp.isSuccess) {
      alert(resp.error ?? 'Failed to delete session')
      return
    }

    // remove deleted session from local history
    if (openMenuIndex.value !== null) {
      sessionHistory.value.splice(openMenuIndex.value, 1)
    }
    closeOverflowMenu()
  } catch (err) {
    console.error('deleteSession error', err)
    alert('Failed to delete session')
  }
}

onMounted(() => { fetch(); loadExerciseCollection() })
onBeforeRouteUpdate((to, from, next) => {
  fetch().then(() => next())
})
</script>

<style scoped>
.history-view { display: flex; flex-direction: column; gap: var(--trk-space-4); }
.view-header { display: flex; flex-direction: column; gap: var(--trk-space-3); }
.view-header .segment-toggle { align-self: stretch; max-width: 640px; margin-top: var(--trk-space-2); }
.item-meta { font-size: 0.875rem; color: var(--trk-text-muted); }
.item-extra { font-weight: 700; color: var(--trk-text); }

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

.history-table { margin-top: var(--trk-space-4); }
</style>
