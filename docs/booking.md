# Bookingforespørsler

## Bruk

Besøkende sender forespørsler gjennom skjemaet på /contact uten å opprette konto. Feltene omfatter kontaktperson, e-post, valgfri telefon og organisasjon, type oppdrag, valgfri ønsket dato og deltakerantall, sted og melding. En ukjent dato kan stå tom. Mottakssiden viser en referanse og forklarer at dato, pris og innhold må avtales med Arben. Referansen er en kort del av forespørselens ID og gir ingen offentlig tilgang til forespørselen.

Under /admin viser startsiden antall nye forespørsler. Innboksen på /admin/bookings kan filtreres etter Ny, Under behandling, Avtalt og Avslått og viser 20 forespørsler per side, nyeste først. Åpne en forespørsel for å lese meldingen og kontaktopplysningene. Åpning endrer ikke status automatisk.

«Svar på e-post» åpner administratorens e-postprogram med mottaker og emne. En eventuell telefonknapp åpner ringefunksjonen. Nettsiden sender ingen e-post, kvittering eller statusmelding. Arben må følge opp gjennom innboksen og kontakte arrangøren selv. Velg status og trykk «Lagre status» for intern oversikt. «Avtalt» skal brukes etter at avtalen er bekreftet med arrangøren. Ingen status oppretter eller publiserer et offentlig arrangement.

## Fra avtalt booking til arrangement

Når «Avtalt» er lagret for et foredrag, en boksamtale eller annet samarbeid, vises «Opprett arrangement fra forespørselen». Skuespilleroppdrag har ikke denne muligheten. Serveren kontrollerer også oppdragstype, gjeldende status og administratortilgang når skjemaet åpnes, slik at en direkte lenke ikke omgår regelen.

Knappen åpner et forhåndsutfylt skjema. Den oppretter ingen databasepost og publiserer ingenting. Forslag til tittel og offentlig beskrivelse, ønsket dato, sted og eventuell organisasjon følger med. For «Annet samarbeid» må offentlig beskrivelse skrives selv. Kontaktperson, e-post, telefon, deltakerantall og den private meldingen kopieres ikke. Manglende ønsket dato eller organisasjon gir tomme felt; kontaktpersonens navn brukes ikke som reserve for arrangørnavn. Stedsteksten beholdes i sin helhet; et stedsnavn over arrangementsskjemaets grense på 200 tegn må kortes ned før lagring.

Dato og klokkeslett redigeres i separate felt, fortsatt i norsk tid. Ønsket dato er et forslag og må kontrolleres mot avtalen. Klokkeslett og gateadresse må fylles inn. Arben velger bilde, tilpasser offentlig tekst og ser forhåndsvisningen før han velger «Lagre som utkast» eller «Vis på nettsiden». «Avbryt» før lagring går tilbake til forespørselen. Skjemaet kan åpnes på nytt etter oppfriskning så lenge forespørselen fortsatt finnes og er avtalt.

Et lagret arrangement administreres videre under «Mine arrangementer». Dette er forhåndsutfylling, ikke en varig kobling mellom postene: endringer i bookingstatus oppdaterer ikke arrangementet, og gjentatt oppretting kan lage flere arrangementer. Åpne derfor det eksisterende arrangementet for senere endringer. Ingen databasemigrering eller nye pakker kreves for denne funksjonen; start API-et på nytt etter oppdateringen.

Sletting finnes under «Flere valg», krever bekreftelse og fjerner forespørselen fra databasen. Det finnes ingen automatisk sletterutine eller gjenoppretting i grensesnittet. Avklar hvilke opplysninger foretaket trenger å beholde, lagringstid og hvordan sikkerhetskopier håndteres før offentlig drift. Ingen fiktiv foretaksinformasjon er lagt inn; endelige firma- og kontaktopplysninger legges til når de er klare.

## Lagring og oppstart

EF-migreringen AddBookingRequests oppretter BookingRequests-tabellen, med kontaktopplysninger, oppdragsdetaljer, status, tidsstempler og en hash brukt ved gjentatt innsending. Migreringen er additiv og endrer ikke eksisterende arrangementer eller brukere. Den er kjørt mot den lokale utviklingsdatabasen. Ved oppsett på en annen maskin brukes prosjektets vanlige migreringskommando, npm run db:migrate, etter at dotnet-verktøyene er gjenopprettet.

Start API-et på nytt etter oppdateringen. Frontend bruker /api på samme vert; eksisterende Vite-proxy brukes lokalt. Databaseberedskap kontrollerer nå også bookingtabellen.

## Tilgang og spamvern

- Bare offentlig oppretting og henting av skjemaets sikkerhetstokens er tilgjengelig uten innlogging. Mottakssvaret inneholder kun en referanse. Det finnes ingen offentlig liste, statusoppslag eller detaljvisning.
- Alle /api/admin/bookings-ruter krever Administrator-rollen, og endringer krever identitetsbundet CSRF-token. Svar på skjema-, admin- og innloggingsruter bruker Cache-Control: no-store.
- Navn, e-post, telefon, dato, oppdragstype, lengder og deltakerantall valideres på serveren. Offentlige klienter kan ikke sette status eller publisere arrangementer. React viser meldingene som tekst.
- Skjemaet krever CSRF-token og en serverbeskyttet innsendingstoken. Innsending før to sekunder eller etter to timer avvises med mulighet for å laste inn skjemaet på nytt uten å tømme feltene.
- Et skjult lokkefelt brukes til å forkaste enkle spamforsøk. Det sendes en nøytral kvittering uten at en forespørsel lagres når feltet er fylt ut.
- Maksimalt fem innsendingsforsøk per IP på 15 minutter, og 60 henting av skjema per IP på 15 minutter. Begrensningene ligger i minnet per API-instans, og mislykkede innsendingsforsøk teller også. Flere personer bak samme IP deler grensen.
- Samme innsendings-ID og innhold gir samme kvittering ved nytt forsøk etter nettverksfeil. En unik database-ID hindrer duplikater også ved samtidige forsøk. Endret innhold med samme ID avvises. Personopplysninger eller utkast lagres ikke i nettleserens localStorage.

Dette reduserer enkle spamforsøk, men verifiserer ikke avsenderens e-post og stanser ikke all automatisert misbruk. Ved offentlig drift må proxyens klient-IP være riktig, HTTPS og Data Protection-nøkler konfigureres og databasens sikkerhetskopiering og tilgang sikres. Flere API-instanser trenger felles ratebegrensning. E-postvarsling, e-postverifisering, betaling og automatisk fakturering er ikke implementert.

## Verifikasjon

Integrasjonstestene dekker privat og idempotent innsending, statusendringer, filtrering, sletting, tilgangskontroll, CSRF, feltvalidering, lokkefelt, tidsgrense og ratebegrensning. Forhåndsutfylling kontrolleres for alle tillatte typer, avvisning av skuespilleroppdrag og ikke-avtalte bookinger, manglende dato/organisasjon, skjerming av private opplysninger og at åpning ikke oppretter arrangementer. Uthenting etter endring fra «Avtalt» avvises.

Testene bruker egne midlertidige PostgreSQL-databaser. Se [README](../README.md) for kommandoer og [development.md](development.md) for manuelle kontroller og resultater fra kodeoppryddingen.
