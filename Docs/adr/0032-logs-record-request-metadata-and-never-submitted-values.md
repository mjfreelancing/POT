---
id: ADR-0032
title: Logs record request metadata and never submitted values
status: accepted
date: 2026-11-04
area: backend
---

# ADR-0032: Logs record request metadata and never submitted values

**Summary:** In the context of an API whose requests carry passwords, one-time codes and personal
financial data, facing the choice between rich request logging and logs that never contain what was
submitted, we decided to log metadata — method, path, status, duration, correlation identifier — and
never a body, a header value or a field from a payload, to achieve diagnostics without turning logs into
a copy of sensitive data, accepting that reproducing a failure sometimes needs the caller's input rather
than the server's record of it.

## Context and Problem Statement

Default request logging is designed to be helpful, which usually means it can include what was sent. In
this application what is sent includes sign-in credentials, verification codes, invitation passwords and
the amounts of a user's accounts.

Logs are read by more people, kept longer and stored in more places than the database. A value that
enters a log has left the controls that were designed around it.

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
path, the status, the duration and the correlation identifier — the caller's when supplied. There is no
configuration in which a body or a header value is logged.

Failures are logged with structured properties and the exception, not with the request content, and the
correlation identifier is how a user's report and a log entry are joined.

### Consequences

- Good, because no log, in any environment, can contain a credential or an account amount.
- Good, because the correlation identifier joins the caller's report to the server's entries, including
  on requests that failed before a handler ran.
- Good, because the rule is uniform, so a developer cannot enable verbosity that leaks in development and
  forgets it is on.
- Bad, because reproducing a failure from logs alone is sometimes impossible without the input that caused
  it.
- Bad, because a debugging session has to rely on tests or on temporary instrumentation rather than on a
  setting.
- Bad, because the request log is less useful for understanding behaviour than a body log would be.

### Confirmation

- Test: `RequestLoggingFixture` asserts the entry carries metadata and the correlation identifier, and that
  health probes are not logged.
- Review: a log statement that includes a request or response value is a defect, and a log level that
  would enable one is not a configuration choice.

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
