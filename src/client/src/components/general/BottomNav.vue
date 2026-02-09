<template>
  <nav class="bottom-nav">
    <router-link
      v-for="item in navItems"
      :key="item.to"
      :to="item.to"
      class="nav-item"
      :class="{ active: isActive(item.to) }"
    >
      <component :is="item.icon" class="nav-icon" />
      <span class="nav-label">{{ item.label }}</span>
    </router-link>
  </nav>
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router'
import { User, BarChart2, Dumbbell, Play } from 'lucide-vue-next'

const route = useRoute()

const navItems = [
  { to: '/stats', label: 'Stats', icon: BarChart2 },
  { to: '/exercises', label: 'Exercises', icon: Dumbbell },
  { to: '/session/new', label: 'Record', icon: Play },
  { to: '/profile', label: 'Profile', icon: User },
]

function isActive(path: string): boolean {
  return route.path === path || route.path.startsWith(path + '/')
}
</script>

<style scoped>
.bottom-nav {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  display: flex;
  justify-content: space-around;
  align-items: center;
  height: 56px;
  padding-bottom: env(safe-area-inset-bottom, 0);
  background: var(--trk-surface);
  border-top: 1px solid var(--trk-surface-border);
  z-index: 100;
}

.nav-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 4px;
  flex: 1;
  height: 100%;
  color: var(--trk-text-muted);
  text-decoration: none;
  transition: color 150ms ease;
  -webkit-tap-highlight-color: transparent;
}

.nav-item:hover,
.nav-item:focus {
  color: var(--trk-text);
}

.nav-item.active {
  color: var(--trk-accent);
}

.nav-icon {
  width: 24px;
  height: 24px;
}

.nav-label {
  font-size: 0.6875rem;
  font-weight: 500;
  letter-spacing: 0.02em;
}
</style>
