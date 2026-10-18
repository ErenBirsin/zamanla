interface CalendarEvent {
  uid: string
  title: string
  description: string
  startAt: string
  endAt: string
  url: string
}

// .ics saatleri UTC ve ayraçsız: 20261019T110000Z
const stamp = (value: string | Date) =>
  new Date(value)
    .toISOString()
    .replace(/[-:]/g, '')
    .replace(/\.\d{3}/, '')

// virgül, noktalı virgül ve satır sonu .ics'te özel anlam taşır
const escape = (text: string) =>
  text
    .replace(/\\/g, '\\\\')
    .replace(/([,;])/g, '\\$1')
    .replace(/\n/g, '\\n')

// telefonun takvim uygulaması dosyayı açıp etkinliği ekler; 1 saat önce hatırlatır
export function downloadIcs(event: CalendarEvent) {
  const lines = [
    'BEGIN:VCALENDAR',
    'VERSION:2.0',
    'PRODID:-//Zamanla//TR',
    'BEGIN:VEVENT',
    `UID:${event.uid}@zamanla`,
    `DTSTAMP:${stamp(new Date())}`,
    `DTSTART:${stamp(event.startAt)}`,
    `DTEND:${stamp(event.endAt)}`,
    `SUMMARY:${escape(event.title)}`,
    `DESCRIPTION:${escape(event.description)}`,
    `URL:${event.url}`,
    'BEGIN:VALARM',
    'TRIGGER:-PT1H',
    'ACTION:DISPLAY',
    `DESCRIPTION:${escape(event.title)}`,
    'END:VALARM',
    'END:VEVENT',
    'END:VCALENDAR',
  ]

  const blob = new Blob([lines.join('\r\n')], { type: 'text/calendar;charset=utf-8' })
  const link = document.createElement('a')
  link.href = URL.createObjectURL(blob)
  link.download = `${event.uid}.ics`
  link.click()
  setTimeout(() => URL.revokeObjectURL(link.href), 1000)
}
