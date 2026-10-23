# Arkitektur og kodekonvensjoner

Nettsiden består av en React-klient og et ASP.NET Core Minimal API som lagrer data i PostgreSQL gjennom Entity Framework Core. Dette er en komponentbasert frontend med et separat API. Prosjektet følger ikke en klassisk MVC-mappestruktur.

```text
React-side → felles HTTP-klient → API-endepunkt → kontrakt/validering → EF Core → PostgreSQL
```

## Frontend

| Mappe      | Ansvar                                                                   |
| ---------- | ------------------------------------------------------------------------ |
| pages      | Offentlige sider og deres presentasjon                                   |
| admin      | Innlogging, administratorens arbeidsflyter og skjemaer                   |
| components | Gjenbrukbar visning: bilder, arrangementskort, booking og navigasjon     |
| hooks      | Felles livssyklus for datahenting og oppdatering av tidsavhengig innhold |
| lib        | HTTP, feilbeskjeder og konvertering mellom UTC og norsk tid              |
| types      | TypeScript-grensesnitt for data som utveksles med API-et                 |
| content    | Presentasjonsinnhold og konfigurerbare kontaktkanaler                    |

lib/http.ts er eneste HTTP-klient. Den sender cookie, henter CSRF-token ved endringer og oversetter API-feil til forståelige meldinger. Innloggingsdata lagres ikke i localStorage.

useApiQuery håndterer GET-kall, lasting, feil, nytt forsøk og avbryting. Et svar knyttes til URL-en og innlastingsforsøket som startet det. Et sent svar fra en tidligere side skal ikke kunne overskrive den siden brukeren nå ser.

Endringer skjer i navngitte hendelsesfunksjoner, for eksempel submitBooking, saveStatus og confirmAction. Vanlige klikk med få linjer logikk kan fortsatt ligge ved knappen. AdminRequestError samler lik feilvisning i kontrollpanelet.

EventEditor laster eksisterende arrangement eller tillatt bookinggrunnlag. EventEditorForm eier de redigerbare verdiene, bildevalget, forhåndsvisningen og lagringen. Skjemaet får startverdier når det åpnes; en bakgrunnsoppdatering skal ikke tømme det brukeren skriver.

AuthProvider eier innloggingsstatus. Context og useAuth er skilt fra komponenten slik at Reacts utviklingsoppdatering kan håndtere dem riktig.

## Backend

| Mappe     | Ansvar                                                                               |
| --------- | ------------------------------------------------------------------------------------ |
| Endpoints | HTTP-ruter, tilgangskrav, statuser og lagringsflyt                                   |
| Contracts | Innkommende felter, validering og svarmodeller                                       |
| Models    | Data som lagres i databasen                                                          |
| Data      | ApplicationDbContext, feltgrenser, indekser og migreringer                           |
| Security  | Identity, administratorrolle, CSRF, ratebegrensning, bookingtoken og kontokommandoer |
| Services  | Filkontroll, bildebehandling og bildelagring                                         |

Tilgang kontrolleres på serveren. At en knapp er skjult i React erstatter aldri autorisasjon eller validering i API-et. Enkelte feltgrenser gjentas bevisst i skjema og backend: skjemaet hjelper brukeren, mens serveren håndhever reglene.

Et nytt arrangement starter alltid som utkast. Publisering krever en egen handling. Bookingstatus endrer ikke offentlige arrangementer. Forhåndsutfylling fra booking krever lagret «Avtalt» og en tillatt oppdragstype; skuespilleroppdrag avvises også ved direkte API-kall. Svarmodellen inneholder bare opplysninger som skal vurderes for det offentlige arrangementet.

## Tid og bilder

- Databasen lagrer arrangementstidspunkt i UTC. lib/dateTime.ts viser og redigerer dem i Europe/Oslo, uavhengig av maskinens tidssone.
- Bookingens ønskede dato er en kalenderdato uten klokkeslett. Manglende klokkeslett blir ikke gjettet.
- Ikke-eksisterende og tvetydige klokkeslett ved overgang mellom sommer- og vintertid avvises.
- Et valgt bilde har en midlertidig forhåndsvisningsadresse som frigis når valget endres eller skjemaet lukkes.
- API-et validerer faktiske bildebytes og lagrer ny JPEG uten metadata. Bare servergenererte bildebaner godtas som arrangementsbilder.

## Tester og vedlikehold

- Integrasjonstestene starter den faktiske API-pipelinen med egne midlertidige PostgreSQL-databaser og bildemapper.
- TestRequests samler cookie-/CSRF-innlogging for testene. Negative tilgangstester sender fortsatt bevisst forespørsler uten disse hjelpemidlene.
- Frontend-testene dekker tidssone, sommertid og kalenderdato uten ekstra testbibliotek.
- Prettier formaterer frontend, skript, konfigurasjon og dokumentasjon. .NET-verktøyet formaterer håndskrevet C#; genererte migreringer utelates.
- Kommentarer forklarer hvorfor noe gjøres, særlig avbryting, sikkerhetsgrenser, idempotens, privat informasjon og tidssonevalg.
- Databasemigreringer kjøres eksplisitt. Kodeoppryddingen krever ingen ny migrering.
