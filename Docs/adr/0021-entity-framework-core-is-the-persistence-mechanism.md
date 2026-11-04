---
id: ADR-0021
title: Entity Framework Core is the persistence mechanism
status: accepted
date: 2026-11-04
area: backend
---

# ADR-0021: Entity Framework Core is the persistence mechanism

**Summary:** In the context of a relational schema with entities, relationships and migrations, facing
the choice between an ORM and hand-written statements, we decided to use Entity Framework Core, to
achieve a schema expressed as typed code with migrations and queries composed in LINQ, accepting the
framework's tracking semantics and its query translation limits as things the code must be written
around.

## Context and Problem Statement

The application's data is relational and mostly conventional: a site has users, a user has roles, a site
has accounts, an account has schedules. The work is query composition — projections filtered by site,
joined across several entities — rather than bulk operations or hand-tuned SQL.

The team writing it is small, and familiarity matters more than the last increment of control.

## Decision Drivers

- Schema and queries should be typed, so a renamed column breaks a build.
- Migrations should be derivable from the model rather than maintained by hand.
- Queries should compose, because the read shapes are projections rather than whole entities.

## Considered Options

1. Entity Framework Core, with migrations generated from the model.
2. Dapper, with hand-written SQL and a hand-maintained schema.
3. A micro-ORM plus a separate migration tool.
4. Raw ADO.NET.

## Decision Outcome

Chosen option: 1. Entities are configured in code, migrations are generated from the model, and reads
compose LINQ projections. Conventions are applied centrally — entity naming, value generation, enriched
enum storage, query filters — rather than per entity.

### Consequences

- Good, because a model change produces a build error where a query expects the old shape, rather than a
  runtime failure.
- Good, because migrations follow the model, so schema history is derived rather than remembered.
- Good, because projections compose, which is how the read endpoints are written.
- Bad, because tracking is on by default and quietly expensive; the code has to make no-tracking the
  default rather than the exception.
- Bad, because some queries translate poorly or not at all, so the model is shaped partly by what the
  provider can turn into SQL.
- Bad, because the generated SQL is not visible in review, so a costly query is easy to write.

### Confirmation

- Review: a read that returns whole entities where a projection would do is questioned, because it is
  both slower and wider than the contract.
- Test: the data-layer fixtures run against a real PostgreSQL instance, so a translation failure is a
  test failure rather than a production one.

## Pros and Cons of the Options

### Entity Framework Core (chosen)

- Good: typed model, derived migrations, composable queries, familiar to the author.
- Bad: hidden SQL, and conventions that must be corrected rather than adopted wholesale.

### Dapper with hand-written SQL

- Good: total control over every statement.
- Bad: every query is a string, the schema is maintained by hand, and a rename is found at runtime.

### Micro-ORM plus a migration tool

- Good: smaller and faster than a full ORM.
- Bad: two tools to keep aligned, and no model to derive the schema from.

### Raw ADO.NET

- Good: nothing between the code and the database.
- Bad: mapping and parameter handling by hand, for data that is entirely conventional.

## More Information

- No-tracking reads and explicit transactions are separate decisions.
- The database engine choice is a separate decision.
