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
decided that destructive journeys provision a tenant through sign-up, approval and invitation, read the
invitation mail from a mail sink, and assert that the seeded identities survive, to achieve proof that
deletion is confined using the paths a user takes, accepting that setup is slower and depends on the email
flow working.

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

Chosen option: 1. A destructive journey provisions what it will destroy by calling the product's routes:
the site-deletion journey creates a site of its own, and account deletion invites a user into the shared
site. Invitation mail is read from a mail-development container the run starts and points the API at, so the
temporary password is a real one. After the journey the seeded identities are asserted to still exist, so
"only this tenant was touched" is a check rather than an assumption.

The server-side integration suite has the stronger form of the same idea: a capturing email writer inside
the host, and a fingerprint of every row of every tenant-scoped table compared before and after.

### Consequences

- Good, because provisioning exercises the real routes, so a change to how a tenant is created breaks the
  scenario rather than a hidden fixture.
- Good, because the canary tenant turns confinement into a check on every destructive action.
- Good, because the mail sink also proves that the invitation actually produced a usable credential.
- Bad, because a scenario costs several requests and a mail round trip, so it is slower than inserted rows.
- Bad, because the end-to-end proof of confinement is only "the seeded users still exist", while the full
  fingerprint lives in the integration suite; the browser journeys are the weaker check.
- Bad, because only the site-deletion journey owns a whole tenant; account deletion provisions a user inside
  the shared site.
- Bad, because the run depends on a mail container, so the destructive journeys cannot run without it.

### Confirmation

- Test: the provisioning helper signs in with the captured password, which proves the invitation produced a
  working credential; no test asserts that a second use of it fails.
- Test: the integration isolation invariants compare a full tenant fingerprint before and after each
  destructive operation.
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
