# Per-Session Login Support Plan

Feature ID: 001
Date created: 2026-04-02
Scope: Planning and design only. No code changes applied in this session.
Audience: Future implementation agent(s), reviewers, and maintainers.

## Objective

Resolve two linked but distinct problems:

1. Client storage is not session-aware.

- Local browser storage currently uses fixed keys and can leak user-specific UI/filter state between users sharing one browser profile.

2. Server auth sessions are effectively single-session per user.

- New login/refresh on one device can invalidate refresh behavior on another device because only one refresh token is stored per user.

## PRD Status

- Stage: Planning
- Code changes: Not started
- Requirement interview mode: Completed for client storage (D001-D007 confirmed)
- Last updated: 2026-04-02

## Product Requirements (Draft)

### Functional Requirements

1. Preserve user preferences across logout/login when those preferences are classified as persistent.
2. Prevent preference/state leakage between:

- different users on the same browser profile
- different app environments (for example production vs docker)
- different browser tabs when state is classified as tab-scoped

3. Allow per-setting storage policy selection (persistent vs tab-session vs server-backed future option).
4. Support deterministic rehydration order on login/app load.
5. Keep backward compatibility by migrating from existing keys.

### Non-Functional Requirements

1. Keep storage behavior predictable and debuggable with explicit key namespacing.
2. Avoid introducing authentication regressions while session architecture evolves.
3. Ensure changes can be rolled out safely behind feature flags where needed.

## Current Browser Storage Inventory (Verified)

The following settings are currently stored in browser localStorage.

