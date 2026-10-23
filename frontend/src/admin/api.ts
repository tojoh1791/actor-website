import type { ActorEvent } from '../types/event'

export interface AdminEvent extends ActorEvent {
  isPublished: boolean
  createdAt: string
  updatedAt: string
}
export type EventInput = Pick<
  ActorEvent,
  | 'title'
  | 'description'
  | 'startsAt'
  | 'endsAt'
  | 'location'
  | 'address'
  | 'organizer'
  | 'externalUrl'
  | 'imageUrl'
>
export interface AdminUser {
  email: string
}

export { ApiError, api } from '../lib/http'

export const eventDate = new Intl.DateTimeFormat('nb-NO', {
  dateStyle: 'long',
  timeStyle: 'short',
  timeZone: 'Europe/Oslo',
})

export function toOsloInput(iso: string): string {
  const parts = new Intl.DateTimeFormat('sv-SE', {
    timeZone: 'Europe/Oslo',
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    hourCycle: 'h23',
  }).formatToParts(new Date(iso))
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
  const matches = [1, 2]
    .map((offset) => new Date(wallTime - offset * 3600000).toISOString())
    .filter((iso) => toOsloInput(iso) === local)
  if (matches.length !== 1)
    throw new Error(
      'Velg et entydig klokkeslett. Dette tidspunktet faller i overgangen mellom sommer- og vintertid.',
    )
  return matches[0]
}
