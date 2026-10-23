import { useParams, useSearchParams } from 'react-router-dom'
import { useApiQuery } from '../hooks/useApiQuery'
import type { AdminEvent } from '../types/event'
import type { BookingEventDraft } from '../types/booking'
import AdminRequestError from './AdminRequestError'
import EventEditorForm from './EventEditorForm'

export default function EventEditor() {
  const { id } = useParams()
  const [search] = useSearchParams()
  const bookingId = !id ? search.get('booking') : null
  const path = id
    ? '/api/admin/events/' + id
    : bookingId
      ? '/api/admin/bookings/' + encodeURIComponent(bookingId) + '/event-draft'
      : null
  const { data, loading, error, errorStatus, reload } = useApiQuery<AdminEvent | BookingEventDraft>(
    path,
  )

  if (loading) return <p role="status">Henter arrangementet …</p>
  if (error)
    return (
      <AdminRequestError
        message={
          errorStatus === 404 && bookingId
            ? 'Forespørselen finnes ikke. Den kan være slettet.'
            : error
        }
        onRetry={reload}
      />
    )

  return (
    <EventEditorForm
      key={(path ?? 'new') + ':' + search.get('preview')}
      initialEvent={data && 'id' in data ? data : null}
      bookingDraft={data && !('id' in data) ? data : null}
      bookingId={bookingId}
      previewRequested={Boolean(id && search.get('preview') === '1')}
    />
  )
}
