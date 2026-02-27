import { defineStore } from 'pinia'
import { nanoid } from 'nanoid'

export type ToastType = 'success' | 'info' | 'warning' | 'error'

export interface ToastItem {
  id: string
  message: string
  type: ToastType
  duration: number // ms
  action?: { text: string; handler: () => void }
}

export const useToastStore = defineStore('toast', {
  state: () => ({
    toasts: [] as ToastItem[]
  }),
  actions: {
    push(toast: Omit<ToastItem, 'id'>) {
      const item: ToastItem = { id: nanoid(), ...toast }
      this.toasts.unshift(item)
      setTimeout(() => this.remove(item.id), toast.duration)
      return item.id
    },
    remove(id: string) {
      this.toasts = this.toasts.filter(t => t.id !== id)
    },
    clear() {
      this.toasts = []
    }
  }
})

// convenience helpers
export function useToast() {
  const store = useToastStore()
  return {
    success: (msg: string, opts: Partial<Pick<ToastItem, 'duration' | 'action'>> = {}) =>
      store.push({ type: 'success', message: msg, duration: 3000, ...opts }),
    info: (msg: string, opts: Partial<Pick<ToastItem, 'duration' | 'action'>> = {}) =>
      store.push({ type: 'info', message: msg, duration: 3000, ...opts }),
    warning: (msg: string, opts: Partial<Pick<ToastItem, 'duration' | 'action'>> = {}) =>
      store.push({ type: 'warning', message: msg, duration: 4000, ...opts }),
    error: (msg: string, opts: Partial<Pick<ToastItem, 'duration' | 'action'>> = {}) =>
      store.push({ type: 'error', message: msg, duration: 5000, ...opts })
  }
}
