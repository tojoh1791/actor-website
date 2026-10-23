import { useId, useLayoutEffect, useRef, useState } from 'react'
import type { ActorEvent } from '../types/event'
import EventImage from './EventImage'

import { eventDate } from '../lib/dateTime'

export default function EventCard({ event }: { event: ActorEvent }) {
  const [expanded, setExpanded] = useState(false)
  const [hasOverflow, setHasOverflow] = useState(false)
  const card = useRef<HTMLElement>(null)
  const contentId = useId()
  const hasTicketLink = Boolean(event.externalUrl && /^https:\/\//i.test(event.externalUrl))
  const canExpand = hasOverflow || Boolean(event.endsAt || event.organizer || hasTicketLink)

  useLayoutEffect(() => {
    if (expanded || !card.current) return
    let active = true
    const measure = () => {
      if (!active || !card.current) return
      setHasOverflow(
        Array.from(card.current.querySelectorAll<HTMLElement>('[data-collapsed-text]')).some(
          (element) =>
            element.scrollHeight > element.clientHeight + 1 ||
            element.scrollWidth > element.clientWidth + 1,
        ),
      )
    }
    measure()
    // Recheck when wrapping changes, including after resizing or font loading.
    const observer = new ResizeObserver(measure)
    observer.observe(card.current)
    void document.fonts.ready.then(measure)
    return () => {
      active = false
      observer.disconnect()
    }
  }, [expanded, event])

  return (
    <article ref={card} className={'event-card' + (expanded ? ' is-expanded' : '')}>
      <EventImage imageUrl={event.imageUrl} />
      <div className="event-card-body">
        <div className="event-card-content" id={contentId}>
          <time className="event-date" data-collapsed-text dateTime={event.startsAt}>
            {eventDate.format(new Date(event.startsAt))}
          </time>
          <h2 data-collapsed-text>{event.title}</h2>
          <p className="event-place">
            <strong className="event-venue" data-collapsed-text>
              {event.location}
            </strong>
            <span className="event-address" data-collapsed-text>
              {event.address}
            </span>
          </p>
          <p className="event-description" data-collapsed-text>
            {event.description}
          </p>
          <div className="event-extra" hidden={!expanded}>
            {event.endsAt && <p>Slutter: {eventDate.format(new Date(event.endsAt))}</p>}
            {event.organizer && <p>Arrangør: {event.organizer}</p>}
            {hasTicketLink && (
              <p>
                <a href={event.externalUrl!} target="_blank" rel="noreferrer">
                  Billetter og mer informasjon ↗
                </a>
              </p>
            )}
          </div>
        </div>
        <div className="event-card-footer">
          {canExpand && (
            <button
              type="button"
              className="event-toggle"
              aria-expanded={expanded}
              aria-controls={contentId}
              aria-label={(expanded ? 'Mindre om ' : 'Mer om ') + event.title}
              onClick={() => setExpanded((value) => !value)}
            >
              {expanded ? 'Mindre' : 'Mer'} <span aria-hidden="true">{expanded ? '↑' : '↓'}</span>
            </button>
          )}
        </div>
      </div>
    </article>
  )
}
