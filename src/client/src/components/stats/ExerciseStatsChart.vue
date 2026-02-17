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

  // read theme colours from CSS variables so chart matches the app theme
  const rootStyles = getComputedStyle(document.documentElement)
  const axisColor = (rootStyles.getPropertyValue('--trk-text-muted') || '#e5e7eb').trim()
  const gridColor = (rootStyles.getPropertyValue('--trk-surface-border') || '#9ca3af').trim()
  const accentColor = (rootStyles.getPropertyValue('--trk-accent') || '#facc15').trim()

  chart = Highcharts.chart(chartEl.value, {
    chart: { type: 'line', backgroundColor: 'transparent' },
    title: { text: '' },
    xAxis: {
      type: 'datetime',
      labels: { style: { color: axisColor } },
      lineColor: axisColor,
      tickColor: axisColor,
      gridLineColor: gridColor
    },
    yAxis: {
    //   title: { text: props.metricLabel, style: { color: axisColor } },
      title: { text: '', style: { color: axisColor } },
      labels: { style: { color: axisColor } },
      gridLineColor: gridColor,
      lineColor: axisColor,
      tickColor: axisColor
    },
    legend: { enabled: false },
    credits: { enabled: false },
    tooltip: { xDateFormat: '%b %e, %Y', pointFormat: '<b>{point.y}</b>' },
    plotOptions: { line: { marker: { enabled: true, radius: 3 }, turboThreshold: 0 } },
    series: [
      { name: props.metricLabel, data: props.seriesData ?? [], color: accentColor }
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
  if (chart.series && chart.series[0]) {
    // bypass strict typings for series update (some TS defs require `type` in union options)
    (chart.series[0] as any).update({ name: label } as any, false)
  }
  chart.redraw()
})

onBeforeUnmount(() => { if (chart) { chart.destroy(); chart = null } })
</script>

<style scoped>
.exercise-stats-chart { min-height: 260px; position: relative; }
.hc-chart { width: 100%; height: clamp(300px, 60vh, 720px); }
.empty-chart { position: absolute; inset: 0; display: flex; align-items: center; justify-content: center; color: var(--trk-text-muted); }
</style>
