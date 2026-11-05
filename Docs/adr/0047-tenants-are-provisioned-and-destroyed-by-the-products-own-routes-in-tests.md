---
id: ADR-0047
title: Tenants are provisioned and destroyed by the product's own routes in tests
status: accepted
date: 2026-10-27
area: cross-cutting
related-prds: [PRD-0031]
supersedes: [ADR-0040]
---

# ADR-0047 (backfilled): Tenants are provisioned and destroyed by the product's own routes in tests

**Summary:** In the context of destructive operations that must be shown to touch only their own tenant,
facing the choice between inserting fixture rows and driving the product's own provisioning routes, we
decided that destructive journeys provision a tenant through sign-up, approval and invitation, read the
sign-up or invitation mail from a mail sink, and assert that the seeded identities survive, to achieve proof that
deletion is confined using the paths a user takes, accepting that setup is slower and depends on the email
flow working.

## Context and Problem Statement

A test that inserts rows directly can prove that a repository method deletes them, and proves nothing about
whether the endpoint does. Worse, a destructive test on shared seed data cannot distinguish "confined to
its own site" from "deleted the neighbour too", because the neighbour is not part of the assertion.

The provisioning a real tenant needs — sign-up, approval, an invitation, an accepted password — is
several steps through the product, one of which is an email.
A seeded identity can also be destroyed only once per database. A destructive spec that consumes one cannot
run in a second browser project, or a second time against the same seed
([PRD-0031](../Future/PRD-0031-automated-tenant-scenario-testing.md), Problem Statement and F1–F3).

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

Chosen option: 1. A destructive journey provisions what it will destroy by calling the product's routes
(`e2e/helpers/provisioning.ts`). The site-deletion journey signs up and is approved, so it owns a site of its
own; account deletion invites a viewer into the shared site. The sign-up's code and password, and the
invitation's password, are read from a maildev container (`pot-e2e-mail-sink`). Global setup starts it on the
SMTP port the API is configured with, so each credential is the one the application generated. After the
journey, two seeded usernames (`e2e_admin`, `e2e_viewer`) are asserted to still exist, so "the seed survived"
is a check rather than an assumption.

The server-side integration suite applies the same idea more strongly. Its scenarios are provisioned through
the routes, and `Invariants/IsolationFixture.cs` fingerprints every tenant-scoped table of a canary tenant and
of the target tenant before and after three operations: removing a member, removing one's own account and
removing a site. It reads messages differently. The test host replaces the email channel writer with
`CapturingEmailWriter`, so a message is read as the model the application submitted, not as mail that was
rendered and sent. That is the mechanism option 4 describes.

### Consequences

- Good, because provisioning exercises the real routes, so a change to how a tenant is created breaks the
  scenario rather than a hidden fixture.
- Good, because in the integration suite the canary tenant turns confinement into a check on each action that
  removes a person or a tenant.
- Good, because the mail sink also proves that the invitation actually produced a usable credential.
- Bad, because a scenario costs several requests and a mail round trip, so it is slower than inserted rows.
- Bad, because the end-to-end proof of confinement is only "the seeded users still exist", while the full
  fingerprint lives in the integration suite; the browser journeys are the weaker check.
- Bad, because only the site-deletion journey owns a whole tenant; account deletion provisions a user inside
  the shared site.
- Bad, because global setup starts the mail container before anything else and fails the run if it cannot.
  No end-to-end test runs unless Docker can start it and ports `1025` and `1080` are free, and that applies to
  every test, not only the destructive journeys.

### Confirmation

- Test: the provisioning helper signs in with the captured password, which proves the invitation produced a
  working credential; no test asserts that a second use of it fails.
- Test: `Pot.AspNetCore.Integration.Tests/Invariants/IsolationFixture.cs` compares the canary's and the target's
  fingerprints before and after member removal, own-account removal and site removal. It runs in the always-on
  integration suite, not an opt-in tier.
- Test: `e2e/tests/smoke/mailSink.test.ts` reads a sign-up code from the sink.
  `e2e/tests/settings/accountDeletion.test.ts` and `siteDeletion.test.ts` assert that the seeded usernames survive.
- Instruction: `.github/instructions/e2e.instructions.md` (Isolation classification: Destructive) and
  `.github/instructions/aspnetcore-integration-tests.instructions.md` (Provisioning a scenario; Isolation invariants).
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

- The shared stack and the parallel classification: [ADR-0037](0037-end-to-end-tests-run-on-one-shared-stack.md)
  and [ADR-0039](0039-every-end-to-end-test-is-classified-by-whether-it-may-run-beside-others.md).
- Queued in-process email delivery:
  [ADR-0024](0024-email-is-queued-in-process-and-rendered-from-razor-components.md). The sink is what makes that
  queue observable to a browser test.
- Source: [PRD-0031](../Future/PRD-0031-automated-tenant-scenario-testing.md) (A, B, D and F requirements).
