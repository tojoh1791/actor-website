import { useEffect, useState } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { api, eventDate } from './api'
import type { AdminEvent } from './api'
import ConfirmDialog from './ConfirmDialog'
import EventImage from '../components/EventImage'

export default function EventList() {
  const location = useLocation()
  const [events, setEvents] = useState<AdminEvent[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState((location.state as { notice?: string } | null)?.notice ?? '')
  const [confirmation, setConfirmation] = useState<{ item: AdminEvent; action: 'delete' | 'hide' } | null>(null)
  const [busy, setBusy] = useState(false)
  const [actionError, setActionError] = useState('')
  const [filter, setFilter] = useState('upcoming')
  const load = () => {
    setLoading(true); setError('')
    api<AdminEvent[]>('/api/admin/events/')
      .then(setEvents).catch(reason => setError(reason instanceof Error ? reason.message : 'Kunne ikke hente arrangementer.'))
      .finally(() => setLoading(false))
  }
  useEffect(load, [])
  const visible = events.filter(item => filter === 'all' || (filter === 'drafts' ? !item.isPublished : new Date(item.startsAt) >= new Date()))
  return <section>
    <div className="admin-section-heading"><div><p className="eyebrow">Oversikt og redigering</p><h1>Mine arrangementer</h1></div><Link className="admin-primary" to="/admin/events/new">+ Legg til arrangement</Link></div>
    {notice && <p role="status" className="admin-success">{notice}</p>}
    <label htmlFor="event-filter">Vis arrangementer</label><select id="event-filter" value={filter} onChange={event => setFilter(event.target.value)}><option value="upcoming">Kommende</option><option value="drafts">Utkast</option><option value="all">Alle, også tidligere</option></select>
    {loading && <p role="status">Henter arrangementer …</p>}
    {error && <div role="alert" className="admin-error"><p>{error}</p><button className="admin-secondary" type="button" onClick={load}>Prøv igjen</button></div>}
    {!loading && !error && visible.length === 0 && <div className="admin-card"><h2>Ingen arrangementer i denne visningen.</h2><p>Velg en annen visning eller legg til et nytt arrangement.</p></div>}
    <div className="admin-event-list">{!error && visible.map(item => <article className="admin-card" key={item.id}>
      <EventImage imageUrl={item.imageUrl} />
      <span className={'admin-status ' + (item.isPublished ? 'published' : '')}>{item.isPublished ? (new Date(item.startsAt) < new Date() ? 'Tidligere arrangement' : 'Synlig på nettsiden') : 'Utkast – bare synlig for deg'}</span>
      <h2>{item.title}</h2><p><time dateTime={item.startsAt}>{eventDate.format(new Date(item.startsAt))}</time><br />{item.location}</p>
      <div className="admin-stack"><Link className="admin-secondary" to={'/admin/events/' + item.id}>Se og endre arrangement</Link>
        {!item.isPublished && <Link className="admin-primary" to={'/admin/events/' + item.id + '?preview=1'}>Se gjennom og publiser</Link>}</div>
      <details><summary>Flere valg</summary><div className="admin-stack">{item.isPublished && <button type="button" className="admin-secondary" onClick={() => { setActionError(''); setConfirmation({ item, action: 'hide' }) }}>Skjul fra nettsiden</button>}
        <button type="button" className="admin-delete-link" onClick={() => { setActionError(''); setConfirmation({ item, action: 'delete' }) }}>Slett arrangement</button></div></details>
    </article>)}</div>
    {confirmation && <ConfirmDialog title={confirmation.action === 'delete' ? 'Slette arrangementet?' : 'Skjule arrangementet?'}
      description={confirmation.action === 'delete' ? '«' + confirmation.item.title + '» blir slettet. Denne handlingen kan ikke angres.' : '«' + confirmation.item.title + '» blir et utkast. Du kan publisere det igjen senere.'}
      confirmLabel={confirmation.action === 'delete' ? 'Ja, slett arrangementet' : 'Ja, skjul fra nettsiden'} busy={busy} error={actionError} onClose={() => setConfirmation(null)}
      onConfirm={async () => {
        setBusy(true); setActionError('')
        try {
          const { item, action } = confirmation
          if (action === 'delete') {
            await api('/api/admin/events/' + item.id, { method: 'DELETE' })
            setEvents(previous => previous.filter(entry => entry.id !== item.id))
          } else {
            const updated = await api<AdminEvent>('/api/admin/events/' + item.id + '/publication', { method: 'PATCH', body: JSON.stringify({ isPublished: false }) })
            setEvents(previous => previous.map(entry => entry.id === updated.id ? updated : entry))
          }
          setConfirmation(null); setNotice(action === 'delete' ? 'Arrangementet er slettet.' : 'Arrangementet er skjult fra nettsiden.')
        } catch (reason) { setActionError(reason instanceof Error ? reason.message : 'Handlingen kunne ikke fullføres.') }
        finally { setBusy(false) }
      }} />}
  </section>
}
