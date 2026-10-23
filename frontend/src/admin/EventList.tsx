import { errorMessage, api } from '../lib/http'
import { useApiQuery } from '../hooks/useApiQuery'
import { useNow } from '../hooks/useNow'
import AdminRequestError from './AdminRequestError'
import { useState } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { eventDate } from '../lib/dateTime'
import type { AdminEvent } from '../types/event'
import ConfirmDialog from './ConfirmDialog'
import EventImage from '../components/EventImage'

export default function EventList() {
  const location = useLocation()
  const {
    data,
    loading,
    error,
    reload,
    setData: setEvents,
  } = useApiQuery<AdminEvent[]>('/api/admin/events/')
  const events = data ?? []
  const now = useNow()
  const [notice, setNotice] = useState((location.state as { notice?: string } | null)?.notice ?? '')
  const [confirmation, setConfirmation] = useState<{
    item: AdminEvent
    action: 'delete' | 'hide'
  } | null>(null)
  const [busy, setBusy] = useState(false)
  const [actionError, setActionError] = useState('')
  const [filter, setFilter] = useState('upcoming')
  const visible = events.filter(
    (item) =>
      filter === 'all' ||
      (filter === 'drafts' ? !item.isPublished : new Date(item.startsAt).getTime() >= now),
  )
  async function confirmAction() {
    if (!confirmation || busy) return
    setBusy(true)
    setActionError('')
    try {
      const { item, action } = confirmation
      if (action === 'delete') {
        await api('/api/admin/events/' + item.id, { method: 'DELETE' })
        setEvents((previous) => (previous ?? []).filter((entry) => entry.id !== item.id))
      } else {
        const updated = await api<AdminEvent>('/api/admin/events/' + item.id + '/publication', {
          method: 'PATCH',
          body: JSON.stringify({ isPublished: false }),
        })
        setEvents((previous) =>
          (previous ?? []).map((entry) => (entry.id === updated.id ? updated : entry)),
        )
      }
      setConfirmation(null)
      setNotice(
        action === 'delete'
          ? 'Arrangementet er slettet.'
          : 'Arrangementet er skjult fra nettsiden.',
      )
    } catch (reason) {
      setActionError(errorMessage(reason, 'Handlingen kunne ikke fullføres.'))
    } finally {
      setBusy(false)
    }
  }

  return (
    <section>
      <div className="admin-section-heading">
        <div>
          <p className="eyebrow">Oversikt og redigering</p>
          <h1>Mine arrangementer</h1>
        </div>
        <Link className="admin-primary" to="/admin/events/new">
          + Legg til arrangement
        </Link>
      </div>
      {notice && (
        <p role="status" className="admin-success">
          {notice}
        </p>
      )}
      <label htmlFor="event-filter">Vis arrangementer</label>
      <select id="event-filter" value={filter} onChange={(event) => setFilter(event.target.value)}>
        <option value="upcoming">Kommende</option>
        <option value="drafts">Utkast</option>
        <option value="all">Alle, også tidligere</option>
      </select>
      {loading && <p role="status">Henter arrangementer …</p>}
      {error && <AdminRequestError message={error} onRetry={reload} />}
      {!loading && !error && visible.length === 0 && (
        <div className="admin-card">
          <h2>Ingen arrangementer i denne visningen.</h2>
          <p>Velg en annen visning eller legg til et nytt arrangement.</p>
        </div>
      )}
      <div className="admin-event-list">
        {!error &&
          visible.map((item) => (
            <article className="admin-card" key={item.id}>
              <EventImage imageUrl={item.imageUrl} />
              <span className={'admin-status ' + (item.isPublished ? 'published' : '')}>
                {item.isPublished
                  ? new Date(item.startsAt).getTime() < now
                    ? 'Tidligere arrangement'
                    : 'Synlig på nettsiden'
                  : 'Utkast – bare synlig for deg'}
              </span>
              <h2>{item.title}</h2>
              <p>
                <time dateTime={item.startsAt}>{eventDate.format(new Date(item.startsAt))}</time>
                <br />
                {item.location}
              </p>
              <div className="admin-stack">
                <Link className="admin-secondary" to={'/admin/events/' + item.id}>
                  Se og endre arrangement
                </Link>
                {!item.isPublished && (
                  <Link className="admin-primary" to={'/admin/events/' + item.id + '?preview=1'}>
                    Se gjennom og publiser
                  </Link>
                )}
                <button
                  type="button"
                  className="admin-delete-link"
                  onClick={() => {
                    setActionError('')
                    setConfirmation({ item, action: 'delete' })
                  }}
                >
                  Slett arrangement
                </button>
              </div>
              {item.isPublished && (
                <details>
                  <summary>Flere valg</summary>
                  <div className="admin-stack">
                    <button
                      type="button"
                      className="admin-secondary"
                      onClick={() => {
                        setActionError('')
                        setConfirmation({ item, action: 'hide' })
                      }}
                    >
                      Skjul fra nettsiden
                    </button>
                  </div>
                </details>
              )}
            </article>
          ))}
      </div>
      {confirmation && (
        <ConfirmDialog
          title={
            confirmation.action === 'delete' ? 'Slette arrangementet?' : 'Skjule arrangementet?'
          }
          description={
            confirmation.action === 'delete'
              ? '«' + confirmation.item.title + '» blir slettet. Denne handlingen kan ikke angres.'
              : '«' +
                confirmation.item.title +
                '» blir et utkast. Du kan publisere det igjen senere.'
          }
          confirmLabel={
            confirmation.action === 'delete' ? 'Ja, slett arrangementet' : 'Ja, skjul fra nettsiden'
          }
          busy={busy}
          error={actionError}
          onClose={() => setConfirmation(null)}
          onConfirm={confirmAction}
        />
      )}
    </section>
  )
}
