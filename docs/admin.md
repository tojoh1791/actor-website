# Administrasjon

## Første innlogging

1. Start PostgreSQL og kjør npm run db:migrate fra prosjektroten.
2. Kjør npm run admin:create i en vanlig, interaktiv terminal.
3. Oppgi e-post og velg et passord på minst 14 tegn. En lang passfrase er tillatt. Passordet vises ikke mens du skriver og lagres bare som Identity-passordhash i databasen.
4. Start API med npm run dev:api og frontend med npm run dev:web i hver sin terminal.
5. Åpne http://localhost:5173/admin. Lagre denne adressen som bokmerke eller snarvei på telefonen.

Det finnes ingen standardkonto, standardpassord eller offentlig registrering. Den offentlige navigasjonen inneholder ingen innloggingslenke.

## Glemt passord eller låst konto

Den som har terminaltilgang til serveren kan kjøre npm run admin:reset-password. Kommandoen spør etter administratorens e-post og nytt passord, opphever eventuell kontolås og ugyldiggjør tidligere innlogginger. Den endrer bare en eksisterende administratorkonto. Ikke legg passord i kommandoargumenter, miljøvariabler eller Git.

Lokale npm-kommandoer laster .env og er laget for utviklingsmiljøet. På en driftsserver må samme CLI-kommando kjøres med serverens egen konfigurasjon og beskyttede terminaltilgang.

## Kontrollpanelet

- /admin: to hovedoppgaver, legge til arrangement og åpne arrangementsoversikten.
- /admin/events: kommende arrangementer, utkast eller alle arrangementer.
- Nye arrangementer starter som utkast.
- Forhåndsvisning før lagring og publisering.
- Et utkast kan lagres uten å vises offentlig.
- Publiserte, kommende arrangementer vises på /events.
- Skjul gjør et publisert arrangement til utkast.
- Slett krever en bekreftelse og kan ikke angres.
- Valgfrie opplysninger ligger under Flere opplysninger.
- Alle tidspunkt redigeres i norsk tid (Europe/Oslo), uavhengig av telefonens tidssone, og lagres i UTC.

Klokkeslett som ikke finnes, eller forekommer to ganger ved overgang mellom sommer- og vintertid, avvises med en forklaring. Velg et annet, entydig tidspunkt. Billetter håndteres gjennom en valgfri ekstern HTTPS-lenke. Billettbetaling er ikke lagt til.

## Bilder til arrangementer

Velg bilde i skjemaet for et nytt eller eksisterende arrangement. Bildegalleriet på telefonen eller filvelgeren på datamaskinen åpnes. Bildet forhåndsvises før lagring. «Bruk standardbildet» fjerner valget når arrangementet lagres. Uten eget bilde brukes det eksisterende portrettet av Arben, med fotokreditering til Are Næsset. Kilde og avklaring av bruksrett er beskrevet i docs/content-sources.md.

JPG, PNG og WebP støttes, opptil 8 MiB og 40 megapiksler. HEIC støttes ikke; eksporter til JPG først. Serveren krever administrator og CSRF, kontrollerer filsignatur og dekoding, begrenser ressursbruk, retter orientering, tilpasser til maksimalt 1600 × 1600 og lagrer en ny JPEG uten metadata, inkludert GPS/EXIF. Bare servergenererte bildebaner som finnes i bildelageret godtas som imageUrl. Ingen eksterne bildelenker eller klientfilnavn brukes til lagring.

API-et lagrer bilder i backend/ActorWebsite.Api/.data/event-images, utenfor Git. EventImages__Directory kan angi en absolutt, varig mappe ved drift. Mappen må inngå i sikkerhetskopiering sammen med databasen og være delt hvis flere API-instanser brukes. I produksjon må /api/event-images/ videresendes til samme API som resten av /api/. Bilder er offentlige når URL-en er kjent, også før arrangementet publiseres; last bare opp bilder som er ment for nettsiden.

Bildet lastes opp når arrangementet lagres, før arrangementsopplysningene lagres. Ved feil i selve lagringen beholdes det opplastede bildet i skjemaet slik at lagringen kan prøves igjen. Ved å forlate skjemaet etter en slik feil kan en ubrukt fil bli liggende. Bytte av bilde, bruk av standardbilde og sletting av arrangement sletter ikke gamle bildefiler. Automatisk opprydding er ikke implementert; en senere oppryddingsjobb må kontrollere alle referanser før filer fjernes.

Ingen databasemigrering kreves: eksisterende imageUrl-felt brukes. Start API-et på nytt etter oppdateringen for å aktivere bildeendepunktene.

## Sikkerhet i implementeringen

