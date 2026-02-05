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
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import AuthLayout from './AuthLayout.vue'

const router = useRouter()
const authStore = useAuthStore()

const email = ref('')
const password = ref('')
const isSubmitting = ref(false)
const errorMessage = ref('')

const onSubmit = async () => {
  if (isSubmitting.value) return
  isSubmitting.value = true
  errorMessage.value = ''

  try {
    await authStore.login({
      email: email.value,
      password: password.value
    })
    
    // Redirect to home or previous page on success
    const redirect = router.currentRoute.value.query.redirect as string
    router.push(redirect || '/')
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Login failed.'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<style scoped>

</style>
