# Arben Bala – nettside og booking

En mobiltilpasset nettside for skuespiller og foredragsholder Arben Bala. Publikum kan lese om Arben, foredraget «Uten frykt» og boken «Balkanbrødre», finne offentlige arrangementer og sende bookingforespørsler. Arben behandler forespørsler og administrerer arrangementer i et eget kontrollpanel.

Prosjektet er utviklet med generativ KI som prototype til DAVE3625 ved OsloMet. Det kjører lokalt og er lagt opp for videre utvikling. Ingen brukere, bookinger eller demoarrangementer opprettes automatisk.

## Funksjoner

- Offentlige sider med biografi, foredrag, bok, arrangementer og booking.
- Like store arrangementskort i sammenfoldet visning, med bilde, to linjer til tittel og «Mer» ved behov. På mobil vises ett kort i bredden.
- Booking uten besøkskonto, med validering, spamvern og mottaksreferanse.
- Privat administratorinnlogging og en mobiltilpasset innboks med statusfiltre.
- Oppretting, redigering, bildevalg, forhåndsvisning, utkast, publisering, skjuling og sletting av arrangementer.
- Forhåndsutfylling av arrangement fra en avtalt booking. Skuespilleroppdrag er unntatt; private meldinger og kontaktopplysninger kopieres ikke.

Et innsendt skjema er en forespørsel. Dato, pris og innhold avtales direkte med Arben. Nettsiden sender foreløpig ingen e-post eller varsler og håndterer ingen betaling.

## Teknologi

| Del        | Teknologi                                                      |
| ---------- | -------------------------------------------------------------- |
| Frontend   | React, TypeScript, Vite og React Router                        |
| Backend    | ASP.NET Core Minimal API og .NET 10                            |
| Data       | Entity Framework Core, Npgsql og PostgreSQL 17                 |
| Innlogging | ASP.NET Core Identity, administratorrolle og cookie            |
| Kontroller | Oxlint, Prettier, Node-testverktøy og xUnit-integrasjonstester |

## Første oppsett

Du trenger Node.js 22.12 eller nyere, .NET 10 SDK som støtter [global.json](global.json), Git og en kjørende Docker Desktop med Linux-containere. CI bruker Node.js 24. Kjør kommandoene fra prosjektroten:

```sh
npm --prefix frontend ci
dotnet restore ActorWebsite.slnx
npm run tools:restore
npm run init:local
npm run db:up
npm run db:migrate
npm run admin:create
```

init:local oppretter en ignorert .env med tilfeldig databasepassord og beholder eksisterende konfigurasjon. admin:create spør etter e-post og et passord på minst 14 tegn i terminalen. Det finnes ingen standardkonto eller offentlig registrering. Bruk **npm run admin:reset-password** hvis en eksisterende administrator trenger nytt passord.

## Start og stopp

Åpne to terminaler i prosjektroten:

```sh
# Terminal 1
npm run dev:api
```

```sh
# Terminal 2
npm run dev:web
```

