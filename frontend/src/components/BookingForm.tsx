import { useEffect, useRef, useState } from 'react'
import type { ChangeEvent } from 'react'
import { ApiError, api } from '../lib/http'
import { bookingTypes } from '../types/booking'

interface FormValues {
  contactName: string; email: string; phone: string; organization: string; eventType: string
  preferredDate: string; location: string; participants: string; message: string; website: string
}
const blank: FormValues = { contactName: '', email: '', phone: '', organization: '', eventType: '', preferredDate: '', location: '', participants: '', message: '', website: '' }
interface FormSettings { csrfToken: string; submissionToken: string }

export default function BookingForm() {
  const [form, setForm] = useState<FormValues>(blank)
  const [settings, setSettings] = useState<FormSettings | null>(null)
  const [requestId, setRequestId] = useState(() => crypto.randomUUID())
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const [fields, setFields] = useState<Record<string, string[]>>({})
  const [refreshNeeded, setRefreshNeeded] = useState(false)
  const [reference, setReference] = useState('')
  const [retry, setRetry] = useState(0)
  const feedback = useRef<HTMLDivElement>(null)
  const successHeading = useRef<HTMLHeadingElement>(null)
  useEffect(() => {
    const controller = new AbortController()
    setLoading(true)
    api<FormSettings>('/api/bookings/form', { signal: controller.signal })
      .then(value => { setSettings(value); setError(''); setRefreshNeeded(false) })
      .catch(() => { if (!controller.signal.aborted) setError('Kunne ikke åpne bookingskjemaet. Prøv igjen om litt.') })
      .finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [retry])
  useEffect(() => { if (reference) successHeading.current?.focus() }, [reference])
  useEffect(() => { if (error && !loading) feedback.current?.focus() }, [error, loading])
  const change = (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = event.target
    setForm(previous => ({ ...previous, [name]: value }))
    setRequestId(crypto.randomUUID())
  }
  const field = (name: keyof FormValues, label: string, options: { required?: boolean; type?: string; maxLength?: number; autoComplete?: string; multiline?: boolean } = {}) => {
    const props = { id: 'booking-' + name, name, value: form[name], onChange: change, required: options.required,
      maxLength: options.maxLength, autoComplete: options.autoComplete, 'aria-invalid': Boolean(fields[name]),
      'aria-describedby': fields[name] ? 'booking-error-' + name : undefined }
    return <div className="booking-field"><label htmlFor={props.id}>{label}{!options.required && <span> (valgfritt)</span>}</label>
      {options.multiline ? <textarea {...props} rows={6} minLength={20} /> : <input {...props} type={options.type ?? 'text'}
        {...(name === 'preferredDate' ? { min: new Intl.DateTimeFormat('sv-SE', { timeZone: 'Europe/Oslo' }).format(new Date()) } : {})}
        {...(name === 'participants' ? { min: 1, max: 100000, step: 1 } : {})} />}
      {fields[name] && <p className="booking-field-error" id={'booking-error-' + name}>{fields[name].join(' ')}</p>}</div>
  }
  if (reference) return <div className="booking-receipt" role="status">
    <p className="eyebrow">Takk for henvendelsen</p><h2 ref={successHeading} tabIndex={-1}>Forespørselen er mottatt.</h2>
    <p>Arben kan nå lese forespørselen i kontrollpanelet og kontakte deg på e-post eller telefon for å avtale detaljene.</p>
    <p><strong>Dette er ikke en bekreftet booking.</strong> Dato, pris og innhold må avtales med Arben.</p>
    <p className="small-note">Din referanse: <strong>{reference}</strong>. Ta vare på denne siden. Det sendes ingen automatisk e-postkvittering.</p>
  </div>
  return <div className="booking-form-panel" id="booking-form">
    <p className="eyebrow">Direkte til Arben</p><h2>Send en forespørsel.</h2>
    <p>Fortell litt om hva dere planlegger. Dere trenger ikke ha alle detaljene klare.</p>
    {error && <div className="booking-error" role="alert" tabIndex={-1} ref={feedback}><p>{error}</p>
      {(!settings || refreshNeeded) && <button type="button" className="booking-retry" disabled={loading} onClick={() => setRetry(value => value + 1)}>Last inn skjemaet på nytt</button>}</div>}
    {loading && <p role="status">Åpner skjemaet …</p>}
    {settings && <form onSubmit={async event => {
      event.preventDefault()
      if (busy || loading) return
      setBusy(true); setError(''); setFields({}); setRefreshNeeded(false)
      try {
        const result = await api<{ reference: string }>('/api/bookings/', {
          method: 'POST', headers: { 'X-CSRF-TOKEN': settings.csrfToken },
          body: JSON.stringify({ ...form, requestId, preferredDate: form.preferredDate || null,
            participants: form.participants ? Number(form.participants) : null, submissionToken: settings.submissionToken }),
        })
        setReference(result.reference)
      } catch (reason) {
        setError(reason instanceof Error ? reason.message : 'Kunne ikke sende forespørselen. Prøv igjen.')
        if (reason instanceof ApiError) {
          setFields(reason.fields)
          setRefreshNeeded((reason.status === 400 && Object.keys(reason.fields).length === 0) || reason.status === 409)
        }
      } finally { setBusy(false) }
    }}>
      <fieldset disabled={busy || loading} className="booking-fields">
        <legend className="visually-hidden">Opplysninger om bookingforespørselen</legend>
        <div className="booking-field-row">{field('contactName', 'Navnet ditt', { required: true, maxLength: 100, autoComplete: 'name' })}
          {field('organization', 'Firma eller organisasjon', { maxLength: 200, autoComplete: 'organization' })}</div>
        <div className="booking-field-row">{field('email', 'E-post', { type: 'email', required: true, maxLength: 254, autoComplete: 'email' })}
          {field('phone', 'Telefon', { type: 'tel', maxLength: 40, autoComplete: 'tel' })}</div>
        <div className="booking-field"><label htmlFor="booking-eventType">Hva ønsker dere å booke?</label>
          <select id="booking-eventType" name="eventType" value={form.eventType} onChange={change} required aria-invalid={Boolean(fields.eventType)} aria-describedby={fields.eventType ? 'booking-error-eventType' : undefined}>
            <option value="">Velg type arrangement</option>{Object.entries(bookingTypes).map(([value, label]) => <option value={value} key={value}>{label}</option>)}
          </select>{fields.eventType && <p className="booking-field-error" id="booking-error-eventType">{fields.eventType.join(' ')}</p>}</div>
        <div className="booking-field-row">{field('preferredDate', 'Ønsket dato', { type: 'date' })}{field('participants', 'Omtrent antall deltakere', { type: 'number' })}</div>
        <p className="small-note">La datoen stå tom hvis den ikke er avklart.</p>
        {field('location', 'Hvor skal arrangementet være?', { required: true, maxLength: 300 })}
        {field('message', 'Fortell om arrangementet', { required: true, multiline: true, maxLength: 5000 })}
        <p className="small-note">Beskriv gjerne målgruppe, ønsket innhold og tidspunkt. Minst 20 tegn.</p>
        <div className="booking-honeypot" aria-hidden="true"><label htmlFor="booking-website">Nettside</label><input id="booking-website" name="website" value={form.website} onChange={change} tabIndex={-1} autoComplete="off" maxLength={200} /></div>
        <p className="booking-privacy">Opplysningene brukes til å behandle henvendelsen og kontakte deg. Bare administrator har tilgang til forespørselen. Unngå sensitive opplysninger i meldingen.</p>
        <button className="button booking-submit" type="submit">{busy ? 'Sender …' : 'Send bookingforespørsel'} <span aria-hidden="true">→</span></button>
        <p className="small-note">En forespørsel er uforpliktende. Bookingen må bekreftes av Arben.</p>
      </fieldset>
    </form>}
  </div>
}
