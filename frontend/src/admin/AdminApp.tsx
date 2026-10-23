import { useEffect, useState } from 'react'
import { Link, Navigate, Route, Routes, useLocation } from 'react-router-dom'
import { AuthProvider, useAuth } from './Auth'
import LoginPage from './LoginPage'
import EventList from './EventList'
import EventEditor from './EventEditor'
import BookingInbox from './BookingInbox'
import BookingDetailsPage from './BookingDetailsPage'
import { api } from './api'
import './admin.css'

function Dashboard() {
  const [newCount, setNewCount] = useState<number | null>(null)
  const [countError, setCountError] = useState(false)
  useEffect(() => {
    let active = true
    api<{ newCount: number }>('/api/admin/bookings/summary').then(value => { if (active) setNewCount(value.newCount) })
      .catch(() => { if (active) setCountError(true) })
    return () => { active = false }
  }, [])
  return <section className="admin-dashboard"><p className="eyebrow">Ditt kontrollpanel</p><h1>Hei, Arben.</h1><p>Hva vil du gjøre?</p>
    <div className="admin-stack"><Link className="admin-primary" to="/admin/events/new">+ Legg til arrangement</Link>
      <Link className="admin-menu-card" to={newCount ? '/admin/bookings?status=new' : '/admin/bookings'}><span><strong>Forespørsler {newCount !== null && newCount > 0 && <span className="admin-new-count">{newCount} {newCount === 1 ? 'ny' : 'nye'}</span>}</strong><span>{countError ? 'Åpne innboksen for å hente forespørslene' : newCount === null ? 'Henter antall nye …' : newCount ? 'Nye arrangører venter på svar' : 'Les henvendelser og følg opp avtaler'}</span></span><span aria-hidden="true">→</span></Link>
      <Link className="admin-menu-card" to="/admin/events"><span><strong>Mine arrangementer</strong><span>Endre innhold og hva publikum ser</span></span><span aria-hidden="true">→</span></Link>
      <a className="admin-menu-card" href="/events" target="_blank" rel="noreferrer"><span><strong>Se den offentlige nettsiden</strong><span>Åpnes i en ny fane</span></span><span aria-hidden="true">↗</span></a></div>
  </section>
}

function AdminRoutes() {
  const auth = useAuth()
  const { pathname } = useLocation()
  const path = pathname.replace(/\/+$/, '')
  const showNavigation = auth.user && path !== '/admin' && path !== '/admin/login'
  const parent = path.startsWith('/admin/events/') ? { to: '/admin/events', label: 'Til arrangementene' }
    : path.startsWith('/admin/bookings/') ? { to: '/admin/bookings', label: 'Til forespørslene' } : null
  const [logoutError, setLogoutError] = useState('')
  const [loggingOut, setLoggingOut] = useState(false)
  return <div className="admin-shell">
    <a className="skip-link" href="#main">Hopp til innhold</a>
    <header className="admin-header"><Link className="brand" to="/admin">Arben Bala<span className="brand-dot">.</span></Link><span>Kontrollpanel</span>
      {auth.user && <button className="admin-logout" disabled={loggingOut} onClick={async () => {
        setLoggingOut(true); setLogoutError('')
        try { await auth.logout() } catch (reason) { setLogoutError(reason instanceof Error ? reason.message : 'Kunne ikke logge ut.') }
        finally { setLoggingOut(false) }
      }}>{loggingOut ? 'Logger ut …' : 'Logg ut'}</button>}</header>
    <main id="main" className="admin-main" tabIndex={-1}>
      {showNavigation && <nav className="admin-navigation" aria-label="Navigasjon i kontrollpanelet">
        <Link className="admin-overview-link" to="/admin"><span aria-hidden="true">←</span> Til oversikten</Link>
        {parent && <Link className="admin-parent-link" to={parent.to}>{parent.label}</Link>}
      </nav>}
      {logoutError && <p role="alert" className="admin-error">{logoutError}</p>}
      {auth.loading ? <p role="status">Kontrollerer innloggingen …</p> : auth.error ? <div role="alert" className="admin-error"><p>{auth.error}</p><button className="admin-secondary" onClick={() => void auth.refresh()}>Prøv igjen</button></div>
        : !auth.user && pathname !== '/admin/login' ? <Navigate to="/admin/login" replace />
        : <Routes>
          <Route path="/admin/login" element={<LoginPage />} />
          <Route path="/admin" element={<Dashboard />} />
          <Route path="/admin/events" element={<EventList />} />
          <Route path="/admin/events/new" element={<EventEditor key="new" />} />
          <Route path="/admin/events/:id" element={<EventEditor key={pathname} />} />
          <Route path="/admin/bookings" element={<BookingInbox />} />
          <Route path="/admin/bookings/:id" element={<BookingDetailsPage key={pathname} />} />
          <Route path="*" element={<section><h1>Siden finnes ikke</h1><p>Bruk knappen over for å gå tilbake til kontrollpanelet.</p></section>} />
        </Routes>}
    </main>
    <footer className="admin-footer">{auth.user ? <span>Arben Balas kontrollpanel</span> : <Link to="/">Til den offentlige nettsiden</Link>}<span>Bare for administrator</span></footer>
  </div>
}
export default function AdminApp() { return <AuthProvider><AdminRoutes /></AuthProvider> }
