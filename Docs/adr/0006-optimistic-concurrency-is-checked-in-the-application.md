---
id: ADR-0006
title: Optimistic concurrency is checked in the application, not by the database
status: accepted
date: 2026-11-04
area: backend
---

# ADR-0006: Optimistic concurrency is checked in the application, not by the database

**Summary:** In the context of entities edited through per-feature update services against a
single relational database, facing the need to reject a stale edit rather than silently overwrite
another user's change, we decided to carry a version number in the request and response body and
compare it in application code before saving, to achieve concurrency handling that does not depend on
the database provider, accepting a race window between the check and the save.

## Context and Problem Statement

Every mutable entity inherits a `long` `Etag`, indexed but not declared as a concurrency token: there
is no `IsConcurrencyToken`, `[ConcurrencyCheck]` or `xmin` anywhere in the data project. The value is
assigned on every insert and update as a Unix millisecond timestamp taken from `DateTime.UtcNow` when
the change is saved, bypassing the application's time provider.

Nine update paths compare the tag the caller supplied with the tag on the entity that was just
loaded, and answer `409 Conflict` when they differ. The tag travels inside the request and response
bodies, not as an HTTP `ETag` header with `If-Match`.

## Decision Drivers

- Concurrency handling should not tie the application to a database provider feature.
- The version should be readable where the client already reads the resource, without a second
  protocol.
- A conflicting edit should be a predictable business failure, not an exception.

## Considered Options

1. Compare a version number in application code before saving (chosen).
2. Mark the value as an Entity Framework concurrency token and set the entry's original value from the
   request, so the `UPDATE` carries the guard.
3. Use PostgreSQL's `xmin` as the version.
4. No concurrency control at all, so the last write wins.

## Decision Outcome

Chosen option: 1. The version is a plain value on the entity, the tag is supplied by the caller, and
each update path compares the two before the change is saved.

The aim is concurrency handling that does not depend on the database. The database-token approach
would make each update path responsible for setting the original value, and the residual race is
accepted because two people editing the same record at the same moment is very unlikely in this
application.

### Consequences

- Good, because the behaviour is identical on any provider and needs no schema-level configuration.
- Good, because a conflict is an ordinary business failure answered with `409 Conflict`, alongside —
  and indistinguishable in shape from — the conflicts raised by unique and reference constraints.
- Bad, because the check is advisory: two callers who load the same row and save within the window
  between the check and the save both succeed, and the second silently replaces the first.
- Bad, because two saves of the same entity within one millisecond produce the same version, so a
  stale tag can still match.
- Bad, because the value is generated from the wall clock rather than the application's time provider,
  so the business clock does not govern it and a clock adjustment is written into the data.
- Neutral, because the client receives the value as a plain number; most of its schemas type it as a
  `bigint` while one types it as a number, which is a typing inconsistency rather than a behaviour
  difference at current magnitudes.

### Confirmation

- Test: the per-feature integration fixtures (`CreateUpdateFixture` for accounts, expenses and
  incomes, `UpsertFixture` for settings, `UpdateFixture` for sites and users, `UpdateStatusFixture`
  for approvals) assert that a stale tag answers `409`.
- Review: a new update path must include its own tag check; there is no cross-cutting enforcement that
  would fail if one were omitted.
- Known gap: no test can assert atomicity, because the design does not provide any.

## Pros and Cons of the Options

### Application check (chosen)

- Good: provider-independent and visible in the service code that already owns the update rules.
- Bad: the race window is real, and closing it later changes the mechanism rather than a parameter.

### Entity Framework concurrency token

- Good: the guard moves into the `UPDATE`, so the check and the write are one atomic step, and the
  existing `DbUpdateConcurrencyException` mapping already answers `409`.
- Bad: the value must be monotonic, and each update path must set the original value from the request,
  so it is more code rather than less.

### PostgreSQL `xmin`

- Good: no column at all, and the database does the work.
- Bad: the value becomes a transaction id, which changes the wire contract and the client, and ties
  the mechanism to one database.

### No control

- Good: nothing to implement.
- Bad: a stale edit overwrites silently, which is the failure this record exists to prevent.

## More Information

- The remaining gap, and the change that would close it — a concurrency token with a monotonic value,
  plus a matching client type — is written up in `DECISIONS-TO-DISCUSS.md` (DD-02) and is awaiting a
  decision. If it is adopted, this record is superseded rather than edited.
