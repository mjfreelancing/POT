# POT Architecture

This document provides a high-level overview of POT's technical architecture.

## Technology stack

These are the families in use. The versions live in `Source/Client/pot-react/package.json` and
`Source/Server/Directory.Packages.props`, which are the only places they are stated.

**Frontend** — React and TypeScript, built with Vite, styled with Tailwind CSS and shadcn/ui, routed with
React Router, with TanStack Query for server state, TanStack Table for data grids, Zustand for small global
state, React Hook Form and Zod for forms and schemas, Recharts for charts, and `vite-plugin-pwa` for the
installable shell.

**Backend** — ASP.NET Core, Minimal APIs, Entity Framework Core over PostgreSQL, FluentValidation, the
AllOverIt library family (ADR-0056), Scalar for the OpenAPI reference UI in Development only, MailKit for
email, and JWT for tokens.

**DevOps** — Docker and Docker Compose locally, nginx serving the built client, and Git.

## Project structure

```
POT/
├── Source/
│   ├── Client/pot-react/                     # React frontend application
│   │   ├── src/
│   │   │   ├── features/                     # Feature modules (auth, accounts, expenses, etc.)
│   │   │   ├── components/                   # Reusable UI components
│   │   │   ├── api/                          # API integration layer
│   │   │   ├── concerns/                     # Cross-cutting concerns (auth, cache, pwa, errors)
│   │   │   ├── contexts/                     # React context providers (access token, auth)
│   │   │   ├── data/                         # Zod schemas and API data types
│   │   │   ├── hooks/                        # Shared hooks
│   │   │   ├── lib/                          # Utilities and helpers
│   │   │   ├── routes/                       # Route definitions
│   │   │   └── stores/                       # Zustand stores (client-only state)
│   │   └── DEVELOPER.md                      # Frontend development guide
│   │
│   ├── Server/                               # ASP.NET Core backend
│   │   ├── Pot.AspNetCore/                   # API endpoints and configuration
│   │   ├── Pot.AspNetCore.Tests/             # Unit tests for API-layer components/services (non-hosted)
│   │   ├── Pot.AspNetCore.Integration.Tests/ # Hosted API integration tests
│   │   ├── Pot.App/                          # Business logic and services
│   │   ├── Pot.App.Tests/                    # Unit tests for app/business layer
│   │   ├── Pot.Data/                         # EF Core data access
│   │   ├── Pot.Data.Tests/                   # Unit tests for data layer
│   │   ├── Pot.Data.Migrations/              # Database migrations
│   │   ├── Pot.EmailSender/                  # Email infrastructure
│   │   ├── Pot.RazorComponents/              # Razor email templates
│   │   ├── Pot.Shared/                       # DTOs and shared types
│   │   ├── Pot.Shared.Tests/                 # Unit tests for shared types
│   │   ├── Pot.TestUtils/                    # Shared test helpers and utilities
│   │   └── DEVELOPER.md                      # Backend development guide
│   │
│   └── Docker/                               # Docker configuration
│       ├── docker-compose-client-server.yml
│       ├── .env                              # Base environment variables (tracked)
│       ├── .env.development.template         # Tracked development env template
│       ├── .env.development                  # Development settings (local, ignored)
│       └── DEVELOPER.md                      # Docker development guide
│
├── contracts/openapi/v1.json                 # Build-generated OpenAPI contract (committed)
├── Docs/                                     # Documentation (see Docs/README.md)
└── README.md                                 # Project overview
```

## Frontend architecture

The frontend is organised by feature over shared layers: features may use the shared `api/`, `concerns/`,
`components/` and `lib/` layers, and those layers never depend on a feature (ADR-0016). Related code sits
beside the feature it serves rather than in a technical folder, transport is centralised in one API layer,
and server state is cached rather than copied into component state (ADR-0017).

The rules an edit must follow are in
[`.github/instructions/client.instructions.md`](../.github/instructions/client.instructions.md); the worked
examples are in `Source/Client/pot-react/DEVELOPER.md`.

## Backend architecture

The backend is layered by responsibility — `Pot.AspNetCore` → `Pot.App` → `Pot.Data`, with `Pot.Shared`
cross-cutting — and each layer mirrors the same feature slices, so a feature is read vertically inside a
layer rather than scattered across it (ADR-0003). An endpoint is a minimal API slice with its own request
and response models (ADR-0002), business logic sits in services behind repositories (ADR-0011), and
expected failures are returned as results rather than thrown (ADR-0015).

