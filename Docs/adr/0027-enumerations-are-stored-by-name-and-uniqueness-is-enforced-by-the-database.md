---
id: ADR-0027
title: Enumerations are stored by name and uniqueness is enforced by the database
status: accepted
date: 2024-12-28
area: backend
---

# ADR-0027 (backfilled): Enumerations are stored by name and uniqueness is enforced by the database

**Summary:** In the context of persisted enumerated values and business-level uniqueness rules, facing
the choice between storing numbers and names, and between application checks and database constraints,
we decided to store enriched enumeration names as text and to back uniqueness with case-insensitive
database indexes while keeping an application check for the message, to achieve readable data that no
code path can make inconsistent, accepting that renaming an enumeration member is a data change.

## Context and Problem Statement

Two related storage questions have the same failure mode: the database is the only place that can
guarantee a rule for every writer, and anything that can be read by a person should be readable in the
data.

An enumeration stored as its numeric value is unreadable in a query and silently changes meaning if a
member is inserted or reordered. A uniqueness rule enforced only in application code is bypassed by any
other writer — a migration fix, a support query, a future feature.

## Decision Drivers

- Data should be inspectable without the code that wrote it.
- A rule the product states must hold for every write, not only for the application's own.
- The client should receive the same names the data and the API use.

## Considered Options

1. Enriched enumerations persisted by name, with per-site case-insensitive unique indexes and an
   application check in front.
2. Numeric enumeration values with a lookup table.
3. Names stored as free text with no shared type.
4. Names stored, uniqueness enforced in application code only.

## Decision Outcome

Chosen option: 1. Enumerations are modelled as enriched types that carry their own display and storage
names, and the model is configured to persist them as text with a bounded length of fifty. A name in the
database is the name in the code, so an insert into the middle of a declaration cannot reinterpret
existing rows. The reference tables that hold them — roles and permissions — have plain, case-sensitive
unique indexes on those names, which are global rather than site-scoped.

Uniqueness of user-entered text is a separate application of the same principle: user-facing names such
as an account's description are held in the database's case-insensitive text type with a composite unique
index scoped to the site, and a settings row is unique per site with a separate global index for the
unscoped case. An application check runs first where one exists, so the caller receives a business
message rather than a constraint violation, but the index is the guarantee.

### Consequences

- Good, because a query, a support fix and an export are readable without decoding numbers.
- Good, because inserting or reordering an enumeration member cannot change what an existing row means.
- Good, because uniqueness holds for any writer, including one that bypasses the application.
- Bad, because renaming an enumeration member is a data migration rather than a code rename.
- Bad, because the storage name is now part of the contract with the client and with stored exports.
- Bad, because two mechanisms exist for one rule, and the application check must produce the message the
  index would otherwise turn into a constraint error.

### Confirmation

- Test: the service fixtures assert the application check answers the business failure, with its error
  code, before the constraint is reached — for the text fields that have such a check.
- Gap: no data fixture inserts a duplicate to show the database refusing it; the constraint is evidenced by
  the migrations that create the indexes, and the refusal path by the exception-handler fixture.
- Review: a new enumeration is added as an enriched type with explicit storage names; a raw `int` column
  is a defect.

## Pros and Cons of the Options

### Names with database uniqueness (chosen)

- Good: readable and guaranteed.
- Bad: renames are migrations, and the name is contract.

### Numbers with a lookup table

- Good: smallest storage, and renames are a row update.
- Bad: every query joins, and every reader needs the table to interpret a value.

### Free text with no shared type

- Good: nothing to model.
- Bad: no compile-time set of values, and every caller re-invents the strings.

### Application-only uniqueness

- Good: the message is easy and there is one mechanism.
- Bad: any other writer breaks the rule, and the data is left inconsistent with no error to trace.

## More Information

- The application check is part of the two-layer validation model, recorded separately.
- Concurrency uses a version value, not the identifier, which is a separate decision.
