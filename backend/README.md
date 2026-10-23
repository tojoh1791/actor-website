# Backend

ASP.NET Core API i ActorWebsite.Api med EF Core, Npgsql og ASP.NET Core Identity.

Fra prosjektroten:

- Installer: dotnet restore ActorWebsite.slnx
- Installer EF-verktøy: npm run tools:restore
- Generer lokal konfigurasjon: npm run init:local
- Start PostgreSQL: npm run db:up
- Opprett/oppdater schema: npm run db:migrate
- Start API: npm run dev:api
- Bygg: npm run build:api

API-et bruker http://localhost:5080. API- og migreringsskriptene laster .env fra prosjektroten. Ingen tilkoblingshemmeligheter ligger i appsettings-filene.

Administratorinnlogging med Identity-cookie, Administrator-rolle, CSRF-beskyttelse og beskyttede arrangementsendepunkter er implementert. Opprett kontoen med npm run admin:create. Se docs/admin.md i prosjektroten for bruk, passordgjenoppretting og sikkerhetsdetaljer.

Opprett ny migrering fra prosjektroten med:

    dotnet ef migrations add Navn --project backend/ActorWebsite.Api --output-dir Data/Migrations

Bruk npm run db:migrate til å bruke migreringen på lokal database. Migreringer kjøres eksplisitt og ikke automatisk ved oppstart.
