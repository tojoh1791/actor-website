import { Link } from 'react-router-dom'
import { useApiQuery } from '../hooks/useApiQuery'

export default function Dashboard() {
  const { data, error } = useApiQuery<{ newCount: number }>('/api/admin/bookings/summary')
  const newCount = data?.newCount ?? null
  const countError = Boolean(error)
  return (
    <section className="admin-dashboard">
      <p className="eyebrow">Ditt kontrollpanel</p>
      <h1>Hei, Arben.</h1>
      <p>Hva vil du gjøre?</p>
      <div className="admin-stack">
        <Link className="admin-primary" to="/admin/events/new">
          + Legg til arrangement
        </Link>
        <Link
          className="admin-menu-card"
          to={newCount ? '/admin/bookings?status=new' : '/admin/bookings'}
        >
          <span>
            <strong>
              Forespørsler{' '}
              {newCount !== null && newCount > 0 && (
                <span className="admin-new-count">
                  {newCount} {newCount === 1 ? 'ny' : 'nye'}
                </span>
              )}
            </strong>
            <span>
              {countError
                ? 'Åpne innboksen for å hente forespørslene'
                : newCount === null
                  ? 'Henter antall nye …'
                  : newCount
                    ? 'Nye arrangører venter på svar'
                    : 'Les henvendelser og følg opp avtaler'}
            </span>
          </span>
          <span aria-hidden="true">→</span>
        </Link>
        <Link className="admin-menu-card" to="/admin/events">
          <span>
            <strong>Mine arrangementer</strong>
            <span>Endre innhold og hva publikum ser</span>
          </span>
          <span aria-hidden="true">→</span>
        </Link>
        <a className="admin-menu-card" href="/events" target="_blank" rel="noreferrer">
          <span>
            <strong>Se den offentlige nettsiden</strong>
            <span>Åpnes i en ny fane</span>
          </span>
          <span aria-hidden="true">↗</span>
        </a>
      </div>
    </section>
  )
}
