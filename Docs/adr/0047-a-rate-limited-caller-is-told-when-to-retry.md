---
id: ADR-0047
title: A rate-limited caller is told when to retry
status: proposed
date: 2025-11-08
area: frontend
related-prds: [PRD-014]
supersedes: [ADR-0002]
---

# ADR-0047 (backfilled): A rate-limited caller is told when to retry

**Summary:** In the context of a server that answers a throttled request with `429` and, where its limiter
knows the wait, a `Retry-After` value, facing a user who is told "too many requests" with no sense of how
long to wait, we decided that the client surfaces the server's guidance as a countdown and holds the action
until it elapses, to achieve behaviour that honours the server's authority without hiding it, accepting
that the user has to wait and act again.

## Context and Problem Statement

For an anonymous caller the server says how long to wait, in `Retry-After` and in the `detail`. An
authenticated rejection names no wait, because the sliding window exposes none
([ADR-0012](0012-one-chained-rate-limit-policy-partitions-by-caller.md)). The client cannot compute the
wait itself: the server owns the limit, the window and the partition, so only the server knows when the
caller may try again.

That nothing in the client retries a `429` automatically is a separate decision, already accepted as
[ADR-0049](0049-the-client-never-retries-a-rate-limited-request.md). This record decides only what the user
is shown instead.

This record restates the direction of ADR-0002, which it supersedes together with ADR-0049, for PRD-014,
still a discovery draft. ADR-0002's choice of `Retry-After` as the canonical source of the wait (D-3) is
left open here.

## Decision Drivers

- The server is the only authority on when the caller may proceed.
- A user who is told "too many requests" needs to know how long, or they will retry immediately.
- Recovery is the user's action, because the client never retries a `429` itself (ADR-0049).

## Considered Options

1. Read the server's wait, show a countdown, and hold the action until it elapses.
2. Show a generic "try again later" message with no timing (the current behaviour).
3. Retry automatically after the indicated delay, showing nothing.

## Decision Outcome

Chosen option: 1. A `429` becomes a rate-limited failure, and the interface shows how long remains and
holds the action until it elapses. Option 3 is excluded by ADR-0049.

The same rule applies to the authentication flows that carry their own guidance: the one-time code policy
reports a wait in minutes on a successful response, not a `429`, and `OtpVerificationForm` already counts
it down. What the interface shows when a `429` names no wait — every authenticated rejection today — is
not decided here (PRD-014 OQ-6).

This record states the intended behaviour. The transport does not yet read the server's guidance: a `429`
is mapped to a rate-limited failure with fixed copy, and no countdown exists for a `429`, so reading the
wait in the interceptor and showing it are what would make the decision true.

### Consequences

- Good, because the user knows how long to wait, which removes the reason to try again immediately.
- Bad, because until the guidance is read, a throttled user is told to wait without being told how long,
  which is the behaviour that invites immediate retries.
- Bad, because where a wait is given it is in two places — the exposed header and the problem detail —
  so a future implementation has to choose one, and an authenticated rejection carries neither, so it
  must also handle no wait at all.
- Bad, because the user must act again after the wait; there is no silent recovery.
- Bad, because the displayed countdown is client-side arithmetic on a server value, so a long wait crossing
  a clock change can be shown inaccurately.
- Bad, because the header must be readable by script, which is why it is in the exposed set, and an omitted
  exposure would turn every `429` into an untimed message.

### Confirmation

- Test: `tests/api/interceptors/axiosInterceptors.test.ts` asserts a `429` produces a `RateLimitedError`
  with the fixed copy whatever the body. Nothing yet asserts a wait, because none is read.
- Test: `HeadersAndRateLimitFixture` asserts what the client would read: `Retry-After` exposed on both
  rejections, present with a matching timed `detail` on the anonymous one, and absent with an untimed
  `detail` on the authenticated one; `CorsOptionsSetupFixture` pins the exposed-header list.
- Review: when the countdown is implemented, a `429` must show it rather than a generic message.

## Pros and Cons of the Options

### Show the wait (chosen)

- Good: honours the server and informs the user.
- Bad: recovery needs a second action.

### Generic message

- Good: nothing to implement.
- Bad: invites immediate retries, which is the behaviour the limit exists to stop.

### Retry after the indicated delay

- Good: transparent when the wait is short.
- Bad: a user watching the screen sees nothing happen, and the retry is indistinguishable from the failure
  being ignored. It is also excluded by ADR-0049.

## More Information

- That the client never retries a `429`:
  [ADR-0049](0049-the-client-never-retries-a-rate-limited-request.md).
- The server's chained policy that produces `429`:
  [ADR-0012](0012-one-chained-rate-limit-policy-partitions-by-caller.md).
- Retry-After being readable by script is a consequence of the cross-origin policy, which is separate.
