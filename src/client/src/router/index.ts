import { createRouter, createWebHistory } from 'vue-router'
import Login from '../components/auth/Login.vue'
import Register from '../components/auth/Register.vue'
import StatsView from '../views/StatsView.vue'
import ExercisesView from '../views/ExercisesView.vue'
import ProfileView from '../views/ProfileView.vue'
import NewSessionView from '../views/NewSessionView.vue'
import { useAuthStore } from '../stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'Home',
      redirect: '/stats'
    },
    {
      path: '/login',
      name: 'Login',
      component: Login,
      meta: { requiresAuth: false }
    },
    {
      path: '/register',
      name: 'Register',
      component: Register,
      meta: { requiresAuth: false }
    },
    {
      path: '/stats',
      name: 'Stats',
      component: StatsView
    },
    {
      path: '/exercises',
      name: 'Exercises',
      component: ExercisesView
    },
    {
      path: '/profile',
      name: 'Profile',
      component: ProfileView
    },
    {
      path: '/session/new',
      name: 'NewSession',
      component: NewSessionView
    }
  ]
})

// Navigation guard to protect routes
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  const requiresAuth = to.meta.requiresAuth !== false // Default to true unless explicitly false

  if (requiresAuth && !authStore.isAuthenticated) {
    next({ name: 'Login', query: { redirect: to.fullPath } })
  } else if ((to.name === 'Login' || to.name === 'Register') && authStore.isAuthenticated) {
    // Redirect authenticated users away from auth pages to the main app
    next({ name: 'Stats' })
  } else {
    next()
  }
})

export default router
