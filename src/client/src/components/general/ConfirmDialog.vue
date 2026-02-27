<template>
  <div v-if="store.open" class="confirm-backdrop" @click.self="cancel">
    <div class="confirm-modal">
      <p class="confirm-message">{{ store.message }}</p>
      <div class="confirm-actions">
        <button class="btn btn-secondary" @click="cancel">No</button>
        <button class="btn btn-danger" @click="ok">Yes</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useConfirmStore } from '../../stores/confirmStore'
const store = useConfirmStore()

function ok()   { store.answer(true) }
function cancel(){ store.answer(false) }
</script>

<style scoped>
.confirm-backdrop {
  position: fixed; inset:0;
  background: rgba(0,0,0,0.4);
  display: flex; align-items:center; justify-content:center;
  z-index:10000;
}

.confirm-modal {
  background: var(--trk-surface);
  padding: 1.5rem;
  border-radius: var(--trk-radius-md);
  width: 90%;
  max-width: 320px;
  box-shadow: 0 4px 16px rgba(0,0,0,0.2);
}

.confirm-message { margin-bottom: 1rem; color: var(--trk-text); }
.confirm-actions { display:flex; justify-content:flex-end; gap:0.5rem; }
</style>
