import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import './styles.css'
import './styles/shared-components.css'
import { setAccessTokenGetter, setAuthFailureHandler, setRefreshHandler } from './api/client'
import { useAuthStore } from './stores/auth'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)

// Initialise auth store synchronously from localStorage before router installs
const authStore = useAuthStore()
authStore.initialise()

app.use(router)

// Setup auth token getter for API client
setAccessTokenGetter(() => {
  return authStore.token
})

setRefreshHandler(async () => {
  return await authStore.refreshAuth()
})

setAuthFailureHandler(() => {
  authStore.forceLogout('expired')
  router.push({ name: 'Login', query: { expired: '1' } })
})

app.mount('#app')
