import type { FormEvent } from 'react'
import { errorMessage, api } from '../lib/http'
import { useApiQuery } from '../hooks/useApiQuery'
import AdminRequestError from './AdminRequestError'
import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { eventDate, bookingDate } from '../lib/dateTime'
import { bookingStatuses, bookingTypes } from '../types/booking'
import type { BookingDetails } from '../types/booking'
import ConfirmDialog from './ConfirmDialog'

export default function BookingDetailsPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const {
    data: item,
    loading,
    error: queryError,
    errorStatus,
    reload,
    setData: setItem,
  } = useApiQuery<BookingDetails>('/api/admin/bookings/' + id)
  const [selectedStatus, setSelectedStatus] = useState<string | null>(null)
  const status = selectedStatus ?? item?.status ?? 'new'
  const loadError =
    errorStatus === 404 ? 'Forespørselen finnes ikke. Den kan være slettet.' : queryError
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [busy, setBusy] = useState(false)
  const [deleting, setDeleting] = useState(false)
  async function saveStatus(event: FormEvent<HTMLFormElement>) {
    if (!item) return
    event.preventDefault()
    if (busy) return
    setBusy(true)
    setError('')
    setNotice('')
    try {
      const updated = await api<BookingDetails>('/api/admin/bookings/' + item.id + '/status', {
        method: 'PATCH',
        body: JSON.stringify({ status }),
      })
      setItem(updated)
      setSelectedStatus(null)
      setNotice('Statusen er lagret. Ingen melding er sendt til arrangøren.')
    } catch (reason) {
      setError(errorMessage(reason, 'Kunne ikke lagre statusen.'))
    } finally {
      setBusy(false)
    }
  }

  async function deleteBooking() {
    if (!item || busy) return
    setBusy(true)
    setError('')
    try {
      await api('/api/admin/bookings/' + item.id, { method: 'DELETE' })
      navigate('/admin/bookings', {
        replace: true,
        state: { notice: 'Forespørselen er slettet.' },
      })
    } catch (reason) {
      setError(errorMessage(reason, 'Kunne ikke slette forespørselen.'))
    } finally {
      setBusy(false)
    }
  }

  if (loading) return <p role="status">Henter forespørselen …</p>
  if (loadError || !item)
    return (
      <section>
        <h1>Forespørsel</h1>
        <AdminRequestError message={loadError} onRetry={reload} />
      </section>
    )
  const emailLink =
    'mailto:' +
    encodeURIComponent(item.email) +
    '?subject=' +
    encodeURIComponent('Bookingforespørsel til Arben Bala – ' + bookingDate(item.preferredDate))
  return (
    <section className="admin-booking-details">
      <span className={'admin-status booking-status-' + item.status}>
        {bookingStatuses[item.status]}
      </span>
      <h1>{item.organization || item.contactName}</h1>
      <p className="admin-note">
        Mottatt {eventDate.format(new Date(item.createdAt))} · Referanse AB-
        {item.id.replaceAll('-', '').slice(0, 8).toUpperCase()}
      </p>
      {notice && (
        <p role="status" className="admin-success">
          {notice}
        </p>
      )}
      {error && !deleting && (
        <p role="alert" className="admin-error">
          {error}
        </p>
      )}
      <article className="admin-card">
        <h2>Dette ønsker arrangøren</h2>
        <dl className="admin-booking-facts">
          <div>
            <dt>Type</dt>
            <dd>{bookingTypes[item.eventType]}</dd>
          </div>
          <div>
            <dt>Ønsket dato</dt>
            <dd>{bookingDate(item.preferredDate)}</dd>
          </div>
          <div>
            <dt>Sted</dt>
            <dd>{item.location}</dd>
          </div>
          <div>
            <dt>Deltakere</dt>
            <dd>{item.participants ?? 'Ikke oppgitt'}</dd>
          </div>
        </dl>
        <h2>Melding fra arrangøren</h2>
        <p className="admin-description">{item.message}</p>
      </article>
      <article className="admin-card">
        <h2>Kontakt arrangøren</h2>
        <p>
          <strong>{item.contactName}</strong>
          <br />
          {item.email}
          {item.phone && (
            <>
              <br />
              {item.phone}
            </>
          )}
        </p>
        <div className="admin-stack">
          <a className="admin-primary" href={emailLink}>
            Svar på e-post ↗
          </a>
          {item.phone && (
            <a className="admin-secondary" href={'tel:' + item.phone.replace(/[^+0-9]/g, '')}>
              Ring arrangøren
            </a>
          )}
        </div>
        <p className="admin-note">
          E-postknappen åpner e-postprogrammet ditt. Avtal dato, pris og innhold direkte med
          arrangøren.
        </p>
      </article>
      <form className="admin-card" onSubmit={saveStatus}>
        <h2>Hold oversikt</h2>
        <label htmlFor="booking-status">Status på forespørselen</label>
        <select
          id="booking-status"
          value={status}
          onChange={(event) => setSelectedStatus(event.target.value)}
          disabled={busy}
        >
          {Object.entries(bookingStatuses).map(([value, label]) => (
            <option key={value} value={value}>
              {label}
            </option>
          ))}
        </select>
        <p className="admin-note">
          Bruk «Avtalt» når du og arrangøren har bekreftet avtalen. Statusen er intern og sender
          ingen melding.
        </p>
        <button className="admin-primary" type="submit" disabled={busy} aria-busy={busy}>
          {busy ? 'Lagrer …' : 'Lagre status'}
        </button>
      </form>
      {item.eventType === 'acting' ? (
        <p className="admin-note">
          Skuespilleroppdrag håndteres i innboksen og kan ikke gjøres om til arrangementer.
        </p>
      ) : item.status === 'agreed' && ['talk', 'book', 'other'].includes(item.eventType) ? (
        <article className="admin-card">
          <h2>Skal publikum kunne komme?</h2>
          <p>
            Lag et arrangement med opplysninger fra forespørselen. Se over innholdet før du lagrer
            eller publiserer.
          </p>
          <Link className="admin-primary" to={'/admin/events/new?booking=' + item.id}>
            Opprett arrangement fra forespørselen
          </Link>
          <p className="admin-note">
            Ingenting publiseres automatisk. Private oppdrag kan bli i innboksen.
          </p>
        </article>
      ) : (
        <p className="admin-note">
          Når en booking er avtalt, kan du lage et offentlig arrangement her. Velg «Avtalt» og lagre
          statusen først.
        </p>
      )}
      <div className="admin-stack">
        <button
          className="admin-delete-link"
          onClick={() => {
            setError('')
            setDeleting(true)
          }}
          disabled={busy}
        >
          Slett forespørselen
        </button>
      </div>
      {deleting && (
        <ConfirmDialog
          title="Slette forespørselen?"
          description="Kontaktopplysningene og meldingen slettes fra innboksen. Dette kan ikke angres. Ingen melding sendes til arrangøren."
          confirmLabel="Ja, slett forespørselen"
          busy={busy}
          error={error}
          onClose={() => setDeleting(false)}
          onConfirm={deleteBooking}
        />
      )}
    </section>
  )
}
