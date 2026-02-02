<template>
  <div class="auth-shell">
    <div class="auth-card">
      <div class="auth-brand">
        <div class="auth-logo">
          <img :src="logoUrl" alt="Trakr Fitness logo" />
        </div>
        <div>
          <h1 class="auth-title">Trakr Fitness</h1>
          <p class="auth-subtitle">Please log in to continue</p>
        </div>
      </div>

      <form @submit.prevent="onSubmit">
        <label class="form-field">
          <span>Email</span>
          <input v-model.trim="email" type="email" placeholder="you@example.com" required/>
        </label>

        <label class="form-field">
          <span>Password</span>
          <input v-model="password" type="password" placeholder="Your password" required/>
        </label>

        <button type="submit" class="btn btn-primary btn-login" :disabled="isSubmitting">
          {{ isSubmitting ? 'Logging in...' : 'Log In' }}
        </button>
      </form>

      <p class="form-hint">
        Don’t have an account?
        <a href="/register">Sign up here</a>
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const logoUrl = new URL('../assets/logo.svg', import.meta.url).href

const email = ref('')
const password = ref('')
const isSubmitting = ref(false)

const onSubmit = async () => {
  if (isSubmitting.value) return
  isSubmitting.value = true

  // TODO: replace with real API call
  await new Promise((r) => setTimeout(r, 600))
  isSubmitting.value = false
}
</script>

<style scoped>
.auth-shell {
  min-height: 100dvh;
  box-sizing: border-box;
  display: grid;
  place-items: center;
  padding: var(--trk-space-5);
  background: var(--trk-bg);
}

.auth-card {
  width: min(90vw, 420px);
  background: var(--trk-surface);
  border: 1px solid var(--trk-surface-border);
  border-radius: var(--trk-radius-lg);
  padding: var(--trk-space-6);
  box-shadow: var(--trk-shadow);
  position: relative;
}

.auth-brand {
  display: grid;
  grid-template-columns: 56px 1fr 56px;
  gap: var(--trk-space-3);
  align-items: center;
  margin-bottom: var(--trk-space-8);
}

.auth-logo {
  width: 56px;
  height: 56px;
  display: grid;
  place-items: center;
}

/* Larger screens */
@media (min-width: 640px) {
  .auth-shell {
    padding: var(--trk-space-8);
  }

  .auth-card {
    width: min(90%, 420px);
    padding: var(--trk-space-8);
  }

  .auth-brand {
    grid-template-columns: 72px 1fr 72px;
    gap: var(--trk-space-4);
    margin-bottom: var(--trk-space-6);
  }

  .auth-logo {
    width: 72px;
    height: 72px;
  }
}
.auth-brand::after {
  content: '';
}

.auth-card::before {
  content: '';
  position: absolute;
  inset: 6px;
  border: 1px solid var(--trk-accent-muted);
  border-radius: calc(var(--trk-radius-lg) - 4px);
  pointer-events: none;
  opacity: 0.6;
}

.auth-logo img {
  width: 100%;
  height: 100%;
  object-fit: contain;
  display: block;
}

.auth-title {
  text-align: center;
  margin: 0;
}

.auth-subtitle {
  text-align: center;
  margin-top: 0.25rem;
  margin-bottom: 0;
  color: var(--trk-text-muted);
}

.btn-login {
  width: 100%;
  text-transform: uppercase;
}
</style>