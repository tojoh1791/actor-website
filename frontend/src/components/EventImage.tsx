import { useState } from 'react'

export const defaultEventImage = '/images/arben-bala.jpg'

export default function EventImage({
  imageUrl,
  className = '',
}: {
  imageUrl?: string | null
  className?: string
}) {
  const [failedUrl, setFailedUrl] = useState<string | null>(null)
  const source = imageUrl && failedUrl !== imageUrl ? imageUrl : defaultEventImage
  return (
    <figure
      className={
        'event-image ' +
        (source === defaultEventImage ? 'event-image-default ' : 'event-image-custom ') +
        className
      }
    >
      <img
        src={source}
        alt={source === defaultEventImage ? 'Arben Bala' : 'Bilde til arrangementet'}
        loading="lazy"
        onError={() => setFailedUrl(imageUrl ?? null)}
      />
      {source === defaultEventImage && <figcaption>Foto: Are Næsset</figcaption>}
    </figure>
  )
}
