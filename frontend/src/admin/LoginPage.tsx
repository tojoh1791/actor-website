import { errorMessage } from '../lib/http'
import { useState } from 'react'
import { Navigate } from 'react-router-dom'
import { useAuth } from './useAuth'

export default function LoginPage() {
  const auth = useAuth()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [rememberMe, setRememberMe] = useState(false)
  const [visible, setVisible] = useState(false)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  if (auth.user) return <Navigate to="/admin" replace />
  return (
    <section className="admin-login">
      <p className="eyebrow">Bare for administrator</p>
      <h1>Velkommen, Arben.</h1>
      <p>Logg inn for å håndtere forespørsler og arrangementer.</p>
      <form
        onSubmit={async (event) => {
          event.preventDefault()
          setBusy(true)
          setError('')
          try {
            await auth.login(email, password, rememberMe)
          } catch (reason) {
            setError(errorMessage(reason, 'Kunne ikke logge inn.'))
          } finally {
            setBusy(false)
          }
        }}
      >
        <label htmlFor="admin-email">E-post</label>
        <input
          id="admin-email"
          type="email"
          autoComplete="username"
          autoCapitalize="none"
          spellCheck={false}
          required
          maxLength={254}
          value={email}
          onChange={(event) => setEmail(event.target.value)}
        />
        <label htmlFor="admin-password">Passord</label>
        <div className="admin-password">
          <input
            id="admin-password"
            type={visible ? 'text' : 'password'}
            autoComplete="current-password"
            required
            maxLength={1024}
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />
          <button
            type="button"
            className="admin-small-button"
            aria-controls="admin-password"
            aria-pressed={visible}
            onClick={() => setVisible(!visible)}
          >
            {visible ? 'Skjul' : 'Vis'}
          </button>
        </div>
        <label className="admin-check">
          <input
            type="checkbox"
            checked={rememberMe}
            onChange={(event) => setRememberMe(event.target.checked)}
          />{' '}
          Husk meg på denne enheten
        </label>
        {error && (
          <p role="alert" className="admin-error">
            {error}
          </p>
        )}
        <button className="admin-primary" type="submit" disabled={busy}>
          {busy ? 'Logger inn …' : 'Logg inn'}
        </button>
      </form>
      <details className="admin-help">
        <summary>Trenger du hjelp med innloggingen?</summary>
        <p>
          Bruk e-postadressen som ble valgt da administratorkontoen ble opprettet. Ved glemt passord
          må den som drifter nettsiden hjelpe deg med å sette et nytt.
        </p>
      </details>
    </section>
  )
}
