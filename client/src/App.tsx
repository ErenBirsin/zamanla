import { ApiStatusCard } from '@/features/system/ApiStatusCard'
import { Logo } from '@/shared/ui/Logo'

const setupSteps = [
  { title: '.NET 10 solution ve 4 katman', done: true },
  { title: 'React 19 + TypeScript + Vite', done: true },
  { title: 'Tailwind ve tasarım renkleri', done: true },
  { title: 'Kimlik ve işletme hesabı (Aşama 1)', done: false },
]

function App() {
  return (
    <main className="mx-auto flex max-w-3xl flex-col gap-8 px-4 py-10 sm:px-6 sm:py-16">
      <Logo />

      <header className="flex flex-col gap-3">
        <span className="w-fit rounded-full bg-clay-soft px-3 py-1 text-xs font-semibold text-clay-dark">
          Aşama 0 · Kurulum
        </span>
        <h1 className="text-4xl font-bold sm:text-5xl">İskelet hazır.</h1>
        <p className="max-w-xl text-base leading-relaxed text-muted">
          Aşağıdaki kart, React’in .NET API’ye istek atıp atamadığını gösteriyor.
        </p>
      </header>

      <div className="grid gap-5 sm:grid-cols-2">
        <ApiStatusCard />

        <section className="flex flex-col gap-4 rounded-card bg-ink p-6 text-sand">
          <span className="text-xs font-semibold tracking-wide text-sand/70">YOL HARİTASI</span>
          <ul className="flex flex-col gap-3">
            {setupSteps.map((step) => (
              <li key={step.title} className="flex items-center gap-3 text-sm">
                <span
                  className={`flex size-5 shrink-0 items-center justify-center rounded-full ${
                    step.done ? 'bg-sage' : 'border border-sand/30'
                  }`}
                >
                  {step.done && (
                    <svg
                      width="11"
                      height="11"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="#fff"
                      strokeWidth="3.5"
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      aria-hidden="true"
                    >
                      <path d="M20 6 9 17l-5-5" />
                    </svg>
                  )}
                </span>
                <span className={step.done ? '' : 'text-sand/60'}>{step.title}</span>
              </li>
            ))}
          </ul>
        </section>
      </div>
    </main>
  )
}

export default App
