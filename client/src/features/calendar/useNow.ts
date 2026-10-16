import { useEffect, useState } from 'react'

// "şu an" çizgisi dakikada bir ilerlesin
export function useNow(intervalMs = 60_000) {
  const [now, setNow] = useState(() => new Date())

  useEffect(() => {
    const id = window.setInterval(() => setNow(new Date()), intervalMs)
    return () => window.clearInterval(id)
  }, [intervalMs])

  return now
}
