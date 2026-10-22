# Frontend

React og TypeScript med Vite og React Router. Offentlige ruter: /, /about, /events og /contact. /admin og /admin/login er foreløpig plassholdere.

Fra prosjektroten:

- Installer: npm --prefix frontend ci
- Start: npm run dev:web
- Bygg: npm run build:web

Utviklingsserveren bruker http://localhost:5173 og videresender /api til http://localhost:5080. Se README.md i prosjektroten for database og backend.

Arrangementslisten har lasting, feil- og tomtilstand. Detaljene bruker HTML details/summary for innebygd tastaturstøtte. Datoer vises i Europe/Oslo.
