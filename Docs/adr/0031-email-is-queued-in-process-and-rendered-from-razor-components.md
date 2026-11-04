---
id: ADR-0031
title: Email is queued in process and rendered from Razor components
status: accepted
date: 2026-11-04
area: backend
---

# ADR-0031: Email is queued in process and rendered from Razor components

**Summary:** In the context of transactional email — verification codes, invitations, password resets,
reminders — facing the choice between a durable delivery mechanism and a channel inside the process, we
decided to queue messages in process, render their bodies from Razor components and send them from a
hosted worker, to achieve working email with no new infrastructure, accepting that a message is lost if
the process stops before it is sent.

## Context and Problem Statement

Sending mail synchronously in a request would make a sign-up wait on somebody else's SMTP server and fail
with it. So the send is moved off the request, which leaves the question of how durable the handover is.

The templates are content in code: the same Razor components used to build markup, rendered to a string
for the body. That keeps the body type-checked and shareable with anything else that renders the same
component.

## Decision Drivers

- A request must not wait for, or fail with, the mail server.
- Templates should be code, so a wrong property is a build error.
- No new infrastructure for a volume measured in messages per day.

## Considered Options

1. An in-process channel drained by a hosted worker, with bodies rendered from Razor components.
2. Send synchronously inside the request.
3. A database outbox drained by a worker, with retry and backoff.
4. An external transactional email service called directly from the request.

## Decision Outcome

Chosen option: 1. A queued message carries only the email type and its configuration; the subject and both
bodies are rendered from a Razor component at send time, when the worker dequeues it. The channel is
unbounded and lives as long as the process.

### Consequences

- Good, because the request completes without waiting for the mail server, and a mail failure cannot fail
  the request that caused it.
- Good, because templates are components: a renamed property breaks the build rather than producing a
  blank line in someone's inbox.
- Good, because no additional component exists to deploy, monitor or pay for.
- Bad, because a message queued and not yet sent is lost if the process stops, with no record that it
  existed and nothing to retry.
- Bad, because there is no backoff: a transient mail failure loses that message rather than delaying it.
- Bad, because the channel is unbounded, so a burst is absorbed by memory rather than by backpressure, and
  it declares single-writer semantics while request threads write to it concurrently.
- Bad, because delivery is not observable after the fact, so "did the invitation go out" is answered from
  logs rather than from state.

### Confirmation

- Test: the channel's fixtures assert a queued item is dequeued and that a send failure is logged and
  swallowed rather than thrown back to the request.
- Gap: no worker fixture asserts end-to-end delivery, and none asserts what happens to messages still
  queued when the host stops.
- Review: a new message is queued rather than sent inline; a request must never call the mail sender
  directly.

## Pros and Cons of the Options

### In-process queue with Razor bodies (chosen)

- Good: no infrastructure, and typed templates.
- Bad: at-most-once delivery, with no record of what was lost.

### Send inside the request

- Good: nothing to queue.
- Bad: the user's request now depends on the mail server's availability and latency.

### Database outbox

- Good: durable, retryable and observable, and it survives a restart.
- Bad: a table, a drainer with claim semantics, retry and backoff policies, and a decision about ordering —
  real work for a small volume.

### External email service from the request

- Good: delivery is somebody else's problem.
- Bad: the request still waits on it, and the credential and failure handling move into the request path.

## More Information

- This record is expected to be superseded by a durable outbox. The intended direction — a database
  outbox drained with claim and lease, with retry and same-day catch-up — is written up in
  `Docs/Future/PRD-019-email-outbox-durable-dispatch.md`; adopting it would supersede this record rather
  than edit it.
- Running the worker inside the API process is a separate decision with its own intended supersession.
