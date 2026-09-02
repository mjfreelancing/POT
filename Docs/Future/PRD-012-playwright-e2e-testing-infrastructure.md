# Playwright E2E Testing Infrastructure — Product Requirements Document

**Feature ID:** 012
**Status:** Approved & Implemented (test catalog complete; dev + prodlike matrices green)
**Last Updated:** 2026-09-02

## Purpose of this document

This is the **Product Requirements Document (PRD)** for POT's Playwright E2E testing
infrastructure. It defines **what** the infrastructure must deliver — goals, non-goals,
requirements, acceptance criteria, and the decisions behind them — so the work can be
planned, implemented, and evaluated.

The **implemented design and current state** (the architecture, operational detail, and the
history of approaches that were tried and rejected) are documented separately in
[ADR 012 — Playwright E2E Testing Infrastructure: Design & Current State](ADR-012-playwright-e2e-testing-infrastructure.md).
Read the PRD for requirements; read the ADR for how they are met.

---

## Table of Contents

1. [Overview](#1-overview)
2. [Goals](#2-goals)
3. [Non-Goals](#3-non-goals)
4. [Requirements](#4-requirements)
5. [Acceptance Criteria](#5-acceptance-criteria)
6. [Constraints](#6-constraints)
7. [Risks and Mitigations](#7-risks-and-mitigations)
8. [Key Decisions and Rationale](#8-key-decisions-and-rationale)
9. [References](#9-references)

---

## 1. Overview

POT needs browser-level (E2E) test coverage for behaviours that unit and integration tests
cannot exercise: end-to-end UI flows, navigation, user interactions, error handling and
feedback, desktop-vs-mobile layout, custom controls, and PWA/theme behaviour.

The infrastructure must run on a developer's local machine, be fast and deterministic
enough to gate work on, and be easy for developers and coding agents to extend. The chosen
approach is a Playwright suite over **one shared stack** — a single API process, a single
client server, and one Testcontainers-managed Postgres database — with test isolation
achieved by a two-class classification model rather than per-test infrastructure.

## 2. Goals

- **G1 — Local, Docker-only execution.** The whole suite runs on a developer machine using
  Testcontainers for the database; no shared or dev-server databases are targeted.
- **G2 — Deterministic and repeatable.** Given a clean start, a run produces the same result
  every time, and repeated runs do not collide with seed data or prior runs.
- **G3 — Parallel-safe by default.** The suite runs the full browser matrix in parallel
  against a shared stack without per-test containers, using classification + teardown for
  isolation.
- **G4 — Fast authoring feedback.** A targeted single-project run is quick enough to be the
  day-to-day loop, and the full matrix is bounded so the shared stack stays responsive.
- **G5 — Extensible and documented.** Tests, fixtures, and helpers follow documented
  conventions that developers and coding agents can follow.
- **G6 — Production-faithful.** Tests exercise only endpoints and UI that exist in the
  production application; the server is not modified for tests.

## 3. Non-Goals

The following are explicitly **out of scope** for this PRD (recorded as non-goals so they
are not re-scoped later):

- **NG1 — CI pipeline integration.** Out of scope; revisit after release gates.
- **NG2 — Cloud-hosted execution.**
- **NG3 — Per-test isolated containers.** All workers share one stack; isolation is by
  classification, not infrastructure.
- **NG4 — Test-only server endpoints.** No server changes purely for test convenience.
- **NG5 — OTP / email-based E2E flows** (password reset, new-user signup). Deferred
  indefinitely — they require a local mail sink, SMTP configuration, and an OTP capture
  helper, and are not needed for the core catalog.
- **NG6 — E2E coverage of calculation logic.** Renewal/accrual/projection mathematics,
  import/export server behaviour, and auth endpoint semantics are owned by server unit +
  integration tests. E2E verifies that the UI **renders** data correctly, not the maths.
- **NG7 — Alternative database modes** (e.g. users-only or blank modes). The suite runs a
  single fully-seeded mode.

## 4. Requirements

Each requirement states what the infrastructure must do, its rationale, and its current
status. "Implemented" means the requirement is met in the current codebase; the ADR
references the concrete design.

### R1 — One shared stack, no per-test containers

The E2E suite SHALL run all tests against **one shared stack**: a single ASP.NET Core API
process (port `5242`), a single client server (port `5175`), and one Testcontainers-managed
Postgres (port `55432`), shared by every worker and all four browser projects.

- **Rationale:** Bounded infrastructure cost and predictable startup; isolation is achieved
  by classification and teardown rather than container-per-test.
- **Status:** Implemented. **Ref:** ADR §3.

### R2 — Single fully-seeded database mode

Every run SHALL fully seed the database: migrations → `e2e/seed/baseline.sql` (site +
canonical users) → import `e2e/seed/financial.export` (63 records) → server renewal/accrual
to the local date. There SHALL be no mode-selection variable or mode-specific scripts.

- **Rationale:** Most tests benefit from seeded data; a single mode removes a script matrix
  and configuration surface for no coverage gain.
- **Status:** Implemented. **Ref:** ADR §4, §8.

### R3 — Test data via committed artifacts only

Site/user data SHALL come from `baseline.sql`; financial data SHALL come from importing the
committed `financial.export` artifact through the server API. Tests SHALL NOT create data in
setup unless the creation call is the behaviour under test.

- **Rationale:** Repeatability and a stable baseline; no hidden state drift.
- **Status:** Implemented. **Ref:** ADR §9.

### R4 — Canonical seeded identities

The seed SHALL establish canonical identities: `e2e_admin` (Admin), `e2e_viewer` (Viewer),
`e2e_pwchange` (Viewer, with its own password so the password-change test can rotate it
without invalidating the shared admin), plus the platform-admin user
`58d1a6b2-bf25-40c1-b3d6-656ccf4a68b5`. Credentials SHALL be centralised in
`e2e/helpers/secrets.ts` and match the hashes in `baseline.sql`.

- **Status:** Implemented. **Ref:** ADR §5.

### R5 — Fresh per-test authentication with access-token reuse

Each authenticated test SHALL perform one fresh API login and be given both the resulting
`storageState` (for the browser context) and the 15-minute `accessToken` (for API
setup/cleanup). Fixtures SHALL exist for admin (`test`), viewer (`viewerTest`), and the
password-change identity (`pwChangeTest`). Tests exercising logged-out pages SHALL use an
empty-storage-state context, not an auth fixture.

- **Rationale:** The server rotates the refresh token on every `/api/auth/refresh`, so a
  refresh cookie is single-use; shared or worker-scoped storage state causes 401s and login
  redirects. Reusing the `accessToken` avoids extra PBKDF2 logins (the largest server-side
  CPU cost under parallel load).
- **Status:** Implemented. **Ref:** ADR §6.

### R6 — Two-class isolation model

Tests SHALL be classified as either **parallel-safe** (read-only: GET-only or UI-only, safe
to run concurrently) or **fixture-managed** (creates/updates/deletes its own records). A
fixture-managed suite SHALL run inside `test.describe.serial`, clean up in `test.afterEach`
(never `afterAll`), create records with deterministic unique descriptions, and delete its
rows through the API using the `accessToken` via one request context.

- **Rationale:** Correctness under local parallel execution; when in doubt, classify as
  fixture-managed.
- **Status:** Implemented. **Ref:** ADR §7, §12.3.

### R7 — Four-project browser matrix

The suite SHALL run on `chromium`, `edge` (msedge channel), `mobile-chrome` (Pixel 7), and
`mobile-safari` (iPhone 14). Tests that are desktop-only SHALL skip on mobile projects via an
in-test guard.

- **Status:** Implemented. **Ref:** ADR §3.1, §10.

### R8 — Dev and prodlike configurations

Two configs SHALL be provided over the same four projects: the default **dev matrix**
(`playwright.config.ts`, Vite dev server) and the on-demand **prodlike gate**
(`playwright.prod.config.ts`, built client via `npm run build && npm run preview`). The
prodlike config SHALL ignore tests that only apply to the dev source (the errorBoundary
chunk-abort test), chromium-only shared-identity mutations (user settings), and desktop-only
filters on mobile. The prodlike config's API webServer SHALL mirror the dev config exactly
(Release build, platform-admin grant, log redirect, start timeout).

- **Rationale:** Prodlike is a production-build gate, not the default loop.
- **Status:** Implemented. **Ref:** ADR §3.2.

### R9 — UI expectations derived from API responses

Tests SHALL derive expected UI values from intercepted API payloads, not hardcoded numbers.
Read-only lists SHALL be prefetched via a request context with the `accessToken`; slow POSTs
SHALL be asserted by capturing the response, awaiting `response.finished()`, then asserting
the UI signal with headroom.

- **Rationale:** Avoids stale fixtures and load-starved `waitForResponse` races.
- **Status:** Implemented. **Ref:** ADR §12.4.

### R10 — Deterministic startup and readiness

Global setup SHALL: start the Postgres container (with `citext` installed before first
connection), run migrations, load `baseline.sql`, import + renew/accrue the financial data,
and gate test start on a **bounded** `GET /_health/ready` check (throws with a pointer to the
server log rather than hanging). The client dev server SHALL disable HMR for E2E (`E2E=1`),
and the shared stack SHALL be pre-warmed by mounting the authenticated routes once so the
first test does not pay the cold start.

- **Status:** Implemented. **Ref:** ADR §8, §13.

### R11 — Load-tuned configuration

The config SHALL: run the API in Release, bound local workers to 2 (1 on CI), use a single
retry as a safety net, record video/trace on retry only, keep the per-test timeout at 60s,
and set the expect timeout to 10s.

- **Rationale:** The shared stack caps sustainable parallelism; raising the timeout worsens
  contention by lengthening heavy tests.
- **Status:** Implemented. **Ref:** ADR §3.1, §10.3, §13.

### R12 — Defined coverage catalog

The suite SHALL cover the areas required by the E2E catalog: smoke, auth, navigation,
rendering, feedback/error handling, CRUD lifecycles, dashboard quick actions, filters,
approvals, user settings, export, mobile layout, the EnrichedCalendar picker, data tables,
modal dialogs, theme, empty states, and the PWA contract.

- **Status:** Implemented (test catalog complete). **Ref:** ADR §14.

## 5. Acceptance Criteria

The PRD is met when all of the following hold:

- **AC1 — Dev matrix green.** The full 4-project dev matrix (`npm run e2e:all:dev`) passes
  three consecutive clean runs. Verified 2026-09-01: **262 passed / 72 skipped / 0 failed**.
- **AC2 — Prodlike gate green.** The full prodlike matrix (`npm run e2e:all:prodlike`)
  passes. Verified 2026-09-01: **258 passed / 72 skipped / 0 failed**.
- **AC3 — Smoke green.** `npm run e2e:smoke` passes (20 tests).
- **AC4 — No cross-run contamination.** Fixture-managed suites clean up in `afterEach`, and
  repeated runs never collide with seed data or prior runs.
- **AC5 — Scripts verified.** The runnable e2e npm scripts execute and pass as expected
  (chromium, smoke, dev matrix, prodlike matrix; edge/mobile/dev are subsets of the matrix;
  UI/headed/debug/report/install are interactive).
- **AC6 — Docs current.** Reference docs describe the current single-mode, two-class system
  only; references to removed approaches live only in the ADR's design-history appendix.

## 6. Constraints

Fixed, non-negotiable rules:

- **C1 — Docker-only execution** against a Testcontainers-managed database.
- **C2 — No test-only server endpoints.**
- **C3 — Committed seed artifacts only** (no ad-hoc API data creation in setup).
- **C4 — No per-test isolated containers**; one API + one database per run.
- **C5 — Single database mode**; no mode-selection variable.
- **C6 — Server behaviour is not modified for tests** (OTP/security code paths unchanged).
- **C7 — Backend targets .NET 10; client is React 19 + Vite 6** (fixed stack).

## 7. Risks and Mitigations

| Risk                                                                                  | Mitigation                                                                                                                                                 |
| ------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Shared-stack contention flakes under high parallelism                                 | Bound workers (2 local / 1 CI); reduce per-test cost (Release API, one login/test, `accessToken` reuse); retries as safety net; do not raise the timeout   |
| `docker stop` leaves port in TIME_WAIT on Windows → next run "port already allocated" | Teardown does not stop the container; Ryuk reaper removes it after exit; stale containers are `docker rm -f`'d at startup with a bounded port-release poll |
| Server starts before the DB container exists                                          | API survives via Npgsql retry; `/_health/ready` gate (bounded) blocks tests until the DB is live                                                           |
| Refresh-token rotation invalidates shared auth state                                  | Fresh login per test; `accessToken` reused for API calls; no shared storage state                                                                          |
| Cold-start flake on the first test                                                    | Shared-stack pre-warm mounts the authenticated routes once before tests run (non-fatal on failure)                                                         |
| Prodlike config drifts from dev (breaks platform-admin flows / fills piped stdout)    | Prodlike API webServer mirrors the dev config exactly (Release, platform-admin grant, log redirect, 180s timeout)                                          |
| Unbounded readiness wait hangs the toolchain                                          | Bounded readiness gate (~2 min) that throws with a pointer to `e2e-server.log`                                                                             |
| `citext` type missing from Npgsql cache                                               | Container init script installs `citext` before the API's first connection                                                                                  |

## 8. Key Decisions and Rationale

| #   | Decision                                                                   | Rationale                                                                                                                                                                |
| --- | -------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| D1  | Single fully-seeded database mode (no users-only / blank modes)            | Most tests need seeded data; removes a mode-selection variable and the script matrix                                                                                     |
| D2  | Keep and repair the shared Testcontainers infrastructure (one API, one DB) | Already largely built and deterministic; rebuilding would discard working code                                                                                           |
| D3  | No isolated-sequence execution class                                       | Renewal/accrual/projection calculations are covered by server unit + integration tests; E2E keeps only read-only UI rendering/correctness of seeded data (parallel-safe) |
| D4  | Fresh per-test login (no worker-scoped shared auth state)                  | The server rotates the refresh token on every refresh; shared storage state is single-use and fails on reuse                                                             |
| D5  | No OTP/MailPit e2e for now                                                 | Requires a new Docker service + SMTP config + OTP helper; defer until the core catalog is green                                                                          |
| D6  | Dev matrix is the default; prodlike is an on-demand gate                   | Build + preview catches production-only issues; not needed for the dev-mode catalog                                                                                      |
| D7  | Keep Vite dev server for the dev matrix (HMR disabled)                     | A built-client local matrix regressed (PWA service-worker install per context; errorBoundary aborts a dev-source chunk) — see ADR design history                         |

Rejected alternatives and the full design history are recorded in
[ADR 012, Appendix A](ADR-012-playwright-e2e-testing-infrastructure.md#appendix-a-design-history--what-was-tried-and-rejected).

## 9. References

- [ADR 012 — Design & Current State](ADR-012-playwright-e2e-testing-infrastructure.md)
- `Source/Client/pot-react/e2e/README.md` — architecture & conventions
- `Source/Client/pot-react/e2e/AUTHORING.md` — authoring guide
- `Source/Client/pot-react/playwright.config.ts`, `playwright.prod.config.ts`,
  `playwright.globalSetup.ts` — implementation
- `.github/instructions/playwright-e2e.instructions.md` — agent rules
