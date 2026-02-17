<template>
  <div class="segment-toggle">
    <button
      v-for="(label, key) in optionsLabels"
      :key="key"
      class="segment-btn"
      :class="{ active: modelValue === key }"
      @click="$emit('update:modelValue', key)"
    >
      {{ label }}
    </button>
  </div>
</template>

<script setup lang="ts">
const props = defineProps<{
  modelValue: string
  optionsLabels: Record<string, string>
}>()

const emit = defineEmits(['update:modelValue'])
</script>

<style scoped>
.segment-toggle {
  display: flex;
  width: 100%;
  background: var(--trk-surface);
  border-radius: var(--trk-radius-md);
}

.segment-btn {
  flex: 1;
  min-width: 0;
  padding: var(--trk-space-3) var(--trk-space-2);
  border: none;
  border-radius: calc(var(--trk-radius-md) - 2px);
  background: transparent;
  color: var(--trk-text-muted);
  cursor: pointer;
  transition: all 150ms ease;
  font-size: clamp(0.2rem, 3.5vw, 1rem);
}

.segment-btn.active {
  background: var(--trk-accent);
  color: var(--trk-bg);
}
</style>
