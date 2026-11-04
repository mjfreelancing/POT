---
id: ADR-0033
title: Tests are per-layer unit projects plus one hosted integration project
status: accepted
date: 2026-11-04
area: backend
---

# ADR-0033: Tests are per-layer unit projects plus one hosted integration project

**Summary:** In the context of a layered server whose rules, queries and HTTP behaviour fail in
different ways, facing the choice between one end-to-end suite and a suite per layer, we decided on
per-layer unit projects plus a single hosted integration project that boots the application against a
real database, to achieve fast tests for most changes and a real pipeline for the parts only a real
pipeline can show, accepting two kinds of test to write and a slower outer suite.

## Context and Problem Statement

A business rule can be tested with substituted repositories in milliseconds. A query needs a real
provider to show a translation failure. A failure becoming a Problem Details body, an authorization
fallback, a middleware ordering — none of those exist outside the pipeline.

A single suite at the top of that pyramid would be slow and would make a rule change wait on a container.
A single suite of unit tests would never show what the framework does with what the code returns.

## Decision Drivers

- Most changes are business rules and queries; their tests should not need a container.
- The pipeline's behaviour is only observable through a real request.
- The suite is run frequently, so its fast portion must be fast.

## Considered Options

1. One project per layer and one hosted integration project.
2. A single end-to-end suite through HTTP.
3. Unit tests only, with no hosted suite.
4. Integration tests with a mocked database or an in-memory provider.

## Decision Outcome

Chosen option: 1. There is a test project per production project, testing that layer with its
neighbours substituted, and one integration project that boots the application with a real database
container and drives it through HTTP.

The integration project owns the things only it can show: status codes, Problem Details bodies, headers,
authorization, the middleware order, and database failures surfacing as ordinary failures. Architecture
rules that are not visible to any single test — endpoint authorization metadata, response declarations —
are pinned there too.

### Consequences

- Good, because a rule change is verified in seconds, so the fast suite is run constantly.
- Good, because the outer suite proves the parts that no unit test can: the pipeline, the serializer, the
  database provider and the error mapping together.
- Good, because a layer's test substitutes its neighbours, so a failing test points at one layer.
- Bad, because a change can need both suites, and the outer one costs a container start.
- Bad, because the per-layer probes mock behaviour the integration suite proves, so some intent is
  expressed twice.
- Bad, because the layer tests can pass while the wiring between layers is wrong, which only the outer
  suite catches.

### Confirmation

- Review: a rule tested only through the hosted suite is an expensive test for something that did not need
  a pipeline; a status code asserted in a unit test is a test in the wrong project.
- Test: the integration project is the only place an assertion about HTTP shape belongs, so its fixtures
  are the confirmation that the layering holds.

## Pros and Cons of the Options

### Per-layer plus hosted integration (chosen)

- Good: fast feedback for the common change, real proof for the uncommon one.
- Bad: two kinds of test and duplicated intent at the boundary.

### Single end-to-end suite

- Good: one kind of test, and it always tests the real thing.
- Bad: minutes per run, and a rule change waits on a container.

### Unit tests only

- Good: fastest possible.
- Bad: nothing proves that the pipeline, the serializer or the provider behave as assumed — the failures
  that reach production first.

### Integration with a fake database

- Good: fast, and still through HTTP.
- Bad: the in-memory provider does not behave like the real one, so query translation, constraints and
  concurrency — the reasons to test at that level — are absent.

## More Information

- The scenario harness that provisions real tenants through the product's own routes builds on the hosted
  suite and is recorded separately.
- Continuous integration does not currently run either suite; that is a separate decision.
