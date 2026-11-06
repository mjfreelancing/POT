---
id: ADR-0034
title: One chained rate-limit policy partitions by caller
status: accepted
date: 2025-11-08
area: backend
---

# ADR-0034 (backfilled): One chained rate-limit policy partitions by caller

**Summary:** In the context of an internet-facing API whose sign-in route is the obvious target for
credential guessing, facing the need to slow abuse without penalising a legitimate user who mistyped a
password, we decided on a single chained policy that partitions by caller address when anonymous and
by user when signed in, using a short fixed window for anonymous traffic and a sliding window for
authenticated traffic, to achieve cheap brute-force resistance, accepting that the protection is
temporary and limits the rate of guessing, not the number of attempts per account.

## Context and Problem Statement

Two populations share the pipeline. Anonymous callers include sign-in, sign-up, password reset and
refresh — the routes worth attacking. Signed-in callers are already known and are trusted more.

An account lockout is the traditional answer to guessing. It has not been built: temporary throttling is
enough for now, and a lockout is deferred until a requirement justifies its added complexity. A lockout's
known costs are set out under the options below.

## Decision Drivers

- Guessing must be slowed at the caller, not at the account.
- The window must be short enough that a throttled request fails fast rather than making the user wait.
- The production limits should be fixed in code, while a test run can raise them without a code change.

## Considered Options

1. One chained policy: fixed window for anonymous callers, sliding window for authenticated users.
2. Per-endpoint limits, tuned per route.
3. Account lockout after repeated failures.
4. A single global limit regardless of who is calling.

## Decision Outcome

Chosen option: 1. Every endpoint group requires the same named policy, which chooses a partition:
anonymous callers are limited per address in a fixed window, and authenticated callers per user in a
sliding window, with no queueing in either. Despite the name `ChainedPolicy`, no limiters are chained:
each request is counted by exactly one partition. Rejection is `429` with a Problem Details body. The
limits are constants in code, not deployment settings.

### Consequences

- Good, because an attacker's address is throttled while the account owner signing in normally is not.
- Good, because signed-in users get a smoother sliding window, which is fair to normal interactive use.
- Good, because with no queue a rejected request returns at once, and the short anonymous window means
  the wait it names is at most ten seconds.
- Bad, because an authenticated rejection names no wait, so a signed-in user is not told how long to
  wait, which can be up to the thirty-second window.
- Bad, because a caller sharing an address — an office, a mobile network — is throttled as one, which
  is a real limit of address-based partitioning.
- Bad, because anonymous partitions depend on the caller address being the real one, so the forwarded
  header trust is part of this control working at all.
- Bad, because nothing prevents repeated guessing spread across many addresses; the control is on rate,
  not on the number of attempts per account.

### Confirmation

- Test: `HeadersAndRateLimitFixture` exercises the chained policy, asserts the anonymous rejection
  carries `Retry-After`, and asserts the authenticated one does not.
- Review: the permit limits and windows are constants, not settings; the configuration section exists so
  a test run can raise them, and must not appear in a deployed environment's settings.
- Review: a new endpoint group must opt into the same policy; a route that omits it is unthrottled, and
  that is a defect rather than a tuning choice.

## Pros and Cons of the Options

### One chained policy (chosen)

- Good: one place to reason about, and it adapts to whether the caller is known.
- Bad: cannot express a route-specific tighter limit without leaving the policy.

### Per-endpoint limits

- Good: the sign-in route could be far stricter than the rest.
- Bad: every route becomes a tuning decision, and the differences are invisible in review.

### Account lockout

- Good: bounds attempts per account regardless of address.
- Bad: hands an attacker a denial-of-service against any known user, and needs a recovery flow that is
  itself abusable.

### One global limit

- Good: trivial to reason about.
- Bad: treats a signed-in user's normal use the same as an anonymous flood.

## More Information

- The client's handling of `429` and `Retry-After` is a separate decision: the client never retries a `429`
  ([ADR-0053](ADR-0053-the-client-never-retries-a-rate-limited-request.md)), and what the user is shown is
  a requirement of [PRD-0014](../Future/PRD-0014-retry-after-client-cooldown-handling.md).
- An account lockout policy could be added later without changing this one; it is deferred rather than
  rejected. Until then there is no lockout of an account, and the temporary rejection is the whole of the
  response to guessing.
- The limits as built: anonymous callers get fifteen permits in a ten-second fixed window per address,
  and authenticated callers fifty permits in a thirty-second sliding window per user, split into ten
  segments.
- `Retry-After` is written, and the `detail` names the wait, only where the limiter exposes the retry
  metadata, which the fixed window does and the sliding window does not, so an authenticated rejection
  carries neither the header nor a timed message.
