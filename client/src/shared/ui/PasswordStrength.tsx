import { getPasswordStrength, passwordRules } from '@/shared/auth/passwordRules'

const barClasses = {
  none: 'bg-line',
  weak: 'bg-clay',
  medium: 'bg-amber',
  strong: 'bg-sage',
}

const textClasses = {
  none: 'text-muted',
  weak: 'text-clay-dark',
  medium: 'text-amber',
  strong: 'text-sage',
}

export function PasswordStrength({ password }: { password: string }) {
  const strength = getPasswordStrength(password)

  return (
    <div className="-mt-2 flex flex-col gap-2">
      <div className="flex items-center justify-between text-[13px]">
        <span className="text-muted">Şifre gücü</span>
        <span className={`font-semibold ${textClasses[strength.tone]}`}>{strength.text}</span>
      </div>
      <div className="grid grid-cols-4 gap-1.5" aria-hidden>
        {[1, 2, 3, 4].map((bar) => (
          <span
            key={bar}
            className={`h-1.5 rounded-full ${strength.level >= bar ? barClasses[strength.tone] : 'bg-line'}`}
          />
        ))}
      </div>

      {/* hocadaki gibi yazmaya başlayınca görünür */}
      {password.length > 0 && (
        <ul className="grid grid-cols-2 gap-x-3 gap-y-1 text-[13px]">
          {passwordRules.map((rule) => {
            const met = rule.test(password)
            return (
              <li
                key={rule.label}
                className={`flex items-center gap-1.5 ${met ? 'text-sage' : 'text-muted'}`}
              >
                <span aria-hidden className="w-3 text-center font-bold">
                  {met ? '✓' : '✗'}
                </span>
                {rule.label}
                <span className="sr-only">{met ? '(sağlandı)' : '(eksik)'}</span>
              </li>
            )
          })}
        </ul>
      )}
    </div>
  )
}
