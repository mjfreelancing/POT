---
id: ADR-0049
title: The client never retries a rate-limited request
status: accepted
date: 2026-11-06
area: frontend
supersedes: [ADR-0002]
---

# ADR-0049: The client never retries a rate-limited request

**Summary:** In the context of a server that rejects a throttled caller with `429` and a client whose query
library retries failed reads by default, facing a rule that holds today only because of how the API hooks
return failures, we decided that nothing in the client retries a `429` automatically, to achieve a rate
limit that keeps working under load, accepting that the user must act again to recover and that the rule
is held by review until a test asserts it.

## Context and Problem Statement

A `429` is the server's decision that this caller may not proceed yet
([ADR-0012](0012-one-chained-rate-limit-policy-partitions-by-caller.md)). It is not a transient fault.
Retrying on a timer contradicts that decision, and several clients doing so would replace a rate limit
with a synchronised retry storm against the endpoint the server is protecting.

The client does not retry a `429` today, but nothing makes it so on purpose:

- `Source/Client/pot-react/src/api/interceptors/axiosInterceptors.ts` maps a `429` to a `RateLimitedError`
  and stops. The only retry in `authInterceptor.ts` is of a request that failed with `401`, after a
  successful refresh.
- TanStack Query retries a failed query three times by default, and the `QueryClient` in
  `Source/Client/pot-react/src/main.tsx` configures no `retry`.
- Those retries never fire only because `useGet` resolves every failure as a `FailResult` value instead of
  throwing (`performOperation` in `src/api/hooks/useApi.ts`; see
  [ADR-0038](0038-server-state-lives-in-a-query-cache-over-one-http-client.md) and
  [ADR-0039](0039-api-hooks-return-results-and-one-interceptor-normalises-failures.md)). A query function
  that threw would start retrying every `429`, and no test would notice.

The rule was first written as D-2 of [ADR-0002](0002-retry-after-client-cooldown-handling.md), a proposed
design for PRD-014, and restated in [ADR-0047](0047-a-rate-limited-caller-is-told-when-to-retry.md), which
stays proposed with that PRD. The rule does not depend on the PRD: it describes the client as it is, so it
is recorded here on its own and is binding now.

## Decision Drivers

- The server is the only authority on when a throttled caller may proceed.
- A retry must not put load back on the limiter that rejected it.
- A rule that holds by accident has to be written down before a refactor removes the accident.

## Considered Options

1. Never retry a `429` automatically; the user acts again (the current behaviour, made a rule).
2. Retry automatically after the server's `Retry-After` delay.
3. Retry with exponential backoff, as for a transient failure.
4. Leave retry behaviour to each hook's or query's own options.

## Decision Outcome

Chosen option: 1, because a `429` is an answer, not a fault.

- **Option 2** obeys the server's timing but hides the rejection from a user who sees nothing happen.
  An authenticated rejection names no delay to wait for anyway (ADR-0012).
- **Option 3** ignores the server's timing and multiplies load exactly when the server is protecting
  itself. Backoff is for transient failures, such as a network error or a `5xx`, not for a rate limit.
- **Option 4** is how a `429` retry would arrive unnoticed, because the library default is three retries.

The rule covers every path that could resend a request: the axios interceptors, the `retry` option of a
query or a mutation, the `QueryClient` defaults, and any loop written by hand. What the user is shown
instead is [ADR-0047](0047-a-rate-limited-caller-is-told-when-to-retry.md).

### Consequences

- Good, because the limiter's rejection is final for that request, so a burst cannot amplify itself.
- Good, because the rule is stated where a reviewer can cite it, rather than being a side effect of how
  the hooks return failures.
- Bad, because recovery always needs a second action from the user, even for a one-second wait.
- Bad, because the rule is held by review: nothing configures `retry` for a `429`, and no test asserts
  that a `429` is not retried. A change that makes the hooks throw, so that the query library sees
  failures, must bring a `retry` predicate that excludes `429` with it.

### Confirmation

- Instruction: `.github/instructions/client.instructions.md` (State and data flow) states the rule and
  cites this record.
- Test: `Source/Client/pot-react/tests/api/interceptors/axiosInterceptors.test.ts` asserts that a `429`
  becomes a `RateLimitedError`. It does not yet assert that no second request is sent.
- Review: a `retry` option, an interceptor branch or a loop that can resend a request answered with `429`
  is rejected.

## More Information

- The server policy that produces a `429`:
  [ADR-0012](0012-one-chained-rate-limit-policy-partitions-by-caller.md).
- What the user is shown when a `429` arrives: [ADR-0047](0047-a-rate-limited-caller-is-told-when-to-retry.md),
  proposed with [PRD-014](../Future/PRD-014-retry-after-client-cooldown-handling.md). PRD-014's test plan
  includes an interceptor test that a `429` is never retried. Once written, that test is this record's
  confirmation.
