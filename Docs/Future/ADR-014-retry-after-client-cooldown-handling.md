# ADR 014 — Retry-After Client Cooldown Handling: Design & Current State

**Status:** Proposed — design direction recorded; not yet implemented
**Date:** 2026-09-02

## Context

[PRD 014](PRD-014-retry-after-client-cooldown-handling.md) defines **what** Retry-After client
handling must deliver — requirements, non-goals, and open questions. This ADR records **how**
the work would be approached: the verified current state of the server contract and the client
gap, the proposed design, and the decisions under consideration. No code changes have been
applied in this session.

## Companion documents

- [PRD 014 — Retry-After Client Cooldown Handling](PRD-014-retry-after-client-cooldown-handling.md)
  — requirements (what/why).
- Server rate limiting source: `Source/Server/Pot.AspNetCore/Concerns/RateLimiting/`.
- Client error pipeline source: `Source/Client/pot-react/src/api/`.

---

## 1. Current State (Verified)

### 1.1 Server contract — implemented

The server is the enforcement authority and already emits a well-formed 429 contract:

- Single chained policy applied to all feature endpoint groups via
  `RequireRateLimiting(RateLimiterPolicy.Chained)`.
- Anonymous: fixed window `15` req / `10` s, keyed `ip:{RemoteIpAddress}`.
- Authenticated: sliding window `50` req / `30` s (10 segments), keyed `user:{subject}`.
- No queueing (fail fast).
- Rejection payload: `429` + `Retry-After` header (seconds) + ProblemDetails body with error
  code `TooManyRequests` and detail `"Too many requests. Please wait and try again after {n}
seconds."`
- Integration-tested: `HeadersAndRateLimitFixture` asserts `429` and `Retry-After` > 0.

### 1.2 Client behaviour — the gap

- `429` is mapped to a typed `RateLimitedError` (`axiosInterceptors.ts`), but the
  `Retry-After` header and server guidance are discarded; the message is a static fallback.
- `RateLimitedError` carries no retry-after data, so no UI can render a countdown.
- No automatic retry of 429 occurs today, but this is not codified as an invariant.
- A client countdown precedent already exists for the application-layer OTP cooldown
  (`retryMinutes` → `OtpVerificationForm`), which is the natural pattern to align with.

---

## 2. Proposed Design

### D-1 — Server remains the sole authority

The client must never self-limit or throttle as a security measure. Client work is limited to
honouring and surfacing server-enforced limits.

### D-2 — Never auto-retry a 429

Automatic retries of rate-limited requests amplify load and contradict the fail-fast limiter
design. This becomes an explicit, tested invariant covering the general interceptor, React
Query retry options, and the auth interceptor (whose only retry today is after a successful
401-triggered refresh).

### D-3 — Canonical retry source

`Retry-After` (delta-seconds) is the canonical source of wait time. When it is absent, fall
back to seconds embedded in the server ProblemDetails; the current generic copy is only the
last-resort fallback.

### D-4 — Carry the value on the error

`RateLimitedError` gains retry-after data (for example `retryAfterSeconds`) captured in
`responseErrorHandler` before the error is wrapped in `FailResult`, so every consumer can
render from one source.

### D-5 — Align with the existing countdown pattern

Auth surfaces (login, signup, OTP verification, password reset) reuse the established
countdown/disabled-resubmit UX already built for the OTP `retryMinutes` flow, driven instead by
the HTTP 429 retry-after value. Non-auth surfaces get actionable copy and a countdown where the
presentation surface supports it.

---

## 3. Alternatives Considered

- **A1 — Client-enforced throttling/rate limiting.** Rejected: the browser is untrusted and
  trivially bypassed; it protects nobody and can only inconvenience legitimate users.
- **A2 — Auto-retry with backoff on 429.** Rejected: retrying rejected requests re-loads the
  limiter and works against the fail-fast design; backoff belongs only to transient failures
  (network, 5xx), not rate limits.
- **A3 — Keep status quo (generic server-side messaging only).** The current state; rejected
  as the target because the user gets no sense of when a retry is possible, driving repeat
  attempts that keep hitting the window.
- **A4 — Parse seconds from the ProblemDetails body only.** Possible, but `Retry-After` is the
  standard, canonical signal; the body is a secondary fallback.

---

## 4. Open Decisions

- Which surfaces show a countdown vs a generic message (PRD OQ-1).
- Shared cooldown hook/component location vs per-form duplication (PRD OQ-2).
- Exact-countdown vs "try again shortly" presentation for short windows (PRD OQ-3).
- Precedence when HTTP 429 and OTP `TooManyAttempts` co-occur (PRD OQ-4).
- Whether the proactive refresh timer pauses when the refresh endpoint is 429-limited
  (PRD OQ-5).
- Fallback copy when `Retry-After` is absent (PRD OQ-6).

---

## 5. Reference — Where Changes Would Land

- `Source/Client/pot-react/src/api/interceptors/axiosInterceptors.ts` — 429 case in
  `responseErrorHandler` (capture header, attach seconds).
- `Source/Client/pot-react/src/api/errors/apiErrors.ts` / `apiErrorResponse.ts` — extend
  `RateLimitedError` and message generation.
- `Source/Client/pot-react/src/features/auth/**` — cooldown UI aligned with
  `OtpVerificationForm.tsx`.
- No server changes; existing `HeadersAndRateLimitFixture` remains the server contract guard.
