# ogame-like-pve

A browser-based, real-time space strategy game inspired by OGame. Build up
your planets, research technology and command fleets, but fight NPC factions
(pirates, alien hives, rogue AIs, ancient derelicts) instead of other players.

See [docs/PLAN.md](docs/PLAN.md) for the design and roadmap.

## Tech stack

- **Backend:** ASP.NET Core (.NET 10) minimal APIs, EF Core, PostgreSQL, ASP.NET Core Identity (cookie auth), SignalR
- **Frontend:** React + TypeScript (Vite), React Router, TanStack Query, `openapi-fetch`, `@microsoft/signalr`
- **Tests:** xUnit + Moq, Testcontainers (PostgreSQL), Vitest + Testing Library

## Getting started

Prerequisites: .NET 10 SDK, Node.js 22, Docker.

```bash
# 1. Start PostgreSQL
docker compose up -d

# 2. Run the API on http://localhost:5157.
#    In Development it applies migrations and creates an open season on startup.
dotnet run --project src/Game.Api

# 3. In another terminal, run the web app on http://localhost:5173
cd web
npm install
npm run dev
```

Open http://localhost:5173, create an account and join the season. The Vite dev
server proxies `/api` and `/hubs` (SignalR) to the API, so the browser sees one
origin and the auth cookie just works.

The development season is small (2 galaxies × 50 systems) and runs at 10×
economy speed, so a first mine takes seconds rather than minutes. If your
database still has a season from before planets existed, reset it with
`docker compose down -v`.

## Running tests and checks

```bash
dotnet test                              # unit + integration tests (needs Docker running)
dotnet format --verify-no-changes        # C# formatting (.editorconfig)

cd web
npm test                                 # frontend tests
npm run lint
npm run build                            # typecheck + production build
```

## API contract

The API's OpenAPI document is generated on every `dotnet build` into
`web/src/api/openapi.json`. The frontend's types are generated from it:

```bash
dotnet build
cd web && npm run gen:api                # updates web/src/api/schema.ts
```

Commit both files. CI fails if either one is out of date.

## Database migrations

```bash
dotnet tool restore
dotnet ef migrations add <Name> --project src/Game.Api --output-dir Infrastructure/Migrations
```

## Project layout

```
src/Game.Core/            game rules: formulas and definitions, no framework dependencies
src/Game.Api/             API: one folder per feature (Auth/, Seasons/, Players/, Planets/, Research/, Realtime/)
  Infrastructure/         DbContext, migrations, generic repository, specifications
tests/Game.Core.Tests/            rules and formulas
tests/Game.Api.Tests/             unit tests (entities)
tests/Game.Api.IntegrationTests/  HTTP tests against a real PostgreSQL (Testcontainers)
web/                      React + TypeScript frontend
docs/PLAN.md              game design and roadmap
```
