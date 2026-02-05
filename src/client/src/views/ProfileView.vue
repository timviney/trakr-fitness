<template>
  <AppShell>
    <div class="profile-view">
      <header class="view-header">
        <h1 class="view-title">Profile</h1>
      </header>

      <div class="profile-card">
        <div class="avatar">
          <User class="avatar-icon" />
        </div>
        <div class="user-info">
          <span class="user-email">{{ userEmail }}</span>
        </div>
      </div>

      <div class="actions">
        <button class="btn btn-logout" @click="handleLogout">
          <LogOut class="btn-icon" />
          Sign Out
        </button>
      </div>

      <div class="app-info">
        <p class="app-version">Trakr.Fitness v0.1.0</p>
      </div>
    </div>
  </AppShell>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { User, LogOut } from 'lucide-vue-next'
import AppShell from '../components/AppShell.vue'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const userEmail = computed(() => authStore.email || 'User')

async function handleLogout() {
  authStore.logout()
  router.push('/login')
}
</script>

<style scoped>
.profile-view {
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

.profile-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--trk-space-4);
  padding: var(--trk-space-6);
  background: var(--trk-surface);
  border-radius: var(--trk-radius-lg);
  border: 1px solid var(--trk-surface-border);
}

.avatar {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  background: var(--trk-surface-border);
  display: flex;
  align-items: center;
  justify-content: center;
}

.avatar-icon {
  width: 40px;
  height: 40px;
  color: var(--trk-text-muted);
}

.user-info {
  text-align: center;
}

.user-email {
  color: var(--trk-text);
  font-size: 1rem;
  font-weight: 500;
}

.actions {
  display: flex;
  flex-direction: column;
  gap: var(--trk-space-3);
}

.btn-logout {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--trk-space-2);
  width: 100%;
  padding: var(--trk-space-4);
  background: transparent;
  border: 1px solid var(--trk-surface-border);
  border-radius: var(--trk-radius-md);
  color: var(--trk-text);
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 150ms ease;
}

.btn-logout:hover {
  background: var(--trk-surface);
  border-color: #ef4444;
  color: #ef4444;
}

.btn-icon {
  width: 20px;
  height: 20px;
}

.app-info {
  text-align: center;
  margin-top: auto;
  padding-top: var(--trk-space-4);
}

.app-version {
  color: var(--trk-text-muted);
  font-size: 0.75rem;
  margin: 0;
}
</style>
