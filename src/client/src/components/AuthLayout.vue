<template>
    <div class="auth-shell">
        <div class="auth-card">
            <div class="auth-brand">
                <div class="auth-logo">
                    <img :src="logoUrl" alt="Trakr Fitness logo" />
                </div>
                <div>
                    <h1 class="auth-title">Trakr Fitness</h1>
                    <p class="auth-subtitle">{{ subtitle }}</p>
                </div>
            </div>

            <form @submit.prevent="onSubmit">
                <slot name="fields" />

                <button type="submit" class="btn btn-primary btn-submit" :disabled="isSubmitting">
                    {{ isSubmitting ? loadingText : buttonText }}
                </button>
            </form>

            <p v-if="errorMessage" class="form-error">{{ errorMessage }}</p>

            <slot name="hint" />
        </div>
    </div>
</template>

<script setup lang="ts">
defineProps<{
    subtitle: string
    errorMessage?: string
    isSubmitting?: boolean
    buttonText: string
    loadingText: string
    onSubmit: () => void | Promise<void>
}>()

const logoUrl = new URL('../assets/logo.svg', import.meta.url).href
</script>

<style scoped>
.auth-shell {
    min-height: 100dvh;
    box-sizing: border-box;
    display: grid;
    place-items: center;
    padding: var(--trk-space-5);
    background: var(--trk-bg);
}

.auth-card {
    width: min(86vw, 600px);
    background: var(--trk-surface);
    border: 1px solid var(--trk-surface-border);
    border-radius: var(--trk-radius-lg);
    padding: var(--trk-space-6);
    box-shadow: var(--trk-shadow);
    position: relative;
}

.auth-brand {
    display: grid;
    grid-template-columns: 56px 1fr 56px;
    gap: var(--trk-space-3);
    align-items: center;
    margin-bottom: var(--trk-space-8);
}

.auth-logo {
    width: 56px;
    height: 56px;
    display: grid;
    place-items: center;
}

.auth-subtitle {
    text-align: center;
    margin-top: 0.25rem;
    margin-bottom: 0;
    color: var(--trk-text-muted);
}

/* Larger screens */
@media (min-width: 640px) {
    .auth-brand {
        grid-template-columns: 72px 1fr 72px;
        gap: var(--trk-space-4);
        margin-bottom: var(--trk-space-6);
    }

    .auth-logo {
        width: 72px;
        height: 72px;
    }
}

.auth-brand::after {
    content: '';
}

.auth-card::before {
    content: '';
    position: absolute;
    inset: 6px;
    border: 1px solid var(--trk-accent-muted);
    border-radius: calc(var(--trk-radius-lg) - 4px);
    pointer-events: none;
    opacity: 0.6;
}

.auth-logo img {
    width: 100%;
    height: 100%;
    object-fit: contain;
    display: block;
}

.auth-title {
    text-align: center;
    margin: 0;
}

.form-error {
    margin-top: var(--trk-space-4);
    color: var(--trk-danger, #e5484d);
    text-align: center;
}

.btn-submit {
    width: 100%;
    text-transform: uppercase;
}
</style>
