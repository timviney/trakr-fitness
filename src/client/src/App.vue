<template>
  <div id="app">
    <Login />
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import Login from './components/Login.vue'

export default defineComponent({
  name: 'App',
  components: { Login }
})
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  padding: 2rem;
}
</style>