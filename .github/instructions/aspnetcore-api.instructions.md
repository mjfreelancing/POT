---
applyTo: "**/*.cs"
---

# ASP.NET Core API Instructions

## Core Rules

### Layering and Feature Design

- Keep HTTP/API concerns, application/domain logic, and persistence concerns in separate layers.
- Prefer vertical-slice organization per feature or operation.
- Keep handlers thin and focused on orchestration: validate, map input, call service, and map result to the transport boundary.

### Endpoint and Validation

- Keep endpoint registrations explicit and discoverable, using the repository's established composition pattern for endpoint and transport wiring.
- Follow the repository's existing request validation and error-response conventions, keeping ProblemDetails mapping consistent.

### Contracts and Security

- Keep API contracts stable and explicit, using external identifiers instead of leaking internal persistence identifiers.
- Enforce authorization at endpoint/service boundaries, respecting tenant or scope isolation patterns already established by the repository.
- Avoid leaking sensitive request/header data in errors.

### Data and Persistence

- Use the existing repository and unit-of-work conventions when they already exist.
- Default reads to no-tracking and opt into tracking only for write paths when applicable.
- Keep schema and migration changes isolated to the repository's dedicated migration workflow (for example the migrations project).
- Treat migration generation as a developer-reviewed step unless the user explicitly asks for it.

### Runtime and Health

- Preserve documented health or readiness endpoints and keep health checks available and stable when modifying runtime startup or infrastructure behavior.

## Expansion Notes

- Add repository-specific commands, project names, and paths in consuming copies of this file.
  - POT: Register endpoints using extension-based chains in `Pot.AspNetCore/Program.cs` (`Add*Endpoints()` pattern).
  - POT: Follow FluentValidation plus Problem Details patterns for request validation and error responses.
  - POT: Keep public contracts on `RowId` (Guid) and preserve `RowId` plus `Etag` conventions.
  - POT: Use `resource:action` permission strings and existing site-isolation patterns.
  - POT: Use repository and unit-of-work conventions in `Pot.Data`; migrations live in `Pot.Data.Migrations`.
  - POT: After editing entities or schema-impacting models, prompt the developer to run and review `add-migration`; do not generate migrations autonomously unless explicitly asked.
  - POT: Keep health checks available at `/_health`.
  - POT: Run server commands from `Source/Server`, including `dotnet run --project Pot.Data.Migrations`, `dotnet run --project Pot.AspNetCore`, and `dotnet test`.
  - POT: Docker API port is `5241`; local non-Docker backend for frontend proxy is `5242`; cross-stack startup uses `docker-start-client-server`.
- Keep transport and persistence rules in the subsection that matches the affected lifecycle stage.
- Keep persistence-provider specifics (for example Postgres/EF conventions) in provider-specific instruction packs.
- Move language-level rules to `csharp.instructions.md` or another language-specific baseline instead of duplicating them here.
