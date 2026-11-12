---
id: ADR-0050
title: Request and failure logs record metadata, never a submitted value
status: accepted
date: 2026-10-22
area: backend
related-prds: [PRD-0030]
---

# ADR-0050 (backfilled): Request and failure logs record metadata, never a submitted value

**Summary:** In the context of an API whose requests carry passwords, one-time codes and personal
financial data, facing the choice between rich request logging and logs that never contain what was
submitted, we decided that the request log records metadata — method, path, status, duration,
correlation identifier — and never a body or a header value, and that a logged failure carries the
exception and server-generated detail, never a submitted value, to achieve diagnostics without turning
logs into a copy of sensitive data, accepting that reproducing a failure sometimes needs the caller's
input rather than the server's record of it.

## Context and Problem Statement

Default request logging is designed to be helpful, which usually means it can include what was sent. In
this application what is sent includes sign-in credentials, verification codes, invitation passwords and
the amounts of a user's accounts.

Logs are read by more people, kept longer and stored in more places than the database. A value that
enters a log has left the controls that were designed around it.

Before [PRD-0030](../prd/PRD-0030-problem-details-consolidation.md), a hand-written middleware wrote
every request body whenever the server ran outside Production and recorded no request metadata, which is
close to option 2 below. PRD-0030 (requirements A2 to A6) replaced it with this decision, to keep
credentials and sensitive financial data out of logs.

This record covers the request log and the logging of failures. What the application's own log
statements may carry, call logging included, is governed separately
([PRD-0035](../prd/PRD-0035-telemetry-tracing-and-redaction.md)).

## Decision Drivers

- A log must never be a secondary store of credentials or financial data.
- A failure reported by a user must still be traceable to the request that caused it.
- The same rule must hold in every environment, not only in production.

## Considered Options

1. Log metadata only, with a correlation identifier linking the request across entries.
2. Log request and response bodies in development, and metadata in production.
3. Log bodies but redact known-sensitive fields.
4. Log nothing per request and rely on exceptions alone.

## Decision Outcome

Chosen option: 1. One entry per request is written when the response completes, carrying the method, the
path, the query string as supplied, the status, the duration and the correlation identifier — the caller's
when supplied. No configuration can enable logging of a request body or a header value: the fields are
chosen in code, not bound from configuration.

Failures are logged with structured properties and the exception, not with the request content, and the
correlation identifier is how a user's report and a log entry are joined.

### Consequences

- Good, because the request log cannot be made to contain a body or a header value in any environment.
- Bad, because a clean request log does not make the logs clean: the application's own log statements
  are outside this decision, and some write credentials today. PRD-0035 sets their rule.
- Good, because the correlation identifier joins the caller's report to the server's entries, including
  on requests that failed before a handler ran.
- Good, because the rule is uniform, so a developer cannot enable verbosity that leaks in development and
  forgets it is on.
- Bad, because request logging itself is enabled by a single logging level entry in `appsettings.json`,
  so removing it stops every entry with no sign at run time; `RequestLoggingFixture` asserts the entry is
  present.
- Bad, because reproducing a failure from logs alone is sometimes impossible without the input that caused
  it.
- Bad, because a debugging session has to rely on tests or on temporary instrumentation rather than on a
  setting.
- Bad, because the request log is less useful for understanding behaviour than a body log would be.

### Confirmation

- Test: `RequestLoggingFixture` asserts the request-log category is configured at `Information`, that the
  entry carries the method, the path and the status, that the health probes are not logged, and that a
  body sent in a request never appears; the correlation identifier in entries is asserted by
  `CorrelationIdLogEnricherFixture`.
- Test: `LoggerExtensionsFixture` asserts that a rejected application result is logged with its message
  and property name only, never the submitted value.
- Review: a request-log field beyond those chosen in code, or a failure log that writes a submitted
  value, is a defect.

## Pros and Cons of the Options

### Metadata only (chosen)

- Good: sensitive data has no path into logs at all.
- Bad: less diagnostic detail.

### Bodies in development only

- Good: full detail when it is most useful.
- Bad: the sensitive values are present in a developer's machine logs, and the switch is one setting away
  from production.

### Bodies with redaction

- Good: mostly full detail.
- Bad: redaction is a list that must be complete, and an unredacted field is discovered when it is already
  logged.

### No request logging

- Good: nothing can leak.
- Bad: no way to trace a user's report to a request, and no view of latency or error rates.

## More Information

- The correlation identifier is also returned to the caller so a report can quote it.
- What a failure body may contain is decided separately, by the same principle applied to responses.
- Traces and metrics, when added, are proposed in
  [ADR-0054](ADR-0054-server-telemetry-is-instrumented-with-dotnet-apis-and-exported-through-opentelemetry.md);
  they carry no value that a log may not.
