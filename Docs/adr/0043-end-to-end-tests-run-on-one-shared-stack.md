---
id: ADR-0043
title: End-to-end tests run on one shared stack against a single seeded database
status: accepted
date: 2026-11-04
area: frontend
---

# ADR-0043: End-to-end tests run on one shared stack against a single seeded database

**Summary:** In the context of browser tests that need a real API, a real database and realistic data,
facing the choice between a stack per test file and one stack for the run, we decided on a single shared
stack — the API and a PostgreSQL container, provisioned once by global setup and seeded through the
product's own routes — to achieve a suite that starts in seconds and exercises the real pipeline,
accepting that the tests share state and must be written knowing it.

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

Chosen option: 1. Global setup starts PostgreSQL in a container and the API against it, waits for the
application to report readiness, and provisions the canonical accounts through the product's own sign-up,
approval and invitation routes rather than by inserting rows. The run then shares that stack, and tests
are written to isolate themselves within it by owning the data they create.

### Consequences

- Good, because the run pays one container start and one migration, so the suite is fast enough to run
  often.
- Good, because seeding through the routes exercises the same code a user would, so the seed cannot
  disagree with the application about what a valid account is.
- Good, because readiness waiting in setup means no test has to handle a stack that is not ready.
- Bad, because the tests share a database: a test that writes broadly can break another, so isolation is a
  discipline each test carries.
- Bad, because a failure can depend on execution order, which is the hardest kind of flake to diagnose.
- Bad, because parallel workers contend on the same data, so tests must be classifiable as safe to run
  beside others or as owning their data.

### Confirmation

- Test: global setup fails the run rather than letting tests start against an unready API, so a broken stack
  is one clear failure rather than a hundred misleading ones.
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

- How a test declares that it can run beside others is a separate decision.
- The execution policy — which configurations run, and when — is separate again.
