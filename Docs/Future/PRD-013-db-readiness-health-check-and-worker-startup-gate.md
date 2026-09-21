# DB Readiness Health Check and Worker Startup Gate PRD

**Status:** Complete
**Priority:** Low
**Last Updated:** 2026-09-21
**Feature ID:** 013
**Scope:** `Source/Server/Pot.AspNetCore` — health check registration, health endpoints, and background worker startup gating. No client, database, or Docker Compose changes.

## Problem Statement

Background workers (`ExpiredOtpCleanupWorker`, `BudgetReminderEmailWorker`) begin executing immediately when the application starts. They attempt database operations on their first tick before the database is guaranteed to be available. In production this is prevented by Docker Compose's `pg_isready` healthcheck combined with `depends_on: condition: service_healthy`, which holds the API container until Postgres is accepting connections.

This safety guarantee is not replicated outside production. Playwright's `webServer` configuration starts the dotnet process and waits for the HTTP port to respond — it does not wait for the database — and `globalSetup` (which starts the Testcontainers Postgres container) runs _after_ the webServer is ready. A local `dotnet run` can likewise start before the database. Because the guarantee cannot be assumed from the hosting environment, the workers gate themselves on database readiness; without that gate each worker retries on its next tick and logs a connection error per attempt, burying genuine runtime errors in noise.

## Environments and Guarantees

| Environment              | DB readiness guarantee                                                                   | Worker startup errors                                                           |
| ------------------------ | ---------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------- |
| Docker Compose           | `pg_isready` + `depends_on: service_healthy`                                             | None — the API does not start until the database is ready                       |
| Integration tests        | Workers removed via `services.RemoveAll<IHostedService>()` in `ApiWebApplicationFactory` | Not applicable                                                                  |
| Local E2E (Playwright)   | Workers gate on the `"database"` readiness check; `globalSetup` polls `/_health/ready`   | None — workers wait for the database instead of failing on every tick           |
| Local dev (`dotnet run`) | Workers gate on the `"database"` readiness check                                         | None — workers wait until the database is ready, logging warnings between polls |

## Solution

### Database readiness health check

`WebApplicationBuilderExtensions.AddPotHealthChecks()` registers `AddDbContextCheck<PotDbContext>("database", tags: ["ready"], …)`. The check's test query requires at least one applied migration, so readiness means the schema is usable — not merely that a connection can be opened.

### Health endpoints

- `/_health` — liveness. Mapped with `Predicate = _ => false`, so it reports `Healthy` while the process runs and executes no checks; production monitoring polls it every 10 s without incurring a database query.
- `/_health/ready` — readiness. Runs the checks tagged `ready` (currently the `"database"` check); this is the endpoint the E2E harness gates on.

### Worker startup gate

`IServiceHealthPoller.WaitForHealthyAsync(...)` blocks until the named check reports `Healthy`, polling at `ServiceHealthPollerOptions.PollingInterval` (2 s default — a startup concern, not configuration) and logging a `Warning` between attempts. Both database-dependent workers, `ExpiredOtpCleanupWorker` and `BudgetReminderEmailWorker`, await it at the top of `ExecuteAsync` before their first loop iteration, so no database work is attempted until the database is ready. Cancellation while waiting surfaces an `OperationCanceledException`, which the host treats as a normal stop during shutdown.

`SendEmailWorker` does not touch the database and has no gate.

### E2E

Playwright's `globalSetup` polls `/_health/ready` (bounded) before any test runs, and the API starts with both workers waiting rather than erroring. By the time `globalSetup` reports the database ready, the workers have already moved into their normal execution loop — no test configuration is involved. See ADR-012 §9 and §13.4.

## Out of Scope

- Changing Docker Compose configuration — the production DB readiness guarantee is already correct.
- Making the health check influence application startup (i.e., blocking the HTTP server from accepting requests) — the API remains available during database startup; only workers gate.
- Health check coverage for other dependencies (email, external services).

## Tests

The gate is a single shared collaborator (`IServiceHealthPoller`), so coverage sits on the poller plus a wiring test per worker:

- **Poller unit tests** — `Pot.AspNetCore.Tests/Concerns/Health/ServiceHealthPollerFixture.cs`: returns without waiting when the check is already healthy; polls (Unhealthy → Degraded → Healthy) until healthy; filters on the requested check name; does not poll when the token is already cancelled; stops waiting when cancelled between polls (the inter-poll delay surfaces `OperationCanceledException`, which the host treats as normal shutdown).
- **Per-DB-worker gate tests** — `Pot.AspNetCore.Tests/Features/Workers/ExpiredOtpCleanupWorkerFixture.cs` and `BudgetReminderEmailWorkerFixture.cs`: the worker requests the `"database"` health check before creating any scope (that is, before its first loop iteration), plus constructor null guards for the dependencies it owns. The "proceeds once healthy" side of the contract is covered by the poller test that returns on `Healthy`, so it is not duplicated per worker.
- **Readiness endpoint integration tests** — `Pot.AspNetCore.Integration.Tests/Features/Health/HealthCheckFixture.cs`: `/_health/ready` returns 200 when the DB context is available and 503 when it is not (`HealthCheckUnhealthyFixture` points the factory at an unreachable port). Integration tests remove hosted services (`services.RemoveAll<IHostedService>()`), so worker gating is deliberately not asserted there.
- **`SendEmailWorker`** — takes no poller dependency, so no gate or test is required.
- **Log output** — not asserted: log capture is unavailable repo-wide while the replacement logging test framework is built (see the `TODO(logging)` blocks in the existing fixtures).

## Design Notes (Rationale and Rejected Alternatives)

Reference notes for future maintainers. They record why the design looks the way it does and which alternatives were considered during design and not taken.

- **No retry library.** Polly was considered unnecessary: the gate is a straightforward `while` + `Task.Delay` poll, so no retry/backoff dependency was introduced for it.
- **The database check is exposed on `/_health/ready`, not `/_health`.** Attaching it to `/_health` was the original intent, but production monitoring polls `/_health` every 10 s and must not incur a database query; liveness (is the process up?) and readiness (is the database usable?) are therefore separate endpoints.
- **One shared `IServiceHealthPoller`, not a private polling helper per worker.** Both database-dependent workers need identical polling behaviour, so the loop lives behind a single injected collaborator: no duplicated logic, one unit-test seam for the gate itself, and the "is the gate awaited before work starts?" check per worker stays trivial.
- **The polling interval is fixed in code, not configuration.** `ServiceHealthPollerOptions.PollingInterval` defaults to 2 seconds — database readiness at startup is a startup concern, not an operational tuning knob.
- **Readiness requires applied migrations, not just an open connection.** A connection can succeed against an unmigrated database, which would release the workers against a schema that is not usable yet; the check therefore requires at least one applied migration.
- **Workers log at `Warning`, not `Error`, while waiting.** The wait is expected and self-healing, so it must not read as a failure in the logs, where it would mask genuine runtime errors.
