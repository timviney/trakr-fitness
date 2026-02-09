<template>
  <div v-if="message" class="error-state">
    <p>{{ message }}</p>
    <button class="btn btn-primary" @click="$emit('retry')">Retry</button>
  </div>
</template>

<script setup lang="ts">
const props = defineProps<{ message: string | null }>()
const emit = defineEmits<{
  (e: 'retry'): void
}>()
</script>

<style scoped>
.error-state {
  text-align: center;
  padding: var(--trk-space-6);
  color: var(--trk-text-muted);
}
</style>
