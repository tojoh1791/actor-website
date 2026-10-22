import { useEffect, useState } from 'react'
import { getUpcomingEvents } from '../lib/api'
import type { ActorEvent } from '../types/event'

const dateFormat = new Intl.DateTimeFormat('nb-NO', {
  dateStyle: 'long', timeStyle: 'short', timeZone: 'Europe/Oslo',
})

export default function EventsPage() {
  const [events, setEvents] = useState<ActorEvent[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    const controller = new AbortController()
    getUpcomingEvents(controller.signal)
      .then(setEvents)
      .catch((reason: unknown) => {
        if (!controller.signal.aborted) {
          setError(reason instanceof Error ? reason.message : 'Noe gikk galt.')
        }
      })
      .finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [])

  return <section className="page-heading">
    <p className="eyebrow">Møt historiene</p>
    <h1>Kommende arrangementer</h1>
    <p className="lead">Møt Arben Bala på foredrag, litteraturarrangementer og andre offentlige møter.</p>
    {loading && <p role="status">Henter arrangementer …</p>}
    {error && <p role="alert">{error}</p>}
    {!loading && !error && events.length === 0 && <div className="event-status"><h2>Nye møter kommer.</h2><p>Det er ingen publiserte arrangementer akkurat nå. Tid, sted og praktisk informasjon legges ut her når nye datoer er klare.</p></div>}
    <div className="event-list">{events.map(event => <details key={event.id}>
      <summary><time dateTime={event.startsAt}>{dateFormat.format(new Date(event.startsAt))}</time><strong>{event.title}</strong><span>{event.location}</span></summary>
      <div className="event-description"><p>{event.description}</p><p>{event.address}</p>{event.organizer && <p>Arrangør: {event.organizer}</p>}</div>
    </details>)}</div>
  </section>
}
