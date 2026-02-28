---
applyTo: "Source/Server/**"
---

# Server Instructions

## Core Rules

### Layering and Feature Design

- Keep HTTP/API concerns, application/domain logic, and persistence concerns in separate layers.
- Prefer vertical-slice organization per feature/operation.
- Keep handlers thin: validate, map input, call service, map result to HTTP response.

### API Contracts and Validation

- Register endpoints using extension-based chains in `Pot.AspNetCore/Program.cs` (`Add*Endpoints()` pattern).
- Follow established FluentValidation + Problem Details patterns for request validation and error responses.
- Keep public API contracts on `RowId` (Guid), not internal integer identifiers.
- Respect authorization and multi-tenant patterns (`resource:action` permissions and site isolation).

### Data and Persistence

- Use existing repository/unit-of-work conventions in `Pot.Data`.
- Default read queries to no-tracking behavior and opt into tracking for write paths.
- Preserve `RowId` + `Etag` conventions in contracts/entities.
- Keep migrations in `Pot.Data.Migrations` and apply them through the migrations project.
- After editing entities/schema-impacting models, database migrations should be created via `add-migration`.
- Migration generation is typically a developer-owned step: prompt the developer to run/review `add-migration`, and do not perform migration generation autonomously.

### Runtime and Health

- Keep health checks available at `/_health`.

## Commands

Run in `Source/Server`:

- `dotnet run --project Pot.Data.Migrations`
- `dotnet run --project Pot.AspNetCore`
- `dotnet test`

## Integration Notes

- Docker API port is `5241`; local non-Docker backend for frontend proxy is `5242`.
- Cross-stack startup is handled from root task `docker-start-pot-client-server`.

## Expansion Notes

- Add rule updates to the subsection that matches the lifecycle stage (contract, persistence, runtime).
- Include concrete paths when introducing new conventions (for example specific projects or folders).
- If a server pattern appears in multiple features, update the relevant server instruction files in the same change.
