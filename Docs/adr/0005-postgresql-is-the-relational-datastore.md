---
id: ADR-0005
title: PostgreSQL is the relational datastore
status: accepted
date: 2024-12-28
area: backend
---

# ADR-0005 (backfilled): PostgreSQL is the relational datastore

**Summary:** In the context of a small application with a conventional relational schema, facing the
choice of database engine, we decided on PostgreSQL, to achieve a capable open-source database with the
features the schema relies on, without licensing cost, accepting that the engine is another component to
operate and that its version varies between environments.

## Context and Problem Statement

The schema needs more than tables and indexes: case-insensitive unique text, composite uniqueness scoped
to a site, generated identifiers, and a provider that Entity Framework Core supports well. Cost matters,
because this is a personal project with a single operator.

The engine was chosen because it is open source and cost-effective for a small application that is not
expected to grow very large. The schema features above are what the schema relies on now; the record does
not show whether they, or the other options below, were weighed when the engine was chosen.

## Decision Drivers

- No licence cost for a database that will be small at every environment.
- Features the schema relies on: `citext` for case-insensitive text, and composite unique indexes over it.


## Considered Options

1. PostgreSQL, run in a container locally and as a hosted server in production.
2. SQL Server, with its developer and small-worker licensing.
3. SQLite, as a file-based database.
4. MySQL or MariaDB.

## Decision Outcome

Chosen option: 1, because it is open source and cost-effective for a small application that is not
expected to grow very large. PostgreSQL runs as a container for local development and for tests, and in
production as a hosted server in Azure, with SSL required on connections.

The schema has since come to rely on `citext`, which lets a unique index back the case-insensitive
uniqueness of names alongside the application's own check. The same engine runs locally and hosted.

### Consequences

- Good, because there is no licence cost at any environment.
- Good, because `citext` columns under composite unique indexes enforce case-insensitive uniqueness in
  the database, as a second line of defence behind the application checks.
- Good, because the local container and the hosted server are the same engine, so a query that works
  locally is not a different dialect in production.
- Bad, because the engine version differs between environments: the local container and the integration
  fixtures run 13, and the end-to-end stack runs 16. The deployment diagram labels the production server
  17.6, which nothing else in the repository confirms. EF is told to target 13.6 (`SetPostgresVersion`).
- Bad, because production requires SSL on the connection, so the connection configuration is not the same
  as the local one.
- Bad, because another component to operate, back up and restore.

### Confirmation

- Test: the hosted integration fixtures run against a real PostgreSQL container, so a dialect mistake is a
  test failure there; the data-layer fixtures use an in-memory provider and would not catch one.
- Review: a schema rule that can be an index or a constraint should be one, rather than an application
  check that a direct database write could bypass.

## Pros and Cons of the Options

### PostgreSQL (chosen)

- Good: free, capable, and supported by the ORM and the host.
- Bad: a server to run, and a version to keep aligned.

### SQL Server

- Good: strong tooling and familiar to many.
- Bad: licensing for a personal project, and no benefit for this schema.

### SQLite

- Good: no server at all.
- Bad: differs in types, concurrency and case-insensitive collation, so the local and hosted behaviour
  would not match in exactly the places the schema depends on.

### MySQL

- Good: free and widely hosted.
- Bad: no `citext` equivalent, because case-insensitivity comes from column collations instead, and no
  advantage over PostgreSQL here.

## More Information

- The version target and the drift between environments are a separate, open item:
  `DECISIONS-TO-DISCUSS.md` (DD-06).
- Enforcing uniqueness in the database as well as by application checks is
  [ADR-0007](0007-uniqueness-is-enforced-by-the-database.md).
