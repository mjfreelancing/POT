---
id: ADR-0024
title: Transactions are explicit and used only where writes must be one unit
status: accepted
date: 2025-10-26
area: backend
---

# ADR-0024 (backfilled): Transactions are explicit and used only where writes must be one unit

**Summary:** In the context of operations that sometimes change several rows through different
repository calls, facing the choice between wrapping everything in a transaction and opening one only
where a single save cannot express the unit, we decided to use an explicit transaction for exactly those
operations and rely on the single save otherwise, to achieve atomicity where it is needed and short
locks where it is not, accepting that the decision of whether an operation needs one is made per feature.

## Context and Problem Statement

A single `SaveChanges` is already atomic: everything it touched commits together or not at all. The
problem appears when one operation must write through two saves — a record and its related rows, or two
aggregates — because a failure between them leaves a half-applied change.

Wrapping everything in a transaction would be uniform, and it would hold locks for longer and couple
unrelated work into one unit for no reason.

An explicit transaction is for exactly the case where EF calls made across several services cannot
otherwise act as one atomic unit, so that a failure rolls all of them back. The alternatives below record
what each would have cost; the record does not show whether they were weighed before the choice.

## Decision Drivers

- A partially applied change is worse than a rejection; some operations genuinely need all or nothing.
- Locks should be held for the shortest span that achieves the unit.
- Most operations write one aggregate in one save.

## Considered Options

1. An explicit transaction only where more than one save must succeed together.
2. A transaction around every request or every command.
3. No explicit transactions, accepting partial application.
4. Compensating actions that undo a partial write.

## Decision Outcome

Chosen option: 1. An operation whose saves must succeed together opens a transaction around the whole
unit. Its early-return failure paths leave without committing, so disposing the transaction at the end
of the operation rolls the work back. Today the only such operation is the data import: its account,
expense and income importers each save once, inside one transaction. Signup and password-reset
verification also save twice without a transaction, because the first save only expires stale codes and
need not roll back with the second.

An operation that saves once does not open a transaction, with one exception: a role assignment opens
one around its single save. The record does not show why.

### Consequences

- Good, because a multi-part operation cannot leave half its work committed.
- Good, because single-save operations hold no transaction of their own, so concurrency is not reduced
  for the common case.
- Good, because the transaction boundary is visible in the service that owns the unit, which is also
  where the failure is handled.
- Bad, because whether an operation needs one is a judgement made per feature, and a missed one is a
  partial write that only shows under failure.
- Bad, because the rollback is implicit: nothing calls it, so an operation that returns early without
  committing relies on the transaction being disposed to undo its work.
- Bad, because a transaction that spans several repository calls relies on those calls sharing one
  context scope, which is a wiring detail rather than something the type system enforces.
- Bad, because a long transaction holds locks and a slow operation inside it blocks writers of the same
  rows for its duration.

### Confirmation

- Instruction: `.github/instructions/server.instructions.md` (Persistence: keep transaction boundaries
  explicit for multi-step writes).
- Gap: no test exercises a rollback. The import is the operation the rule exists for, and nothing covers
  its transaction:
  - `ImportVersionGateFixture` rejects the package before the transaction is opened, with the transaction
    factory substituted;
  - no hosted fixture calls the import;
  - the authorization matrix ignores it.
- Review: a service whose saves must succeed together but run without a transaction is a defect, and an
  operation that opens one for a single save is questioned.

## Pros and Cons of the Options

### Explicit where needed (chosen)

- Good: shortest locks, and the unit is visible in the code that owns it.
- Bad: a per-feature decision that can be forgotten.

### Transaction per request

- Good: uniform, and impossible to forget.
- Bad: holds a transaction across work that has nothing to do with the write, including the response
  being written.

### No transactions

- Good: nothing to reason about.
- Bad: a failure at the wrong moment leaves data that no rule anticipates.

### Compensating actions

- Good: no locks.
- Bad: the compensation is another write that can fail, and the window between them is observable.

## More Information

- The repository access model this sits on is
  [ADR-0023](0023-services-access-data-through-feature-repositories.md).
- Optimistic concurrency decides what happens when two writers reach the same row, which is separate from
  the unit of work: [ADR-0006](0006-optimistic-concurrency-is-checked-in-the-application.md).
