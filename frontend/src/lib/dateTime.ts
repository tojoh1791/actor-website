export const eventDate = new Intl.DateTimeFormat('nb-NO', {
  dateStyle: 'long',
  timeStyle: 'short',
  timeZone: 'Europe/Oslo',
})

const inputDateTime = new Intl.DateTimeFormat('sv-SE', {
  timeZone: 'Europe/Oslo',
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
  hour: '2-digit',
  minute: '2-digit',
  hourCycle: 'h23',
})

export function toOsloInput(iso: string): string {
  const parts = inputDateTime.formatToParts(new Date(iso))
  const part = (type: string) => parts.find((item) => item.type === type)?.value
  return (
    part('year') +
    '-' +
    part('month') +
    '-' +
    part('day') +
    'T' +
    part('hour') +
    ':' +
    part('minute')
  )
}

export function fromOsloInput(local: string): string {
  const [year, month, day, hour, minute] = local.split(/[-T:]/).map(Number)
  const wallTime = Date.UTC(year, month - 1, day, hour, minute)
  // Oslo uses UTC+1 or UTC+2. Accept exactly one match; a DST gap or repeated hour is ambiguous.
  const matches = [1, 2]
    .map((offset) => new Date(wallTime - offset * 3600000).toISOString())
    .filter((iso) => toOsloInput(iso) === local)
  if (matches.length !== 1)
    throw new Error(
      'Velg et entydig klokkeslett. Dette tidspunktet faller i overgangen mellom sommer- og vintertid.',
    )
  return matches[0]
}

const dateOnly = new Intl.DateTimeFormat('nb-NO', { dateStyle: 'long', timeZone: 'Europe/Oslo' })
const inputDate = new Intl.DateTimeFormat('sv-SE', { timeZone: 'Europe/Oslo' })

export function bookingDate(value: string | null) {
  // A booking date has no time. Noon UTC preserves its calendar day in Oslo.
  return value ? dateOnly.format(new Date(value + 'T12:00:00Z')) : 'Dato ikke avklart'
}

export function osloDate(timestamp: number) {
  return inputDate.format(new Date(timestamp))
}
