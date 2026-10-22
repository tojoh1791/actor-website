# Innhold, bilder og kontaktkonfigurasjon

Kilder sjekket 22. oktober 2026. Tekstene på nettsiden er egne, korte oppsummeringer. Det er ikke lagt inn anmeldelser, sitater, priser eller kommende datoer uten bekreftelse.

## Faktakilder

- Skuespillerkarriere og utvalgte roller: https://pullman.as/talent/arben-bala
- Tittel og temaer for foredraget Uten frykt: https://www.athenas.no/foredragsholdere/arben-bala/
- Boken Balkanbrødre, forfatter Anders Moe, forlag Pullman Publishing og utgivelse i 2025: https://www.ark.no/produkt/balkanbrodre-9788293891291

Boken presenteres som skjønnlitteratur basert på Arbens liv. Romanhandling brukes ikke som en faktabiografi. Tekstene bør gjennomgås med Arben før ordinær publisering.

## Bildefiler

- frontend/public/images/arben-bala.jpg: portrett hentet fra Pullman Management. Fotograf: Are Næsset. Krediteres ved bildet og i sidefoten.
  Kilde: https://pullman-management.imgix.net/AHN_ArbenBala_-1-1-1.jpg?auto=compress%2Cformat&fit=clip&fm=jpeg&q=80&s=3ce056069b9bc136cd67f94eb5a691f0&w=750
- frontend/public/images/balkanbrodre.jpg: bokomslag fra ARKs produktpresentasjon. Forlag: Pullman Publishing. ISBN 9788293891291.
  Kilde: https://images.cdn.europe-west1.gcp.commercetools.com/b0c1af64-23c6-499f-8892-0976d37c1c31/default-6_4CEGF6-large.jpg

Filene er lagret lokalt slik at nettsiden ikke trenger å laste bilder fra tredjepart ved hvert besøk. Kildene dokumenterer ikke en fri bildelisens; tillatelse til bruk på den endelige offentlige nettsiden må avklares med Arben/rettighetshaver.

## Kontakt og booking

frontend/src/content/site.ts samler navn, bildebaner, bokinformasjon, roller og contactSettings.

- email: e-postadresse for henvendelser, eller null.
- phone: offentlig telefonnummer, eller null.
- bookingUrl: avtalt ekstern bookingadresse eller en fremtidig lokal skjemarute, eller null.

Ingen kanal er valgt. Alle tre står som null, og ingen falsk adresse eller aktivt bestillingsskjema vises. BookingLink bruker bookingUrl først, deretter e-post. Uten kanal leder lenken til kontaktsiden, som opplyser at kontaktinformasjon kommer. Telefon vises på kontaktsiden når det er satt.

Bruk bare godkjente lenker; eksterne bookinglenker skal bruke HTTPS. En fremtidig lokal skjemarute må implementeres før bookingUrl settes til den. Ikke legg hemmeligheter i denne filen; den inngår i frontend-bygget.

En bookingforespørsel skal ikke automatisk opprette eller publisere et offentlig arrangement. Denne innholdsoppdateringen innfører ingen innsending, e-postutsending, bookinglagring eller ny backend-funksjonalitet.

## Kontroll av denne oppdateringen

- Frontend-produksjonsbygg med TypeScript: bestått.
- HTTP-kontroll av sider og begge lokale bilder: bestått.
- Nettleserkontroll av alle seks offentlige sider ved 390 pikslers bredde: ingen horisontal overflyt; portrett og bokomslag lastet.
- Forsiden kontrollert med 1280 pikslers layout: ingen horisontal overflyt.
- Navigasjon fra forside til foredrag og videre til kontaktsiden: bestått.
- Uten valgt kanal vises ingen e-postadresse, telefonlenke eller aktivt bestillingsskjema.

Kontrollen gjelder innhold, presentasjon og navigasjon. Kontaktkanalene med faktiske verdier må kontrolleres når de tas i bruk. Backend og databasen er ikke endret i denne oppdateringen.
