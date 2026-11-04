---
id: ADR-0022
title: PostgreSQL is the relational datastore
status: accepted
date: 2026-11-04
area: backend
---

# ADR-0022: PostgreSQL is the relational datastore

**Summary:** In the context of a small application with a conventional relational schema, facing the
choice of database engine, we decided on PostgreSQL, to achieve a capable open-source database with the
features the schema relies on, without licensing cost, accepting that the engine is another component to
operate and that its version varies between environments.

## Context and Problem Statement

The schema needs more than tables and indexes: case-insensitive unique text, composite uniqueness scoped
to a site, generated identifiers, and a provider that Entity Framework Core supports well. Cost matters,
because this is a personal project with a single operator.

## Decision Drivers

- No licence cost for a database that will be small at every environment.
- Features the schema relies on: `citext` for case-insensitive text, and composite unique indexes over it.
- A managed hosting option, because the deployment is a single operator's.

## Considered Options

1. PostgreSQL, run in a container locally and as a managed server in production.
2. SQL Server, with its developer and small-worker licensing.
3. SQLite, as a file-based database.
4. MySQL or MariaDB.

## Decision Outcome

Chosen option: 1. PostgreSQL runs as a container for local development and for tests, and as a managed
server in production with SSL required on connections.

The engine's case-insensitive text type is what makes per-site uniqueness of names implementable as an
index rather than as application comparison, and the same engine can be run locally and hosted.

### Consequences

- Good, because there is no licence cost at any environment, and the managed option removes most of the
  operational work.
- Good, because `citext` and partial indexes express the uniqueness rules without application code.
- Good, because the local container and the hosted server are the same engine, so a query that works
  locally is not a different dialect in production.
- Bad, because the engine version differs between environments: the local container and the integration
  fixtures run 13, the end-to-end stack runs 16 and the deployed server runs 17.6, and the provider is
  pinned to the oldest of them.
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
- Bad: historically weaker support for the case-insensitive-text and partial-index behaviours the schema
  uses, and no advantage over PostgreSQL here.

## More Information

- The version target and the drift between environments is a separate, open item.
- Uniqueness being enforced by the database rather than only by application checks is a separate decision.
