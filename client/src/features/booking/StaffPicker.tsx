import { staffColorClasses } from '@/features/staff/staffOptions'
import { initials } from './hours'
import type { PublicStaff } from './types'

interface StaffPickerProps {
  staff: PublicStaff[]
  value: string
  onChange: (staffId: string) => void
}

const avatar = 'flex size-14 items-center justify-center rounded-full text-[15px] font-bold'
const selectedRing = 'ring-2 ring-clay ring-offset-2 ring-offset-sand'

// boş değer "fark etmez": o saatte boş olan personel atanır
export function StaffPicker({ staff, value, onChange }: StaffPickerProps) {
  return (
    <div className="-mx-5 flex gap-3.5 overflow-x-auto px-5 pt-1 pb-1">
      <button
        type="button"
        aria-pressed={value === ''}
        onClick={() => onChange('')}
        className="flex w-16 shrink-0 cursor-pointer flex-col items-center gap-1.5"
      >
        <span
          className={`${avatar} border-2 border-dashed border-[#cfc4b6] bg-surface ${value === '' ? selectedRing : ''}`}
        >
          <svg
            width="20"
            height="20"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
            aria-hidden
          >
            <path d="M16 3h5v5M4 20 21 3M21 16v5h-5M15 15l6 6M4 4l5 5" />
          </svg>
        </span>
        <span className="text-xs text-ink-soft">Fark etmez</span>
      </button>
      {staff.map((member) => {
        const selected = value === member.id
        return (
          <button
            key={member.id}
            type="button"
            aria-pressed={selected}
            onClick={() => onChange(member.id)}
            className="flex w-16 shrink-0 cursor-pointer flex-col items-center gap-1.5"
          >
            <span
              className={`${avatar} ${staffColorClasses[member.color]} ${selected ? selectedRing : ''}`}
            >
              {initials(`${member.firstName} ${member.lastName}`)}
            </span>
            <span className={`truncate text-xs ${selected ? 'font-semibold' : 'text-ink-soft'}`}>
              {member.firstName}
            </span>
          </button>
        )
      })}
    </div>
  )
}
