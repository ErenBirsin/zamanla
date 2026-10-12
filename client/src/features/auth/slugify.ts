const turkishLetters: Record<string, string> = { ç: 'c', ğ: 'g', ı: 'i', ö: 'o', ş: 's', ü: 'u' }

// "Kuaför Ayşe" -> "kuafor-ayse"
export function slugify(value: string) {
  return value
    .toLocaleLowerCase('tr')
    .replace(/[çğıöşü]/g, (letter) => turkishLetters[letter])
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[^a-z0-9]+/g, '-')
    .slice(0, 100)
    .replace(/^-+|-+$/g, '')
}
