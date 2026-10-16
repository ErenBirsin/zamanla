import { getErrorMessages } from '@/shared/api/http'

interface FormErrorProps {
  error: unknown
}

export function FormError({ error }: FormErrorProps) {
  if (!error) return null

  return (
    <div
      role="alert"
      className="flex flex-col gap-1 rounded-xl border border-clay/30 bg-clay-soft/50 px-4 py-3 text-sm text-clay-dark"
    >
      {getErrorMessages(error).map((message) => (
        <p key={message}>{message}</p>
      ))}
    </div>
  )
}
