---
id: ADR-0042
title: Every end-to-end test is classified by whether it may run beside others
status: accepted
date: 2026-05-16
area: cross-cutting
related-prds: [PRD-0012, PRD-0031]
supersedes: [ADR-0043]
---

# ADR-0042 (backfilled): Every end-to-end test is classified by whether it may run beside others

**Summary:** In the context of one shared database for the whole end-to-end run, facing the choice between
serialising everything and letting tests run in parallel, we decided that every test is classified before
it is written. A parallel-safe test never calls a state-changing endpoint or asserts on shared mutable
state. A fixture-managed suite runs serially and owns the records it creates and removes them. A test that
destroys the identity or tenant it runs as is destructive and provisions that tenant itself. The class then
shapes how the file is written. The aim is parallelism where it is safe without hiding the tests that cannot
have it, accepting that the classification is the author's judgement rather than something the runner checks.

## Context and Problem Statement

Parallelism is worth having: the full matrix takes several minutes even on two workers (`playwright.config.ts`
records about six to seven), so it is run deliberately rather than on every change. It is only safe if two tests
running together cannot observe each other.

Two kinds of test dominate. One reads: it opens a list, filters, charts data that the seed created and
nothing else touches. The other writes: it creates an account, edits it, deletes it. The first can run
beside anything; the second must not collide with another test doing the same thing to the same records. A
third exists for the journeys that destroy the identity or tenant they run as (account deletion, site
deletion). They cannot use a seeded identity, which can be destroyed only once per database, so they
provision their own.

## Decision Drivers

- The suite's duration should not be dominated by tests that could safely run together.
- A test that writes must not break a test that reads.
- The classification should be visible where the test is written, not in a separate manifest.

## Considered Options

1. Classify each test as parallel-safe, fixture-managed or destructive, and write it to its class.
2. Run the whole suite serially.
3. Run everything in parallel and rely on each test to be careful.
4. A database per parallel worker.

## Decision Outcome

Chosen option: 1. A fixture-managed suite is a `test.describe.serial` block. It creates its records with
unique values, operates only on them and removes them in `afterEach`, so it can run beside another of its
kind. A parallel-safe test calls no state-changing endpoint. A destructive one provisions its own identity
or tenant through `e2e/helpers/provisioning.ts` and destroys that. The class is chosen before the test is
written (`.github/instructions/e2e.instructions.md`, `e2e/AUTHORING.md`). It shows in the file's shape: the
serial block with its `(fixture-managed)` title, or the provisioning call.

Nothing requires the class to be written down, and the suite is uneven. Twenty-two of the 46 spec files name
a class in a comment or suite title, and none names itself destructive. `settings/siteDetails.test.ts` edits
the shared site's description, and restores it, without a label. `fullyParallel: true` runs every test in
parallel, within a file as well as across files, unless the suite is declared serial.

### Consequences

- Good, because the majority of tests can run together, so the suite stays short.
- Good, because a test that writes shows it in a serial block, its cleanup or a provisioning call, and its
  isolation is visible in what it creates.
- Good, because a flake has a likely cause: a test that assumed isolation it did not declare.
- Bad, because the classification is an author's claim that nothing enforces; a parallel-safe test that
  writes passes in isolation and fails under load.
- Bad, because unique data must be generated, which is a small ceremony per test.
- Bad, because a fixture-managed test that removes its data in a teardown can leave it behind if the worker
  crashes, and some suites keep their rows by design (`users/userManagement.test.ts`,
  `approvals/approvals.test.ts`). The database therefore accumulates within a run. Each run starts from a new
  container, so nothing carries over to the next one.
- Bad, because a test that must change a seeded identity fits none of the three classes.
  `settings/userSettings.test.ts` changes `e2e_pwchange`, and `dashboard/quickActions.test.ts` sweeps its own
  seeded site. Both are kept safe only by running on the `chromium` project alone, through `testIgnore` in the
  configuration, which is a manifest outside the test.

### Confirmation

- Instruction: `.github/instructions/e2e.instructions.md` (Isolation classification) and `e2e/AUTHORING.md`
  ("Pick the test class FIRST") define the three classes and the structure each one requires.
- Review: a new file's class is evident from its structure or a comment, and a suite that writes anything it
  did not create is not parallel-safe.
- Review: a fixture-managed suite creates its own uniquely-named records and removes them; a destructive
  suite provisions its own tenant and asserts the seeded identities are untouched.

## Pros and Cons of the Options

### Classification by the author (chosen)

- Good: parallelism where it is safe, and the tests that cannot have it are visible in their structure.
- Bad: an unverified judgement, unevenly written down.

### Fully serial

- Good: nothing to classify.
- Bad: minutes of run time for tests that could have run together.

### Everything in parallel

- Good: fastest.
- Bad: the collisions are intermittent and blamed on the application rather than on the test.

### A database per worker

- Good: parallel writes without interference.
- Bad: a container and a migration per worker, which returns most of the startup cost the shared stack
  removed.

## More Information

- The shared stack this depends on: [ADR-0040](ADR-0040-end-to-end-tests-run-on-one-shared-stack.md).
- How the destructive class provisions its own tenant:
  [ADR-0051](ADR-0051-tenants-are-provisioned-and-destroyed-by-the-products-own-routes-in-tests.md).
- Sources: [PRD-0012](../Future/PRD-0012-playwright-e2e-testing-infrastructure.md) R6 (two classes) and
  [PRD-0031](../Future/PRD-0031-automated-tenant-scenario-testing.md) F1–F3 (the destructive class).
