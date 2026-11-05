---
id: ADR-0044
title: Every end-to-end test declares whether it may run beside others
status: accepted
date: 2026-05-16
area: frontend
---

# ADR-0044 (backfilled): Every end-to-end test declares whether it may run beside others

**Summary:** In the context of one shared database for the whole end-to-end run, facing the choice between
serialising everything and letting tests run in parallel, we decided that a suite is written as
parallel-safe — reading only data it did not create — or as fixture-managed — owning the records it
creates and removing them — or, where it must destroy a tenant, as destructive and provisioning its own,
and that the classification is stated at the top of the file, to achieve parallelism where it is safe
without hiding the tests that cannot have it, accepting that the classification is the author's assertion
rather than something the runner enforces.

## Context and Problem Statement

Parallelism is worth having: the suite is the slowest gate in a change. It is only safe if two tests
running together cannot observe each other.

Two kinds of test dominate. One reads: it opens a list, filters, charts data that the seed created and
nothing else touches. The other writes: it creates an account, edits it, deletes it. The first can run
beside anything; the second must not collide with another test doing the same thing to the same records. A
third exists for the journeys that destroy a whole tenant, which cannot share anything at all.

## Decision Drivers

- The suite's duration should not be dominated by tests that could safely run together.
- A test that writes must not break a test that reads.
- The classification should be visible where the test is written, not in a separate manifest.

## Considered Options

1. Classify each test as parallel-safe or fixture-managed, declared in the test.
2. Run the whole suite serially.
3. Run everything in parallel and rely on each test to be careful.
4. A database per parallel worker.

## Decision Outcome

Chosen option: 1. A fixture-managed suite creates its records with unique values, operates only on them,
and removes them, so it can run beside another of its kind; a parallel-safe one reads only seed data; a
destructive one provisions its own tenant through the product's routes and destroys that. The
classification is stated at the top of the file, so a reviewer sees it and a new file must choose.

It is a statement rather than a declaration the runner can check, and the current suite does not apply it
consistently — some suites mutate the shared site without a label. Playwright runs files in parallel and
the label is what tells a reader which ones may.

### Consequences

- Good, because the majority of tests can run together, so the suite stays short.
- Good, because a test that writes says so, and its isolation is visible in what it creates.
- Good, because a flake has a likely cause: a test that assumed isolation it did not declare.
- Bad, because the classification is an author's claim that nothing enforces; a parallel-safe test that
  writes passes in isolation and fails under load.
- Bad, because unique data must be generated, which is a small ceremony per test.
- Bad, because a fixture-managed test that removes its data in a teardown can leave it behind if the test
  crashes the worker, so the shared database accumulates.

### Confirmation

- Review: every new file states its classification in its header, and a suite that writes anything it did
  not create is not parallel-safe.
- Review: a fixture-managed suite creates its own uniquely-named records and removes them; a destructive
  suite provisions its own tenant and asserts the seeded identities are untouched.

## Pros and Cons of the Options

### Declared classification (chosen)

- Good: parallelism where safe, honesty where not.
- Bad: an unverified claim.

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

- The shared stack this depends on is a separate decision.
- The classes of test that provision their own tenant are part of the scenario work and are separate again.
