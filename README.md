# Actor Website

Nettside for en skuespiller, med kommende arrangementer og planlagt administrasjon.

## Status

Prosjektgrunnlaget er satt opp. Frontend har offentlige sider og henter kommende arrangementer fra API-et. Backend har PostgreSQL-tilkobling, arrangementsmodell, Identity-datamodell og første databasemigrering.

Innlogging, administratoropprettelse og oppretting/redigering/sletting/publisering av arrangementer er **ikke implementert ennå**. Nettsiden har kildebasert informasjon om Arben Bala, foredraget Uten frykt og romanen Balkanbrødre, med portrett og bokomslag. Kontakt- og bookingkanal er ikke valgt ennå. Ingen demoarrangementer eller brukere opprettes automatisk.

## Teknologi

- React, TypeScript, Vite og React Router
- ASP.NET Core / .NET 10
- Entity Framework Core og Npgsql
- ASP.NET Core Identity som grunnlag for senere innlogging
- PostgreSQL 17 via Docker Compose

## Forutsetninger

- Node.js 24 LTS med npm
- .NET 10 SDK (10.0.203 eller nyere kompatibel 10.0 SDK)
- Docker Desktop med Linux-containere, startet og klar
- Git

Kjør alle kommandoene nedenfor fra prosjektets rotmappe.

## Første oppsett

1. Installer frontend-pakkene: `npm --prefix frontend ci`
2. Installer .NET-pakkene: `dotnet restore ActorWebsite.slnx`
3. Installer prosjektets lokale databaseverktøy: `npm run tools:restore`
4. Generer lokal konfigurasjon: `npm run init:local`
5. Start databasen: `npm run db:up`
6. Opprett databasetabellene: `npm run db:migrate`

Steg 4 lager en ignorert .env med et tilfeldig databasepassord og beholder eksisterende konfigurasjon ved gjentatt kjøring. .env.example inneholder kun eksempelverdier. Ikke legg ekte passord i kildekoden.

## Start utviklingsmiljøet

Åpne to terminaler i prosjektroten:

- Terminal 1: `npm run dev:api`
- Terminal 2: `npm run dev:web`

Åpne http://localhost:5173 i nettleseren. API-et bruker http://localhost:5080. Vite videresender forespørsler under /api til backend.

Hvis du kjører backend direkte med dotnet, må du selv sette ConnectionStrings__DefaultConnection. npm-skriptene for API og migrering laster .env automatisk.

## Verifisering

- `npm run check` bygger frontend og backend.
- http://localhost:5080/api/health viser om API-et kjører.
- http://localhost:5080/api/health/ready sjekker database og arrangementstabell; returnerer 503 når disse ikke er klare.
- http://localhost:5080/api/events returnerer publiserte arrangementer med starttidspunkt i fremtiden, sortert etter start. En ny database gir en tom liste.
- http://localhost:5080/openapi/v1.json viser API-beskrivelsen i utviklingsmiljøet.

Prosjektet inkluderer også en HTTP-fil med eksempelspørringer og en GitHub Actions-arbeidsflyt som bygger begge delene. Automatiserte funksjonstester legges til når administrasjonsflyten implementeres.

## Stoppe

Stopp utviklingsserverne med Ctrl+C. `npm run db:stop` stopper databasen og beholder data i Docker-volumet.

## Feilsøking

- Manglende Docker-motor: åpne Docker Desktop og vent til motoren er klar før db:up.
- Opptatt port: database bruker 5432, API 5080 og frontend 5173. Stopp prosessen som bruker porten, eller tilpass konfigurasjonen.
- Arrangementer kan ikke lastes: sjekk API, readiness-endepunktet og at db:migrate er kjørt.
- Endret databasepassord: PostgreSQL bruker passordet fra da volumet ble initialisert. Å endre .env oppdaterer ikke passordet i en eksisterende database.

Dette er et lokalt utviklingsoppsett. Produksjonspublisering, HTTPS, administratorflyt og produksjonskonfigurasjon kommer senere.

Se docs/ for prosjektplanen og docs/development.md for videre arbeid.
