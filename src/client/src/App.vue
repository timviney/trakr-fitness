<template>
  <div id="app">
    <router-view />
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from './stores/auth'

const authStore = useAuthStore()
const router = useRouter()

onMounted(async () => {
  if (authStore.isTokenExpired && authStore.refreshToken) {
    const refreshed = await authStore.refreshAuth()
    if (refreshed && router.currentRoute.value.name === 'Login') {
      router.push({ name: 'Stats' })
    }
  }
})
</script>

<style>
/* Global styles are in styles.css */
</style>