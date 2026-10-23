# Demodata og manus til demonstrasjonen

Alle personer, organisasjoner og arrangementer i disse eksemplene er fiktive. E-postadressene slutter på example.test og skal ikke brukes til ekte kontakt. Telefon kan stå tomt. Ingen e-post sendes fra løsningen.

## Ferdig innhold i databasen

Fra prosjektroten:

```sh
npm run demo:seed
```

PostgreSQL må kjøre, og migreringene må være installert. Kommandoen bruker den lokale .env-konfigurasjonen og kan bare kjøres i Development. Den starter ingen webserver og oppretter ingen kontoer. Vanlig oppstart legger aldri inn demodata.

| Arrangement                       | Tid fra første kjøring | Visning                               |
| --------------------------------- | ---------------------- | ------------------------------------- |
| Uten frykt – med håp              | 7 dager                | Offentlig                             |
| Balkanbrødre: historien bak boken | 14 dager               | Offentlig                             |
| Veien videre                      | 21 dager               | Offentlig                             |
| Fra livshistorie til scenen       | 28 dager               | Offentlig, lang beskrivelse med «Mer» |
| En kveld med Arben Bala           | 42 dager               | Offentlig                             |
| Fortellinger om tilhørighet       | 14 dager tilbake       | Bare i adminhistorikken               |
| Uten frykt – ny møteplass         | 35 dager               | Utkast, bare i admin                  |

Arrangementene starter kl. 18.00 norsk tid og varer i 90 minutter. De bruker standardbildet av Arben. Møteplassene er fiktive; kartadressene peker til byene. De fem kommende arrangementene viser variasjon i titler, steder og tekstlengde.

Innboksen får åtte forespørsler: tre nye, to under behandling, to avtalte og én avslått. De dekker foredrag, boksamtale, skuespilleroppdrag og annet samarbeid. En ny forespørsel har åpen dato. Fellesrom kulturforening har et avtalt foredrag som kan gjøres om til et arrangementsutkast. Historiefilm produksjon har et avtalt skuespilleroppdrag som skal bli i innboksen.

Kommandoen bruker faste ID-er. Kjøring på nytt beholder titler, datoer, publiseringsvalg og statuser du har endret og oppretter ingen duplikater. Slettede demoposter blir lagt inn igjen. Kommandoen flytter ikke eksisterende datoer fremover; juster dem i kontrollpanelet dersom demonstrasjonen skjer lenge senere. Eksisterende innhold slettes aldri.

## Bookingeksempel 1: foredrag

Åpne /contact. Kopier verdiene inn i de tilsvarende feltene. Datoen nedenfor er et eksempel; velg en fremtidig dato hvis du demonstrerer senere.

| Felt i skjemaet               | Verdi                           |
| ----------------------------- | ------------------------------- |
| Navnet ditt                   | Nora Eksempel                   |
| Firma eller organisasjon      | Horisont kulturforum (demo)     |
| E-post                        | nora@example.test               |
| Telefon                       | La feltet stå tomt              |
| Hva ønsker dere å booke?      | Foredrag                        |
| Ønsket dato                   | 18.02.2027                      |
| Omtrent antall deltakere      | 100                             |
| Hvor skal arrangementet være? | Horisont møteplass, Oslo (demo) |

**Fortell om arrangementet:**

```text
DEMO – dette er en fiktiv bookingforespørsel.

Vi ønsker å invitere Arben Bala med foredraget «Uten frykt» til en åpen kveld om tilhørighet, valg og nye muligheter. Målgruppen er voksne og ungdom over 16 år, og vi regner med omtrent 100 deltakere.

Vi ønsker oppstart kl. 18.00, med omtrent 60 minutter foredrag og 30 minutter til spørsmål. Vi stiller med mikrofon, prosjektor og en vert som tar imot publikum. Kan du gi beskjed om tilgjengelighet, honorar og eventuelle tekniske behov?
```

Etter innsending: vis mottaksreferansen. Logg inn på /admin, åpne «Horisont kulturforum (demo)», velg «Avtalt» og trykk «Lagre status». Knappen blir aktiv igjen når lagringen er ferdig. Ingen melding sendes til arrangøren.

