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
[PRD-0030](../prd/PRD-0030-problem-details-consolidation.md) consolidated the responses into one.

## Decision Drivers

- One shape, so the client has one parser and one way to choose a message.
- No submitted value in a failure body, because the same value may be a password or a financial figure.
  Keeping it out of the logs is
  [ADR-0050](ADR-0050-request-and-failure-logs-record-metadata-never-a-submitted-value.md).
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

Chosen option: 1. Every failure response is an RFC 9457 Problem Details document, and the application
writes one even where the framework would have left the response without a body. Every document
carries a correlation identifier. A `400` is always a validation problem, whose messages are an `errors`
map keyed by field; every other status carries a user-safe `detail` instead. A conflict carries a stable
`type` naming its category, so a caller branches on the type rather than on an error code or on prose,
and no error code is serialised. The document is written in one shape whatever the request's `Accept`
header asks for.

The member-by-member rules — which members each status carries, the default `detail` per status and the
conflict types — are documented in the Problem Details section of
[`Source/Server/DEVELOPER.md`](../../Source/Server/DEVELOPER.md#problem-details) and specified by
[PRD-0030](../prd/PRD-0030-problem-details-consolidation.md).

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

- Three producers write failures the framework would not: the exception handler writes a body for an
  unhandled exception, the status-code pages write one for a status that otherwise has none, and the rate
  limiter writes its own rejection.
- A status other than a conflict keeps the framework's per-status `type` where the framework defines one,
  and a `429` has none, as `HeadersAndRateLimitFixture` asserts.
- Expected failures being values rather than exceptions is a separate decision; this record is about how
  both kinds are written on the wire.
- The client's single normalising interceptor is recorded for the client separately.
