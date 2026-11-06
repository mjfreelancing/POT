---
id: ADR-0040
title: End-to-end tests run on one shared stack against a single seeded database
status: accepted
date: 2026-05-11
area: cross-cutting
related-prds: [PRD-0012]
supersedes: [ADR-0043]
---

# ADR-0040 (backfilled): End-to-end tests run on one shared stack against a single seeded database

**Summary:** In the context of browser tests that need a real API, a real database and realistic data,
facing the choice between a stack per test file and one stack for the run, we decided on a single shared
stack — a PostgreSQL container, the API, the client and a mail sink, started once for the run — to achieve
a suite that pays its setup cost once per run and exercises the real pipeline, accepting that the tests share state and
must be written knowing it.

## Context and Problem Statement

A browser test that mocks the API proves the interface renders; it does not prove that the interface and
the server agree. So the suite runs against a real API on a real database.

Running that per test file costs a container start and a migration per file, which dominates the run. One
stack for the whole run is far cheaper, and every test then has to cope with the fact that it is not alone
in the database.

## Decision Drivers

- The suite must exercise the real pipeline, not a mock.
- Startup cost should be paid once per run, not once per file.
- Seed data must be realistic and produced the way the product produces it.

## Considered Options

1. One shared stack, one seeded database, provisioned by global setup.
2. A stack per test file or per worker.
3. A mocked API.
4. A stack per test, with a database snapshot restored between them.

## Decision Outcome

Chosen option: 1. The Playwright configuration starts the API and the client as web servers. Global setup
then starts the mail sink and the PostgreSQL container, runs the migrations, inserts the canonical sites,
users and role assignments as SQL (`e2e/seed/baseline.sql`), and waits for the API to report readiness on
`/_health/ready`. Only then does it load the financial data through the product's own import and renewal
routes. The run shares that stack, in two workers locally or one when the `CI` variable is set (no pipeline
sets it; see [ADR-0031](ADR-0031-builds-and-deployments-are-run-by-hand.md)). Tests are written to isolate
themselves within it by owning the data they create.

### Consequences

- Good, because the run pays one container start and one migration, so the suite is fast enough to run
  often.
- Good, because seeding the financial data through the routes exercises the same code a user would, so the
  seed cannot disagree with the application about what a valid import is.
- Bad, because the canonical identities are inserted as SQL, so that part of the seed can drift from what
  the product would accept.
- Good, because readiness waiting in setup means no test has to handle a stack that is not ready.
- Bad, because the tests share a database: a test that writes broadly can break another, so isolation is a
  discipline each test carries.
- Bad, because a failure can depend on execution order, which is the hardest kind of flake to diagnose.
- Bad, because parallel workers contend on the same data, so tests must be classifiable as safe to run
  beside others or as owning their data.

### Confirmation

- Setup: `waitForDatabaseReady` in `playwright.globalSetup.ts` polls `/_health/ready` at most 60 times and then
  throws, and every other setup step throws on failure. A broken stack fails the run once instead of letting
  tests start against it.
- Config: `playwright.config.ts` and `playwright.prod.config.ts` each declare one `globalSetup`, one API and one
  client `webServer`, both with `reuseExistingServer: false`, so a run owns exactly one stack.
- Instruction: `.github/instructions/e2e.instructions.md` (Isolation classification: all workers share one API
  process on `5242` and one Testcontainers database on `55432`).
- Review: a test that mutates data it does not own is a defect, and a new test states how it isolates
  itself.

## Pros and Cons of the Options

### One shared stack (chosen)

- Good: cheapest startup, and a real pipeline throughout.
- Bad: shared state, and order dependence to guard against.

### A stack per file

- Good: perfect isolation.
- Bad: a container start and a migration per file, which is minutes rather than seconds.

### Mocked API

- Good: fastest and most controlled.
- Bad: proves nothing about the client and server agreeing, which is the reason the suite exists.

### A stack per test with a restored snapshot

- Good: isolation with realistic data.
- Bad: snapshot and restore per test is a cost and a mechanism of its own.

## More Information

- How a test is classified for running beside others:
  [ADR-0042](ADR-0042-every-end-to-end-test-is-classified-by-whether-it-may-run-beside-others.md).
- Which configurations run, and when:
  [ADR-0041](ADR-0041-end-to-end-tests-run-on-a-development-profile-by-default-and-a-production-build-profile-gates-client-changing-prds.md).
- How destructive journeys get a tenant of their own:
  [ADR-0051](ADR-0051-tenants-are-provisioned-and-destroyed-by-the-products-own-routes-in-tests.md).
- Source: [PRD-0012](../Future/PRD-0012-playwright-e2e-testing-infrastructure.md) R1, R2 and R10. The earlier combined
  design record is [ADR-0043](ADR-0043-playwright-e2e-testing-infrastructure.md).
