import { useEffect } from 'react'
import { Link, NavLink, Route, Routes, useLocation } from 'react-router-dom'
import BookingLink from './components/BookingLink'
import HomePage from './pages/HomePage'
import AboutPage from './pages/AboutPage'
import TalksPage from './pages/TalksPage'
import BookPage from './pages/BookPage'
import ContactPage from './pages/ContactPage'
import EventsPage from './pages/EventsPage'
import AdminApp from './admin/AdminApp'

const pageTitles: Record<string, string> = {
  '/': 'Arben Bala | Skuespiller og foredragsholder',
  '/about': 'Om Arben | Arben Bala',
  '/talks': 'Uten frykt – foredrag | Arben Bala',
  '/book': 'Balkanbrødre | Arben Bala',
  '/events': 'Kommende arrangementer | Arben Bala',
  '/contact': 'Kontakt og booking | Arben Bala',
  '/admin': 'Administrasjon | Arben Bala',
  '/admin/login': 'Innlogging | Arben Bala',
  '/admin/bookings': 'Forespørsler | Arben Bala',
}

function PageNavigation() {
  const { pathname } = useLocation()
  useEffect(() => {
    document.title = pageTitles[pathname] ?? (pathname.startsWith('/admin/') ? 'Kontrollpanel | Arben Bala' : 'Siden finnes ikke | Arben Bala')
    window.scrollTo({ top: 0, behavior: 'instant' })
    // Move focus after route changes so keyboard users reach the new content.
    document.getElementById('main')?.focus({ preventScroll: true })
  }, [pathname])
  return null
}

export default function App() {
  const { pathname } = useLocation()
  if (pathname === '/admin' || pathname.startsWith('/admin/')) return <><PageNavigation /><AdminApp /></>
  return <>
    <PageNavigation />
    <a className="skip-link" href="#main">Hopp til innhold</a>
    <header className="site-header"><Link className="brand" to="/" aria-label="Arben Bala – forsiden">Arben Bala<span className="brand-dot">.</span></Link><nav aria-label="Hovedmeny"><NavLink to="/about">Om Arben</NavLink><NavLink to="/talks">Foredrag</NavLink><NavLink to="/book">Boken</NavLink><NavLink to="/events">Arrangementer</NavLink><NavLink className="nav-contact" to="/contact">Kontakt og booking <span aria-hidden="true">↗</span></NavLink></nav></header>
    <main id="main" tabIndex={-1}><Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="/about" element={<AboutPage />} />
      <Route path="/talks" element={<TalksPage />} />
      <Route path="/book" element={<BookPage />} />
      <Route path="/events" element={<EventsPage />} />
      <Route path="/contact" element={<ContactPage />} />


      <Route path="*" element={<section className="page-heading"><h1>Siden finnes ikke</h1><Link to="/">Til forsiden</Link></section>} />
    </Routes></main>
    <footer className="site-footer"><div><Link className="brand" to="/">Arben Bala<span className="brand-dot">.</span></Link><p>Skuespiller og foredragsholder</p></div><div className="footer-links"><Link to="/events">Arrangementer</Link><BookingLink className="text-link" /></div><p className="footer-credit">Portrett: Are Næsset</p></footer>
  </>
}
