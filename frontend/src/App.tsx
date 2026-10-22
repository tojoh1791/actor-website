import { Link, NavLink, Route, Routes } from 'react-router-dom'
import EventsPage from './pages/EventsPage'

function HomePage() {
  return <section className="hero">
    <p className="eyebrow">Skuespill · Foredrag · Fortellinger</p>
    <h1>Historier som<br />skaper møter.</h1>
    <p>En plass for scenekunst, livshistorier og samtaler med publikum.</p>
    <Link className="button" to="/events">Se kommende arrangementer</Link>
  </section>
}

function AboutPage() {
  return <section><p className="eyebrow">Bak historiene</p><h1>Om skuespilleren</h1><p>Biografi, skuespillerbakgrunn og historien bak boken kommer her.</p></section>
}

function ContactPage() {
  return <section><p className="eyebrow">La oss møtes</p><h1>Kontakt og booking</h1><p>Kontaktinformasjon for foredrag, forestillinger og samarbeid legges til når innholdet er avklart.</p></section>
}

function AdminPage() {
  return <section><h1>Administrasjon</h1><p>Innlogging og administrasjon av arrangementer utvikles i neste steg.</p></section>
}

export default function App() {
  return <>
    <a className="skip-link" href="#main">Hopp til innhold</a>
    <header className="site-header"><Link className="brand" to="/">Skuespiller</Link><nav aria-label="Hovedmeny">
      <NavLink to="/" end>Hjem</NavLink><NavLink to="/about">Om</NavLink><NavLink to="/events">Arrangementer</NavLink><NavLink to="/contact">Kontakt</NavLink>
    </nav></header>
    <main id="main" tabIndex={-1}><Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="/about" element={<AboutPage />} />
      <Route path="/events" element={<EventsPage />} />
      <Route path="/contact" element={<ContactPage />} />
      <Route path="/admin" element={<AdminPage />} />
      <Route path="/admin/login" element={<AdminPage />} />
      <Route path="*" element={<section><h1>Siden finnes ikke</h1><Link to="/">Til forsiden</Link></section>} />
    </Routes></main>
    <footer>Skuespill · Foredrag · Fortellinger</footer>
  </>
}
