---
applyTo: "**/*.cs"
---

# ASP.NET Core API Instructions

## Core Rules

### Endpoint and Validation

- Keep endpoint registrations explicit and discoverable.
- Keep request validation and ProblemDetails mapping consistent.
- Keep handlers minimal and focused on orchestration.

### Contracts and Security

- Keep API contracts stable and explicit.
- Enforce authorization at endpoint/service boundaries.
- Avoid leaking sensitive request/header data in errors.

### Persistence and Runtime

- Default reads to no-tracking where appropriate.
- Keep migrations isolated to the migrations project.
- Keep health checks available and stable.

## Expansion Notes

- Keep persistence-provider specifics (for example Postgres/EF conventions) in provider-specific instruction packs.
