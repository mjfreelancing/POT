---
id: ADR-0024
title: Transactions are explicit and used only where writes must be one unit
status: accepted
date: 2026-11-04
area: backend
---

# ADR-0024: Transactions are explicit and used only where writes must be one unit

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

Chosen option: 1. An operation that writes through more than one repository call opens a transaction
around the whole unit, and the failure path rolls it back. An operation that saves once does not.

### Consequences

- Good, because a multi-part operation cannot leave half its work committed.
- Good, because single-save operations hold no transaction of their own, so concurrency is not reduced
  for the common case.
- Good, because the transaction boundary is visible in the service that owns the unit, which is also
  where the failure is handled.
- Bad, because whether an operation needs one is a judgement made per feature, and a missed one is a
  partial write that only shows under failure.
- Bad, because a transaction that spans several repository calls relies on those calls sharing one
  context scope, which is a wiring detail rather than something the type system enforces.
- Bad, because a long transaction holds locks and a slow operation inside it blocks writers of the same
  rows for its duration.

### Confirmation

- Test: the failure path of a multi-write operation is exercised with the second write failing, and the
  first is asserted absent afterwards.
- Review: a service that performs two saves without a transaction is a defect, and an operation that opens
  one for a single save is questioned.

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

- The repository access model this sits on is a separate decision.
- Optimistic concurrency decides what happens when two writers reach the same row, which is separate from
  the unit of work.
