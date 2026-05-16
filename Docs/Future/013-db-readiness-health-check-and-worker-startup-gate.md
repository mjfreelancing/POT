# DB Readiness Health Check and Worker Startup Gate PRD

**Status:** Backlog
**Priority:** Low
**Last Updated:** 2026-05-16
**Feature ID:** 013

## Problem Statement

Background workers (`ExpiredOtpCleanupWorker`, `BudgetReminderEmailWorker`) begin executing immediately when the application starts. They attempt database operations on their first tick before the database is guaranteed to be available. In production this is prevented by Docker Compose's `pg_isready` healthcheck combined with `depends_on: condition: service_healthy`, which holds the API container until Postgres is accepting connections.

This safety guarantee is not replicated during local E2E test runs. Playwright's `webServer` configuration starts the dotnet process and waits for the HTTP port to respond — it does not wait for the database. `globalSetup` (which starts the Testcontainers Postgres container) runs _after_ the webServer is ready. This creates a startup gap of several seconds during which the workers repeatedly attempt database connections, catch the resulting exceptions, log errors, and retry on their next scheduled tick.

The errors are transient and do not cause test failures, but they produce misleading log noise that could mask genuine runtime errors.

## Current Behavior

| Environment              | DB readiness guarantee                                                                   | Worker startup errors                                                               |
| ------------------------ | ---------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------- |
| Docker Compose           | `pg_isready` + `depends_on: service_healthy`                                             | None — API does not start until DB is ready                                         |
| Integration tests        | Workers removed via `services.RemoveAll<IHostedService>()` in `ApiWebApplicationFactory` | Not applicable                                                                      |
| Local E2E (Playwright)   | None — webServer starts before `globalSetup`                                             | DB connection errors logged per worker tick until Testcontainers container is ready |
| Local dev (`dotnet run`) | Developer-dependent                                                                      | Possible if DB not yet started                                                      |

## Proposed Solution

### 1. Custom DB health check

Register a named health check (e.g., `"database"`) that verifies a live database connection can be opened. ASP.NET Core's `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` package (already referenced) can provide this via `AddDbContextCheck<PotDbContext>`. Map the check to the existing `/_health` endpoint.

The health endpoint should return `Healthy` only when the database is reachable and accepting connections.

### 2. Worker startup gate

Each DB-dependent worker (`ExpiredOtpCleanupWorker`, `BudgetReminderEmailWorker`) injects `IHealthCheckService` and, before entering its execution loop, polls the `"database"` health check at a regular interval (e.g., every 2 seconds) until the result is `Healthy` or the cancellation token fires. During this polling phase the worker logs a `Warning` rather than an `Error` so the intent is clear in the logs.

`SendEmailWorker` does not interact with the database and requires no startup gate.

### 3. Benefit in E2E

When Playwright starts the API, the workers detect the DB is not yet ready and wait silently (log warnings only). By the time `globalSetup` completes and Testcontainers signals DB readiness, the workers transition from polling to their normal execution loop automatically. No test configuration changes are required.

## Implementation Notes

- The health check registration belongs in `WebApplicationBuilderExtensions` alongside other infrastructure registrations.
- Workers gain a constructor dependency on `IHealthCheckService`. The polling loop should be a private helper method (e.g., `WaitForDatabaseReadyAsync`) to keep `ExecuteAsync` readable.
- The `/_health` endpoint mapping already exists in `Program.cs`; it should be updated to include the DB check in its check set (currently it may return `Healthy` unconditionally or reflect a different subset of checks).
- The polling interval and maximum wait duration should be constants within the worker, not configuration — this is a startup-time concern only, not a tuneable operational parameter.
- Polly is not required. A simple `while` + `Task.Delay` is sufficient for the polling loop given the straightforward use case.

## Out of Scope

- Changing Docker Compose configuration — the production DB readiness guarantee is already correct.
- Making the health check influence application startup (i.e., blocking the HTTP server from accepting requests) — the API should remain available during DB startup; only workers should gate.
- Health check coverage for other dependencies (email, external services).

## Test Considerations

- Unit tests for each DB worker should cover the startup gate: worker does not proceed to its loop body if the health check returns `Unhealthy`, and does proceed once it returns `Healthy`.
- An integration test should verify `/_health` returns the expected status when the DB context is available.
- `SendEmailWorker` requires no additional tests related to this feature.
