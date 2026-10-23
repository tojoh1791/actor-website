import { errorMessage } from '../lib/http'
import Dashboard from './Dashboard'
import AdminRequestError from './AdminRequestError'
import { useState } from 'react'
import { Link, Navigate, Route, Routes, useLocation } from 'react-router-dom'
import { AuthProvider } from './Auth'
import { useAuth } from './useAuth'
import LoginPage from './LoginPage'
import EventList from './EventList'
import EventEditor from './EventEditor'
import BookingInbox from './BookingInbox'
import BookingDetailsPage from './BookingDetailsPage'
import './admin.css'

function AdminRoutes() {
  const auth = useAuth()
  const { pathname } = useLocation()
  const path = pathname.replace(/\/+$/, '')
  const showNavigation = auth.user && path !== '/admin' && path !== '/admin/login'
  const parent = path.startsWith('/admin/events/')
    ? { to: '/admin/events', label: 'Til arrangementene' }
    : path.startsWith('/admin/bookings/')
      ? { to: '/admin/bookings', label: 'Til forespørslene' }
      : null
  const [logoutError, setLogoutError] = useState('')
  const [loggingOut, setLoggingOut] = useState(false)
  async function logout() {
    if (loggingOut) return
    setLoggingOut(true)
    setLogoutError('')
    try {
      await auth.logout()
    } catch (reason) {
      setLogoutError(errorMessage(reason, 'Kunne ikke logge ut.'))
    } finally {
      setLoggingOut(false)
    }
  }

  return (
    <div className="admin-shell">
      <a className="skip-link" href="#main">
        Hopp til innhold
      </a>
      <header className="admin-header">
        <Link className="brand" to="/admin">
          Arben Bala<span className="brand-dot">.</span>
        </Link>
        <span>Kontrollpanel</span>
        {auth.user && (
          <button className="admin-logout" disabled={loggingOut} onClick={logout}>
            {loggingOut ? 'Logger ut …' : 'Logg ut'}
          </button>
        )}
      </header>
      <main id="main" className="admin-main" tabIndex={-1}>
        {showNavigation && (
          <nav className="admin-navigation" aria-label="Navigasjon i kontrollpanelet">
            <Link className="admin-overview-link" to="/admin">
              <span aria-hidden="true">←</span> Til oversikten
            </Link>
            {parent && (
              <Link className="admin-parent-link" to={parent.to}>
                {parent.label}
              </Link>
            )}
          </nav>
        )}
        {logoutError && (
          <p role="alert" className="admin-error">
            {logoutError}
          </p>
        )}
        {auth.loading ? (
          <p role="status">Kontrollerer innloggingen …</p>
        ) : auth.error ? (
          <AdminRequestError message={auth.error} onRetry={() => void auth.refresh()} />
        ) : !auth.user && pathname !== '/admin/login' ? (
          <Navigate to="/admin/login" replace />
        ) : (
          <Routes>
            <Route path="/admin/login" element={<LoginPage />} />
            <Route path="/admin" element={<Dashboard />} />
            <Route path="/admin/events" element={<EventList />} />
            <Route path="/admin/events/new" element={<EventEditor />} />
            <Route path="/admin/events/:id" element={<EventEditor key={pathname} />} />
            <Route path="/admin/bookings" element={<BookingInbox />} />
            <Route path="/admin/bookings/:id" element={<BookingDetailsPage key={pathname} />} />
            <Route
              path="*"
              element={
                <section>
                  <h1>Siden finnes ikke</h1>
                  <p>Bruk knappen over for å gå tilbake til kontrollpanelet.</p>
                </section>
              }
            />
          </Routes>
        )}
      </main>
      <footer className="admin-footer">
        {auth.user ? (
          <span>Arben Balas kontrollpanel</span>
        ) : (
          <Link to="/">Til den offentlige nettsiden</Link>
        )}
        <span>Bare for administrator</span>
      </footer>
    </div>
  )
}
export default function AdminApp() {
  return (
    <AuthProvider>
      <AdminRoutes />
    </AuthProvider>
  )
}
