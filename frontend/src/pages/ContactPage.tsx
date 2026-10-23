import { Link } from 'react-router-dom'
import BookingForm from '../components/BookingForm'
import { contactSettings } from '../content/site'

export default function ContactPage() {
  return (
    <>
      <section className="contact-intro">
        <p className="eyebrow">For arrangører</p>
        <h1>
          Et møte starter
          <br />
          <em>med en samtale.</em>
        </h1>
        <p className="lead">Foredrag, kulturarrangement eller et nytt samarbeid med Arben Bala?</p>
        <p>Les om «Uten frykt» og tenk gjerne gjennom hvem dere ønsker å samle, hvor og når.</p>
      </section>
      <section className="contact-grid section-space" aria-label="Booking og kontakt">
        <BookingForm />
        <aside className="booking-preparation">
          <h2>Hva skjer etterpå?</h2>
          <ol>
            <li>Forespørselen legges i Arbens innboks.</li>
            <li>Arben kan kontakte deg for å finne en løsning som passer.</li>
            <li>Dere avtaler dato, pris og innhold direkte.</li>
          </ol>
          <p className="small-note">
            Bookingen er først avtalt når Arben har bekreftet den. Ingen betaling skjer gjennom
            skjemaet.
          </p>
          <Link className="text-link" to="/talks">
            Les om foredraget <span aria-hidden="true">→</span>
          </Link>
          {contactSettings.email && (
            <p>
              <a href={'mailto:' + contactSettings.email}>{contactSettings.email}</a>
            </p>
          )}
          {contactSettings.phone && (
            <p>
              <a href={'tel:' + contactSettings.phone.replaceAll(' ', '')}>
                {contactSettings.phone}
              </a>
            </p>
          )}
        </aside>
      </section>
      <section className="audience-note">
        <div>
          <p className="eyebrow">For deg som vil komme og lytte</p>
          <h2>Finn ditt neste arrangement.</h2>
        </div>
        <Link className="text-link" to="/events">
          Se hva som skjer <span aria-hidden="true">→</span>
        </Link>
      </section>
    </>
  )
}
