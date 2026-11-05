---
id: ADR-0028
title: Cascades are restricted and deletion is an explicit, ordered sweep
status: accepted
date: 2024-12-28
area: backend
---

# ADR-0028 (backfilled): Cascades are restricted and deletion is an explicit, ordered sweep

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

Chosen option: 1. Foreign keys from an entity to its owner restrict, and deleting an aggregate is a
sequence of named repository operations in dependency order — children, then the parent — inside one
save, so the whole removal either applies or does not. The join tables that link roles to permissions and
users to roles are the exception: their keys cascade, because those rows exist only as pairs and are never
addressed on their own.

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
- Bad, because a large site's removal is many statements in one save, which holds its transaction longer
  than a cascade would.

### Confirmation

- Test: `DeletionSweepFixture` deletes an aggregate and asserts that no dependent row remains and that the
  neighbouring site's rows are untouched.
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

- The explicit transaction that makes a sweep all-or-nothing is a separate decision.
- What may be deleted, and by whom, is a product rule recorded in the requirements for each operation.
