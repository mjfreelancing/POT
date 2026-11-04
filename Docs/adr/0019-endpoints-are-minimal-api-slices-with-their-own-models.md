---
id: ADR-0019
title: Endpoints are minimal API slices with their own request and response models
status: accepted
date: 2026-11-04
area: backend
---

# ADR-0019: Endpoints are minimal API slices with their own request and response models

**Summary:** In the context of an API with no views and a client that calls it operation by operation,
facing the choice between controllers shared per resource and a handler per operation, we decided that
each operation is a minimal API slice with its own request and response models, a static handler and
hand-written mapping, to achieve operations that change independently and declare exactly what they
return, accepting some repetition between similar operations.

## Context and Problem Statement

A controller groups operations by resource, which is convenient when the operations belong together. In
this application they are separate: creating an account, changing a status, renewing a schedule and
projecting balances have different models, different rules and different failure sets. Grouping them
forces shared models and a shared pipeline, and every addition widens the group.

Minimal APIs make the operation the unit: its route, its metadata and its handler are declared together.

## Decision Drivers

- One operation's model change should not ripple into its neighbours.
- What an operation accepts, returns and fails with should be readable in one place.
- Registration should read as the list of operations the API offers.

## Considered Options

1. One minimal API slice per operation, with per-operation models and hand-written mapping.
2. Controllers grouped by resource, with shared request models.
3. Minimal APIs returning entities directly, with no separate models.
4. Handlers generated from the contract.

## Decision Outcome

Chosen option: 1. Each operation has its own folder containing its handler, its request and response
models, its mapping, its validation and its declared responses. Handlers are static, take only the
services they use, and return typed results. Mapping is written by hand — no mapping library — so a
shape change is a compile error rather than a silent convention.

Endpoints are registered as a chain of per-feature extension methods, so the registration reads as the
API's surface.

### Consequences

- Good, because an operation can change its models, rules and failures without touching another.
- Good, because a handler's dependencies are visible in its signature, and mocking them in a test needs
  nothing but those services.
- Good, because metadata — response types, failures, rate-limit policy, anonymity — is declared at the
  operation it applies to.
- Bad, because similar operations repeat similar models instead of sharing one, which looks like
  duplication until the operations diverge.
- Bad, because hand-written mapping is more code than a convention-based mapper, and every new field is
  a second edit.
- Bad, because a shared behaviour must be applied per slice or per group, where a controller base class
  would have offered inheritance.

### Confirmation

- Test: `EndpointResponseMetadataFixture` pins each endpoint's declared failures, which is only
  meaningful because each slice declares its own.
- Test: the per-feature handler fixtures exercise the slice end to end through the hosted API.
- Review: a shared request model introduced between operations is a decision to be challenged, not a
  refactor; only genuinely common shapes are shared.

## Pros and Cons of the Options

### Slice per operation (chosen)

- Good: independent change, and explicit metadata.
- Bad: repetition across operations that look alike.

### Controllers per resource

- Good: one place per resource, with shared filters and a familiar model.
- Bad: shared models become a constraint, and the group grows until its pipeline is doing conditionals.

### Returning entities

- Good: the least code.
- Bad: the database shape becomes the contract, and an internal field becomes a published one by
  accident.

### Generated handlers

- Good: contract-first, with the code derived.
- Bad: a generator between the design and the behaviour, for an API whose contract is already explicit.

## More Information

- The layering the slices sit in is a separate decision.
- The contract document each slice contributes to is a separate decision.
