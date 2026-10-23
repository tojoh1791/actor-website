import { Link } from 'react-router-dom'
import BookingLink from '../components/BookingLink'
import Portrait from '../components/Portrait'
import { site } from '../content/site'

export default function TalksPage() {
  return (
    <>
      <section className="page-split">
        <div>
          <p className="eyebrow">Foredrag med Arben Bala</p>
          <h1>Uten frykt.</h1>
          <p className="lead">Om tilhørighet, ansvar og motet til å velge en annen vei.</p>
          <p>
            Med sin egen historie som utgangspunkt forteller Arben om veien fra Kosovo til Norge og
            om å finne en ny retning i livet.
          </p>
          <p>
            Foredraget tar opp hvordan unge kan trekkes inn i kriminelle miljøer, og inviterer til
            samtaler om press, valg og håp.
          </p>
          <BookingLink />
        </div>
        <Portrait priority />
      </section>
      <section className="section-space" aria-labelledby="themes-title">
        <p className="eyebrow">Temaer i foredraget</p>
        <h2 id="themes-title">Historier som åpner for samtaler.</h2>
        <ul className="theme-list">
          {site.talk.themes.map((theme, index) => (
            <li key={theme}>
              <span aria-hidden="true">0{index + 1}</span>
              <h3>{theme}</h3>
            </li>
          ))}
        </ul>
      </section>
      <section className="invitation section-space">
        <div>
          <p className="eyebrow">Planlegg et møte</p>
          <h2>
            For skoler, arbeidsliv
            <br />
            og kulturarrangører.
          </h2>
          <p>Ønsket dato, sted og målgruppe er et godt utgangspunkt for en bookingforespørsel.</p>
        </div>
        <div>
          <BookingLink />
          <p className="small-note">
            Vil du heller delta som publikum? <Link to="/events">Se offentlige arrangementer.</Link>
          </p>
        </div>
      </section>
    </>
  )
}
