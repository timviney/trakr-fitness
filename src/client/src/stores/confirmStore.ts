import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useConfirmStore = defineStore('confirm', () => {
  const open = ref(false)
  const message = ref('')
  let resolver: ((value: boolean) => void) | null = null

  function request(msg: string): Promise<boolean> {
    message.value = msg
    open.value = true
    return new Promise<boolean>(resolve => {
      resolver = resolve
    })
  }

  function answer(choice: boolean) {
    open.value = false
    resolver?.(choice)
    resolver = null
  }

  return { open, message, request, answer }
})

export function useConfirm() {
  const store = useConfirmStore()
  return {
    confirm: (msg: string) => store.request(msg)
  }
}
