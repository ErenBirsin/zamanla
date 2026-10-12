import { type ComponentProps, useId } from 'react'

interface FormFieldProps extends ComponentProps<'input'> {
  label: string
  error?: string
  hint?: string
}

export function FormField({ label, error, hint, ...inputProps }: FormFieldProps) {
  const id = useId()
  const messageId = `${id}-message`
  const message = error ?? hint

  return (
    <div className="flex flex-col gap-1.5">
      <label htmlFor={id} className="text-[13px] font-semibold">
        {label}
      </label>
      <input
        id={id}
        aria-invalid={error ? true : undefined}
        aria-describedby={message ? messageId : undefined}
        className="h-12 w-full rounded-xl border border-[#dcd0c1] bg-surface px-3.5 text-[15px] outline-none placeholder:text-muted/60 focus:border-clay focus:ring-3 focus:ring-clay-soft aria-invalid:border-clay"
        {...inputProps}
      />
      {message && (
        <p id={messageId} className={`text-[13px] ${error ? 'text-clay-dark' : 'text-muted'}`}>
          {message}
        </p>
      )}
    </div>
  )
}