- Nettside: [http://localhost:5173](http://localhost:5173)
- Kontrollpanel: [http://localhost:5173/admin](http://localhost:5173/admin)
- API-livstegn: [http://localhost:5080/api/health](http://localhost:5080/api/health)
- Databaseberedskap: [http://localhost:5080/api/health/ready](http://localhost:5080/api/health/ready)

Vite videresender /api til backend på port 5080. API-adressen inneholder ikke selve nettsiden. API- og migreringsskriptene laster .env automatisk. Start API-et på nytt etter endringer i C#-koden.

Stopp hver utviklingsserver med **Ctrl+C** i terminalen den kjører i. **npm run db:stop** stopper PostgreSQL og beholder data i Docker-volumet.

## Bygg, tester og kodekvalitet

```sh
npm run check
npm run test:api
npm run format:api:check
```

check kontrollerer formatering, kjører kodekontroll og datotester for frontend og bygger begge delene. Backend bygges i Release, slik at en kjørende utviklingsserver i Debug ikke låser de samme filene.

Integrasjonstestene krever en kjørende lokal PostgreSQL. Hver test lager og fjerner sin egen database med et tilfeldig actor_tests_-navn og bruker de faktiske migreringene. Testkontoen må kunne opprette databaser. Applikasjonsdatabasen brukes ikke til testdata; ACTOR_TEST_POSTGRES kan angi en egen testtilkobling. Bruk aldri produksjonslegitimasjon i tester.

| Kommando           | Formål                                                    |
| ------------------ | --------------------------------------------------------- |
| npm run build:web  | Bygg frontend til frontend/dist                           |
| npm run build:api  | Bygg API og testprosjekt                                  |
| npm run test:web   | Kontroller norsk tid, sommertid og datohåndtering         |
| npm run test:api   | Kjør integrasjonstestene                                  |
| npm run lint:web   | Finn kodefeil og uheldige React-mønstre                   |
| npm run format     | Formater TypeScript, CSS, konfigurasjon og dokumentasjon  |
| npm run format:api | Formater håndskrevet C#; utelat migreringer og byggefiler |

## Finn frem i koden

```text
frontend/src/
  pages/        Offentlige sider
  admin/        Innlogging, kontrollpanel og redigeringsskjema
  components/   Gjenbrukbare visningskomponenter
  hooks/        Felles innlasting av data og oppdatering av klokkeslett
  lib/          HTTP-klient, feil og norsk datohåndtering
  types/        TypeScript-kontrakter for arrangementer, booking og bruker
  content/      Presentasjonsinnhold og konfigurerbare kontaktkanaler
backend/
  ActorWebsite.Api/
    Contracts/  API-innhold, validering og eksplisitte svarmodeller
    Endpoints/  HTTP-ruter, tilgangskrav og arbeidsflyter
    Models/     Databasemodeller
    Data/       DbContext og migreringer
    Security/   Identity, CSRF, spamvern og kontokommandoer
    Services/   Kontroll og lagring av arrangementsbilder
  ActorWebsite.Api.Tests/  Integrasjonstester og felles testhjelpere
docs/           Bruk, arkitektur, kilder og utviklingsstatus
scripts/        Lokalt oppsett og oppstart av .NET-kommandoer
```

Frontend bruker React-komponenter og hooks. Backend bruker Minimal API med egne datakontrakter og EF-modeller. Se [arkitekturbeskrivelsen](docs/architecture.md) for ansvarsfordelingen.

## Demonstrasjon til innleveringen

Kjør **npm run demo:seed** for å legge inn syv fiktive arrangementer og åtte bookingforespørsler i den lokale utviklingsdatabasen. Fem arrangementer vises offentlig; ett er tidligere og ett er et utkast. Datoene beregnes fra dagen kommandoen kjøres første gang. Kommandoen beholder eksisterende innhold og endringer og legger ikke inn duplikater ved gjentatt kjøring. Den kan bare kjøres i Development-miljøet.

Se [demodata og kopierbare bookingeksempler](docs/demodata.md) for ferdig innhold til skjemaene og forslag til demonstrasjon. Alle demoarrangementer er fiktive og merket som demo.

1. Send en bookingforespørsel fra /contact.
2. Logg inn på /admin, åpne forespørselen og lagre statusen «Avtalt».
3. Velg «Opprett arrangement fra forespørselen». Fyll inn klokkeslett og adresse, kontroller dato og tekst, og velg eventuelt et bilde.
4. Forhåndsvis og lagre som utkast. Kontroller at det ikke vises offentlig.
5. Publiser arrangementet og finn det på /events.
6. Vis at skuespilleroppdrag forblir i innboksen, uten opprett-knapp.

Oppgaveteksten ber om demolenke, kildekode og en kort beskrivelse av applikasjonen og formålet. Forklar også hvordan KI ble brukt, og hvilke feil som ble oppdaget og forbedret gjennom testing. Se [utviklingsnotatene](docs/development.md).

## Lokal lagring og videre drift

PostgreSQL-data ligger i Docker-volumet. Opplastede bilder ligger som standard i backend/ActorWebsite.Api/.data/event-images, og utviklingsnøkler ligger i .data/keys. Disse mappene og .env er utelatt fra Git. Sikkerhetskopiering må omfatte både database og bilder.

Før offentlig drift må vert, HTTPS, proxy, varig lagring av nøkler og bilder, sikkerhetskopiering og gjenoppretting konfigureres og testes. Endelige kontakt-/foretaksopplysninger og bruksrett til bildene må avklares. To-faktorinnlogging, automatiske e-postvarsler, betaling og automatisk opprydding i gamle bilder er ikke implementert.

## Feilsøking

- **Docker svarer ikke:** Åpne Docker Desktop og vent til motoren er klar før npm run db:up.
- **Arrangementer eller forespørsler kan ikke lastes:** Kontroller at API-et kjører, at /api/health/ready svarer, og at npm run db:migrate er kjørt.
- **404 i nettleseren:** Åpne nettsiden på port 5173. Port 5080 er API-et. Hvis Vite velger en annen port, bruk frontend-adressen som vises i terminalen.
- **Porten er opptatt:** Standardportene er 5173, 5080 og 5432. Kontroller terminaler med eksisterende servere før du starter flere.
- **Endret databasepassord:** Et eksisterende PostgreSQL-volum beholder passordet det ble opprettet med. Endring av .env endrer ikke passordet i databasen.
- **NU1900 ved .NET-bygg:** NuGet kunne ikke hente sårbarhetsdata. Et vellykket bygg bekrefter ikke at den kontrollen er utført; kjør gjenoppretting igjen når tjenesten er tilgjengelig.

## Mer dokumentasjon

- [Administrasjon og innlogging](docs/admin.md)
- [Booking og overgang til arrangement](docs/booking.md)
- [Arkitektur og kodekonvensjoner](docs/architecture.md)
- [Utviklingsstatus og testing](docs/development.md)
- [Innholdskilder og bilder](docs/content-sources.md)
- [Arrangementskort](docs/event-cards.md)
