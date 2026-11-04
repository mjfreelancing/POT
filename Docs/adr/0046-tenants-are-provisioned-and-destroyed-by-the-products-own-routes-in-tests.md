---
id: ADR-0046
title: Tenants are provisioned and destroyed by the product's own routes in tests
status: accepted
date: 2026-11-04
area: frontend
---

# ADR-0046: Tenants are provisioned and destroyed by the product's own routes in tests

**Summary:** In the context of destructive operations that must be shown to touch only their own tenant,
facing the choice between inserting fixture rows and driving the product's own provisioning routes, we
decided that scenario tests create real tenants through sign-up, approval and invitation, fingerprint a
canary tenant around every destructive action, and read invitation mail from a test-only sink, to achieve
proof that deletion is confined using the same paths a user takes, accepting that setup is slower and
depends on email flows working.

## Context and Problem Statement

A test that inserts rows directly can prove that a repository method deletes them, and proves nothing about
whether the endpoint does. Worse, a destructive test on shared seed data cannot distinguish "confined to
its own site" from "deleted the neighbour too", because the neighbour is not part of the assertion.

The provisioning a real tenant needs — sign-up, approval, an invitation, an accepted password — is
several steps through the product, one of which is an email.

## Decision Drivers

- A destructive action should be proven against real tenants, not fixture rows.
- Confinement needs something outside the target to check, or it is not being checked at all.
- Provisioning through the routes means the test cannot disagree with the product about what is valid.

## Considered Options

1. Provision through the product's routes, capture mail in a test sink, and fingerprint a canary tenant.
2. Insert fixture rows directly, and assert on them.
3. Use the shared seeded users for destructive tests.
4. Mock the email provider to read the invitation.

## Decision Outcome

Chosen option: 1. A scenario provisions its tenants by calling the sign-up, approval and invitation routes,
reading the temporary password from a test-only mail sink that captures what the application sends. Before
any destructive action, a canary tenant is fingerprinted; afterwards it must be unchanged, so "only this
tenant was touched" is an assertion rather than an assumption.

The destructive end-to-end class provisions its own tenant rather than operating on the seeded ones, so it
can destroy that tenant completely.

### Consequences

- Good, because provisioning exercises the real routes, so a change to how a tenant is created breaks the
  scenario rather than a hidden fixture.
- Good, because the canary tenant turns confinement into a check on every destructive action.
- Good, because the mail sink also proves that the invitation actually produced a usable credential.
- Bad, because a scenario costs several requests and a mail round trip, so it is slower than inserted rows.
- Bad, because a destructive test that destroys its own tenant is not repeatable against the same data,
  which makes a rerun provision again.
- Bad, because the mail sink is test-only infrastructure inside the product's delivery path, and it has to
  remain inert in every other environment.

### Confirmation

- Test: the invitation scenario asserts a captured message contains a password that then works once.
- Test: the isolation invariants assert the canary tenant is byte-for-byte unchanged after each destructive
  action.
- Review: a destructive test that operates on seed data is a defect, and a new destructive endpoint is added
  to the invariant sweep rather than tested by a one-off assertion.

## Pros and Cons of the Options

### Real provisioning with a mail sink (chosen)

- Good: real paths, real credentials, and confinement asserted.
- Bad: slower setup, and a test-only sink in the delivery path.

### Inserted fixture rows

- Good: fast and precise.
- Bad: bypasses the endpoints under test, and cannot prove confinement.

### Shared seeded users

- Good: no setup at all.
- Bad: the shared data is exactly what a destructive test must not touch.

### A mocked email provider

- Good: no sink in the product.
- Bad: the mock replaces the code that sends, so the invitation's actual message is never produced.

## More Information

- The shared stack and the parallel classification are separate decisions.
- Queued in-process email delivery is a separate decision; the sink is what makes that queue observable to
  a test.
