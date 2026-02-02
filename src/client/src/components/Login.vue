<template>
  <div class="login">
    <div class="card">
      <div class="brand">
        <div class="logo">
          <img :src="logoUrl" alt="Trakr Fitness logo" />
        </div>
        <div>
          <h1 class="title">Trakr Fitness</h1>
          <p class="subtitle">Please log in to continue</p>
        </div>
      </div>

      <form @submit.prevent="onSubmit">
        <label class="field">
          <span>Email</span>
          <input v-model.trim="email" type="email" placeholder="you@example.com" required/>
        </label>

        <label class="field">
          <span>Password</span>
          <input v-model="password" type="password" placeholder="Your password" required/>
        </label>

        <button type="submit" class="primary" :disabled="isSubmitting">
          {{ isSubmitting ? 'Logging in...' : 'Log In' }}
        </button>
      </form>

      <p class="hint">
        Don’t have an account?
        <a href="/register">Sign up here</a>.
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
.login {
  min-height: 100dvh;
  box-sizing: border-box;
  display: grid;
  place-items: center;
  padding: 2rem;
  background:
    radial-gradient(600px 300px at 20% 10%, rgba(250, 204, 21, 0.08), transparent 60%),
    #0b0f14;
  color: #e5e7eb;
}

.card {
  width: 80%;
  max-width: 420px;
  background: #0f141b;
  border: 1px solid #2b323b;
  border-radius: 14px;
  padding: 2rem;
  box-shadow: 0 18px 40px rgba(0, 0, 0, 0.55);
  position: relative;
}

.card::before {
  content: '';
  position: absolute;
  inset: 3px;
  border: 1px solid #a08206;
  border-radius: 10px;
  pointer-events: none;
}

.brand {
  display: grid;
  grid-template-columns: 52px 1fr 52px;
  gap: 1rem;
  align-items: center;
  margin-bottom: 1.5rem;
}

.logo {
  width: 50px;
  height: 50px;
  display: grid;
  place-items: center;
}

.logo img {
  width: 100%;
  height: 100%;
  object-fit: contain;
  display: block;
}

.title {
  text-align: center;
}

.subtitle {
  text-align: center;
  margin-top: 0.25rem;
  margin-bottom: 0;
  color: #9ca3af;
}

.field {
  display: grid;
  gap: 0.35rem;
  margin-bottom: 1rem;
  text-align: left;
}

.field span {
  color: #cbd5e1;
  font-size: 0.9rem;
}

.field input {
  padding: 0.7rem 0.8rem;
  border-radius: 10px;
  border: 1px solid #2b323b;
  background: #0b1016;
  color: #e5e7eb;
  outline: none;
}

.field input:focus {
  border-color: #facc15;
  box-shadow: 0 0 0 3px rgba(250, 204, 21, 0.15);
}

.primary {
  width: 100%;
  padding: 0.75rem 0.85rem;
  border-radius: 10px;
  border: none;
  background: #facc15;
  color: #0b0f14;
  font-weight: 800;
  letter-spacing: 0.02em;
  cursor: pointer;
  text-transform: uppercase;
}

.primary:disabled {
  opacity: 0.75;
  cursor: not-allowed;
}

.primary:hover {
  transform: translateY(-1px);
  background: #fde047;
  box-shadow: 0 6px 12px rgba(15, 23, 42, 0.25);
}

.hint {
  margin-top: 1rem;
  color: #9ca3af;
  font-size: 0.9rem;
}

.hint a {
  color: #facc15;
}
</style>