- ASP.NET Core Identity hasher passord og låser konto i 15 minutter etter fem feil.
- I tillegg tillates maksimalt ti innloggingsforsøk per IP på ti minutter per API-instans.
- Alle /api/admin/events-endepunkter krever innlogget bruker med Administrator-rollen.
- Innlogging, utlogging og endringer krever en identitetsbundet CSRF-token sammen med cookie.
- Cookies er HttpOnly, SameSite=Strict og krever HTTPS utenfor Development.
- Innlogging varer maksimalt åtte timer uten aktivitet. Husk meg gjør cookien varig på enheten innenfor denne perioden; aktiv bruk kan forlenge økten.
- Security stamp kontrolleres på hver autentisert forespørsel. Utlogging ugyldiggjør alle administratorens eksisterende innlogginger, også på andre enheter.
- JSON-forespørsler begrenses til 64 KiB i Kestrel. Backend validerer påkrevde felt, lengder, tidspunkt og HTTPS-lenker.
- Administrasjons- og autentiseringssvar bruker Cache-Control: no-store.
- Passord og innloggingstokens lagres ikke i nettleserens localStorage.
- Data Protection-nøkler lagres lokalt og ignoreres i Git under utvikling. På Windows beskyttes de med DPAPI.

Dette er et lokalt utviklingsoppsett. Før offentlig drift må HTTPS, betrodd reverse proxy, varig og beskyttet lagring av Data Protection-nøkler, sikkerhetskopiering/gjenoppretting og feilovervåking konfigureres for valgt vert. Limiterens IP-adresse må være korrekt når appen kjører bak en proxy. To-faktorinnlogging er ikke implementert.

## Tester

Kjør npm run test:api med lokal PostgreSQL startet. Testkontoen må kunne opprette egne testdatabaser. Bruk aldri produksjonslegitimasjon til tester.

Hver test oppretter en egen PostgreSQL-database med et tilfeldig actor_tests_-navn, bruker de faktiske EF-migreringene og fjerner databasen ved avslutning. Applikasjonsdatabasen brukes ikke til testdata. ACTOR_TEST_POSTGRES kan overstyre tilkoblingen for et eget testmiljø.

Testene dekker avvisning uten innlogging, rollekrav, manglende/fremmed CSRF-token, kontolås og begrensning av forsøk, cookieinnstillinger, ugyldiggjøring ved utlogging, validering, offentlig filtrering og flyten opprette/redigere/publisere/skjule/slette. Samme tester kjøres i GitHub Actions med PostgreSQL.

Bildeoppdateringen har 17 beståtte integrasjonstester totalt, inkludert alle tre bildeformater, uthenting uten innlogging, størrelsestilpasning, fjerning av metadata, lagring/publisering av bildevalg, tilbakegang til standardbilde og avvisning av ugyldige filer, for store filer og ugyldige bildereferanser. Bildetestene bruker egne midlertidige mapper som fjernes etter kjøringen. Frontend og backend bygger. NuGets automatiske sårbarhetssjekk svarte ikke under denne oppdateringen (NU1900), så den sjekken kunne ikke bekreftes.

Nettleserkontroll i separat testdatabase: filvelger, forhåndsvisning, opplasting og lagring, gjenåpning med beholdt bilde, offentlig visning og bytte tilbake til standardbilde er kontrollert. Smal mobilvisning hadde én kolonne uten horisontal overflyt; større skjerm hadde to kolonner. Begge bilder lastet. Egne bilder vises i sin helhet, slik at plakater og bokomslag ikke beskjæres. Portrettet som brukes som standard fyller bildeflaten.

Et avbrutt testprogram kan etterlate egne actor_tests_-databaser. Fjern bare databaser som er bekreftet å tilhøre en avsluttet testkjøring. Testverktøyet sletter aldri applikasjonsdatabasen.

Bookingskjema og forespørselsinnboks er nå implementert; se [booking.md](booking.md) for arbeidsflyt, oppstart og avgrensninger. Google-innlogging og automatisk passordgjenoppretting med e-post er ikke implementert.

## Verifisert ved implementering

- Frontend og backend bygger uten feil eller advarsler; .NET-pakkelås og EF-modell er kontrollert.
- Alle 11 integrasjonstester bestod mot PostgreSQL.
- Nettlesertest med en separat, midlertidig konto og egen database: innlogging, mobilvisning, forhåndsvisning, publisering, offentlig visning, skjuling og avbryting av sletting er kontrollert.
- Den interaktive terminaltesten for kontooppretting ble stoppet av den automatiske godkjenningskontrollen før e-post eller passord ble skrevet inn. Kontokommandoene bygger, men hele terminaldialogen er derfor ikke verifisert i denne økten. Opprett den faktiske kontoen manuelt med npm run admin:create.
