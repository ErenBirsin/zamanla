const relativeFormat = new Intl.RelativeTimeFormat('tr', { numeric: 'auto' })

// "az önce", "5 dakika önce", "2 saat önce", "dün"
export function formatRelative(value: string) {
  const seconds = Math.round((new Date(value).getTime() - Date.now()) / 1000)
  if (seconds > -60) {
    return 'az önce'
  }

  const minutes = Math.round(seconds / 60)
  if (minutes > -60) {
    return relativeFormat.format(minutes, 'minute')
  }

  const hours = Math.round(minutes / 60)
  if (hours > -24) {
    return relativeFormat.format(hours, 'hour')
  }

  return relativeFormat.format(Math.round(hours / 24), 'day')
}
