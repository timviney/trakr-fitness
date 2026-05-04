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
        <button class="btn btn-reset" @click="toggleResetForm">
          <KeyRound class="btn-icon" />
          Reset Password
        </button>

        <form v-if="showResetForm" class="reset-form" @submit.prevent="handleReset">
          <p v-if="resetError" class="reset-error">{{ resetError }}</p>
          <p v-if="resetSuccess" class="reset-success">{{ resetSuccess }}</p>

          <label class="form-field">
            <span>Current Password</span>
            <input
              v-model="oldPassword"
              type="password"
              placeholder="Enter current password"
              required
              :disabled="isResetting"
            />
          </label>

          <label class="form-field">
            <span>New Password</span>
            <input
              v-model="newPassword"
              type="password"
              placeholder="Enter new password"
              required
              :disabled="isResetting"
            />
          </label>

          <button class="btn btn-primary btn-reset-submit" type="submit" :disabled="isResetting">
            {{ isResetting ? 'Resetting...' : 'Reset' }}
          </button>
        </form>

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
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import { User, LogOut, KeyRound } from 'lucide-vue-next'
import AppShell from '../components/general/AppShell.vue'
import { useAuthStore } from '../stores/auth'
import { api } from '../api/api'
import { ApiErrorMessages } from '../api/api-error'

const router = useRouter()
const authStore = useAuthStore()

const userEmail = computed(() => authStore.email || 'User')

const showResetForm = ref(false)
const oldPassword = ref('')
const newPassword = ref('')
const isResetting = ref(false)
const resetError = ref('')
const resetSuccess = ref('')

function toggleResetForm() {
  showResetForm.value = !showResetForm.value
  oldPassword.value = ''
  newPassword.value = ''
  resetError.value = ''
  resetSuccess.value = ''
}

async function handleReset() {
  resetError.value = ''
  resetSuccess.value = ''

  if (!authStore.email) {
    resetError.value = 'No email found. Please sign in again.'
    return
  }

  isResetting.value = true

  const response = await api.auth.resetPassword({
    email: authStore.email,
    oldPassword: oldPassword.value,
    newPassword: newPassword.value,
  })

  isResetting.value = false

  if (response.isSuccess) {
    resetSuccess.value = 'Password reset successfully.'
    oldPassword.value = ''
    newPassword.value = ''
  } else if (response.error) {
    resetError.value = ApiErrorMessages[response.error] ?? 'Failed to reset password.'
  }
}

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

.btn-reset {
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

.btn-reset:hover {
  background: var(--trk-surface);
  border-color: var(--trk-accent, #3b82f6);
  color: var(--trk-accent, #3b82f6);
}

.reset-form {
  display: flex;
  flex-direction: column;
  padding: var(--trk-space-4);
  background: var(--trk-surface);
  border: 1px solid var(--trk-surface-border);
  border-radius: var(--trk-radius-md);
}

.reset-form :deep(.form-field) {
  margin-bottom: var(--trk-space-3);
}

.reset-error {
  color: var(--trk-danger, #ef4444);
  font-size: 0.875rem;
  margin: 0 0 var(--trk-space-3);
  text-align: center;
}

.reset-success {
  color: #22c55e;
  font-size: 0.875rem;
  margin: 0 0 var(--trk-space-3);
  text-align: center;
}

.btn-reset-submit {
  width: 100%;
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
