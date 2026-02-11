<template>
    <div class="section-toggle" role="button" tabindex="0" :aria-expanded="modelValue" @click="toggle"
        @keydown="onKeydown">
        <span class="section-title">
            <slot />
        </span>
        <ChevronDown class="section-icon" :class="{ rotated: modelValue }" aria-hidden="true" />
    </div>
</template>

<script setup lang="ts">
import { ChevronDown } from 'lucide-vue-next'

const props = defineProps({
    modelValue: { type: Boolean, required: true },
})
const emit = defineEmits(['update:modelValue'])

function toggle() {
    emit('update:modelValue', !props.modelValue)
}

function onKeydown(e: KeyboardEvent) {
    if (e.key === 'Enter' || e.key === ' ') {
        e.preventDefault()
        toggle()
    }
}
</script>

<style scoped>
.section-toggle {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: var(--trk-space-3);
    padding: calc(var(--trk-space-2) - 2px) calc(var(--trk-space-3) - 2px);
    background: transparent;
    border-radius: var(--trk-radius-sm);
    cursor: pointer;
}

.section-toggle .section-title {
    font-weight: 600;
}

.section-toggle .section-icon {
    width: 20px;
    height: 20px;
    color: var(--trk-text-muted);
    flex-shrink: 0;
    transition: transform 180ms cubic-bezier(.2, .9, .2, 1);
    transform-origin: center;
}

.section-toggle .section-icon.rotated {
    transform: rotate(180deg);
}

@media (prefers-reduced-motion: reduce) {
    .section-toggle .section-icon {
        transition: none;
    }
}
</style>
