<template>
  <!-- teleported to body to avoid stacking context issues and ensure full viewport coverage -->
  <Teleport to="body">
    <transition name="modal-fade" appear>
      <div
        v-if="store.open"
        class="modal-overlay confirm-overlay"
        @click.self="cancel"
        role="dialog"
        aria-modal="true"
        aria-labelledby="confirm-title"
        aria-describedby="confirm-message"
      >
        <div class="modal confirm-modal" @click.stop>
          <div class="modal-content">
            <p id="confirm-message" class="confirm-message">{{ store.message }}</p>
          </div>
          <div class="modal-actions">
            <button type="button" class="btn btn-secondary" @click="cancel">
              Cancel
            </button>
            <button
              type="button"
              class="btn btn-primary"
              @click="ok"
              ref="confirmBtn"
            >
              Yes
            </button>
          </div>
        </div>
      </div>
    </transition>
  </Teleport>
</template>

<script setup lang="ts">
import { Teleport, onMounted, onBeforeUnmount, ref, watch } from 'vue'
import { useConfirmStore } from '../../stores/confirmStore'
const store = useConfirmStore()
const confirmBtn = ref<HTMLElement | null>(null)

function ok() {
  store.answer(true)
}
function cancel() {
  store.answer(false)
}

function handleKey(e: KeyboardEvent) {
  if (e.key === 'Escape' && store.open) {
    cancel()
  }
}

onMounted(() => {
  document.addEventListener('keydown', handleKey)
})
onBeforeUnmount(() => {
  document.removeEventListener('keydown', handleKey)
})

// focus the primary action when the dialog opens
watch(
  () => store.open,
  open => {
    if (open) {
      // next tick to ensure DOM rendered
      setTimeout(() => confirmBtn.value?.focus(), 0)
    }
  }
)

</script>

<style scoped>
/* leverage the shared modal layout but tweak for confirmation styling */
.confirm-modal {
  text-align: center;
  padding: 3rem 1.25rem 2rem;
  max-width: 360px;
  width: 90%;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.25);
  display: flex;
  flex-direction: column;
  gap: var(--trk-space-4);
  border-radius: var(--trk-radius-lg);
}

.confirm-message {
  color: var(--trk-text);
  font-size: 1rem;
  line-height: 1.4;
}

/* center the actions */
.modal-actions {
  justify-content: center;
}

.confirm-overlay {
  /* center rather than bottom-sheet */
  align-items: center;
}

/* fade / scale animation for modal entrance */
.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: opacity 200ms ease, transform 200ms ease;
}
.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
  transform: translateY(12px) scale(0.98);
}
</style>
