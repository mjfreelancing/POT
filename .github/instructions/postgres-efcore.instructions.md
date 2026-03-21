---
applyTo: "**/*.cs"
---

# Postgres + EF Core Instructions

## Core Rules

### Modeling and Migrations

- Keep schema changes explicit and migration-backed.
- Keep migration generation and review as a deliberate step.
- Keep migration scripts small and focused.

### Data Access

- Keep query patterns consistent and intentional (tracking vs no-tracking).
- Avoid N+1 query patterns and over-fetching.
- Keep transaction boundaries explicit for multi-step writes.

### Safety

- Preserve concurrency and integrity constraints.
- Avoid destructive schema/data operations unless explicitly requested.

## Expansion Notes

- Keep provider-specific runtime details in project overlays.
