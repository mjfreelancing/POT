---
applyTo: "Source/Server/**"
---

# Server Instructions

## Core Rules

### Layering and Feature Design

- Keep HTTP/API concerns, application/domain logic, and persistence concerns in separate layers.
- Prefer vertical-slice organization per feature or operation.
- Keep handlers thin: validate, map input, call service, and map result to the transport boundary.

### Contracts and Validation

- Register endpoints and transport wiring using the repository's established composition pattern.
- Follow the repository's existing request validation and error-response conventions.
- Keep public contracts stable and use external identifiers instead of leaking internal persistence identifiers.
- Respect authorization and tenant or scope isolation patterns already established by the repository.

### Data and Persistence

- Use the existing repository and unit-of-work conventions when they already exist.
- Default read queries to no-tracking behavior and opt into tracking only for write paths when applicable.
- Keep schema and migration changes isolated to the repository's dedicated migration workflow.
- Treat migration generation as a developer-reviewed step unless the user explicitly asks for it.

### Runtime and Health

- Preserve documented health or readiness endpoints when modifying runtime startup or infrastructure behavior.

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
  - POT: Docker API port is `5241`; local non-Docker backend for frontend proxy is `5242`; cross-stack startup uses `docker-start-pot-client-server`.
- Keep transport and persistence rules in the subsection that matches the affected lifecycle stage.
- Move language-level rules to `csharp.instructions.md` or another language-specific baseline instead of duplicating them here.
