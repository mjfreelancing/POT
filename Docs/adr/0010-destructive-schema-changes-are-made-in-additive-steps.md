---
id: ADR-0010
title: Destructive schema changes are made in additive steps where possible
status: accepted
date: 2024-12-28
area: backend
---

# ADR-0010 (backfilled): Destructive schema changes are made in additive steps where possible

**Summary:** In the context of schema changes that remove or reinterpret existing data, facing a schema
that cannot be rolled back with the application binary, we decided to deliver such a change where possible
as separate append-only migrations — a compatible schema, then the code change, then the removal — to
achieve old and new code that both work against the schema during a deployment, accepting that a
destructive change takes more than one release.

## Context and Problem Statement

A column that is dropped or narrowed cannot be reversed by rolling back the application binary, so a
rolling deployment can have old and new code running against one schema. Removing data in the same
change as the code that stops using it leaves the old code, while it still runs, querying something that
no longer exists, and leaves no way back but a restore.

Destructive changes are split into separate steps, where possible, for safer rolling deployments.

## Decision Drivers

- During a rolling deployment the old code must keep working against the new schema.
- A deployment that has to be rolled back must not need a database restore.

## Considered Options

1. Additive steps: a schema both versions can use, then the code change, then the removal.
2. Remove or reinterpret data in the same change as the code that stops using it.
3. Allow any destructive change, but apply it with production stopped for a maintenance window.

## Decision Outcome

Chosen option: 1. A change that removes or reinterprets existing data is delivered in steps: first a
schema that both the old and new code can use, then the code change, then the removal — each an
append-only migration.

The rule is applied where possible rather than without exception: PRD-0023's accrual storage was removed
in two releases, and its drop was applied with production stopped for a maintenance window, while a
user's stored refresh token and its expiry (May 2026) and the account BSB and number (September 2026)
were dropped in the same change as the code that stopped using them.

### Consequences

- Good, because additive steps keep old and new code compatible during a rolling deployment.
- Good, because each step can be rolled back on its own until the removal ships.
- Bad, because a destructive change takes more than one release, and a removal that is waiting for its
  later step is easy to forget.
- Bad, because "where possible" is a judgement: two of the three recorded removals shipped in one step,
  so the rule is a default rather than a guarantee.

### Confirmation

- Review: a migration that drops or narrows a column is questioned when it ships in the same release as
  the code that stops using it; it is split where that is possible.
- Gap: nothing detects a destructive migration mechanically, and nothing records why a removal shipped in
  one step.

## Pros and Cons of the Options

### Additive steps (chosen)

- Good: old and new code both work during a deployment, and each step is reversible.
- Bad: more releases, and a removal to remember.

### Remove in the same change

- Good: one release, and no leftover schema.
- Bad: the old code breaks while it is still running, and only a restore undoes the change.

### Maintenance window

- Good: no compatibility step to design.
- Bad: the product is unavailable for every destructive change.

## More Information

- How migrations are applied is [ADR-0009](0009-migrations-run-in-a-separate-process-before-the-api-starts.md).
- The removals cited are [PRD-0023](../Future/PRD-0023-dynamic-accrual-calculation.md) and
  [PRD-0021](../Future/PRD-0021-remove-account-bsb-and-number.md).
