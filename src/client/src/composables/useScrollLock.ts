import { watch, onBeforeUnmount, Ref } from 'vue'

// Manage no-scroll class on the root/html/body elements with scroll preservation.
// We still use `position: fixed` in the CSS for iOS bounce prevention; that causes
// the viewport to jump to the top when the class is applied. To avoid the jump we
// stash the current scrollY and apply a negative `top` offset on the <html> element
// while the lock is active.

let storedScroll = 0

function updateLock(open: boolean) {
  const docEl = document.documentElement
  const body = document.body

  if (open) {
    // remember where we were and lock at that position
    storedScroll = window.scrollY || docEl.scrollTop || 0
    docEl.style.top = `-${storedScroll}px`
  }

  const elems = [docEl, body]
  elems.forEach(el => {
    if (open) el.classList.add('no-scroll')
    else el.classList.remove('no-scroll')
  })

  if (!open) {
    // restore original scroll position and clear offset
    docEl.style.top = ''
    window.scrollTo(0, storedScroll)
  }
}

export function lockScroll() {
  updateLock(true)
}

export function unlockScroll() {
  updateLock(false)
}

export function useScrollLock(trigger: Ref<boolean>) {
  const stop = watch(trigger, updateLock, { immediate: true })
  onBeforeUnmount(() => {
    stop()
    updateLock(false)
  })
}
