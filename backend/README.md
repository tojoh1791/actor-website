# Backend

ASP.NET Core Minimal API med Entity Framework Core, Npgsql og ASP.NET Core Identity. Se [prosjektets README](../README.md) for komplett oppsett og [arkitekturbeskrivelsen](../docs/architecture.md) for kodekonvensjoner.

Endpoints beskriver rutene og arbeidsflytene. Contracts inneholder API-felter og validering, mens Models og Data beskriver lagringen. Security håndterer innlogging, tilgang og spamvern. Services/EventImageStore.cs validerer og lagrer bilder.

API-et kjører normalt på [http://localhost:5080](http://localhost:5080). Start med **npm run dev:api** fra prosjektroten. Skriptet laster den ignorerte .env; tilkoblingshemmeligheter legges ikke i appsettings.

Databaseoppdateringer kjøres eksplisitt med **npm run db:migrate**. Ved en ny modellendring kan en migrering opprettes med:

```sh
dotnet ef migrations add Navn --project backend/ActorWebsite.Api --output-dir Data/Migrations
```

Bruk **npm run build:api**, **npm run test:api** og **npm run format:api:check** for kontroller. Integrasjonstestene krever PostgreSQL og lager egne midlertidige databaser; se [README](../README.md#bygg-tester-og-kodekvalitet).

Administratorkonto opprettes i en interaktiv terminal med **npm run admin:create**. Se [administrasjonsveiledningen](../docs/admin.md) for passordgjenoppretting, tilgangskrav og lagring.
