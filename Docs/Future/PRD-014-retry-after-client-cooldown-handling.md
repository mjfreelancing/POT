# Retry-After Client Cooldown Handling PRD

**Status:** Proposed (discovery draft)
**Priority:** Medium
**Last Updated:** 2026-09-02
**Feature ID:** 014
**Scope:** Client-side (React) graceful handling of HTTP 429 responses. No server-side changes required.
**Audience:** Future implementation agent(s), reviewers, and maintainers.

## Purpose

Define how the React client should honour the server's existing HTTP 429 rate-limit contract — reading the `Retry-After` header and driving countdown-aware messaging and cooldown — while keeping the server as the sole enforcement authority.

## Problem Statement

The server already returns `429 Too Many Requests` with a `Retry-After` header and a ProblemDetails body when a rate-limit window is exceeded. The client converts that 429 into a typed `RateLimitedError`, but currently:

1. Throws away the `Retry-After` header and the server's "try again after N seconds" guidance.
2. Shows static generic copy ("Too many requests. Please wait a moment and try again.") with no countdown.
3. Gives the user no signal for when it is safe to retry, so a blocked login/signup flow produces repeated manual retries that keep hitting the same limit.
4. Has no explicit, documented invariant that 429 responses are never auto-retried.

## Current Behaviour

### Server contract (already implemented — no change proposed)

- ASP.NET Core rate limiting is registered via `AddPotRateLimiting()` and applied to all feature endpoint groups through `RequireRateLimiting(RateLimiterPolicy.Chained)`.
- A single chained policy (`RateLimiterPolicy.CreateChainedPolicy`) partitions as:
  - **Anonymous**: fixed window, `15` requests / `10` seconds, keyed by `ip:{RemoteIpAddress}`.
  - **Authenticated**: sliding window, `50` requests / `30` seconds (10 segments), keyed by `user:{subject}`.
- No request queue — requests are rejected immediately when the limit is reached (fail fast).
- On rejection (`RateLimiterOptionsSetup.OnRejected`):
  - Status code `429`.
  - `Retry-After` response header set to seconds when the lease exposes `RetryAfter` metadata.
  - ProblemDetails body produced by `ApiDetailErrorFactory.CreateTooManyRequests(...)`: error code `TooManyRequests`, detail `"Too many requests. Please wait and try again after {n} seconds."`
- Integration coverage already asserts 429 + `Retry-After` > 0 (`HeadersAndRateLimitFixture`).
- Separate application-layer OTP cooldown (`OtpService`, 3 attempts / 5 minutes) returns a `retryMinutes` value that the client already renders as a countdown — distinct from the HTTP 429 middleware path.

### Client behaviour today (the gap)

- `responseErrorHandler` maps status `429` to `new RateLimitedError(getRateLimitedMessage(...))`.
- `getRateLimitedMessage` returns a static string and ignores the response body; `Retry-After` is not read anywhere.
- 429 responses are not auto-retried today (good), but no invariant codifies it.
- A countdown precedent already exists for the OTP application-layer case: `OtpVerificationForm`, `SignupDialog`, and `PasswordResetDialog` render a resend countdown from `retryMinutes` returned by `TooManyAttempts`.

## Proposed Solution

Candidate requirements; nothing implemented yet.

- **R1 — Capture retry guidance.** In `responseErrorHandler`, for status `429`, read the `Retry-After` header (delta-seconds) and attach the seconds to `RateLimitedError` (for example `retryAfterSeconds`). Fall back to parsing the server ProblemDetails guidance when the header is absent.
- **R2 — Never auto-retry a 429 (invariant).** Keep 429 out of all automatic retry paths: the general interceptor, React Query retry options, and the auth interceptor (which today retries only after a successful 401-triggered refresh). Add tests that lock this in.
- **R3 — Cooldown on auth surfaces.** Login, signup, OTP verification, and password-reset surfaces show the remaining wait and disable resubmission during the cooldown, consistent with the existing `retryMinutes` countdown pattern but sourced from `RateLimitedError.retryAfterSeconds`.
- **R4 — Graceful global fallback.** Non-auth endpoints surface the rate-limit error with actionable copy (and a countdown where the presentation surface supports it); otherwise they retain a clear, non-dead-end message.
- **R5 — Message accuracy.** When server guidance is available, show it (for example "try again in N seconds"); preserve the current generic copy purely as a fallback so there is no client-side contract break.

## Non-Goals

- No client-enforced rate limiting or request throttling as a security control — the client is untrusted and trivially bypassable.
- No retry/backoff logic for rate-limited requests.
- No server-side changes, including no changes to rate-limit policies or configuration values.
- No auth lockout/abuse policy work — that remains with [PRD 008](PRD-008-authentication-abuse-protection-and-lockout-policy.md).

## Out of Scope (Initial)

- Generic client request queueing/coalescing beyond the existing 401-refresh queue.
- Reworking the server rate-limit rejection payload or header semantics.

## Open Questions (Discovery)

- **OQ-1**: Which surfaces must render a countdown versus a generic message — auth forms only, or a global error surface too?
- **OQ-2**: Should the cooldown be a shared hook/component (consult shared helpers before introducing new code, per repo convention) or implemented per form?
- **OQ-3**: Exact countdown vs "try again shortly" — anonymous fixed windows reset within ~10s and authenticated within ~30s, so countdowns are short; confirm the desired presentation.
- **OQ-4**: Message/interaction precedence when both HTTP 429 and the OTP `TooManyAttempts` (with `retryMinutes`) can apply to the same flow.
- **OQ-5**: Whether the proactive refresh timer should pause/suppress when the refresh endpoint itself is 429-limited.
- **OQ-6**: Fallback behaviour when `Retry-After` is absent from a rejection (R1 fallback path) — confirm the server can actually omit it and what the client should show.

## Validation / Test Considerations

- **Client unit tests (interceptor):** a 429 response maps `Retry-After` seconds onto `RateLimitedError`; a missing header falls back correctly; 429 is never auto-retried.
- **Component tests:** cooldown countdown copy and disabled resubmission render as specified, matching the OTP countdown pattern.
- **Server integration tests:** the existing `HeadersAndRateLimitFixture` already covers 429 + `Retry-After`; extend only if a new client-side contract requires it.
- **E2E (optional):** Playwright config already overrides `RateLimiting` limits for test runs, so a 429-path E2E is feasible but not required.

## Related Documents

- Future index: [Docs/Future/README.md](README.md)
- Companion design record: [ADR 014 — Retry-After Client Cooldown Handling (Design & Current State)](ADR-014-retry-after-client-cooldown-handling.md)
- Server rate limiting: `Source/Server/Pot.AspNetCore/Concerns/RateLimiting/RateLimiterOptionsSetup.cs`, `RateLimiterPolicy.cs`, `RateLimiterDefaults.cs`
- Server integration coverage: `Source/Server/Pot.AspNetCore.Integration.Tests/Security/HeadersAndRateLimitFixture.cs`
- Client error pipeline: `Source/Client/pot-react/src/api/interceptors/axiosInterceptors.ts`, `src/api/errors/apiErrors.ts`, `src/api/errors/apiErrorResponse.ts`
- Client countdown precedent: `Source/Client/pot-react/src/features/auth/shared/components/OtpVerificationForm.tsx`
- Server auth-abuse plan (separate concern): [PRD 008](PRD-008-authentication-abuse-protection-and-lockout-policy.md)
