import type { ActorEvent } from '../types/event'

export async function getUpcomingEvents(signal?: AbortSignal): Promise<ActorEvent[]> {
  const response = await fetch('/api/events', { signal })
  if (!response.ok) throw new Error('Kunne ikke hente arrangementene. Prøv igjen senere.')
  return response.json() as Promise<ActorEvent[]>
}
