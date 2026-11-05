---
id: ADR-0012
title: Every failure is written as a Problem Details document
status: accepted
date: 2025-01-11
area: backend
related-prds: [PRD-0030]
---

# ADR-0012 (backfilled): Every failure is written as a Problem Details document

**Summary:** In the context of a client that has to explain a failure to a user and a support process
that has to find the request that caused it, facing a framework that leaves many error responses with
no body at all, we decided that every failure response carries an RFC 9457 Problem Details document
with a correlation identifier and a user-safe message — in `detail`, or for a `400` in an `errors` map
keyed by field — and a stable `type` for each conflict category, to achieve one shape the client can read
whatever produced it, accepting that the server must fill in bodies the framework would have left
empty.

## Context and Problem Statement

Errors arrive from different places: request validation, a business rule refusing an action, the
authorization pipeline, the rate limiter, an unhandled exception, and the database. Out of the box they
share nothing: some carry a framework validation body, some carry nothing, some carry text written for
a developer.

A client that receives no body cannot say anything useful, so it invents a generic message; a support
request that quotes only a status code cannot be traced to a request at all.
A consistent shape is what lets the client interpret and handle every error predictably;
[PRD-0030](../Future/PRD-0030-problem-details-consolidation.md) consolidated the responses into one.

## Decision Drivers

- One shape, so the client has one parser and one way to choose a message.
- No submitted value in a body or a log, because the same value may be a password or a financial
  figure.
- A correlation identifier on every failure, so a user's report can be matched to a log entry.
- A stable, machine-readable type wherever a caller must tell failures of one status apart, so it need
  not match prose.

## Considered Options

1. RFC 9457 Problem Details for every failure, written by the application even where the framework
   would not write one.
2. Only the status code, with no body.
3. A bespoke error envelope shared with the client.
4. Problem Details where the framework produces it, and nothing elsewhere.

## Decision Outcome

Chosen option: 1. Every failure body carries `status`, a correlation identifier (`correlationId`), the
framework trace identifier (`traceId`) and the request path as `instance`. A `400` is always a validation
problem: the framework's validation title, an `errors` map from field to messages (a message for the whole
request under the empty key), and no `detail`. Every other status carries no `errors` and a user-safe
`detail`, the application's own or a per-status default. A bespoke `urn:pot:*` type is published for the
three conflict categories, which is where a caller needs to tell one conflict from another; every other
status keeps the framework's per-status `type` where the framework defines one, and a `429` has none. No
error code is serialised, so a validation failure and a refused business rule share one shape and differ
only in the messages in `errors`.

Three producers write failures the framework would not: the exception handler writes a body for an
unhandled exception, the status-code pages write one for a status that otherwise has none, and the rate
limiter writes its own rejection. The body is written whatever the request's `Accept` header asks for,
which is what makes the shape uniform for a caller that did not ask for JSON.

### Consequences

- Good, because the client parses one document, and on a conflict can branch on `type` rather than on
  prose.
- Good, because a failure always carries the identifier the caller quoted when they reported it, on
  success as a header and on failure in the body.
- Good, because no submitted value reaches the body, so a validation failure cannot echo a password.
- Bad, because the server owns wording the framework would otherwise have chosen, so the text is ours
  to keep accurate and consistent.
- Bad, because the contract has to be declared per endpoint, and an endpoint that declares the wrong
  failure set is wrong in its contract while behaving correctly.
- Bad, because a proxy that answers before the application does — a gateway, a timeout — can still
  produce a response with no body, so the client must still cope with one.

### Confirmation

- Test: `CorrelationAndProblemDetailsFixture` asserts the document shape and that the correlation
  identifier survives into the failure body.
- Test: `ShouldBeValidationProblemAsync`
  (`Pot.AspNetCore.Integration.Tests/Host/Extensions/HttpResponseMessageExtensions.cs`) asserts every
  validation failure it checks carries no submitted value, no error-code member and no `detail`, with a
  non-empty `errors` map.
- Test: `HeadersAndRateLimitFixture` asserts the `429` body carries `detail`, `status` and
  `correlationId`, and no `type` or `errors`.
- Test: `EndpointResponseMetadataFixture` pins the failures every endpoint shares — `400` as the validation
  problem, `500` as Problem Details, a declared `409` as Problem Details, `413` and `408` on routes that read
  a body, and no `422`. It does not compare each endpoint's declared set with its behaviour, and no
  endpoint declares the `429` that every rate-limited group can return.
- Outside this decision: both health probes are short-circuited and answer in plain text, and a request
  aborted by the client gets no body. Both are deliberate exceptions rather than gaps.
- Review: a handler that writes an error body by hand is a defect; expected failures are returned as
  results and converted in one place.

## Pros and Cons of the Options

### Problem Details everywhere (chosen)

- Good: a standard shape, and one place in the client that reads it.
- Bad: every status that the framework leaves bodiless must be filled in deliberately.

### Status code only

- Good: nothing to maintain.
- Bad: the client cannot distinguish a refusal from a fault, and support has nothing to quote.

### A bespoke envelope

- Good: total freedom over shape.
- Bad: reinvents a standard, and every consumer learns a private format.

### Problem Details where the framework offers it

- Good: least work.
- Bad: the same endpoint would answer in two different shapes depending on which stage failed, which is
  the inconsistency this record removes.

## More Information

- Expected failures being values rather than exceptions is a separate decision; this record is about how
  both kinds are written on the wire.
- The client's single normalising interceptor is recorded for the client separately.
