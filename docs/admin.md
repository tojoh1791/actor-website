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

- /admin: legge til arrangement, åpne forespørsler og administrere arrangementer.
- Undersidene har en tydelig «Til oversikten»-knapp øverst. På detaljsider finnes også en lenke til arrangementslisten eller innboksen. Oversikten har ingen tilbakeknapp til seg selv.
- /admin/events: kommende arrangementer, utkast eller alle arrangementer.
- Nye arrangementer starter som utkast.
- Forhåndsvisning før lagring og publisering.
- Et utkast kan lagres uten å vises offentlig.
- Publiserte, kommende arrangementer vises på /events.
- Publiserte arrangementer med passert starttid vises under «Tidligere», med nyeste først. Også tidligere arrangementer kan skjules og publiseres igjen.
- Skjul gjør et publisert arrangement til utkast.
- «Skjul fra nettsiden» og «Slett arrangement» vises direkte på kortet, med bekreftelse før handlingen utføres.
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

## Tester og videre dokumentasjon

Kjør npm run test:api med en kjørende PostgreSQL. Testene bruker de faktiske migreringene, egne midlertidige databaser og egne bildemapper. De dekker innlogging, roller, CSRF, kontolås, cookies, utlogging, validering, arrangementer, bilder og booking. Testdata legges ikke i applikasjonsdatabasen. Se [README](../README.md) for oppstart og kommandoer.

Frontend-testene kontrollerer blant annet norsk tid og overganger mellom sommer- og vintertid. Kjør npm run test:web. Kodekontroll og bygg kjøres med npm run check.

Se [booking.md](booking.md) for forespørselsinnboksen og overgang fra avtalt booking til arrangement, og [development.md](development.md) for utviklingsstatus og manuelle testscenarioer.

Et avbrutt testprogram kan etterlate en actor_tests_-database. Fjern bare databaser som er bekreftet å tilhøre en avsluttet testkjøring. Testverktøyet sletter aldri applikasjonsdatabasen.