| Item ID | Domain            | Storage Key     | Stored Fields                                                                            | Current Store | Source                                                                                                                                                                                                                           |
| ------- | ----------------- | --------------- | ---------------------------------------------------------------------------------------- | ------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| S001    | Theme             | pot-ui-theme    | theme: dark \| light \| system                                                           | localStorage  | [Source/Client/pot-react/src/App.tsx](../../Source/Client/pot-react/src/App.tsx#L57), [Source/Client/pot-react/src/components/theme/ThemeProvider.tsx](../../Source/Client/pot-react/src/components/theme/ThemeProvider.tsx#L54) |
| S002    | Auth/User cache   | pot-user        | userInfo object (persisted Zustand slice)                                                | localStorage  | [Source/Client/pot-react/src/stores/useUserStore.ts](../../Source/Client/pot-react/src/stores/useUserStore.ts#L24)                                                                                                               |
| S003    | Dashboard UI      | pot-dashboard   | quickActionsOpen, accountsOpen, incomesOpen, expensesOpen, expensesPeriod, incomesPeriod | localStorage  | [Source/Client/pot-react/src/features/dashboard/hooks/useDashboardStorage.ts](../../Source/Client/pot-react/src/features/dashboard/hooks/useDashboardStorage.ts#L4)                                                              |
| S004    | Accounts filter   | pot-accounts    | filterDescription                                                                        | localStorage  | [Source/Client/pot-react/src/features/accounts/hooks/useAccountStorage.ts](../../Source/Client/pot-react/src/features/accounts/hooks/useAccountStorage.ts#L8)                                                                    |
| S005    | Incomes filters   | pot-incomes     | selectedAccountId, filterDescription                                                     | localStorage  | [Source/Client/pot-react/src/features/incomes/hooks/useIncomeStorage.ts](../../Source/Client/pot-react/src/features/incomes/hooks/useIncomeStorage.ts#L4)                                                                        |
| S006    | Expenses filters  | pot-expenses    | selectedAccountId, filterDescription                                                     | localStorage  | [Source/Client/pot-react/src/features/expenses/hooks/useExpenseStorage.ts](../../Source/Client/pot-react/src/features/expenses/hooks/useExpenseStorage.ts#L4)                                                                    |
| S007    | Projections state | pot-projections | startDate, metric, period, hiddenSeries                                                  | localStorage  | [Source/Client/pot-react/src/features/projections/hooks/useProjectionStorage.ts](../../Source/Client/pot-react/src/features/projections/hooks/useProjectionStorage.ts#L24)                                                       |

Notes:

- No active sessionStorage usage found in current client source.
- Access token is currently memory-only, not browser persisted.
- AUTH_STORAGE_KEY exists in types but does not currently drive active persisted auth token storage.

## Storage Policy Decision Framework

Each setting will be assigned one of the following policies.

1. Persistent per environment + user (recommended for true preferences)

- Survives logout/login.
- Isolated between prod and docker.

2. Tab-session per environment + user (recommended for temporary working state)

- Resets when tab session ends.
- Useful for transient filters.

3. Hybrid (persistent default + tab-session override)

- Rehydrate from persistent, then overlay tab state if present.

## Rehydration Requirement

If hybrid/tab policies are used, load order must be:

1. Tab-session state (if exists)
2. Persistent per environment + user preferences
3. Application defaults

## Completed Client Storage Decision Matrix

| Decision ID | Setting Item                             | Decision Status | Selected Policy                                    |
| ----------- | ---------------------------------------- | --------------- | -------------------------------------------------- |
| D001        | S001 Theme (pot-ui-theme)                | Confirmed       | Persistent per environment + user                  |
| D002        | S002 User cache (pot-user)               | Confirmed       | Persistent per environment + user, clear on logout |
| D003        | S003 Dashboard UI (pot-dashboard)        | Confirmed       | Persistent per environment + user                  |
| D004        | S004 Accounts filter (pot-accounts)      | Confirmed       | Tab-session per environment + user                 |
| D005        | S005 Incomes filters (pot-incomes)       | Confirmed       | Tab-session per environment + user                 |
| D006        | S006 Expenses filters (pot-expenses)     | Confirmed       | Tab-session per environment + user                 |
| D007        | S007 Projections state (pot-projections) | Confirmed       | Hybrid                                             |

## Decision Log

1. D001 (S001 Theme): Persistent per environment + user.

- Rationale: Supports fast visual differentiation between production and docker and keeps theme stable across relogin.

2. D002 (S002 User cache): Persistent per environment + user, clear on logout.

- Rationale: Preserves permission caching benefits per environment while preventing stale user leakage after logout.

3. D003 (S003 Dashboard UI): Persistent per environment + user.

- Rationale: Dashboard section visibility and period choices are user preferences and should survive relogin.

4. D004 (S004 Accounts filter): Tab-session per environment + user.

- Rationale: Accounts filters are working context and should reset with a new tab session.

5. D005 (S005 Incomes filters): Tab-session per environment + user.

- Rationale: Incomes filter state is temporary operational context.

6. D006 (S006 Expenses filters): Tab-session per environment + user.

- Rationale: Expenses filter state is temporary operational context.

7. D007 (S007 Projections state): Hybrid.

- Rationale: Keep long-lived projection preferences while treating date-focused analysis context as session-level.

## Finalized Storage Requirements (Client)

| Item ID | Key             | Final Policy                                       | Field-Level Requirement                                                                   |
| ------- | --------------- | -------------------------------------------------- | ----------------------------------------------------------------------------------------- |
| S001    | pot-ui-theme    | Persistent per environment + user                  | Persist theme across relogin; isolate production vs docker settings.                      |
| S002    | pot-user        | Persistent per environment + user, clear on logout | Persist cached userInfo only for active user context; clear on logout to prevent leakage. |
| S003    | pot-dashboard   | Persistent per environment + user                  | Persist panel open/close and period preferences.                                          |
| S004    | pot-accounts    | Tab-session per environment + user                 | filterDescription resets on new tab session.                                              |
| S005    | pot-incomes     | Tab-session per environment + user                 | selectedAccountId and filterDescription reset on new tab session.                         |
| S006    | pot-expenses    | Tab-session per environment + user                 | selectedAccountId and filterDescription reset on new tab session.                         |
| S007    | pot-projections | Hybrid                                             | Persistent: metric, period, hiddenSeries. Tab-session: startDate.                         |

Implementation note:

- Environment scope is mandatory so production and docker tabs remain independent even when using the same browser profile.

## Problem Statement (As Observed)

- You can log in on browser/device A.
- You then log in on browser/device B.
- Device A later gets logged out (typically on refresh attempt or token expiration path).

This is consistent with a rotating refresh token design where only one refresh token is persisted per user.

## Deep Analysis Summary (From Earlier Discussion)

### A. Client-side storage is not session-aware by design

Evidence:

- Fixed localStorage keys are used for feature state:
  - [Source/Client/pot-react/src/features/accounts/hooks/useAccountStorage.ts](../../Source/Client/pot-react/src/features/accounts/hooks/useAccountStorage.ts#L8)
  - [Source/Client/pot-react/src/features/dashboard/hooks/useDashboardStorage.ts](../../Source/Client/pot-react/src/features/dashboard/hooks/useDashboardStorage.ts#L4)
  - [Source/Client/pot-react/src/features/incomes/hooks/useIncomeStorage.ts](../../Source/Client/pot-react/src/features/incomes/hooks/useIncomeStorage.ts#L4)
  - [Source/Client/pot-react/src/features/expenses/hooks/useExpenseStorage.ts](../../Source/Client/pot-react/src/features/expenses/hooks/useExpenseStorage.ts#L4)
  - [Source/Client/pot-react/src/features/projections/hooks/useProjectionStorage.ts](../../Source/Client/pot-react/src/features/projections/hooks/useProjectionStorage.ts#L24)
- User store is persisted to localStorage under one shared key:
  - [Source/Client/pot-react/src/stores/useUserStore.ts](../../Source/Client/pot-react/src/stores/useUserStore.ts#L24)
- There is no use of sessionStorage in current client source:
  - Search result confirmed no sessionStorage references in src

Implication:

- localStorage is origin-wide for the browser profile.
- Any account signing in on that same browser profile can inherit stale per-user filters/preferences unless keys are scoped.

### B. Server refresh-token model is effectively single active session per user

Evidence:

- Single refresh token fields on UserEntity:
  - [Source/Server/Pot.Data/Entities/UserEntity.cs](../../Source/Server/Pot.Data/Entities/UserEntity.cs#L38)
  - [Source/Server/Pot.Data/Entities/UserEntity.cs](../../Source/Server/Pot.Data/Entities/UserEntity.cs#L40)
- Login/refresh writes a new refresh token to the same user fields:
  - [Source/Server/Pot.AspNetCore/Concerns/Auth/AuthService.cs](../../Source/Server/Pot.AspNetCore/Concerns/Auth/AuthService.cs#L250)
- Refresh uses equality against that single stored refresh token:
  - [Source/Server/Pot.AspNetCore/Concerns/Auth/AuthService.cs](../../Source/Server/Pot.AspNetCore/Concerns/Auth/AuthService.cs#L162)
  - [Source/Server/Pot.AspNetCore/Concerns/Auth/AuthService.cs](../../Source/Server/Pot.AspNetCore/Concerns/Auth/AuthService.cs#L192)
- Login and refresh set refresh cookie from this single rotating token:
  - [Source/Server/Pot.AspNetCore/Features/Auth/Login/Handler.cs](../../Source/Server/Pot.AspNetCore/Features/Auth/Login/Handler.cs#L42)
  - [Source/Server/Pot.AspNetCore/Features/Auth/Refresh/Handler.cs](../../Source/Server/Pot.AspNetCore/Features/Auth/Refresh/Handler.cs#L48)

Implication:

- Device B refresh/login rotates persisted refresh token.
- Device A still holds old cookie token.
- A fails refresh at next attempt and is forced to logout.

### C. Client logout behavior amplifies session invalidation visibility

Evidence:

- On 401 path, interceptor attempts refresh and clears tokens on failure:
  - [Source/Client/pot-react/src/api/interceptors/authInterceptor.ts](../../Source/Client/pot-react/src/api/interceptors/authInterceptor.ts#L81)
  - [Source/Client/pot-react/src/api/interceptors/authInterceptor.ts](../../Source/Client/pot-react/src/api/interceptors/authInterceptor.ts#L104)
- clearTokens triggers logout flow:
  - [Source/Client/pot-react/src/concerns/auth/authTokenProvider.ts](../../Source/Client/pot-react/src/concerns/auth/authTokenProvider.ts#L35)
- Auth context proactive refresh also logs out on refresh failure:
  - [Source/Client/pot-react/src/features/auth/contexts/AuthContext.tsx](../../Source/Client/pot-react/src/features/auth/contexts/AuthContext.tsx#L121)

### D. Existing docs already acknowledge multi-tab limitation

- [Docs/AUTHENTICATION.md](../AUTHENTICATION.md#L1182)
- [Docs/AUTHENTICATION.md](../AUTHENTICATION.md#L1190)

## Baseline Plan of Attack (Previously Proposed)

## Phase 0: Decide product/session policy first

Decisions required before implementation:

1. Server session policy

- Option A: single active session per user (current behavior)
- Option B: multi-device independent sessions (recommended for your issue)
- Option C: bounded concurrent sessions (for example max N)

2. Client storage policy

- Option A: per-user persistent only
- Option B: per-user persistent plus per-tab/session ephemeral (recommended baseline)
- Option C: fully server-backed preferences

Recommendation from earlier conversation:

- Server: Option B
- Client: Option B

## Phase 1: Client session-aware storage model

Goal:

- Prevent cross-user bleed in browser-stored UI/filter/pref state.

Plan:

1. Define storage data classes

- Per-user persistent
- Per-tab/session scoped
- Non-persistent

2. Introduce key namespacing

- Proposed key shape:
  - pot:{userRowId}:{scope}:{feature}:{key}
- scope values:
  - persistent (localStorage)
  - session:{tabSessionId} (sessionStorage)

3. Add unified storage abstraction

- One helper/adapter that:
  - picks localStorage or sessionStorage by scope
  - applies namespacing
  - handles migration from legacy fixed keys

4. Login/logout boundary rules

- On login: initialize scoped context from authenticated user
- On logout: remove current-user scoped keys and current tab session keys
- Retain intentionally global app keys if desired (for example theme)

5. Verify behavior

- User A then User B on same browser profile: no data bleed
- Multiple tabs for same user: expected isolation for session-scoped items
- Hard refresh: only intended persistence survives

## Phase 2: Server multi-session refresh architecture

Goal:

- Login on one device should not invalidate other active device sessions by default.

Plan:

1. Introduce AuthSession entity/table

- Fields:
  - SessionRowId
  - UserRowId
  - RefreshTokenHash (store hash, not plaintext)
  - ExpiresUtc
  - CreatedUtc
  - LastSeenUtc
  - RevokedUtc
  - Optional metadata: userAgent, ipAddress, deviceLabel

2. Shift from user-level refresh token fields to session-level records

- One login creates one session row
- Refresh rotation updates only that session row
- Legacy user RefreshToken fields can remain temporarily for migration compatibility

3. Refresh flow redesign

- Cookie carries opaque refresh token mapped to a session
- Server validates session + hash + expiry + revoked state
- Rotation should be transactional
- Add reuse detection strategy for rotated/old token replay

4. Logout semantics redesign

- Logout current session endpoint: revoke only current session
- Optional logout-all endpoint: revoke all sessions for user
- Keep password-change as global revoke behavior

5. Access token behavior

- Keep TokenVersion claim for global revocation paths
- Optional future enhancement: include session id claim to support per-session access-token revocation

## Phase 3: Test strategy

Integration tests to add:

1. Multi-device independent refresh

- Device A login and refresh succeeds
- Device B login and refresh succeeds
- A refresh still succeeds after B login

2. Logout current session

- Logout on A does not logout B

3. Logout all sessions

- All sessions invalidated

4. Refresh replay detection

- Reuse of rotated token is blocked and handled

5. Expiry and revoked-state behavior

- Expired session token rejected
- Revoked session token rejected

6. Password change behavior

- Existing policy: forces re-authentication globally

Client tests to add:

1. Storage key namespace generation
2. Storage migration from legacy keys
3. Logout cleanup only scoped keys
4. sessionStorage/localStorage behavior separation

## Phase 4: Rollout and migration

1. Feature-flag rollout

- Keep old auth flow fallback during initial rollout

2. Backward compatibility

- Support legacy token path for transition window if needed

3. Observability

- Add session lifecycle audit logs
- Track refresh failure rates and revocation outcomes

4. Validate in staged environment before broad release

## Additional Implementation Context For Future Agent

### Current auth model details to preserve

1. Access token lifespan and generation

- Access token uses JWT and short expiry (15 mins)
- [Source/Server/Pot.AspNetCore/Concerns/Auth/JwtService.cs](../../Source/Server/Pot.AspNetCore/Concerns/Auth/JwtService.cs#L11)

2. TokenVersion revocation checks

- JWT validation checks token_version against DB on authenticated requests
- [Source/Server/Pot.AspNetCore/Concerns/Auth/Configuration/JwtBearerEventsSetup.cs](../../Source/Server/Pot.AspNetCore/Concerns/Auth/Configuration/JwtBearerEventsSetup.cs#L51)
- [Source/Server/Pot.AspNetCore/Concerns/Auth/Configuration/JwtBearerEventsSetup.cs](../../Source/Server/Pot.AspNetCore/Concerns/Auth/Configuration/JwtBearerEventsSetup.cs#L83)

3. Cookie behavior

- Refresh cookie configured under Authentication options
- [Source/Server/Pot.AspNetCore/Concerns/Auth/Configuration/AuthenticationOptions.cs](../../Source/Server/Pot.AspNetCore/Concerns/Auth/Configuration/AuthenticationOptions.cs#L12)

### Explicit non-goals for first implementation pass

1. Do not redesign all auth endpoints at once.
2. Do not remove TokenVersion global revoke behavior.
3. Do not force server-side session state for access tokens.
4. Do not mix unrelated auth hardening changes into the same PR.

### Suggested execution order (implementation slicing)

1. Introduce DB schema for AuthSession + migrations.
2. Add session-aware refresh service path behind feature toggle.
3. Update login to create session rows.
4. Update refresh endpoint to session-row rotation.
5. Update logout current-session semantics.
6. Add optional logout-all endpoint.
7. Add client storage scoping abstraction.
8. Migrate existing storage keys gradually.
9. Add tests at each step.
10. Update AUTHENTICATION.md and operational docs.

## Open Product Decisions (Must Be Confirmed)

1. Should multi-device concurrent sessions be allowed?
2. Should logout from one device affect only that device by default?
3. Is logout-all required in the first release?
4. Do you want a Session Management UI (list active sessions + revoke) now or later?

## Client Implementation Follow-Up (Coding-Ready)

Purpose:

- Provide a direct execution baseline for a client-focused coding agent.

Implementation scope:

- Client only.
- No server auth/session table changes in this phase.

Must preserve:

- Existing behavior for API auth token flow (access token in memory, refresh via cookie).
- Existing feature behavior, except where storage scope changes are intentional.

### Workstream A: Namespace and Scope Infrastructure

Requirements:

1. Introduce deterministic storage namespacing with three dimensions:

- Environment scope (production vs docker isolation)
- User scope (per authenticated user)
- Storage mode (persistent vs tab-session)

2. Support policies:

- Persistent per environment + user
- Tab-session per environment + user
- Hybrid (persistent baseline plus tab-session override)

3. Define stable environment identifier source.

- Prefer explicit app configuration value.
- Fallback may derive from API base URL if needed.

4. Provide migration support from existing flat keys.

- Read old key once.
- Write to namespaced key(s).
- Remove old key when successful.

### Workstream B: Apply Policies To Existing Settings

Apply finalized policy map to current storage points:

1. Theme (S001)

- Update [Source/Client/pot-react/src/App.tsx](../../Source/Client/pot-react/src/App.tsx#L57) and [Source/Client/pot-react/src/components/theme/ThemeProvider.tsx](../../Source/Client/pot-react/src/components/theme/ThemeProvider.tsx#L54) to use environment + user persistent keying.

2. User cache (S002)

- Update [Source/Client/pot-react/src/stores/useUserStore.ts](../../Source/Client/pot-react/src/stores/useUserStore.ts#L24) for environment + user persistent scope.
- Enforce clear-on-logout in existing logout flow.

3. Dashboard preferences (S003)

- Update [Source/Client/pot-react/src/features/dashboard/hooks/useDashboardStorage.ts](../../Source/Client/pot-react/src/features/dashboard/hooks/useDashboardStorage.ts#L4) to environment + user persistent scope.

4. Accounts/Incomes/Expenses filters (S004-S006)

- Update [Source/Client/pot-react/src/features/accounts/hooks/useAccountStorage.ts](../../Source/Client/pot-react/src/features/accounts/hooks/useAccountStorage.ts#L8), [Source/Client/pot-react/src/features/incomes/hooks/useIncomeStorage.ts](../../Source/Client/pot-react/src/features/incomes/hooks/useIncomeStorage.ts#L4), and [Source/Client/pot-react/src/features/expenses/hooks/useExpenseStorage.ts](../../Source/Client/pot-react/src/features/expenses/hooks/useExpenseStorage.ts#L4) to tab-session scoped storage.

5. Projections hybrid behavior (S007)

- Update [Source/Client/pot-react/src/features/projections/hooks/useProjectionStorage.ts](../../Source/Client/pot-react/src/features/projections/hooks/useProjectionStorage.ts#L24) so:
  - Persistent: metric, period, hiddenSeries
  - Tab-session: startDate

### Workstream C: Rehydration and Logout Rules

1. Rehydration order must be:

- tab-session
- persistent
- defaults

2. Logout behavior:

- Clear current user-scoped caches immediately.
- Keep only data intentionally persistent across relogin for the same user and environment.

3. Cross-user safety:

- Never read another user's persisted namespace when user identity changes.

### Workstream D: Validation And Testing

Required validation scenarios:

1. Environment isolation

- Production and docker tabs keep independent theme and preference state.

2. User isolation

- User A and User B do not inherit each other's settings in same browser profile.

3. Tab-session behavior

- S004-S006 reset in new tab sessions.

4. Hybrid behavior

- S007 startDate resets per tab-session.
- S007 metric/period/hiddenSeries persist per environment + user.

5. Migration behavior

- Existing keys are migrated without losing intended preferences.

### Acceptance Criteria For Client PR

1. All S001-S007 policies implemented exactly as defined in Finalized Storage Requirements (Client).
2. Environment-scoped separation demonstrated for production vs docker.
3. No regression in login/logout or token refresh behavior.
4. Documentation update included with final key policy map and migration notes.

### Suggested PR Breakdown

1. PR 1: Storage namespace utility + migration infrastructure.
2. PR 2: Theme, user cache, dashboard persistence migration.
3. PR 3: Accounts/Incomes/Expenses tab-session migration.
4. PR 4: Projections hybrid split + regression tests.

## Risks and Mitigations

1. Refresh rotation race conditions

- Mitigation: transactional update and deterministic conflict handling.

2. Cookie domain/samesite misconfiguration across envs

- Mitigation: verify local/dev/prod cookie config matrix and test via integration tests.

3. Legacy localStorage incompatibilities

- Mitigation: migration reads old keys once, writes namespaced keys, then cleans safely.

4. Operational blind spots

- Mitigation: structured auth/session logs and basic metrics before full rollout.

## Acceptance Criteria (Planning-Level)

1. Device A and B can remain logged in concurrently after independent refresh cycles.
2. Logout current session only affects current device/browser session.
3. Logout-all invalidates all user sessions.
4. Client storage no longer leaks per-user state across different users in same browser profile.
5. Existing security controls (TokenVersion for global revoke paths, short-lived access tokens, httpOnly refresh cookie) remain intact.

## Hand-off Checklist For Future Agent

1. Re-read this document and [Docs/AUTHENTICATION.md](../AUTHENTICATION.md).
2. Confirm policy decisions with product owner before coding.
3. Produce an implementation ADR for auth session model.
4. Implement schema and service changes behind a flag.
5. Add integration tests first for expected multi-session behavior.
6. Implement client storage scoping with migration support.
7. Update documentation and rollout notes.

## Related Documents

- Feature index: [Docs/Future/README.md](README.md)
- Current auth docs: [Docs/AUTHENTICATION.md](../AUTHENTICATION.md)
