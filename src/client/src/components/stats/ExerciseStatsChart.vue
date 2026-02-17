<template>
  <div class="exercise-stats-chart">
    <Loader v-if="loading" :loading="true" />
    <div ref="chartEl" class="hc-chart" role="img" aria-label="Exercise time series chart"></div>
    <div v-if="!loading && (!seriesData || seriesData.length === 0)" class="empty-chart">No data for selected exercise.</div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, onBeforeUnmount } from 'vue'
import type { Ref } from 'vue'
import Loader from '../general/Loader.vue'
import Highcharts from 'highcharts'

const props = defineProps<{
  seriesData: Array<[number, number]> | undefined
  metricLabel: string
  loading?: boolean
}>()

const chartEl: Ref<HTMLDivElement | null> = ref(null)
let chart: Highcharts.Chart | null = null

function createChart() {
  if (!chartEl.value) return

  chart = Highcharts.chart(chartEl.value, {
    chart: { type: 'line', backgroundColor: 'transparent' },
    title: { text: '' },
    xAxis: { type: 'datetime' },
    yAxis: { title: { text: props.metricLabel } },
    legend: { enabled: false },
    credits: { enabled: false },
    tooltip: { xDateFormat: '%b %e, %Y', pointFormat: '<b>{point.y}</b>' },
    plotOptions: { line: { marker: { enabled: true, radius: 3 }, turboThreshold: 0 } },
    series: [
      { name: props.metricLabel, data: props.seriesData ?? [], color: '#facc15' }
    ]
  })
}

onMounted(() => createChart())

watch(() => props.seriesData, (next) => {
  if (!chart || !chart.series || !chart.series[0]) return
  // update series data
  ;(chart.series[0] as Highcharts.Series).setData(next ?? [], true)
})

watch(() => props.metricLabel, (label) => {
  if (!chart) return
  chart.yAxis[0].setTitle({ text: label })
  if (chart.series && chart.series[0]) (chart.series[0] as Highcharts.Series).update({ name: label }, false)
  chart.redraw()
})

onBeforeUnmount(() => { if (chart) { chart.destroy(); chart = null } })
</script>

<style scoped>
.exercise-stats-chart { min-height: 260px; position: relative; }
.hc-chart { width: 100%; height: clamp(300px, 60vh, 720px); }
.empty-chart { position: absolute; inset: 0; display: flex; align-items: center; justify-content: center; color: var(--trk-text-muted); }
</style>
