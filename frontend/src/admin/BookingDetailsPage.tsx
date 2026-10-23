import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { ApiError, api, eventDate } from './api'
import { bookingDate, bookingStatuses, bookingTypes } from '../types/booking'
import type { BookingDetails } from '../types/booking'
import ConfirmDialog from './ConfirmDialog'

export default function BookingDetailsPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [item, setItem] = useState<BookingDetails | null>(null)
  const [status, setStatus] = useState('new')
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState('')
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [busy, setBusy] = useState(false)
  const [deleting, setDeleting] = useState(false)
  const [retry, setRetry] = useState(0)
  useEffect(() => {
    let active = true
    setLoading(true); setLoadError('')
    api<BookingDetails>('/api/admin/bookings/' + id).then(value => { if (active) { setItem(value); setStatus(value.status) } })
      .catch(reason => { if (active) setLoadError(reason instanceof ApiError && reason.status === 404 ? 'Forespørselen finnes ikke. Den kan være slettet.' : reason instanceof Error ? reason.message : 'Kunne ikke hente forespørselen.') })
      .finally(() => { if (active) setLoading(false) })
    return () => { active = false }
  }, [id, retry])
  if (loading) return <p role="status">Henter forespørselen …</p>
  if (loadError || !item) return <section><h1>Forespørsel</h1><div role="alert" className="admin-error"><p>{loadError}</p><button className="admin-secondary" onClick={() => setRetry(value => value + 1)}>Prøv igjen</button></div><Link to="/admin/bookings">← Til forespørslene</Link></section>
  const emailLink = 'mailto:' + encodeURIComponent(item.email) + '?subject=' + encodeURIComponent('Bookingforespørsel til Arben Bala – ' + bookingDate(item.preferredDate))
  return <section className="admin-booking-details">
    <Link className="admin-back" to="/admin/bookings">← Til forespørslene</Link>
    <span className={'admin-status booking-status-' + item.status}>{bookingStatuses[item.status]}</span>
    <h1>{item.organization || item.contactName}</h1>
    <p className="admin-note">Mottatt {eventDate.format(new Date(item.createdAt))} · Referanse AB-{item.id.replaceAll('-', '').slice(0, 8).toUpperCase()}</p>
    {notice && <p role="status" className="admin-success">{notice}</p>}
    {error && !deleting && <p role="alert" className="admin-error">{error}</p>}
    <article className="admin-card"><h2>Dette ønsker arrangøren</h2><dl className="admin-booking-facts">
      <div><dt>Type</dt><dd>{bookingTypes[item.eventType]}</dd></div><div><dt>Ønsket dato</dt><dd>{bookingDate(item.preferredDate)}</dd></div>
      <div><dt>Sted</dt><dd>{item.location}</dd></div><div><dt>Deltakere</dt><dd>{item.participants ?? 'Ikke oppgitt'}</dd></div>
    </dl><h2>Melding fra arrangøren</h2><p className="admin-description">{item.message}</p></article>
    <article className="admin-card"><h2>Kontakt arrangøren</h2><p><strong>{item.contactName}</strong><br />{item.email}{item.phone && <><br />{item.phone}</>}</p>
      <div className="admin-stack"><a className="admin-primary" href={emailLink}>Svar på e-post ↗</a>
        {item.phone && <a className="admin-secondary" href={'tel:' + item.phone.replace(/[^+0-9]/g, '')}>Ring arrangøren</a>}</div>
      <p className="admin-note">E-postknappen åpner e-postprogrammet ditt. Avtal dato, pris og innhold direkte med arrangøren.</p>
    </article>
    <form className="admin-card" onSubmit={async event => {
      event.preventDefault(); if (busy) return
      setBusy(true); setError(''); setNotice('')
      try {
        const updated = await api<BookingDetails>('/api/admin/bookings/' + item.id + '/status', { method: 'PATCH', body: JSON.stringify({ status }) })
        setItem(updated); setStatus(updated.status); setNotice('Statusen er lagret. Ingen melding er sendt til arrangøren.')
      } catch (reason) { setError(reason instanceof Error ? reason.message : 'Kunne ikke lagre statusen.') }
      finally { setBusy(false) }
    }}><h2>Hold oversikt</h2><label htmlFor="booking-status">Status på forespørselen</label>
      <select id="booking-status" value={status} onChange={event => setStatus(event.target.value)} disabled={busy}>{Object.entries(bookingStatuses).map(([value, label]) => <option key={value} value={value}>{label}</option>)}</select>
      <p className="admin-note">Bruk «Avtalt» når du og arrangøren har bekreftet avtalen. Statusen er intern og sender ingen melding. Offentlige arrangementer legger du til under «Mine arrangementer».</p>
      <button className="admin-primary" type="submit" disabled={busy || status === item.status}>{busy ? 'Lagrer …' : 'Lagre status'}</button>
    </form>
    <details className="admin-help"><summary>Flere valg</summary><p>Slett forespørsler du ikke lenger trenger å beholde.</p><button className="admin-delete-link" onClick={() => { setError(''); setDeleting(true) }} disabled={busy}>Slett forespørselen</button></details>
    {deleting && <ConfirmDialog title="Slette forespørselen?" description="Kontaktopplysningene og meldingen slettes fra innboksen. Dette kan ikke angres. Ingen melding sendes til arrangøren." confirmLabel="Ja, slett forespørselen" busy={busy} error={error}
      onClose={() => setDeleting(false)} onConfirm={async () => {
        setBusy(true); setError('')
        try { await api('/api/admin/bookings/' + item.id, { method: 'DELETE' }); navigate('/admin/bookings', { replace: true, state: { notice: 'Forespørselen er slettet.' } }) }
        catch (reason) { setError(reason instanceof Error ? reason.message : 'Kunne ikke slette forespørselen.') }
        finally { setBusy(false) }
      }} />}
  </section>
}
