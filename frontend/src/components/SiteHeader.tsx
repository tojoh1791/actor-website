import { useRef, useState } from 'react'
import { Link, NavLink } from 'react-router-dom'

export default function SiteHeader() {
  const [open, setOpen] = useState(false)
  const menuButton = useRef<HTMLButtonElement>(null)
  return <header className="site-header" onKeyDown={event => {
    if (event.key === 'Escape' && open) {
      setOpen(false)
      menuButton.current?.focus()
    }
  }}>
    <Link className="brand" to="/" aria-label="Arben Bala – forsiden">Arben Bala<span className="brand-dot">.</span></Link>
    <button ref={menuButton} type="button" className="site-menu-toggle" aria-expanded={open} aria-controls="site-navigation" onClick={() => setOpen(value => !value)}>
      <span aria-hidden="true">{open ? '×' : '☰'}</span> {open ? 'Lukk' : 'Meny'}
    </button>
    <nav id="site-navigation" className={open ? 'is-open' : undefined} aria-label="Hovedmeny">
      <NavLink to="/about">Om Arben</NavLink>
      <NavLink to="/talks">Foredrag</NavLink>
      <NavLink to="/book">Boken</NavLink>
      <NavLink to="/events">Arrangementer</NavLink>
      <NavLink className="nav-contact" to="/contact">Kontakt og booking <span aria-hidden="true">↗</span></NavLink>
    </nav>
  </header>
}
