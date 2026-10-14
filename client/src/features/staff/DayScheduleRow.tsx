import {
  type Control,
  type FieldErrors,
  type UseFormRegister,
  useFieldArray,
  useWatch,
} from 'react-hook-form'
import type { StaffValues } from './schemas'

interface DayScheduleRowProps {
  index: number
  label: string
  control: Control<StaffValues>
  register: UseFormRegister<StaffValues>
  errors: FieldErrors<StaffValues>
}

const timeInputClass =
  'h-10 min-w-0 flex-1 rounded-xl border border-[#dcd0c1] bg-surface px-3 text-sm tabular-nums outline-none focus:border-clay focus:ring-3 focus:ring-clay-soft sm:w-32 sm:flex-none'

export function DayScheduleRow({ index, label, control, register, errors }: DayScheduleRowProps) {
  const enabled = useWatch({ control, name: `schedule.${index}.enabled` })
  // her günün aralık listesi ayrı; öğle arası için ikinci aralık eklenir
  const { fields, append, remove } = useFieldArray({ control, name: `schedule.${index}.ranges` })

  const rangeErrors = errors.schedule?.[index]?.ranges
  const listError = rangeErrors?.root?.message ?? rangeErrors?.message

  return (
    <div className="flex flex-col gap-2 border-b border-line-soft py-3 last:border-b-0 sm:flex-row sm:items-start">
      <label className="flex w-36 shrink-0 cursor-pointer items-center gap-2.5 text-sm font-semibold sm:h-10">
        <input
          type="checkbox"
          className="size-4.5 accent-clay"
          {...register(`schedule.${index}.enabled`)}
        />
        {label}
      </label>

      {enabled ? (
        <div className="flex flex-1 flex-col gap-2">
          {fields.map((field, rangeIndex) => {
            const error = rangeErrors?.[rangeIndex]?.end?.message
            const rangeLabel = `${label} ${rangeIndex + 1}. aralık`

            return (
              <div key={field.id} className="flex flex-col gap-1">
                <div className="flex items-center gap-2">
                  <input
                    type="time"
                    step={300}
                    aria-label={`${rangeLabel} başlangıcı`}
                    className={timeInputClass}
                    {...register(`schedule.${index}.ranges.${rangeIndex}.start`)}
                  />
                  <span className="text-muted">–</span>
                  <input
                    type="time"
                    step={300}
                    aria-label={`${rangeLabel} bitişi`}
                    aria-invalid={error ? true : undefined}
                    className={`${timeInputClass} aria-invalid:border-clay`}
                    {...register(`schedule.${index}.ranges.${rangeIndex}.end`)}
                  />
                  {fields.length > 1 && (
                    <button
                      type="button"
                      onClick={() => remove(rangeIndex)}
                      aria-label={`${rangeLabel} kaldır`}
                      className="size-9 shrink-0 cursor-pointer rounded-full text-lg leading-none text-muted hover:bg-line-soft hover:text-ink"
                    >
                      ×
                    </button>
                  )}
                </div>
                {error && <p className="text-[13px] text-clay-dark">{error}</p>}
              </div>
            )
          })}
          <button
            type="button"
            onClick={() => append({ start: '', end: '' })}
            className="cursor-pointer self-start rounded-full px-2 py-1 text-xs font-semibold text-clay-dark hover:bg-clay-soft"
          >
            + Aralık ekle
          </button>
          {listError && <p className="text-[13px] text-clay-dark">{listError}</p>}
        </div>
      ) : (
        <p className="text-sm text-muted sm:flex sm:h-10 sm:items-center">Kapalı</p>
      )}
    </div>
  )
}
