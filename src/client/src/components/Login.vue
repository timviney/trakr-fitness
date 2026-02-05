<template>
  <AuthLayout 
    subtitle="Please log in to continue" 
    :error-message="errorMessage" 
    :is-submitting="isSubmitting"
    button-text="Log In"
    loading-text="Logging in..."
    :on-submit="onSubmit"
  >
    <template #fields>
      <label class="form-field">
        <span>Email</span>
        <input v-model.trim="email" type="email" placeholder="you@example.com" required/>
      </label>

      <label class="form-field">
        <span>Password</span>
        <input v-model="password" type="password" placeholder="Your password" required/>
      </label>
    </template>

    <template #hint>
      <p class="form-hint">
        Don't have an account?
        <router-link to="/register">Sign up here</router-link>
      </p>
    </template>
  </AuthLayout>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { api } from '../api/api'
import AuthLayout from './AuthLayout.vue'

const email = ref('')
const password = ref('')
const isSubmitting = ref(false)
const errorMessage = ref('')

const onSubmit = async () => {
  if (isSubmitting.value) return
  isSubmitting.value = true
  errorMessage.value = ''

  try {
    await api.auth.login({
      email: email.value,
      password: password.value
    })
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Login failed.'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<style scoped>

</style>
