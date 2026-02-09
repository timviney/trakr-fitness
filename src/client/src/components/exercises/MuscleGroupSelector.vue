<template>
  <div class="cgs" tabindex="0" :aria-disabled="disabled">
    <div class="desktop-grid">
      <aside class="left">
        <ul class="list">
          <li
            v-for="(c, idx) in filteredCategories"
            :key="c.id"
            :class="{ active: c.id === vModel.categoryId }"
            @click="selectCategory(c)"
            :data-idx="idx"
          >
            {{ c.name }}
          </li>
        </ul>
      </aside>

      <section class="right">
        <div v-if="!vModel.categoryId" class="empty-note">Select a category</div>
        <ul v-else class="list">
          <li
            v-for="(g, idx) in filteredGroupsForSelected"
            :key="g.id"
            :class="{ active: g.id === vModel.groupId }"
            @click="selectGroup(g)"
            :data-idx="idx"
          >
            {{ g.name }}
          </li>
        </ul>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { MuscleCategory, MuscleGroup } from '../../api/modules/muscles';


const props = defineProps<{
  categories?: MuscleCategory[]
  groups?: MuscleGroup[]
  modelValue?: { categoryId: string; groupId: string }
  disabled?: boolean
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: { categoryId: string; groupId: string }): void
}>()

const disabled = !!props.disabled

const vModel = computed({
  get: () => props.modelValue ?? { categoryId: '', groupId: '' },
  set: (v: { categoryId: string; groupId: string }) => emit('update:modelValue', v),
})

const filteredCategories = computed(() => {
  if (!props.categories) return []
  return props.categories
})

const filteredGroupsForSelected = computed(() => {
  if (!props.groups) return []
  let groups = props.groups
  if (vModel.value.categoryId) {
    groups = groups.filter((g) => String(g.categoryId) === String(vModel.value.categoryId))
  }
  return groups
})

function selectCategory(c: MuscleCategory) {
  if (disabled) return
  vModel.value = { categoryId: c.id, groupId: '' }
}
function selectGroup(g: MuscleGroup) {
  if (disabled) return
  vModel.value = { categoryId: vModel.value.categoryId, groupId: g.id }
}

</script>

<style scoped>
.cgs { outline: none; }
.title { margin: 0 0 8px 0; font-size: 1rem; font-weight: 600; }
.desktop-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
.left { min-width: 160px; }
.list { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; gap: 6px; }
.list li { padding: 8px; border-radius: 6px; cursor: pointer; }
.list li.active { background: var(--trk-accent); color: var(--trk-text-dark); }
.empty-note { color: var(--trk-text-muted); padding: 8px; }
</style>
