---
id: ADR-0007
title: Uniqueness is enforced by the database
status: accepted
date: 2024-12-28
area: backend
---

# ADR-0007 (backfilled): Uniqueness is enforced by the database

**Summary:** In the context of business-level uniqueness rules on names, facing the choice between
application checks and database constraints, we decided to back every such rule with a unique index —
case-insensitive and scoped to its owner for user-entered names — while keeping an application check in
front for the message, to achieve a rule that no code path can break, accepting two mechanisms for one
rule.

## Context and Problem Statement

The database is the only place that can guarantee a rule for every writer. A uniqueness rule enforced
only in application code is bypassed by any other writer — a migration fix, a support query, a future
feature — and leaves the data inconsistent with no error to trace.

The application check gives the caller a clear business-rule error, and the index is a second defence
against a coding error or direct database access. Case-insensitive uniqueness relies on PostgreSQL's
`citext` ([ADR-0005](0005-postgresql-is-the-relational-datastore.md)).

## Decision Drivers

- A rule the product states must hold for every write, not only for the application's own.
- The caller should receive a business message, not a constraint violation.

## Considered Options

1. Unique indexes, case-insensitive and scoped to their owner, with an application check in front.
2. Uniqueness enforced in application code only.
3. Unique indexes only, with the constraint violation translated into the answer.

## Decision Outcome

Chosen option: 1. User-facing names are held in the database's case-insensitive text type under a unique
index scoped to their owner — an account's description per site, an expense's or income's description
per account, and a site's name and a username across the whole database. A settings row is unique per
site, category and key, and its key is case-sensitive text. The two reference tables, roles and
permissions, have plain, case-sensitive unique indexes on their names, which are global rather than
site-scoped. An application check runs first where one exists, so the caller receives a business message
rather than a constraint violation, but the index is the guarantee.

The settings index was created by hand as two partial indexes, one for rows with a site and one for
global rows with none. Settings have since been made site-specific, so the global index can match no row,
and the model snapshot records the site index without its filter.

### Consequences

- Good, because uniqueness holds for any writer, including one that bypasses the application.
- Good, because a duplicate that slips past the check still fails, and is answered `409` rather than
  stored.
- Bad, because two mechanisms exist for one rule, and the application check must produce the message the
  index would otherwise turn into a constraint error.
- Bad, because the in-memory provider used by the data-layer tests ignores `citext` and unique indexes,
  so only the hosted suite can show the index at work.

### Confirmation

- Test: `CheckDescriptionDoesNotExistFixture` asserts the account check refuses a description already used
  on the same site, with its message, and allows one used on another site. It runs on the in-memory
  provider, so it does not exercise case-insensitivity.
- Test: the hosted `Features/Accounts/CreateUpdateFixture` and `Features/Expenses/CreateUpdateFixture`
  assert that an exact duplicate answers 409 against PostgreSQL, and `DatabaseExceptionHandlerFixture`
  asserts that a unique-constraint violation reaching the database is answered 409.
- Gap: no test writes past the application check, or sends a duplicate that differs only by case, to show
  the index refusing it; the constraint is evidenced by the migrations that create the indexes.
- Review: a new uniqueness rule ships with its index; an application check alone is a defect.

## Pros and Cons of the Options

### Index with an application check (chosen)

- Good: guaranteed, and the caller still gets a business message.
- Bad: two mechanisms to keep in step.

### Application-only uniqueness

- Good: the message is easy and there is one mechanism.
- Bad: any other writer breaks the rule, and the data is left inconsistent with no error to trace.

### Index only

- Good: one mechanism, and it is the guarantee.
- Bad: every duplicate reaches the caller as a generic conflict rather than a message naming the rule.

## More Information

- The application check is part of the two-layer validation model,
  [ADR-0013](0013-request-shape-is-validated-at-the-edge-and-rules-in-check-chains.md).
- Enumerations stored by name are [ADR-0006](0006-enumerations-are-stored-by-name.md).
