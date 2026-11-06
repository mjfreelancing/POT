---
id: ADR-0051
title: Tenants are provisioned and destroyed by the product's own routes in tests
status: accepted
date: 2026-10-27
area: cross-cutting
related-prds: [PRD-0031]
supersedes: [ADR-0043]
---

# ADR-0051 (backfilled): Tenants are provisioned and destroyed by the product's own routes in tests

**Summary:** In the context of destructive operations that must be shown to touch only their own tenant,
facing the choice between inserting fixture rows and driving the product's own provisioning routes, we
decided that test tenants are provisioned through the product's own routes and that confinement is
asserted against a tenant outside the target, to achieve proof that deletion is confined using the paths
a user takes, accepting that setup is slower and depends on the email flow working.

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

1. Provision through the product's routes, and assert confinement against a tenant outside the target.
2. Insert fixture rows directly, and assert on them.
3. Use the shared seeded users for destructive tests.
4. A test-only provisioning endpoint that creates a ready-made tenant in one call.

## Decision Outcome

Chosen option: 1. A test that destroys a tenant, or a person in one, first provisions what it will destroy
by calling the product's own routes — sign-up, approval, invitation and the accepted password — rather than
inserting rows or consuming the shared seed. Each credential it signs in with is one the application
generated, so the test signs in the way a user would.

After the destructive operation, the test asserts that something outside the target is unchanged, so
confinement is a check rather than an assumption. The integration suite fingerprints every tenant-scoped
table of a canary tenant, and of the target tenant, before and after the operation; the browser suite
asserts that the seeded identities survive.

### Consequences

- Good, because provisioning exercises the real routes, so a change to how a tenant is created breaks the
  scenario rather than a hidden fixture.
- Good, because in the integration suite the canary tenant turns confinement into a check on each action that
  removes a person or a tenant.
- Good, because signing in with the generated credential proves that the invitation produced a usable one.
- Bad, because a scenario costs several requests and a mail round trip, so it is slower than inserted rows.
- Bad, because the end-to-end proof of confinement is only "the seeded users still exist", while the full
  fingerprint lives in the integration suite; the browser journeys are the weaker check.
- Bad, because the credentials arrive by email, so the browser suite depends on a mail sink, and every
  end-to-end run fails if it cannot start, not only the destructive journeys.

### Confirmation

- Test: the provisioning helper (`e2e/helpers/provisioning.ts`) signs in with the captured password, which
  proves the invitation produced a working credential; no test asserts that a second use of it fails.
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

### Real provisioning with a confinement check (chosen)

- Good: real paths, real credentials, and confinement asserted.
- Bad: slower setup, and a dependency on the email flow.

### Inserted fixture rows

- Good: fast and precise.
- Bad: bypasses the endpoints under test, and cannot prove confinement.

### Shared seeded users

- Good: no setup at all.
- Bad: the shared data is exactly what a destructive test must not touch.

### A test-only provisioning endpoint

- Good: one fast call, and the rows are still written by the server rather than by the test.
- Bad: it bypasses sign-up, approval and invitation, so a change to how a tenant is created does not break
  the scenario, and a route that exists only for tests must be kept out of production.

## More Information

- Current journeys: site deletion signs up and is approved, so it owns a site of its own; account deletion
  invites a viewer into the shared site, so only site deletion owns a whole tenant. Both assert afterwards
  that `e2e_admin` and `e2e_viewer` still exist.
- Mail capture in the browser suite: the sign-up's code and password, and the invitation's password, are
  read from a maildev container (`pot-e2e-mail-sink`). Global setup starts it on the SMTP port the API is
  configured with, before anything else, and fails the run if it cannot; no end-to-end test runs unless
  Docker can start it and ports `1025` and `1080` are free. A mocked email provider was not used there,
  because it would replace the code that sends, so the invitation's actual message would never be produced.
- Mail capture in the integration suite: the test host replaces the email channel writer with
  `CapturingEmailWriter`, so a message is read as the model the application submitted, not as mail that was
  rendered and sent.
- PRD-0031 allows direct writes only for states the product cannot produce.
- The shared stack and the parallel classification: [ADR-0040](ADR-0040-end-to-end-tests-run-on-one-shared-stack.md)
  and [ADR-0042](ADR-0042-every-end-to-end-test-is-classified-by-whether-it-may-run-beside-others.md).
- Queued in-process email delivery:
  [ADR-0027](ADR-0027-email-is-queued-in-process.md). The sink is what makes that
  queue observable to a browser test.
- Source: [PRD-0031](../Future/PRD-0031-automated-tenant-scenario-testing.md) (A, B, D and F requirements).
