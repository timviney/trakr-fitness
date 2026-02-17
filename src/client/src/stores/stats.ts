import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api } from '../api/api'
import type { SessionHistoryItemResponse } from '../api/modules/sessions'

export const useStatsStore = defineStore('stats', () => {
  const sessionHistory = ref<SessionHistoryItemResponse[]>([])
  const historyLoading = ref<boolean>(false)
  const historyError = ref<string | null>(null)

  async function fetchSessionHistory() {
    historyLoading.value = true
    historyError.value = null

    try {
      const resp = await api.sessions.getSessionsHistory()
      if (!resp.isSuccess) {
        historyError.value = resp.error ?? 'Failed to load session history'
        sessionHistory.value = []
      } else {
        sessionHistory.value = resp.data ?? []
      }
      return resp
    } catch (err) {
      console.error('fetchSessionHistory error', err)
      historyError.value = 'Failed to load session history'
      sessionHistory.value = []
      return { isSuccess: false, error: historyError.value }
    } finally {
      historyLoading.value = false
    }
  }

  function clearHistory() {
    sessionHistory.value = []
    historyError.value = null
  }

  return {
    sessionHistory,
    historyLoading,
    historyError,
    fetchSessionHistory,
    clearHistory
  }
})
