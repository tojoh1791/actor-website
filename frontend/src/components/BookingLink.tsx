import { Link } from 'react-router-dom'
import { contactSettings } from '../content/site'

export default function BookingLink({ className = 'button' }: { className?: string }) {
  const { bookingUrl, email } = contactSettings
  if (bookingUrl) {
    return bookingUrl.startsWith('/')
      ? <Link className={className} to={bookingUrl}>Send bookingforespørsel <span aria-hidden="true">↗</span></Link>
      : <a className={className} href={bookingUrl}>Send bookingforespørsel <span aria-hidden="true">↗</span></a>
  }
  if (email) {
    return <a className={className} href={'mailto:' + email + '?subject=' + encodeURIComponent('Bookingforespørsel til Arben Bala')}>Send bookingforespørsel <span aria-hidden="true">↗</span></a>
  }
  return <Link className={className} to="/contact">Kontakt og booking <span aria-hidden="true">↗</span></Link>
}
