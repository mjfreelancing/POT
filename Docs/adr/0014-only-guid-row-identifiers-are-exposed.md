---
id: ADR-0014
title: Only GUID row identifiers are exposed; integer keys stay internal
status: accepted
date: 2025-01-19
area: backend
---

# ADR-0014 (backfilled): Only GUID row identifiers are exposed; integer keys stay internal

**Summary:** In the context of an API whose routes address a record directly, facing the choice between
exposing the database's own key and a separate public identifier, we decided that every entity carries
a GUID `RowId` that is the only identifier to appear in a route, a payload or an export, while the
database keeps an internal integer key, to achieve addresses that cannot be enumerated, accepting a
second identifier per row and an index on it.

## Context and Problem Statement

The database needs a key: small, sequential, cheap to index, and usable as a foreign key. That key is
also the worst possible thing to publish. An incrementing number in a URL tells every caller how many
records exist, lets them walk the space, and makes a leaked link guessable for its neighbours.

Two uses want different things from an identifier, so the design answers both rather than compromising.
The public identifier is a security measure, to make routes harder to guess than incrementing identifiers
would. The record does not show whether the other options below were weighed.

## Decision Drivers

- A route must not reveal how many records exist or let a caller walk to a neighbour.
- The database key should stay small and sequential for storage and index efficiency.
- An identifier that a user can share — in a link, an export, a support message — should be stable and
  opaque.

## Considered Options

1. A GUID `RowId` exposed everywhere, with the database's integer key kept internal.
2. Expose the integer key everywhere.
3. Expose a sequential but obfuscated value, such as a scrambled integer or a hash of it.
4. Expose the GUID and drop the integer key entirely.

## Decision Outcome

Chosen option: 1. `RowId` is a random GUID assigned when a row is added, or carried over from the export
file when an import creates the row. It sits under a unique index and is the identifier in routes, request
and response payloads, token claims and export files. The integer key remains a database implementation detail:
foreign keys inside the database are integer keys, and no payload or route carries one.

### Consequences

- Good, because an address cannot be guessed or walked, and the record count is not disclosed by an
  identifier.
- Good, because the database keeps the efficient key it wants for joins and indexes.
- Good, because identifiers can be shared — in a link, an export or a support message — without
  disclosing the record count, while the database keeps integer keys for its own joins.
- Bad, because every entity table carries a second identifier and an index to resolve it; join tables,
  which are never addressed individually, carry neither.
- Bad, because a GUID is larger than an integer in every index and payload that carries it.
- Neutral, because a GUID is not an authorization mechanism: the rules that decide who may read a record
  are enforced server-side, and an opaque identifier only removes the enumeration the key would have
  offered.

### Confirmation

- Test: the endpoint fixtures address records by `RowId`, and the cross-tenant fixtures show that knowing
  another tenant's identifier still answers `404`.
- Review: a payload that carries an integer key, and a route that accepts one, are defects.- Instruction: `.github/instructions/server.instructions.md` (Architecture): public contracts use `RowId`
  and `Etag`, and never expose internal persistence ids.
- Review: every identifier route segment is declared `{id:guid}`, so a route that accepts an integer key
  would have to change that constraint, which is the change to challenge.

## Pros and Cons of the Options

### GUID exposed, integer internal (chosen)

- Good: opaque addresses, efficient storage.
- Bad: two identifiers to keep in step.

### Integer key everywhere

- Good: one identifier, smallest payloads.
- Bad: enumeration, and a visible record count, with no way to fix it later without changing every
  contract and every stored reference.

### Obfuscated sequential value

- Good: smaller than a GUID and not obviously sequential.
- Bad: an encoding, not an identifier: reversing or leaking the scheme restores the enumeration, and it
  is not collision-free across generators.

### GUID only

- Good: one identifier.
- Bad: gives up the sequential key that makes the clustered index and its joins cheap.

## More Information

- Tenant isolation is enforced by query filters and site-scoped reads, which is where "who may read this"
  is decided; an opaque identifier does not replace it.
- Concurrency uses a separate version value, not the identifier.
