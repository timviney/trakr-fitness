import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import './styles.css'
import { setAuthTokenGetter } from './api/client'
import { useAuthStore } from './stores/auth'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)

// Setup auth token getter for API client
setAuthTokenGetter(() => {
  const authStore = useAuthStore()
  return authStore.token
})

app.mount('#app')
