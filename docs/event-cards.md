# Arrangementskort og farger

Den offentlige arrangementsoversikten bruker EventCard-komponenten. Alle lukkede kort har like høye bildeflater og tekstfelt ved samme skjermbredde. Tittelen har plass til to linjer med uendret skriftstørrelse. Sted, adresse og en beskrivelse på inntil tre linjer har også reserverte høyder, sammen med plassen til Mer-knappen.

Arrangementsoversikten bruker hele innholdsbredden på opptil 1200 px. Den har to brede kort per rad på større skjermer, og ett kort per rad ved skjermbredder på 680 px eller mindre. Oversikten har dermed ikke den smalere grensen på 850 px som brukes til andre sideintroduksjoner.

Mer vises når tekst blir avkortet, eller arrangementet har ekstra opplysninger som sluttid, arrangør eller billettlenke. Knappen utvider bare det aktuelle kortet og endres til Mindre. Hele tittelen, adressen og beskrivelsen kan da leses. Knappen støtter tastatur og formidler åpen/lukket tilstand med aria-expanded og aria-controls. Avkorting kontrolleres på nytt ved endret bredde og etter at skrifter er lastet. Kortene vises i én kolonne på mobil og to på større skjermer.

Den lyse bakgrunnen er beholdt. Dyp rød (#932f36) brukes som aksent på knapper, lenker, datoer og små detaljer, med en mørkere rødfarge ved peking. Grønntoning på dekorative bakgrunner og kanter er erstattet med varme, nøytrale toner. Grønne bekreftelses- og publiseringsmarkører i kontrollpanelet beholdes som statusfarger.

## Kontrollert

- Frontend-produksjonsbygg med TypeScript bestod.
- Nettleserkontroll med separate eksempeldata: kort med kort, lang og svært lang tittel, korte og lange beskrivelser og ekstra praktisk informasjon.
- Alle fire lukkede kort hadde lik høyde, også over flere rader, og identisk skriftstørrelse og reserverte høyder for tittel.
- Utvidelse viste hele beskrivelsen; nabokortene beholdt høyden sin. Sammentrekking med tastatur virket.
- Sluttid, arrangør og billettlenke ble synlige under Mer.
- Mobilvisning ved omtrent 385 CSS-pikslers bredde: én kolonne, like høye lukkede kort og ingen horisontal overflyt. Lang tittel og adresse kunne utvides uten avkorting.

Kontrollen endret ingen arrangementer i applikasjonsdatabasen. Backend og pakker er uendret i denne oppdateringen.
