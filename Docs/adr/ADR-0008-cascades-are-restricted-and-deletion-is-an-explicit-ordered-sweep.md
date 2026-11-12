---
id: ADR-0008
title: Cascades are restricted and deletion is an explicit, ordered sweep
status: accepted
date: 2024-12-28
area: backend
related-prds: [PRD-0022]
---

# ADR-0008 (backfilled): Cascades are restricted and deletion is an explicit, ordered sweep

**Summary:** In the context of a schema where removing a site or a user must remove what belongs to
them, facing the choice between database cascades and an explicit ordered deletion, we decided that
foreign keys restrict rather than cascade and that deletion is a deliberate sequence of repository
calls, to achieve removal that is written where it is intended rather than implied by the schema,
accepting that every deletable aggregate needs a sweep and that an omission is a constraint error rather
than a silent orphan.

## Context and Problem Statement

A cascade deletes rows the deleting code never names. That is convenient, and it means a single
`DELETE` on a parent can remove a subtree whose extent is only visible by reading the model. When the
table is wrong, or the intent was narrower, the data is already gone.

Restricting makes the schema refuse instead: deleting a parent with children fails until the children
are handled, which turns an unintended deletion into an error.

Restricting cascades is meant to prevent accidental data loss through a coding error or direct database
access. [PRD-0022](../prd/PRD-0022-user-and-site-deletion.md) makes removing a user or a site a
permanent hard delete, and the user and site sweeps carry it out.

## Decision Drivers

- Deleting one thing should never quietly delete more than the code states.
- A failure caused by an incomplete deletion should be visible while it can still be fixed.
- Removing a tenant must be complete: a partial deletion is worse than a refusal.

## Considered Options

1. Restrict all foreign-key deletes and perform deletion as an explicit ordered sweep.
2. Cascade deletes on ownership relationships.
3. Soft delete, marking rows as removed and filtering them out.
4. Expose no deletion at all.

## Decision Outcome

Chosen option: 1. A model convention sets every foreign key on an entity type to restrict, so a new
relationship restricts without being configured. Removing a record together with what depends on it is
a sequence of named repository operations in dependency order — children, then the parent — inside one
save, so the whole removal either applies or does not. A record whose dependents are not swept cannot be
deleted while they exist.

The join tables that link roles to permissions and users to roles are the exception: they are not entity
types of their own, so the convention does not reach them and their keys keep the framework's default
cascade. The record does not show whether that exception was chosen or is a side effect of the
convention's scope.

There is no soft-delete column and no filtered read path: a deleted row is gone, and a reference to it
fails rather than returning a tombstone.

### Consequences

- Good, because the extent of a deletion is the code that performs it, and review can see it.
- Good, because an omission fails the transaction with a constraint error rather than leaving orphaned
  rows or deleting more than intended.
- Good, because uniqueness and queries need no "is deleted" filter, so a rule cannot be bypassed by a
  forgotten predicate.
- Bad, because every deletable aggregate needs a sweep that must be kept in step with the schema, and a
  new dependent table means a new step.
- Bad, because deletion is irreversible: there is no undo, and recovery is a backup.
- Bad, because a large site's removal loads and tracks every dependent row in memory and then issues many
  statements in one save, which holds its transaction longer than a cascade would.

### Confirmation

- Test: `DeletionSweepFixture` runs the user and site sweeps against PostgreSQL and asserts the dependent
  counts, that no dependent row remains, and that another site's rows are untouched.
- Test: `IsolationFixture` removes a member, the caller's own account and a whole site through the API,
  and asserts that a removed tenant leaves no row in any site-scoped table, that a canary tenant is
  unchanged, and that `OrphanSweep` finds no row referencing a removed one. Its tables come from the model
  rather than a list, so a new dependent table the sweep misses fails it.
- Gap: no test injects a failure mid-sweep to show the partial removal is rolled back; the guarantee rests
  on the single save's implicit transaction.
- Review: a cascade in a migration is a decision to be challenged; a new dependent table needs its sweep
  step in the same change.

## Pros and Cons of the Options

### Restrict and sweep (chosen)

- Good: deletion is explicit, complete or refused.
- Bad: a sweep per aggregate, and no undo.

### Cascades on ownership

- Good: less code, and completeness comes from the schema.
- Bad: the deleted extent is invisible at the deleting call, which is the failure this decision exists to
  prevent.

### Soft delete

- Good: reversible, and history is preserved.
- Bad: every read, uniqueness rule and query filter must exclude the rows, and one forgotten predicate
  is a leak or a duplicate; storage grows without bound.

### No deletion

- Good: nothing to get wrong.
- Bad: accounts, users and whole tenants could never be removed, which the product requires.

## More Information

- The convention is `DbContextBase.DisableCascadeDelete`. The sweeps that exist are the user and site
  removals of [PRD-0022](../prd/PRD-0022-user-and-site-deletion.md).
- A financial account is not swept: deleting one that still has expenses or incomes is refused with a
  business error, and expenses and incomes are deleted one row at a time.
- A sweep is all-or-nothing because it is one save, not because of an explicit transaction; when an
  explicit transaction is used instead is
  [ADR-0029](ADR-0029-transactions-are-explicit-and-only-where-writes-must-be-one-unit.md).
- What may be deleted, and by whom, is a product rule recorded in the requirements for each operation.
