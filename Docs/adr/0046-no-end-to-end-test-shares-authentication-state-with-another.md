---
id: ADR-0046
title: No end-to-end test shares authentication state with another
status: accepted
date: 2026-09-02
area: cross-cutting
related-prds: [PRD-0012]
supersedes: [ADR-0044]
---

# ADR-0046: No end-to-end test shares authentication state with another

**Summary:** In the context of a browser suite that runs its tests in parallel against one API and one
database, facing a refresh token that is rotated on every use and test identities whose credentials and
profile some journeys change, we decided that every test authenticates with its own fresh login and that
a journey which changes its own account runs as a seeded identity nothing else logs in as, to achieve
isolation that does not depend on run order, worker reuse or browser project, accepting a password
verification per test and a dedicated seeded row for each such journey.

## Context and Problem Statement

The refresh token is renewed on every refresh and the session row keeps only the current value
([ADR-0022](0022-access-tokens-are-short-lived-and-backed-by-a-server-side-session.md)), so a refresh
cookie is valid once. A browser context created from a saved storage state therefore carries a credential
that the first page load spends. Playwright reuses a worker across several tests by default, and runs the
four browser projects of the development matrix concurrently, so any state saved for reuse is handed to a
test that cannot use it.

A second kind of sharing is the identity itself. Signing in updates `LastLoggedInUtc`, which the save
pipeline treats as a modification and which therefore bumps the row's `Etag`; a password change revokes
the user's sessions and bumps the user version
([ADR-0037](0037-access-tokens-are-rejected-once-their-session-or-user-version-changes.md)). One seeded
account used by every suite is therefore both a concurrency hazard for optimistic concurrency and a
credential that one test can destroy for all of them.

Both problems were met by trial and error rather than by design: the login fixture was introduced as a
single login per worker writing `e2e/.auth/{parallelIndex}.json` and reusing it, which failed
intermittently once a worker was reused across more than one authenticated test, and was replaced by a
login per test in `Source/Client/pot-react/e2e/fixtures/auth.ts` (commit `a5fbd0c5`, 2026-09-02; the
earlier fixture is commit `ef05aebb`, 2026-05-16). The fixture's comments and
`Source/Client/pot-react/e2e/tests/settings/userSettings.test.ts` record the failures that forced each
step.

## Decision Drivers

- A credential that is valid once must not be stored and reused across tests.
- The suite runs in parallel: across workers within a project, and across the four projects of the matrix.
- A test that changes its own account must not be able to break another test's session.
- A failure must be reproducible. A reused session fails only when a worker or project happens to be
  reused, which is the hardest kind of failure to diagnose.
- The API's password verification is CPU-bound and is the largest server-side cost under parallel load, so
  the number of logins per test has to stay at one.

## Considered Options

1. One fresh login per test, and a dedicated seeded identity for every journey that changes its own
   account (the current design).
2. One login per worker, its storage state saved to a file and reused by the tests in that worker.
3. One login per run, shared by every test and project.
4. A login per API operation, so each request context starts from a known session.
5. One shared identity for every authenticated test, with no dedicated accounts.

## Decision Outcome

Chosen option: 1, because it is the only option that holds under worker reuse and project concurrency.

Option 2 was built first and did not hold: the saved cookie was spent by the first page load of the first
test in the worker, and later tests received a 401 and were redirected to the login page. Option 3 fails
for the same reason, with a wider blast radius. Option 4 turns one verification into several per test.
Option 5 fails whenever a test changes its own password, and raced on the shared row's `Etag` even before
that, because every login is a modification of `LastLoggedInUtc`.

The two halves of the decision are one rule: a test owns its authentication state, both the session it
holds and the account it signs in as. A test that logs in fresh but shares its account can still be broken
by another test; a test with its own account but a shared session is broken by the first page load.

### Consequences

- Good, because a suite's result no longer depends on which tests ran before it, on which worker it landed,
  or on the order its projects started.
- Good, because a password change, a session revocation or a profile edit is confined to the identity that
  journey owns.
- Good, because the single login still pays for itself twice: the fixture yields both `storageState` and
  the 15-minute access token, so a fixture-managed suite reuses that token for API setup and cleanup
  instead of logging in again per operation.
- Bad, because every authenticated test pays one password verification, the suite's largest server-side
  CPU cost under parallel load.
- Bad, because each journey that changes its own account needs its own seeded row, with a committed hash in
  `e2e/seed/baseline.sql`, and regenerating the seed when an identity changes.
- Bad, because owning an identity excludes concurrency over it: the settings journey that edits one
  account's profile and password is collected on one browser project only, since two projects running it
  at once collide on that account.

### Confirmation

- Test: `Source/Client/pot-react/e2e/tests/settings/userSettings.test.ts` runs both journeys through
  `pwChangeTest`; it fails with a 401 or a 409 if the session or the identity is shared.
- Fixture: `Source/Client/pot-react/e2e/fixtures/auth.ts` yields state per test and per identity; there is
  no path that stores state for reuse.
- Instruction: `.github/instructions/e2e.instructions.md` (Fixtures and config) states the rule and cites
  this record.
- Review: a change that writes a storage state to a file for reuse, or logs a second suite in as
  `e2e_pwchange`, is rejected.

## More Information

The abandoned worker-scoped approach, and the other approaches tried and dropped while the suite was
being made reliable, are recorded in
[ADR-0044](0044-playwright-e2e-testing-infrastructure.md) (Appendix A, design history).
