import { useApiQuery } from '../hooks/useApiQuery'
import type { ActorEvent } from '../types/event'
import EventCard from '../components/EventCard'
import { useSearchParams } from 'react-router-dom'

export default function EventsPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const past = searchParams.get('view') === 'past'
  const { data, loading, error } = useApiQuery<ActorEvent[]>(
    past ? '/api/events?past=true' : '/api/events',
  )
  const events = data ?? []

  return (
    <section className="page-heading events-page">
      <p className="eyebrow">Møt historiene</p>
      <h1>Arrangementer</h1>
      <p className="lead">
        Møt Arben Bala på foredrag, litteraturarrangementer og andre offentlige møter.
      </p>
      <div className="event-filters" role="group" aria-label="Vis arrangementer">
        <button
          type="button"
          aria-pressed={!past}
          aria-controls="event-results"
          onClick={() => setSearchParams({})}
        >
          Kommende
        </button>
        <button
          type="button"
          aria-pressed={past}
          aria-controls="event-results"
          onClick={() => setSearchParams({ view: 'past' })}
        >
          Tidligere
        </button>
      </div>
      <div id="event-results" aria-busy={loading}>
        <h2 className="visually-hidden">
          {past ? 'Tidligere arrangementer' : 'Kommende arrangementer'}
        </h2>
        {loading && <p role="status">Henter {past ? 'tidligere ' : ''}arrangementer …</p>}
        {error && <p role="alert">{error}</p>}
        {!loading && !error && events.length === 0 && (
          <div className="event-status">
            <h2>{past ? 'Ingen tidligere arrangementer ennå.' : 'Nye møter kommer.'}</h2>
            <p>
              {past
                ? 'Publiserte arrangementer vises her når datoen er passert.'
                : 'Det er ingen publiserte arrangementer akkurat nå. Tid, sted og praktisk informasjon legges ut her når nye datoer er klare.'}
            </p>
          </div>
        )}
        <div className="event-list">
          {events.map((event) => (
            <EventCard event={event} key={event.id} />
          ))}
        </div>
      </div>
    </section>
  )
}
