interface StepHeadingProps {
  step: number
  title: string
}

const STEP_COUNT = 3

export function StepHeading({ step, title }: StepHeadingProps) {
  return (
    <div className="flex flex-col gap-2.5">
      <div className="grid grid-cols-3 gap-1.5" aria-hidden>
        {Array.from({ length: STEP_COUNT }, (_, index) => (
          <span
            key={index}
            className={`h-1 rounded-full ${index < step ? 'bg-clay' : 'bg-line'}`}
          />
        ))}
      </div>
      <span className="text-xs font-semibold text-muted">
        ADIM {step} / {STEP_COUNT}
      </span>
      <h2 className="text-[26px] leading-tight font-bold">{title}</h2>
    </div>
  )
}
