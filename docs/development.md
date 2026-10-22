# Utviklingsstatus og neste steg

## Satt opp

- React/TypeScript med Vite, React Router og låste installasjoner gjennom package-lock.json.
- ASP.NET Core på .NET 10 med pakkelåser, PostgreSQL-provider og lokalt dotnet-ef-verktøy.
- Databasemodeller for arrangementer og Identity-brukere, og første EF-migrering.
- API for publiserte kommende arrangementer, livstegn og databaseberedskap.
- Offentlige sider med plassholderinnhold, mobiltilpasset grunnlayout og utvidbare arrangementskort.
- Lokal database i Docker, ignorert lokal konfigurasjon og oppstartsveiledning.
- GitHub Actions for bygg av frontend og backend.

## Avgrensning

Dette oppsettet er et grunnlag, ikke en ferdig innlevering. Innlogging, administratoropprettelse og administrasjon av arrangementer gjenstår. Ingen brukere eller arrangementer er forhåndsutfylt. Endelig biografi, bilder og kontaktinformasjon må avklares.

## Foreslått neste arbeidsøkt

1. Fastsett valideringsregler og regler for arrangementenes start/slutt og publisering.
2. Implementer én administrator og sikker innlogging/utlogging uten offentlig registrering.
3. Implementer beskyttet oppretting, redigering, sletting og publisering/avpublisering.
4. Bygg administrasjonsgrensesnittet og test hele flyten mot PostgreSQL.
5. Legg inn avklart presentasjonsinnhold og lag demonstrasjonen.

## Tekniske beslutninger i grunnoppsettet

- Vite er valgt som frontend-oppsett. React Router håndterer rutene.
- Frontend og API bruker /api gjennom utviklingsproxy. Produksjonsruting må konfigureres ved publisering.
- PostgreSQL lagrer tidspunkt som timestamptz. Verdier skal skrives i UTC; frontend viser Europe/Oslo.
- Kommende arrangementer betyr foreløpig publisert og starttidspunkt større enn eller lik nå.
- Arrangementskort viser dato, tittel, sted og utvidbare detaljer. Eksterne lenker og bilder vises først etter at validering og innhold er implementert.
- Administratordata bruker Identity-tabeller. Det finnes ingen åpen registreringsrute.
- PostgreSQL eksponeres bare på lokal maskin. Datavolumet beholdes når databasen stoppes.

## Manuelle akseptansescenarioer når administrasjon er implementert

- Opprett et upublisert arrangement: det skal ikke vises offentlig.
- Publiser det: det skal vises på riktig plass i listen.
- Rediger og avpubliser: endringene skal vises og arrangementet skal deretter skjules.
- Start applikasjonen på nytt: lagrede data skal finnes fortsatt.
- Forsøk ugyldige tidspunkt og manglende felter: API og skjema skal forklare feilen.
- Forsøk endring uten innlogging: API skal avvise forespørselen.
- Test mobilbredde og naviger med tastatur.

Dokumenter faktiske resultater og KI-støttede forbedringer etter hvert; ikke marker disse scenarioene som bestått før funksjonene finnes og er testet.

## Verifisering ved oppsett (22. oktober 2026)

- Frontend-produksjonsbygg: bestått.
- Backend-bygg i Debug og Release: bestått, uten advarsler eller feil.
- .NET-gjenoppretting med locked-mode: bestått.
- EF-modell og første migrering samsvarer; SQL-skript for tabellene er generert.
- HTTP-kontroll av forside, om-side, API-livstegn, utviklingsproxy og OpenAPI: bestått.
- Docker Compose-konfigurasjon: validert.
- Database: ikke startet. Docker Desktop-motoren kom ikke i gang i oppsettsøkten. Migreringen er derfor ikke brukt på PostgreSQL, og arrangementslisten er ikke verifisert mot en kjørende database.
- Readiness-endepunktet returnerte forventet 503 uten database.

Start Docker Desktop og kjør npm run db:up og npm run db:migrate for å fullføre lokal databaseinitialisering. Start deretter API og frontend som beskrevet i README.
