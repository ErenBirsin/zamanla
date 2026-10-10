import { isAxiosError } from 'axios'
import { useEffect, useState } from 'react'
import { http } from '@/shared/api/http'

export type ApiHealth = 'checking' | 'healthy' | 'unhealthy' | 'unreachable'

export function useApiHealth() {
  const [status, setStatus] = useState<ApiHealth>('checking')
  const [checkedAt, setCheckedAt] = useState<Date | null>(null)
  const [attempt, setAttempt] = useState(0)

  useEffect(() => {
    let ignore = false

    http
      // veritabanı kontrolü yavaş cevap verebiliyor
      .get('/health', { timeout: 30_000 })
      .then(() => {
        if (!ignore) setStatus('healthy')
      })
      .catch((error: unknown) => {
        if (ignore) return
        const databaseDown = isAxiosError(error) && error.response?.status === 503
        setStatus(databaseDown ? 'unhealthy' : 'unreachable')
      })
      .finally(() => {
        if (!ignore) setCheckedAt(new Date())
      })

    return () => {
      ignore = true
    }
  }, [attempt])

  const check = () => {
    setStatus('checking')
    setAttempt((current) => current + 1)
  }

  return { status, checkedAt, check }
}
