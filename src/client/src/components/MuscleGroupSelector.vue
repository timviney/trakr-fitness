<template>
  <div class="cgs" tabindex="0" :aria-disabled="disabled">
    <div class="desktop-grid">
      <aside class="left">
        <ul class="list">
          <li
            v-for="(c, idx) in filteredCategories"
            :key="c.id"
            :class="{ active: c.id === localValue.categoryId }"
            @click="selectCategory(c)"
            :data-idx="idx"
          >
            {{ c.name }}
          </li>
        </ul>
      </aside>

      <section class="right">
        <div v-if="!localValue.categoryId" class="empty-note">Select a category</div>
        <ul v-else class="list">
          <li
            v-for="(g, idx) in filteredGroupsForSelected"
            :key="g.id"
            :class="{ active: g.id === localValue.groupId }"
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
import { ref, computed, watch } from 'vue'
import { MuscleCategory, MuscleGroup } from '../api/modules/muscles';


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

const localValue = ref({
  categoryId: props.modelValue?.categoryId ?? '',
  groupId: props.modelValue?.groupId ?? '',
})

watch(
  () => props.modelValue,
  (v) => {
    if (!v) return
    localValue.value = { categoryId: v.categoryId ?? '', groupId: v.groupId ?? '' }
  },
  { immediate: true, deep: true }
)

watch(
  localValue,
  (v) => emit('update:modelValue', { categoryId: v.categoryId, groupId: v.groupId }),
  { deep: true }
)

const filteredCategories = computed(() => {
  if (!props.categories) return []
  return props.categories
})

const filteredGroupsForSelected = computed(() => {
  if (!props.groups) return []
  let groups = props.groups
  if (localValue.value.categoryId) {
    groups = groups.filter((g) => String(g.categoryId) === String(localValue.value.categoryId))
  }
  return groups
})

function selectCategory(c: MuscleCategory) {
  if (disabled) return
  localValue.value.categoryId = c.id
  localValue.value.groupId = ''
}
function selectGroup(g: MuscleGroup) {
  if (disabled) return
  localValue.value.groupId = g.id
}

</script>

<style scoped>
.cgs { outline: none; }
.title { margin: 0 0 8px 0; font-size: 1rem; font-weight: 600; }
.desktop-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
.left { min-width: 160px; }
.list { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; gap: 6px; }
.list li { padding: 8px; border-radius: 6px; cursor: pointer; }
.list li.active { background: var(--trk-accent); color: var(--trk-bg); }
.empty-note { color: var(--trk-text-muted); padding: 8px; }
</style>
