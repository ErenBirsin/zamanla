const currency = new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' })

export function formatPrice(value: number) {
  return currency.format(value)
}
