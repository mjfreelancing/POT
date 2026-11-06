---
id: ADR-0006
title: Enumerations are stored by name
status: accepted
date: 2024-12-28
area: backend
---

# ADR-0006 (backfilled): Enumerations are stored by name

**Summary:** In the context of persisted enumerated values, facing the choice between storing a member's
number and storing its name, we decided to model enumerations as enriched types and persist each as its
name in text, to achieve data that is readable without the code and that inserting a member cannot
reinterpret, accepting that renaming a member is a data change.

## Context and Problem Statement

Anything that can be read by a person should be readable in the data. An enumeration stored as its
numeric value is unreadable in a query and silently changes meaning if a member is inserted or reordered.

Names are stored to keep queries readable and to avoid ordering problems when a member is inserted. An
enriched type lets the stored name differ from the member name, which matters once the name is in the
data.

## Decision Drivers

- Data should be inspectable without the code that wrote it.
- Inserting or reordering a member must not change what an existing row means.
- The client should receive the same names the data and the API use.

## Considered Options

1. Enriched enumerations persisted by name.
2. Numeric enumeration values with a lookup table.
3. Names stored as free text with no shared type.

## Decision Outcome

Chosen option: 1. Enumerations are modelled as enriched types, each member carrying a name — the member
name by default, or an explicit one such as `site:manage` for a permission — and the model is configured
to persist every enriched type as that name, as text with a bounded length of fifty. A name in the
database is the name in the code, so an insert into the middle of a declaration cannot reinterpret
existing rows. The names are columns on the entities that use them: an expense's frequency, a user's
status, a setting's category, a one-time code's reason and status.

### Consequences

- Good, because a query, a support fix and an export are readable without decoding numbers.
- Good, because inserting or reordering an enumeration member cannot change what an existing row means.
- Bad, because renaming an enumeration member is a data migration rather than a code rename.
- Bad, because the storage name is now part of the contract with the client and with stored exports.

### Confirmation

- Review: a new enumeration is added as an enriched type, stored by name; a raw `int` column is a defect.
- Gap: no test asserts that an enumeration column is stored as text; the migrations that create the
  columns are the evidence.

## Pros and Cons of the Options

### Names (chosen)

- Good: readable, and safe against inserted or reordered members.
- Bad: renames are migrations, and the name is contract.

### Numbers with a lookup table

- Good: smallest storage, and renames are a row update.
- Bad: every query joins, and every reader needs the table to interpret a value.

### Free text with no shared type

- Good: nothing to model.
- Bad: no compile-time set of values, and every caller re-invents the strings.

## More Information

- The uniqueness of user-entered names, and of the role and permission names, is
  [ADR-0007](ADR-0007-uniqueness-is-enforced-by-the-database.md).
