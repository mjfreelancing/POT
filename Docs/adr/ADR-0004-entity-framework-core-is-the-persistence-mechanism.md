---
id: ADR-0004
title: Entity Framework Core is the persistence mechanism
status: accepted
date: 2024-12-28
area: backend
---

# ADR-0004 (backfilled): Entity Framework Core is the persistence mechanism

**Summary:** In the context of a relational schema with entities, relationships and migrations, facing
the choice between an ORM and hand-written statements, we decided to use Entity Framework Core, to
achieve a schema expressed as typed code with migrations and queries composed in LINQ, accepting the
framework's tracking semantics and its query translation limits as things the code must be written
around.

## Context and Problem Statement

The application's data is relational and mostly conventional: a site has users, a user has roles, a site
has accounts, an account has schedules. The work is query composition — projections filtered by site,
joined across several entities — rather than bulk operations or hand-tuned SQL.

Entity Framework Core was chosen for prior experience with it, its built-in migration support, and how
easily LINQ queries compose. The record does not show whether the other options below were weighed.

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

Chosen option: 1. Entity Framework Core 10 with the Npgsql provider is referenced centrally, entities are
configured in code, migrations are generated from the model, and reads compose LINQ projections.
Conventions that apply to every entity are set once on the context rather than per entity.

### Consequences

- Good, because a model change produces a build error where a query expects the old shape, rather than a
  runtime failure.
- Good, because migrations follow the model, so schema history is derived rather than remembered.
- Good, because projections compose, which is how the read endpoints are written.
- Bad, because the framework tracks by default, which is quietly expensive, so the code has to override
  that default ([ADR-0011](ADR-0011-services-access-data-through-feature-repositories.md)).
- Bad, because some queries translate poorly or not at all, so the model is shaped partly by what the
  provider can turn into SQL.
- Bad, because the generated SQL is not visible in review, so a costly query is easy to write.

### Confirmation

- Review: a read that returns whole entities where a projection would do is questioned, because it is
  both slower and wider than the contract.
- Test: the data-layer fixtures run against the in-memory provider, so repository logic is covered
  without a container. That provider ignores `citext`, unique indexes and SQL translation, so those are
  covered only by the hosted suite, which runs against a real PostgreSQL container. Whether that split is
  the intended test strategy is still open (`DECISIONS-TO-DISCUSS.md`, DD-12d).

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

- The central conventions live in `DbContextBase`: entity naming and `RowId` value generation
  ([ADR-0014](ADR-0014-only-guid-row-identifiers-are-exposed.md)), enriched enumerations stored by name
  ([ADR-0006](ADR-0006-enumerations-are-stored-by-name.md)) and restricted cascades
  ([ADR-0008](ADR-0008-cascades-are-restricted-and-deletion-is-an-explicit-ordered-sweep.md)). The four
  tenant query filters are declared per entity in `PotDbContext`
  ([ADR-0030](ADR-0030-tenant-confinement-is-structural-query-filters-and-site-scoped-reads.md)).
- Untracked reads with identity resolution are part of the repository access model,
  [ADR-0011](ADR-0011-services-access-data-through-feature-repositories.md). Explicit transactions are
  [ADR-0029](ADR-0029-transactions-are-explicit-and-only-where-writes-must-be-one-unit.md).
- The database engine choice is [ADR-0005](ADR-0005-postgresql-is-the-relational-datastore.md).
