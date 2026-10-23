import { useEffect, useState } from 'react'
import type { ChangeEvent } from 'react'
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { ApiError, api, eventDate, fromOsloInput, toOsloInput } from './api'
import type { AdminEvent, EventInput } from './api'
import EventImage from '../components/EventImage'
import { bookingDate } from '../types/booking'
import type { BookingEventDraft } from '../types/booking'

interface FormValues {
  title: string; description: string; startDate: string; startTime: string; endsAt: string
  location: string; address: string; organizer: string; externalUrl: string
}
const blank: FormValues = { title: '', description: '', startDate: '', startTime: '', endsAt: '', location: '', address: '', organizer: '', externalUrl: '' }
function previewTime(value: string) {
  try { return eventDate.format(new Date(fromOsloInput(value))) }
  catch { return value.replace('T', ' kl. ') + ' (norsk tid)' }
}

export default function EventEditor() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [search] = useSearchParams()
  const bookingId = !id ? search.get('booking') : null
  const [bookingDraft, setBookingDraft] = useState<BookingEventDraft | null>(null)
  const [form, setForm] = useState<FormValues>(blank)
  const [current, setCurrent] = useState<AdminEvent | null>(null)
  const [loading, setLoading] = useState(Boolean(id || bookingId))
  const [loadError, setLoadError] = useState('')
  const [error, setError] = useState('')
  const [fields, setFields] = useState<Record<string, string[]>>({})
  const [preview, setPreview] = useState(false)
  const [busy, setBusy] = useState(false)
  const [retry, setRetry] = useState(0)
  const [imageUrl, setImageUrl] = useState<string | null>(null)
  const [imageFile, setImageFile] = useState<File | null>(null)
  const [localImageUrl, setLocalImageUrl] = useState<string | null>(null)
  const [imageError, setImageError] = useState('')
  useEffect(() => {
    if (!imageFile) { setLocalImageUrl(null); return }
    const url = URL.createObjectURL(imageFile)
    setLocalImageUrl(url)
    return () => URL.revokeObjectURL(url)
  }, [imageFile])
  useEffect(() => {
    if (!id && !bookingId) return
    let active = true
    setLoading(true); setLoadError('')
    const load = async () => {
      if (bookingId) {
        const draft = await api<BookingEventDraft>('/api/admin/bookings/' + encodeURIComponent(bookingId) + '/event-draft')
        if (!active) return
        setBookingDraft(draft)
        setForm({ ...blank, title: draft.title, description: draft.description, startDate: draft.preferredDate ?? '', location: draft.location, organizer: draft.organizer ?? '' })
        return
      }
      const item = await api<AdminEvent>('/api/admin/events/' + id)
      if (!active) return
      setCurrent(item)
      setImageUrl(item.imageUrl)
      setImageFile(null)
      const [startDate, startTime] = toOsloInput(item.startsAt).split('T')
      setForm({ title: item.title, description: item.description, startDate, startTime,
        endsAt: item.endsAt ? toOsloInput(item.endsAt) : '', location: item.location,
        address: item.address, organizer: item.organizer ?? '', externalUrl: item.externalUrl ?? '' })
      setPreview(search.get('preview') === '1')
    }
    load().catch(reason => { if (active) setLoadError(reason instanceof ApiError && reason.status === 404 && bookingId ? 'Forespørselen finnes ikke. Den kan være slettet.' : reason instanceof Error ? reason.message : 'Kunne ikke hente arrangementet.') })
      .finally(() => { if (active) setLoading(false) })
    return () => { active = false }
  }, [id, bookingId, search, retry])

  const input = (): EventInput => {
    const { startDate, startTime, ...details } = form
    if (!startDate || !startTime) throw new Error('Velg dato og klokkeslett for arrangementet.')
    return { ...details, startsAt: fromOsloInput(startDate + 'T' + startTime), endsAt: form.endsAt ? fromOsloInput(form.endsAt) : null,
      organizer: form.organizer || null, externalUrl: form.externalUrl || null, imageUrl }
  }
  function validate(): EventInput | null {
    setError(''); setFields({})
    try {
      const value = input()
      if (value.endsAt && new Date(value.endsAt) <= new Date(value.startsAt)) {
        setFields({ endsAt: ['Slutt må være etter start.'] }); setError('Slutt må være etter start.'); return null
      }
      return value
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Kontroller dato og klokkeslett.'); return null
    }
  }
  async function save(publish: boolean) {
    const value = validate()
    if (!value) return
    setBusy(true)
    try {
      if (imageFile) {
        let imageBytes: ArrayBuffer
        try { imageBytes = await imageFile.arrayBuffer() }
        catch { throw new Error('Bildet kunne ikke leses fra enheten. Gå tilbake og velg bildet på nytt.') }
        const uploaded = await api<{ imageUrl: string }>('/api/admin/event-images', {
          method: 'POST', body: imageBytes, headers: { 'Content-Type': imageFile.type || 'application/octet-stream' },
        })
        value.imageUrl = uploaded.imageUrl
        setImageUrl(uploaded.imageUrl)
        setImageFile(null)
      }
      const saved = await api<AdminEvent>('/api/admin/events/' + (current?.id ?? ''), {
        method: current ? 'PUT' : 'POST', body: JSON.stringify(value),
      })
      setCurrent(saved)
      if (publish && !saved.isPublished) {
        await api('/api/admin/events/' + saved.id + '/publication', { method: 'PATCH', body: JSON.stringify({ isPublished: true }) })
      }
      navigate('/admin/events', { replace: true, state: { notice: publish || saved.isPublished ? 'Arrangementet er lagret og synlig på nettsiden.' : 'Arrangementet er lagret som utkast. Bare du kan se det.' } })
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Kunne ikke lagre. Prøv igjen.')
      if (reason instanceof ApiError) setFields(reason.fields)
    } finally { setBusy(false) }
  }
  function chooseImage(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0]
    event.target.value = ''
    if (!file) return
    setImageError('')
    if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type)) {
      setImageError('Velg et JPG-, PNG- eller WebP-bilde.'); return
    }
    if (file.size > 8 * 1024 * 1024) {
      setImageError('Bildet må være mindre enn 8 MB.'); return
    }
    setImageFile(file)
  }
  function field(name: keyof FormValues, label: string, options: { type?: string; required?: boolean; maxLength?: number; multiline?: boolean } = {}) {
    const fieldErrors = fields[name] ?? (name === 'startDate' || name === 'startTime' ? fields.startsAt : undefined)
    const shared = { id: 'event-' + name, name, value: form[name], required: options.required,
      maxLength: options.maxLength, 'aria-invalid': Boolean(fieldErrors), 'aria-describedby': fieldErrors ? 'error-' + name : undefined,
      onChange: (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => setForm(previous => ({ ...previous, [name]: event.target.value })) }
    return <div className="admin-field"><label htmlFor={shared.id}>{label}{!options.required && <span className="admin-optional"> (valgfritt)</span>}</label>
      {options.multiline ? <textarea {...shared} rows={5} /> : <input {...shared} type={options.type ?? 'text'} />}
      {fieldErrors && <p id={'error-' + name} className="admin-field-error">{fieldErrors.join(' ')}</p>}</div>
  }
  if (loading) return <p role="status">Henter arrangementet …</p>
  if (loadError) return <div role="alert" className="admin-error"><p>{loadError}</p><button className="admin-secondary" onClick={() => setRetry(retry + 1)}>Prøv igjen</button></div>
  return <section className="admin-editor">
    <p className="eyebrow">{preview ? 'Se gjennom før du lagrer' : current ? 'Rediger arrangement' : 'Nytt arrangement'}</p>
    <h1>{preview ? 'Ser dette riktig ut?' : current ? 'Endre arrangement' : 'Legg til arrangement'}</h1>
    {bookingDraft && <div className="admin-booking-source"><p>Hentet fra en avtalt booking. Kontroller datoen, fyll inn klokkeslett og adresse, og tilpass teksten for publikum. Den private meldingen og kontaktopplysningene er ikke kopiert.</p><Link to={'/admin/bookings/' + bookingId}>Tilbake til forespørselen</Link></div>}
    {error && <div role="alert" className="admin-error"><p>{error}</p>{preview && <button type="button" className="admin-secondary" onClick={() => setPreview(false)}>Gå tilbake til feltene</button>}</div>}
    {preview ? <>
      <p>Se over hva publikum vil få se.</p>
      <article className="admin-card admin-preview"><EventImage imageUrl={localImageUrl || imageUrl} /><h2>{form.title}</h2><p>{form.startDate && form.startTime && previewTime(form.startDate + 'T' + form.startTime)}</p><p><strong>{form.location}</strong><br />{form.address}</p>
        <p className="admin-description">{form.description}</p>{form.endsAt && <p>Slutter: {previewTime(form.endsAt)}</p>}{form.organizer && <p>Arrangør: {form.organizer}</p>}{form.externalUrl && <p className="admin-url">Lenke: {form.externalUrl}</p>}</article>
      <div className="admin-stack"><button type="button" className="admin-primary" disabled={busy} onClick={() => void save(true)}>{busy ? 'Lagrer …' : current?.isPublished ? 'Lagre endringene på nettsiden' : 'Vis på nettsiden'}</button>
        {!current?.isPublished && <button type="button" className="admin-secondary" disabled={busy} onClick={() => void save(false)}>Lagre som utkast</button>}
        <button type="button" className="admin-secondary" disabled={busy} onClick={() => setPreview(false)}>Gå tilbake og endre</button></div>
      <p className="admin-note">Utkast er bare synlige for deg. Publiserte, kommende arrangementer vises for alle.</p>
    </> : <form onSubmit={event => { event.preventDefault(); if (validate()) { setPreview(true); window.scrollTo({ top: 0 }) } }}>
      <p>Fyll inn detaljene. Du får se gjennom før du lagrer.</p>
      {field('title', 'Hva heter arrangementet?', { required: true, maxLength: 200 })}
      <fieldset className="admin-image-picker"><legend>Bilde til arrangementet <span className="admin-optional">(valgfritt)</span></legend>
        <EventImage imageUrl={localImageUrl || imageUrl} />
        <p className="admin-note">{imageFile ? 'Valgt bilde: ' + imageFile.name : imageUrl ? 'Dette bildet vises sammen med arrangementet.' : 'Portrettet av Arben brukes som standardbilde.'}</p>
        <label className="admin-secondary admin-file-button">{imageFile || imageUrl ? 'Bytt bilde' : 'Velg bilde'}
          <input type="file" accept="image/jpeg,image/png,image/webp" onChange={chooseImage} aria-describedby="image-help" />
        </label>
        {(imageFile || imageUrl) && <button type="button" className="admin-secondary" onClick={() => { setImageFile(null); setImageUrl(null); setImageError('') }}>Bruk standardbildet</button>}
        <p className="admin-note" id="image-help">Velg fra telefonen eller datamaskinen. JPG, PNG eller WebP, maks. 8 MB og 40 megapiksler. Bildet lagres når du lagrer arrangementet.</p>
        {(imageError || fields.imageUrl) && <p role="alert" className="admin-field-error">{imageError || fields.imageUrl?.join(' ')}</p>}
      </fieldset>
      {field('description', 'Hva skal publikum få vite?', { required: true, maxLength: 10000, multiline: true })}
      {bookingDraft && <p className="admin-note">Ønsket dato i forespørselen: {bookingDate(bookingDraft.preferredDate)}. Kontroller at dette er datoen dere har avtalt.</p>}
      <div className="admin-date-time">
        {field('startDate', 'Dato', { type: 'date', required: true })}
        {field('startTime', 'Klokkeslett', { type: 'time', required: true })}
      </div>
      <p className="admin-note">Alle klokkeslett er i norsk tid (Europe/Oslo).</p>
      {field('location', 'Hvor skal det være?', { required: true, maxLength: 200 })}
      {field('address', 'Adresse', { required: true, maxLength: 500 })}
      <details open={Boolean(fields.endsAt || fields.organizer || fields.externalUrl)}><summary>Flere opplysninger (valgfritt)</summary>
        {field('endsAt', 'Sluttdato og klokkeslett', { type: 'datetime-local' })}
        {field('organizer', 'Arrangør', { maxLength: 200 })}
        {field('externalUrl', 'Lenke til billetter eller mer informasjon', { type: 'url', maxLength: 2048 })}<p className="admin-note">Bruk en fullstendig lenke som begynner med https://.</p>
      </details>
      <div className="admin-stack"><button type="submit" className="admin-primary">Se gjennom arrangementet →</button><Link className="admin-secondary" to={bookingId ? '/admin/bookings/' + bookingId : '/admin/events'}>Avbryt</Link></div>
    </form>}
  </section>
}
