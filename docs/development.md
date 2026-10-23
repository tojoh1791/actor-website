# Utviklingsstatus og testing

## Implementert

Prototypen har offentlige presentasjonssider, arrangementer, bookingskjema, administratorinnlogging, privat forespørselsinnboks og administrasjon av arrangementer. Avtalte bookinger kan brukes til å forhåndsutfylle et arrangement, med unntak for skuespilleroppdrag. Status eller innsending publiserer aldri noe automatisk.

Oppstart og tilgjengelige kommandoer finnes i [README](../README.md). [Arkitekturbeskrivelsen](architecture.md) forklarer hvor de ulike delene av koden ligger. Dette dokumentet erstatter den gamle statuslisten fra første prosjektoppsett.

## Kodeoppryddingen

- Felles GET-hook erstatter gjentatt innlasting, feilbehandling og avbryting i sidene.
- Felles HTTP-klient og datohjelpere erstatter parallelle implementeringer.
- Arrangementets innlasting og redigeringsskjema har hver sin komponent.
- Datakontrakter og validering er flyttet ut av rutefilene i backend.
- Innloggings-/CSRF-hjelpere er samlet for integrasjonstestene.
- Ubrukt kontaktlayout er fjernet fra CSS. Stylesheetene har tydelige seksjonskommentarer.
- Prettier, C#-formatering og kodekontroll gjør samme stil mulig i videre utvikling.

## Automatisert verifikasjon

Kjør **npm run check**, **npm run test:api** og **npm run format:api:check** fra prosjektroten. Databasen må kjøre for integrasjonstestene.

Frontend-testene kontrollerer vintertid, sommertid, tvetydige og ikke-eksisterende klokkeslett og riktig kalenderdato i Oslo. De krever ikke database.

Integrasjonstestene dekker innlogging, cookie, roller, CSRF, kontolås, ratebegrensning, arrangementer, opplasting og kontroll av bilder, privat booking, gjentatt innsending, filtrering, statuser, sletting og reglene for forhåndsutfylling. Hver test bruker sin egen database og rydder opp etter seg. Applikasjonsdatabasen skal aldri brukes til testdata.

Dokumenter faktiske resultater fra hver kjøring. Et bygg som viser NU1900 kan ha lykkes selv om NuGets sårbarhetskontroll ikke kunne hentes. Dette er to separate kontroller.

## Manuell kontroll før demonstrasjon

1. Kontroller offentlige sider på mobil og større skjerm, inkludert åpning/lukking av mobilmenyen.
2. Send en booking og kontroller mottaksreferanse og at innsendingen ikke oppretter et arrangement.
3. Logg inn, filtrer innboksen, åpne forespørselen og lagre «Avtalt».
4. Åpne det forhåndsutfylte arrangementet. Kontroller at privat melding og kontaktopplysninger ikke er kopiert.
5. Fyll inn tid og adresse, velg bilde og forhåndsvis. Lagre som utkast og kontroller at det fortsatt er privat.
6. Åpne utkastet, publiser det og kontroller tittel, tid, bilde og detaljer offentlig.
7. Skjul arrangementet igjen, og kontroller at det forsvinner fra den offentlige listen.
8. Kontroller at skuespilleroppdrag ikke får opprett-knapp, og at en direkte lenke avvises.
9. Prøv et manglende arrangement, nytt forsøk etter feil, avbryt-knapper, utlogging og tilgang uten innlogging.
10. Start applikasjonen på nytt og kontroller at lagrede data fortsatt finnes.

## Bruk av KI i oppgaven

KI er brukt til idéarbeid, planlegging, utvikling, testing og opprydding i dialog med studenten. Konkrete forbedringer fra arbeidet er like store arrangementskort, enklere mobilnavigasjon, tydelige tilbakeknapper og forhåndsutfylling fra booking med skjerming av private opplysninger.

Til innleveringen bør studenten vise den fungerende arbeidsflyten og forklare egne valg, hva KI bidro med og hvilke feil og forbedringer testingen avdekket. En kort beskrivelse og demo følger oppgaveteksten; en produksjonslansering er ikke nødvendig for å vise prototypen.

## Videre arbeid før offentlig drift

Vert, HTTPS, proxy, varig nøkkel-/bildelagring, sikkerhetskopiering og gjenoppretting må konfigureres og testes. Endelige kontakt- og foretaksopplysninger og bildebruksrett må avklares. E-postvarsler, to-faktorinnlogging, betaling og automatisk bildeopprydding er ikke implementert.

Booking og arrangement er selvstendige poster etter forhåndsutfylling. Det finnes foreløpig ingen varig kobling eller sperre mot å opprette flere arrangementer fra samme booking. Senere endringer gjøres i det allerede opprettede arrangementet.

## Verifisert etter kodeoppryddingen, 23. oktober 2026

- Samlekontrollen bestod: formatering, kodekontroll uten advarsler, tre datotester og bygg av frontend og backend.
- Alle 27 integrasjonstester bestod mot separate testdatabaser. C#-formateringen bestod også kontrollen.
- Nettlesertest bekreftet innsending og mottaksreferanse, innlogging, statuslagring, forhåndsutfylling uten privat melding, bildevalg og opplasting, privat utkast, gjenåpning med beholdt bilde/tid og eksplisitt publisering.
- Offentlige kort hadde lik høyde i sammenfoldet visning, én kolonne på mobil og to på større skjerm. Utviding av detaljer, skjuling, feilmelding med nytt forsøk og utlogging/avvisning uten innlogging fungerte.
- NuGets sårbarhetskontroll var utilgjengelig og ga NU1900. Byggene og testene bestod; pakkesårbarhetskontrollen kunne ikke bekreftes.

Nettlesertesten brukte egen testkonto, midlertidig database og separat bildelager. Ingen faktiske bookingforespørsler ble opprettet i applikasjonsdatabasen.
