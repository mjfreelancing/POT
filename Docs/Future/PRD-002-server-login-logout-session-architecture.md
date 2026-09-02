# Server Login/Logout Session Architecture Placeholder

Feature ID: 002
Date created: 2026-04-02
Scope: Server-side auth/session planning only. No code changes applied in this session.
Audience: Future implementation agent(s), reviewers, and maintainers.

## Purpose

Define how server login/logout/refresh should support multi-session behavior (for example, independent device sessions) without breaking existing security controls.

## Current State Summary (Verified)

Current server behavior is effectively single refresh-token session per user.

1. User entity stores one refresh token and one refresh expiry.

- [Source/Server/Pot.Data/Entities/UserEntity.cs](../../Source/Server/Pot.Data/Entities/UserEntity.cs#L38)
- [Source/Server/Pot.Data/Entities/UserEntity.cs](../../Source/Server/Pot.Data/Entities/UserEntity.cs#L40)

2. Login/refresh overwrites that single token.

- [Source/Server/Pot.AspNetCore/Concerns/Auth/AuthService.cs](../../Source/Server/Pot.AspNetCore/Concerns/Auth/AuthService.cs#L250)

3. Refresh flow validates incoming token against that single stored value.

- [Source/Server/Pot.AspNetCore/Concerns/Auth/AuthService.cs](../../Source/Server/Pot.AspNetCore/Concerns/Auth/AuthService.cs#L162)
- [Source/Server/Pot.AspNetCore/Concerns/Auth/AuthService.cs](../../Source/Server/Pot.AspNetCore/Concerns/Auth/AuthService.cs#L192)

4. Refresh token is issued/rotated via HTTP-only cookie endpoints.

- Login endpoint sets cookie: [Source/Server/Pot.AspNetCore/Features/Auth/Login/Handler.cs](../../Source/Server/Pot.AspNetCore/Features/Auth/Login/Handler.cs#L42)
- Refresh endpoint sets cookie: [Source/Server/Pot.AspNetCore/Features/Auth/Refresh/Handler.cs](../../Source/Server/Pot.AspNetCore/Features/Auth/Refresh/Handler.cs#L48)
- Logout clears cookie: [Source/Server/Pot.AspNetCore/Features/Auth/Logout/Handler.cs](../../Source/Server/Pot.AspNetCore/Features/Auth/Logout/Handler.cs#L29)

5. Access token revocation guard uses TokenVersion claim checks on each request.

- [Source/Server/Pot.AspNetCore/Concerns/Auth/Configuration/JwtBearerEventsSetup.cs](../../Source/Server/Pot.AspNetCore/Concerns/Auth/Configuration/JwtBearerEventsSetup.cs#L51)
- [Source/Server/Pot.AspNetCore/Concerns/Auth/Configuration/JwtBearerEventsSetup.cs](../../Source/Server/Pot.AspNetCore/Concerns/Auth/Configuration/JwtBearerEventsSetup.cs#L83)

## Known User-Visible Symptom

When a user logs in or refreshes on device/browser B, device/browser A can be forced out later because A's refresh token no longer matches the single stored token.

## Relationship To Feature 001

- Feature 001 addresses client-side storage policies and does not change server auth behavior in its first phase.
- This Feature 002 document is the server-side companion plan.
- Related: [Docs/Future/PRD-001-per-session-login-support.md](PRD-001-per-session-login-support.md)

## PRD Status

- Stage: Implemented
- Code changes: Complete (delivered together with PRD 001 as a single rollout)
- Decisions finalized: AuthSession row per login; per-session logout does NOT increment TokenVersion; password change IS global revoke (TokenVersion++); legacy RefreshToken/RefreshTokenExpiryUtc columns removed
- Last updated: 2026-05-03

## Draft Security Coordination Notes (Not Agreed)

The following notes are captured for PRD refinement only. They are not approved decisions and should not be treated as implementation commitments.

### Problem / Concern

1. Current server auth has no per-account failed-login tracking or lockout policy.
2. Existing API rate limiting reduces abuse pressure but does not provide account-level brute-force protection.
3. Implementing lockout now against the current single-refresh-token model may create avoidable rework when session-backed refresh architecture is introduced.

### Initial Recommendations (Draft)

1. Keep Feature 002 focused on session architecture and explicitly define integration seams needed by auth-abuse controls.
2. Ensure the AuthSession model can support future security metadata and audit linkage without reworking core session tables.
3. Defer lockout, failed-attempt policy, and operational abuse controls to a dedicated follow-on feature (Feature 008) after 001 and 002 are complete.
4. Preserve stable auth response contracts so future lockout/throttling behavior can be added without breaking client flows.

## Design Goals

1. Support independent concurrent sessions (or a consciously chosen alternative policy).
2. Preserve strong logout and password-change revocation semantics.
3. Avoid auth regressions and minimize migration risk.
4. Keep API contracts stable where possible.

## Non-Goals (Initial)

1. No broad redesign of unrelated auth features (signup/password reset/approvals).
2. No removal of TokenVersion-based global revoke controls.
3. No large cross-cutting refactors outside auth/session flow.

## Open Product Decisions (Must Be Confirmed)

1. Session model policy:

- Option A: Single active session per user (current)
- Option B: Multi-device independent sessions
- Option C: Limited concurrent sessions (max N)

2. Logout behavior:

- Current-session logout only by default?
- Is explicit logout-all required in first release?

3. Password-change policy:

- Continue global session invalidation? (recommended yes)

4. Session visibility:

- Is a user-facing session management page required now or later?

## Open Questions (PRD Refinement)

1. Should session policy in 002 be multi-session by default, or bounded concurrent sessions (max N) from day one?
2. Should logout semantics include both current-session and logout-all in initial rollout, or phase logout-all later?
3. Should AuthSession include device/user-agent/IP metadata in first release, or only core token/session fields?
4. Should refresh token rotation include explicit reuse-detection handling in first release or in a follow-on phase?
5. Should 002 add minimal audit hooks now (for session events only), or leave all auth-abuse audit to Feature 008?
6. Should password-change behavior invalidate all sessions immediately (existing TokenVersion approach) or allow future policy modes?
7. What migration strategy is preferred for users with existing single-token state during rollout (hard cutover vs compatibility window)?
8. Which observability signals are mandatory for 002 completion versus deferred to 008?

## Proposed Technical Direction (Baseline)

1. Introduce a session-backed refresh model.

- Add AuthSession persistence (one row per active session/device context).
- Store refresh token as hash, not plaintext.

2. Refresh rotation becomes per-session.

- Rotating token on session A does not invalidate session B.
- Reuse detection for rotated/old token attempts.

3. Keep TokenVersion for global revocations.

- Useful for password change and optional logout-all semantics.

4. Maintain HTTP-only refresh cookie pattern.

- Reuse current cookie handling conventions unless policy requires changes.

## Suggested Data Model (Draft)

AuthSession (proposed fields):

- SessionRowId (Guid)
- UserRowId (Guid)
- RefreshTokenHash (string)
- ExpiresUtc (DateTime)
- CreatedUtc (DateTime)
- LastSeenUtc (DateTime)
- RevokedUtc (DateTime?)
- Optional metadata: UserAgent, IpAddress, DeviceLabel

## Suggested Server Work Phases

1. Phase A: ADR + schema design.
2. Phase B: Migration and repository support.
3. Phase C: Login creates session records.
4. Phase D: Refresh validates/rotates per session.
5. Phase E: Logout current session revokes only current record.
6. Phase F: Optional logout-all endpoint/behavior.
7. Phase G: Observability and audit logs.

## Test Plan Baseline

Integration tests to add/expand:

1. Two independent sessions remain valid across refresh cycles.
2. Logout current session does not revoke other sessions.
3. Logout-all revokes all sessions (if implemented).
4. Reuse of rotated token is rejected.
5. Expired/revoked session token rejected.
6. Password change invalidates sessions per selected policy.

Targeted execution convention (from repo guidance):

- Run integration tests from Source/Server using project-level filter:
- dotnet test Pot.AspNetCore.Integration.Tests/Pot.AspNetCore.Integration.Tests.csproj --filter "FullyQualifiedName~<FixtureOrTestName>"

## Risks And Mitigations

1. Token rotation race conditions.

- Mitigation: transactional update path and deterministic conflict handling.

2. Backward compatibility during migration from single-token model.

- Mitigation: staged rollout and temporary compatibility logic.

3. Cookie/env config mismatches.

- Mitigation: validate local/dev/prod cookie config matrix and integration coverage.

## Handoff Checklist For Next Agent

1. Re-read this document and [Docs/AUTHENTICATION.md](../AUTHENTICATION.md).
2. Confirm product decisions listed under Open Product Decisions.
3. Create ADR for session policy and migration strategy.
4. Produce implementation plan broken into safe PR slices.
5. Start with schema and tests before endpoint behavior changes.
6. Keep Feature 001 and Feature 002 changes coordinated but independently releasable.

## Related Documents

- Future index: [Docs/Future/README.md](README.md)
- Client-side companion plan: [Docs/Future/PRD-001-per-session-login-support.md](PRD-001-per-session-login-support.md)
- Auth abuse controls follow-on: [Docs/Future/PRD-008-authentication-abuse-protection-and-lockout-policy.md](PRD-008-authentication-abuse-protection-and-lockout-policy.md)
- Current auth reference: [Docs/AUTHENTICATION.md](../AUTHENTICATION.md)
