---
id: ADR-0026
title: Request shape is validated at the edge and data-dependent rules in check chains
status: accepted
date: 2025-01-11
area: backend
---

# ADR-0026 (backfilled): Request shape is validated at the edge and data-dependent rules in check chains

**Summary:** In the context of requests that can be wrong in two different ways — malformed, and
unacceptable given the data — facing the choice between one validation layer and two, we decided that
request validators check shape before the handler proceeds and entity checks enforce data-dependent
rules after the record is loaded, to achieve field errors for the client and business refusals that do
not need a database to be described, accepting that a rule's home is decided by whether it needs data.

## Context and Problem Statement

"Name is required" and "an account with this name already exists" are both refusals, but only the first
is answerable from the request alone. Putting both in a validator forces the validator to reach the
database; putting both in the domain layer means a malformed request travels further than it should and
the client loses its field map.

The two also fail differently: a shape failure is a `400` with an `errors` map keyed by field, while a
data-dependent refusal is a business failure with its own type.

## Decision Drivers

- A malformed request should be refused before any data is read.
- Field-level messages should reach the client where they belong, beside the input.
- Data-dependent rules must be replaceable in a test without a request or a serializer.

## Considered Options

1. Validators for request shape, check chains for data-dependent rules.
2. All rules in validators, including those that read data.
3. All rules in the domain layer, after mapping.
4. Data annotations on request models, with no separate validator.

## Decision Outcome

Chosen option: 1. A request that carries fields needing shape rules has a validator that checks presence,
length, format, allowed values and relations between its own fields, producing field-keyed messages; the
few requests whose input is an identifier or a flag have none. Passing that, the handler maps the request
to an input and the service runs its checks, which are small classes with one rule each, receiving the
entity being changed — loaded for an update, constructed for a create — and returning the first failure.

The order of the chain is declared only where it matters; elsewhere it is whatever the registrations
produce, so a check that depends on another's outcome is a signal to declare one.

A check sees the entity and the requested change, so it can express "this would remove the last
administrator" without knowing anything about HTTP.

### Consequences

- Good, because a malformed request costs no database read and cannot produce a half-evaluated rule.
- Good, because field errors reach the client in the shape it can place beside the input.
- Good, because each data-dependent rule is a small class with one reason to change, and its test needs
  no request.
- Bad, because a rule must be classified as shape or data, and a rule that is arguably both has to pick a
  home.
- Bad, because validation of one request is now in two places, so a reader has to look at both to know
  everything a request must satisfy.
- Bad, because the check chain is explicit plumbing — a list of small classes whose order is mostly
  registration order — where one method would be shorter and its order obvious.

### Confirmation

- Test: the request-validator fixtures assert the field-keyed messages for the requests that have one;
  most validators have no fixture of their own and are covered through the endpoints.
- Test: the check fixtures assert each rule in isolation with a constructed state.
- Test: the handler fixtures assert a shape failure answers `400` with `errors` before any data is
  touched.
- Review: a validator that reads the database, or a check that inspects HTTP, is a defect.

## Pros and Cons of the Options

### Two layers, split by kind of rule (chosen)

- Good: each rule is where its evidence is.
- Bad: one request's rules must be read in two places.

### Everything in validators

- Good: one place per request, and field errors everywhere.
- Bad: validators become data access, so testing a rule needs a repository substitute and the "validation
  is cheap" property is lost.

### Everything in the domain layer

- Good: one model of what is acceptable.
- Bad: a malformed request is mapped and loaded before being refused, and the client loses field
  placement.

### Data annotations

- Good: declared on the model, no separate file.
- Bad: cannot express anything beyond a single field's shape, and the client's messages become framework
  text.

## More Information

- How the failures of both kinds are written on the wire is a separate decision.
- Authorization rules are enforced before either layer and are separate again.
