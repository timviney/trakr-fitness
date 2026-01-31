<template>
  <div class="hello">
    <div class="animation">
      <img :src="currentFrameSrc" alt="animation" class="animation-img" />
    </div>

    <h1>Welcome to Trakr Fitness!</h1>
    <p>This is the Vue 3 + TypeScript starter template.</p>

    <div class="controls">
      <button @click="togglePlay">{{ isPlaying ? 'Pause' : 'Play' }}</button>
      <label class="fps">FPS: <input type="range" min="1" max="5" v-model.number="fps" /></label>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount, watch } from 'vue'
import frame1 from '@/assets/animation/ohp_1.svg'
import frame2 from '@/assets/animation/ohp_2.svg'
import frame3 from '@/assets/animation/ohp_3.svg'
import frame4 from '@/assets/animation/ohp_4.svg'
import frame5 from '@/assets/animation/ohp_5.svg'
import frame6 from '@/assets/animation/ohp_6.svg'
import frame7 from '@/assets/animation/ohp_7.svg'

const frames = [frame1, frame2, frame3, frame4, frame5, frame6, frame7]
const fps = ref(5)
const isPlaying = ref(true)
const current = ref(0)
let timer: number | undefined

const start = () => {
  stop()
  timer = window.setInterval(() => {
    current.value = (current.value + 1) % frames.length
  }, 1000 / Math.max(1, fps.value))
}

const stop = () => {
  if (timer !== undefined) {
    clearInterval(timer)
    timer = undefined
  }
}

const togglePlay = () => {
  isPlaying.value = !isPlaying.value
  if (isPlaying.value) start()
  else stop()
}

const currentFrameSrc = computed(() => frames[current.value])

watch(fps, () => {
  if (isPlaying.value) start()
})

onMounted(() => start())
onBeforeUnmount(() => stop())
</script>

<style scoped>
.hello {
  text-align: center;
}

.animation {
  width: 200px;
  height: 200px;
  margin: 0 auto 1rem;
}

.animation-img {
  width: 100%;
  height: 100%;
  object-fit: contain;
  display: block;
}

.controls {
  display: flex;
  gap: 1rem;
  justify-content: center;
  margin-top: 1rem;
  align-items: center;
}

.controls .fps input { vertical-align: middle }
</style>