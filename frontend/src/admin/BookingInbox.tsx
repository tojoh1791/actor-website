import { useEffect, useState } from 'react'
import { Link, useLocation, useSearchParams } from 'react-router-dom'
import { api, eventDate } from './api'
import { bookingDate, bookingStatuses, bookingTypes } from '../types/booking'
import type { BookingSummary } from '../types/booking'

interface InboxPage { items: BookingSummary[]; total: number; page: number; pageSize: number }

export default function BookingInbox() {
  const [search, setSearch] = useSearchParams()
  const status = search.get('status') ?? 'all'
  const page = Math.max(1, Number(search.get('page')) || 1)
  const [data, setData] = useState<InboxPage | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [retry, setRetry] = useState(0)
  const location = useLocation()
  useEffect(() => {
    let active = true
    setLoading(true); setError('')
    api<InboxPage>('/api/admin/bookings/?' + new URLSearchParams({ status, page: String(page) }))
      .then(value => { if (active) setData(value) })
      .catch(reason => { if (active) setError(reason instanceof Error ? reason.message : 'Kunne ikke hente forespørslene.') })
      .finally(() => { if (active) setLoading(false) })
    return () => { active = false }
  }, [status, page, retry])
  return <section>
    <p className="eyebrow">Booking via nettsiden</p><h1>Forespørsler</h1>
    <p>Les henvendelser, kontakt arrangøren og hold oversikt over hva dere har avtalt.</p>
    {(location.state as { notice?: string } | null)?.notice && <p role="status" className="admin-success">{(location.state as { notice: string }).notice}</p>}
    <label htmlFor="booking-filter">Vis forespørsler</label>
    <select id="booking-filter" value={status} onChange={event => setSearch({ status: event.target.value })}><option value="all">Alle forespørsler</option>{Object.entries(bookingStatuses).map(([value, label]) => <option value={value} key={value}>{label}</option>)}</select>
    {loading && <p role="status">Henter forespørsler …</p>}
    {error && <div role="alert" className="admin-error"><p>{error}</p><button className="admin-secondary" onClick={() => setRetry(value => value + 1)}>Prøv igjen</button></div>}
    {!loading && !error && data && <>
      {data.items.length === 0 ? <div className="admin-card"><h2>Ingen forespørsler i denne visningen.</h2><p>Nye henvendelser fra bookingskjemaet vises her.</p>{page > 1 && <button className="admin-secondary" onClick={() => setSearch({ status })}>Til første side</button>}</div>
        : <><p className="admin-note">{data.total} {data.total === 1 ? 'forespørsel' : 'forespørsler'} · side {data.page} av {Math.ceil(data.total / data.pageSize)}</p>
          <div className="admin-booking-list">{data.items.map(item => <Link key={item.id} className="admin-booking-card" to={'/admin/bookings/' + item.id}>
            <span className={'admin-status booking-status-' + item.status}>{bookingStatuses[item.status]}</span>
            <h2>{item.organization || item.contactName}</h2>{item.organization && <p>{item.contactName}</p>}
            <p><strong>{bookingTypes[item.eventType]}</strong><br />{bookingDate(item.preferredDate)}<br />{item.location}</p>
            <span className="admin-note">Mottatt {eventDate.format(new Date(item.createdAt))}</span>
            <span className="admin-booking-open">Åpne forespørselen <span aria-hidden="true">→</span></span>
          </Link>)}</div>
        </>}
      <nav className="admin-pagination" aria-label="Sider i innboksen">
        {page > 1 && <button className="admin-secondary" onClick={() => setSearch({ status, page: String(page - 1) })}>← Forrige side</button>}
        {page * data.pageSize < data.total && <button className="admin-secondary" onClick={() => setSearch({ status, page: String(page + 1) })}>Neste side →</button>}
      </nav>
    </>}
  </section>
}
