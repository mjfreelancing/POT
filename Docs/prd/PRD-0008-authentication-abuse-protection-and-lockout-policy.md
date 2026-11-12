# Authentication Abuse Protection and Lockout Policy (Draft)

Feature ID: 0008
Date created: 2026-04-11
Scope: Server-side authentication abuse protections (planning only). No code changes applied in this session.
Audience: Future implementation agent(s), reviewers, security advisor(s), and maintainers.

## Purpose

Define a cohesive, post-session-architecture plan for authentication abuse controls, including failed-login tracking, lockout policy, auditability, and operational response.

## Status

- Stage: Proposed
- Code changes: Not started
- Decisions finalized: None yet
- Last updated: 2026-11-06 (per-replica rate-limit risk added)

## Important Note (Not Agreed)

Everything in this document is a draft for PRD refinement. None of the recommendations below are approved decisions.

## Problem / Concern

1. The current platform lacks per-account failed-login attempt tracking and lockout policy controls.
2. Existing rate limiting is valuable but does not fully address account-focused brute-force or credential-stuffing patterns.
3. Implementing lockout logic before Feature 002 session architecture may create rework as token/session persistence changes.
4. Product and security policy decisions (for example user messaging and unlock behavior) are not yet finalized.
5. **Known risk (recorded 2026-11-06): rate limits are per replica.** The HTTP rate limiter keeps its
   counters in each API process's memory. The API container app allows 1 to 10 replicas (ADR-0032,
   "Known scale-out risks"), so a caller whose requests are spread across N replicas gets up to N times
   the configured limit. This matters most for the one-time code endpoints (sign-up and password reset
   verification). There, the HTTP rate limit and the code's 15-minute expiry are currently the only
   brakes on guessing, because the three-attempt limit is not persisted between requests. Accepted for
   now while the API normally runs one replica.

## Relationship to Features 001 and 002

1. Feature 001 focuses on client storage/session scoping and should remain separate from server auth-abuse policy implementation.
2. Feature 002 defines the server session architecture and should provide the technical seams this feature depends on.
3. Feature 008 should start after 001 and 002 are complete (or after their critical architectural decisions are finalized and stable).

## Initial Recommendations (Draft)

1. Implement Feature 008 as one cohesive security feature after 001 and 002 to avoid partial rollout and rework.
2. Reuse the session model from 002 to ensure lockout and audit behavior are compatible with multi-session semantics.
3. Keep API error contracts consistent and avoid account-enumeration leakage in auth failure responses.
4. Introduce configurable policy options for thresholds and durations rather than hardcoding values.
5. Include operational controls (structured logs, monitoring, and support unlock workflows) in scope from day one.

## Proposed Scope (Draft)

1. Failed-login attempt tracking policy.
2. Temporary lockout policy and unlock semantics.
3. Authentication attempt audit trail for security/ops visibility.
4. Admin/support unlock capability (authorization-controlled).
5. Integration and regression tests for brute-force and lockout flows.
6. Operational runbook and alerting baseline.

## Candidate Work Phases (Draft)

1. Phase A: Confirm policy decisions and produce ADR.
2. Phase B: Data model and migration design.
3. Phase C: Auth flow updates (failed-attempt tracking, lockout enforcement, audit logging).
4. Phase D: API response policy and client-impact validation.
5. Phase E: Admin/support tooling and operational dashboards.
6. Phase F: Integration testing and rollout plan.

## Open Questions (PRD Refinement)

1. Should lockout be account-level only, session-level only, or hybrid?
2. What are the default policy thresholds (attempt count, window, lockout duration)?
3. Should lockout messaging remain fully generic for security, or reveal limited user guidance for UX?
4. Should unlock be automatic by time only, admin-assisted, user self-service, or a combination?
5. Should repeated lockouts trigger progressive backoff, fixed duration, or adaptive risk policy?
6. Should suspicious activity be keyed primarily by account, IP, device fingerprint, or weighted combination?
7. How should credential-stuffing detection be handled for distributed low-rate attacks?
8. What is the required audit retention period and purge policy for auth-attempt records?
9. Which alerts are mandatory for production readiness versus optional enhancements?
10. What rollout/feature-flag strategy should be used to reduce false-positive lockouts during initial deployment?
11. What contract guarantees are required so client behavior remains stable under new auth-abuse states?
12. Should password reset or password change clear lockout state immediately, conditionally, or never?
13. Must attempt counting and rate limits hold across replicas, for example by keeping counters in the
    database? Or is a per-replica limit acceptable while the API normally runs one replica, possibly with
    the maximum pinned lower?

## Dependencies

1. Feature 002 decisions on session model, logout semantics, and token rotation behavior.
2. Existing authorization model for admin/support operations.
3. Observability standards for logging and alerting.

## Out of Scope (Initial)

1. Full fraud/risk engine with advanced device intelligence.
2. Broad redesign of signup or password reset UX beyond abuse-protection requirements.
3. Non-auth API abuse protections unrelated to login/refresh endpoints.

## Related Documents

- Future index: [Docs/prd/README.md](README.md)
- Client and session companion plan: [Docs/prd/PRD-0001-per-session-login-support.md](PRD-0001-per-session-login-support.md)
- Session architecture baseline: [Docs/prd/PRD-0002-server-login-logout-session-architecture.md](PRD-0002-server-login-logout-session-architecture.md)
- Current auth reference: [Docs/AUTHENTICATION.md](../AUTHENTICATION.md)
