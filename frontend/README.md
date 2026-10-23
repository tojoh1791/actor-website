# Frontend

React og TypeScript med Vite og React Router. Se [prosjektets README](../README.md) for komplett oppsett, oppstart og tester og [arkitekturbeskrivelsen](../docs/architecture.md) for ansvarsfordelingen i koden.

Fra prosjektroten:

```sh
npm --prefix frontend ci
npm run dev:web
```

Nettsiden kjører normalt på [http://localhost:5173](http://localhost:5173). /api videresendes til backend på port 5080. Kontrollpanelet finnes på /admin; offentlige sider er /, /about, /talks, /book, /events og /contact.

Presentasjonsinnhold og kontaktkanaler ligger i src/content/site.ts. HTTP, feil og norsk datohåndtering ligger i src/lib. Sidene deler innlastingslogikk gjennom src/hooks/useApiQuery.ts. Se [innholdskildene](../docs/content-sources.md) før bilder eller biografi brukes offentlig.

Kjør **npm run lint:web**, **npm run test:web**, **npm run build:web** og **npm run format:check** fra prosjektroten for kontroller. **npm run format** retter formateringen.
