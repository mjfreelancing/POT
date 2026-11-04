---
id: ADR-0014
title: Every failure is written as a Problem Details document
status: accepted
date: 2026-11-04
area: backend
---

# ADR-0014: Every failure is written as a Problem Details document

**Summary:** In the context of a client that has to explain a failure to a user and a support process
that has to find the request that caused it, facing a framework that leaves many error responses with
no body at all, we decided that every failure response carries an RFC 9457 Problem Details document
with a stable type, a user-safe detail and an error code, to achieve one shape the client can read
whatever produced it, accepting that the server must fill in bodies the framework would have left
empty.

## Context and Problem Statement

Errors arrive from different places: request validation, a business rule refusing an action, the
authorization pipeline, the rate limiter, an unhandled exception, and the database. Out of the box they
share nothing: some carry a framework validation body, some carry nothing, some carry text written for
a developer.

A client that receives no body cannot say anything useful, so it invents a generic message; a support
request that quotes only a status code cannot be traced to a request at all.

## Decision Drivers

- One shape, so the client has one parser and one way to choose a message.
- No submitted value in a body or a log, because the same value may be a password or a financial
  figure.
- A correlation identifier on every failure, so a user's report can be matched to a log entry.
- A stable, machine-readable type per failure kind, so behaviour can be tested without matching prose.

## Considered Options

1. RFC 9457 Problem Details for every failure, written by the application even where the framework
   would not write one.
2. Only the status code, with no body.
3. A bespoke error envelope shared with the client.
4. Problem Details where the framework produces it, and nothing elsewhere.

## Decision Outcome

Chosen option: 1. Failures are written as Problem Details with a stable `type` per kind, a `title`, an
HTTP-status-bearing `status`, a user-safe `detail`, an `errors` map for validation, an error code, the
caller's correlation identifier and the framework trace identifier.

Two pipeline stages do the work the framework does not: the exception handler writes a body for an
unhandled exception, and the status-code pages write one for a status that otherwise has none. Both run
late enough that every refusal above them is covered.

### Consequences

- Good, because the client parses one document and can branch on `type` or the error code rather than
  on prose.
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
- Test: `EndpointResponseMetadataFixture` pins which failures each endpoint declares, so the contract
  matches the behaviour.
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
