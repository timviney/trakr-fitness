<template>
  <AuthLayout 
    subtitle="Create your account" 
    :error-message="errorMessage"
    :is-submitting="isSubmitting"
    button-text="Sign Up"
    loading-text="Creating account..."
    :on-submit="onSubmit"
  >
    <template #fields>
      <div v-if="successMessage" class="success-message">
        {{ successMessage }}
      </div>

      <label class="form-field">
        <span>Email</span>
        <input v-model.trim="email" type="email" placeholder="you@example.com" required/>
      </label>

      <label class="form-field">
        <span>Password</span>
        <input v-model="password" type="password" placeholder="Your password" required/>
      </label>

      <label class="form-field">
        <span>Confirm Password</span>
        <input v-model="confirmPassword" type="password" placeholder="Confirm your password" required/>
      </label>
    </template>

    <template #hint>
      <p class="form-hint">
        Already have an account?
        <router-link to="/login">Log in here</router-link>
      </p>
    </template>
  </AuthLayout>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { api } from '../api/api'
import AuthLayout from './AuthLayout.vue'

const router = useRouter()

const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const isSubmitting = ref(false)
const errorMessage = ref('')
const successMessage = ref('')

const onSubmit = async () => {
  if (isSubmitting.value) return
  
  errorMessage.value = ''
  successMessage.value = ''

  if (password.value !== confirmPassword.value) {
    errorMessage.value = 'Passwords do not match.'
    return
  }

  isSubmitting.value = true

  try {
    const response = await api.auth.register({
      email: email.value,
      password: password.value
    })

    if (response.isSuccess) {
      successMessage.value = '🎉 Account created successfully! Redirecting to login in 3 seconds...'
      
      // Disable form during success state
      isSubmitting.value = true
      
      // Redirect after 3 seconds
      setTimeout(() => {
        router.push('/login')
      }, 3000)
    } else {
      if (response.error === 'EmailTaken') {
        errorMessage.value = 'This email is already registered.'
      } else if (response.error === 'InvalidEmail') {
        errorMessage.value = 'Please enter a valid email address.'
      } else if (response.error === 'WeakPassword') {
        errorMessage.value = 'Password is too weak. Please use a stronger password.'
      } else {
        errorMessage.value = 'Registration failed. Please try again.'
      }
    }
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Registration failed.'
    isSubmitting.value = false
  }
}
</script>

<style scoped>
.success-message {
  background-color: var(--trk-success-bg);
  border: 1px solid var(--trk-success-border);
  color: var(--trk-success-text);
  padding: 0.75rem 1rem;
  border-radius: var(--trk-radius-md);
  margin-bottom: var(--trk-space-4);
  text-align: center;
  font-weight: 500;
  animation: slideIn 0.3s ease-out;
}

@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
