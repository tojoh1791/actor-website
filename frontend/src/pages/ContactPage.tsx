import { Link } from 'react-router-dom'
import BookingLink from '../components/BookingLink'
import { contactSettings } from '../content/site'

export default function ContactPage() {
  const hasChannel = Boolean(contactSettings.bookingUrl || contactSettings.email || contactSettings.phone)
  return <>
    <section className="contact-intro"><p className="eyebrow">For arrangører</p><h1>Et møte starter<br /><em>med en samtale.</em></h1><p className="lead">Foredrag, kulturarrangement eller et nytt samarbeid med Arben Bala?</p><p>Les om «Uten frykt» og tenk gjerne gjennom hvem dere ønsker å samle, hvor og når.</p></section>
    <section className="contact-grid section-space" aria-label="Booking og kontakt">
      <div className="contact-channel"><p className="eyebrow">Kontakt og booking</p>{hasChannel ? <><h2>Ta kontakt.</h2>{(contactSettings.bookingUrl || contactSettings.email) && <BookingLink />}{contactSettings.email && <p><a href={'mailto:' + contactSettings.email}>{contactSettings.email}</a></p>}{contactSettings.phone && <p><a href={'tel:' + contactSettings.phone.replaceAll(' ', '')}>{contactSettings.phone}</a></p>}</> : <><h2>Kontaktinformasjon<br />kommer.</h2><p>Bookingkanalen er ikke åpnet på nettsiden ennå. Kontaktinformasjon legges ut her når den er klar.</p><Link className="text-link" to="/talks">Les om foredraget <span aria-hidden="true">→</span></Link></>}</div>
      <div className="booking-preparation"><h2>Planlegger du et arrangement?</h2><p>Dette er nyttig å ha med i en fremtidig forespørsel:</p><ul><li>Navn på arrangør og kontaktperson</li><li>Ønsket dato og sted</li><li>Type arrangement og målgruppe</li><li>Omtrent hvor mange som skal delta</li></ul><p className="small-note">En forespørsel er utgangspunktet for en avtale. Dato og innhold må bekreftes.</p></div>
    </section>
    <section className="audience-note"><div><p className="eyebrow">For deg som vil komme og lytte</p><h2>Finn ditt neste arrangement.</h2></div><Link className="text-link" to="/events">Se hva som skjer <span aria-hidden="true">→</span></Link></section>
  </>
}