The rules an edit must follow are in
[`.github/instructions/server.instructions.md`](../.github/instructions/server.instructions.md); the worked
examples are in `Source/Server/DEVELOPER.md`.

## Cashflow model

POT is obligation-first: income and expenses are dated obligations, and daily guidance is derived from
them. Two accrual figures are computed on the server — a dynamic one for operational simulation and a stable
one for planning — and no accrual value is ever stored; every accrual-bearing read computes from one engine
for the same as-of date, so two screens cannot disagree about the same day.

How the metrics, arrears, availability, guardrails and the day-by-day projection fit together is
[the cashflow model](explanation/cashflow-model.md). The individual terms are in the
[glossary](reference/glossary.md).

## Testing

A unit project per layer, plus one hosted integration project that boots the application and drives it over
HTTP (ADR-0037). Which kind a test is decides which project owns it — the definition is in
[`aspnetcore-integration-tests.instructions.md`](../.github/instructions/aspnetcore-integration-tests.instructions.md)
(Unit or integration?) — and the conventions for both are in
[`dotnet-tests.instructions.md`](../.github/instructions/dotnet-tests.instructions.md). Shared helpers live
in `Pot.TestUtils`.

## Database

PostgreSQL through Entity Framework Core, with enum values stored by name, uniqueness enforced by the
database and cascades restricted to an explicit ordered sweep (ADR-0004, ADR-0005, ADR-0006, ADR-0007,
ADR-0008). Migrations live in `Pot.Data/Migrations` and are applied by a separate runner before the API
starts (ADR-0009), in additive steps where a change would otherwise be destructive (ADR-0010).

## Authentication and authorization

Short-lived JWT access tokens backed by a server-side session, permissions resolved from the database on
every request and checked as `resource:action`, each tenant confined to its site (ADR-0021, ADR-0022,
ADR-0030). Sign-up requires email verification and platform-admin approval, and the verification codes are
single-use and expiring (ADR-0024).

The flows are in [authentication](explanation/authentication.md); the token, cookie, claim and session
details are in [auth and sessions](reference/auth-and-sessions.md).

## Docker

The full stack — PostgreSQL, the API, and nginx serving the client — is started by the
`docker-start-client-server` task. [Run the Docker stack](how-to/run-the-docker-stack.md) covers using it;
[ports and environments](reference/ports-and-environments.md) owns every port and lists what must change
with one.

## Development workflow

Setup is a tutorial rather than an architecture topic. [Getting started](tutorials/getting-started.md) takes
a newcomer from a clone to a running stack, [first-time setup](tutorials/first-time-setup.md) covers the
initial account and platform administrator, and the values the stack reads are described in
[configuration](reference/configuration.md).

## API communication

REST over `/api`, JSON, JWT bearer tokens. Every failure is an RFC 9457 Problem Details document: a `400`
carries the validation problem's `errors` map and every other failure carries `detail`, and every body
carries `correlationId`, `traceId` and `instance` (ADR-0012, ADR-0048). The base URL per environment is in
[ports and environments](reference/ports-and-environments.md).

## Client state

Server state is a TanStack Query cache with explicit invalidation; small global state is Zustand; the
short-lived access token is held in React context rather than in a store; forms are React Hook Form over Zod
schemas (ADR-0016, ADR-0017). The rules that follow from this are in
[`client.instructions.md`](../.github/instructions/client.instructions.md).

## Build and deployment

**Development** — the Vite dev server with hot module replacement, the API run locally or in a container,
and a PostgreSQL container or a local installation.

**Production** — two Azure Container Apps in one environment: nginx serving the built client, and a
container that runs the migration runner and then the API, each on its own hostname behind a managed
certificate, against a managed PostgreSQL server that requires SSL. There is no Docker Compose in
production, and TLS terminates at the ingress (ADR-0032, ADR-0033). Builds, tests and deployments are run by
hand — there is no continuous integration (ADR-0031) — and the deployed server version is not recorded in the
repository, so it has to be confirmed in the portal (ADR-0005).

---

**For detailed technical information:**

- [Frontend Development Guide](../Source/Client/pot-react/DEVELOPER.md)
- [Backend Development Guide](../Source/Server/DEVELOPER.md)
- [Docker Development Guide](../Source/Docker/DEVELOPER.md)
