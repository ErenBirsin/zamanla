import { Link } from 'react-router'
import type { SetupStep } from './useSetupProgress'

interface SetupChecklistProps {
  steps: SetupStep[]
  completed: number
}

export function SetupChecklist({ steps, completed }: SetupChecklistProps) {
  const percent = Math.round((completed / steps.length) * 100)

  return (
    <section
      aria-labelledby="setup-title"
      className="flex max-w-3xl flex-col gap-5 rounded-card border border-line bg-surface p-6"
    >
      <div className="flex flex-col gap-3">
        <div className="flex flex-wrap items-baseline justify-between gap-2">
          <h2 id="setup-title" className="text-xl font-bold">
            Salonunu randevuya hazırla
          </h2>
          <span className="text-sm text-muted">
            {steps.length} adımdan {completed} tanesi tamam
          </span>
        </div>
        <div
          role="progressbar"
          aria-label="Kurulum ilerlemesi"
          aria-valuemin={0}
          aria-valuemax={steps.length}
          aria-valuenow={completed}
          className="h-2 overflow-hidden rounded-full bg-line-soft"
        >
          <div
            className="h-full rounded-full bg-sage transition-all"
            style={{ width: `${percent}%` }}
          />
        </div>
      </div>

      <ol className="flex flex-col">
        {steps.map((step, index) => (
          <li
            key={step.title}
            className="flex flex-col gap-3 border-t border-line-soft py-4 first:border-t-0 first:pt-0 last:pb-0 sm:flex-row sm:items-center"
          >
            <div className="flex flex-1 items-start gap-3.5">
              <span
                aria-hidden
                className={`flex size-8 shrink-0 items-center justify-center rounded-full text-sm font-bold ${
                  step.done ? 'bg-sage text-white' : 'border border-line text-muted'
                }`}
              >
                {step.done ? '✓' : index + 1}
              </span>
              <div className="flex flex-col gap-0.5">
                <h3 className={`font-semibold ${step.done ? 'text-muted line-through' : ''}`}>
                  {step.title}
                  <span className="sr-only">{step.done ? ' (tamamlandı)' : ' (bekliyor)'}</span>
                </h3>
                <p className="text-sm text-muted">{step.done ? step.summary : step.description}</p>
              </div>
            </div>
            {!step.done && (
              <Link
                to={step.to}
                className="ml-11.5 inline-flex h-10 shrink-0 items-center self-start rounded-full bg-clay px-4 text-sm font-semibold text-white hover:bg-clay-dark sm:ml-0 sm:self-center"
              >
                {step.action}
              </Link>
            )}
          </li>
        ))}
      </ol>
    </section>
  )
}
