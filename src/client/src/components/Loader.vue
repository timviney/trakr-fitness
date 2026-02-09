<template>
    <div v-if="loading" class="loading-state">
        <Dumbbell class="loading-spinner" />
        <span class="loading-text">loading</span>
    </div>
</template>

<script setup lang="ts">
import { Dumbbell } from 'lucide-vue-next'
const props = defineProps<{loading: boolean}>()
</script>

<style scoped>
.loading-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: var(--trk-space-3);
    padding: var(--trk-space-8);
    color: var(--trk-text-muted);
}

.loading-state span {
    display: inline-block;
    min-width: 10ch;
    text-align: left;
    padding-left: 4ch;
}

.loading-spinner {
    width: 32px;
    height: 32px;
    animation: spin 2s linear infinite;
    color: var(--trk-accent);
}

.loading-text::after{
    content: '...';
    animation: dots 1.5s linear infinite;
}

@keyframes spin {
    to {
        transform: rotate(360deg);
    }
}

@keyframes dots {
  0%, 20% { content: '.'; }
  40% { content: '..'; }
  80% { content: '...'; }
}
</style>