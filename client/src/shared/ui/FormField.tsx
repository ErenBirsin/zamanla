import { type ComponentProps, useId } from 'react'

interface FormFieldProps extends ComponentProps<'input'> {
  label: string
  error?: string
  hint?: string
  // inputun solunda sabit duran kısa metin, örneğin telefon için +90
  prefix?: string
}

export function FormField({ label, error, hint, prefix, ...inputProps }: FormFieldProps) {
  const id = useId()
  const messageId = `${id}-message`
  const message = error ?? hint

  return (
    <div className="flex flex-col gap-1.5">
      <label htmlFor={id} className="text-[13px] font-semibold">
        {label}
      </label>
      <div className="flex gap-2">
        {prefix && (
          <span className="flex h-12 items-center rounded-xl border border-[#dcd0c1] bg-sand-soft px-3.5 text-[15px] font-semibold">
            {prefix}
          </span>
        )}
        <input
          id={id}
          aria-invalid={error ? true : undefined}
          aria-describedby={message ? messageId : undefined}
          className="h-12 w-full min-w-0 flex-1 rounded-xl border border-[#dcd0c1] bg-surface px-3.5 text-[15px] outline-none placeholder:text-muted/60 focus:border-clay focus:ring-3 focus:ring-clay-soft aria-invalid:border-clay"
          {...inputProps}
        />
      </div>
      {message && (
        <p id={messageId} className={`text-[13px] ${error ? 'text-clay-dark' : 'text-muted'}`}>
          {message}
        </p>
      )}
    </div>
  )
}