Velg «Opprett arrangement fra forespørselen». Kontaktinformasjon og den private meldingen kopieres ikke til arrangementet. Se over forhåndsutfyllingen og bruk disse verdiene:

| Felt i arrangementseditoren | Verdi                                                 |
| --------------------------- | ----------------------------------------------------- |
| Tittel                      | Uten frykt – en kveld med Arben Bala (demo)           |
| Dato                        | 18.02.2027, eller samme fremtidige dato som bookingen |
| Klokkeslett                 | 18:00                                                 |
| Sted                        | Horisont møteplass (demo)                             |
| Adresse                     | Oslo, Norge                                           |
| Arrangør                    | Horisont kulturforum (demo)                           |
| Bilde                       | Bruk standardbildet                                   |
| Ekstern lenke               | La feltet stå tomt                                    |

**Offentlig beskrivelse:**

```text
Dette er et fiktivt demoarrangement.

Møt Arben Bala til en personlig foredragskveld om tilhørighet, viktige valg og muligheten til å begynne på nytt. Med utgangspunkt i sin egen historie inviterer Arben til refleksjon og samtale.

Foredraget varer omtrent én time, etterfulgt av spørsmål fra publikum. Dørene åpner kl. 17.30. Kvelden passer for voksne og ungdom fra 16 år.
```

Forhåndsvis og lagre som utkast. Vis at det ikke ligger på den offentlige arrangementssiden. Publiser fra kontrollpanelet og last arrangementssiden på nytt. Nå vises arrangementet for publikum.

## Bookingeksempel 2: skuespilleroppdrag

Bruk dette for å vise at et skuespilleroppdrag ikke kan opprettes som et offentlig arrangement.

| Felt i skjemaet               | Verdi                    |
| ----------------------------- | ------------------------ |
| Navnet ditt                   | Adrian Eksempel          |
| Firma eller organisasjon      | Lyspunkt film (demo)     |
| E-post                        | adrian@example.test      |
| Telefon                       | La feltet stå tomt       |
| Hva ønsker dere å booke?      | Skuespilleroppdrag       |
| Ønsket dato                   | La feltet stå tomt       |
| Omtrent antall deltakere      | 10                       |
| Hvor skal arrangementet være? | Filmstudio i Oslo (demo) |

**Fortell om arrangementet:**

```text
DEMO – dette er en fiktiv bookingforespørsel.

Vi ønsker å høre om Arben Bala er interessert i en rolle i kortfilmen «Et nytt kapittel». Vi planlegger to opptaksdager i Oslo og ønsker å avtale en gjennomlesning før datoene fastsettes.

Dette er et privat skuespilleroppdrag uten publikum. Kan vi få informasjon om tilgjengelighet og honorar?
```

Sett status til «Avtalt» i innboksen. Vis at knappen for å opprette arrangement ikke finnes, og at oppdraget ikke vises offentlig.

## Forslag til en kort demonstrasjon

1. Vis de kommende arrangementene, et kort med lang tittel og «Mer»-knappen. Vis ett kort i bredden på mobil.
2. Send foredragsforespørselen fra eksempel 1 og vis referansen.
3. Vis adminoversikten med nye forespørsler. Åpne den nye forespørselen, endre status og opprett et utkast.
4. Vis skillet mellom utkast og publisert arrangement. Åpne kartlenken om du vil vise veibeskrivelse.
5. Vis det avtalte skuespilleroppdraget fra Historiefilm produksjon eller send eksempel 2. Forklar hvorfor det blir i innboksen.
6. Vis filtrene for forespørsler og arrangementer. Åpne slettebekreftelsen og velg «Avbryt».

Skjemaet har spamvern: vent noen sekunder etter at det åpnes før innsending. Grensen er fem bookinginnsendinger per IP per 15 minutter. Forbered demoen uten å sende mange prøveskjemaer like før opptaket. Demo-kommandoen fyller databasen direkte og bruker ikke denne kvoten.
