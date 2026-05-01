# AuthSession Retention and Background Cleanup Policy (Draft)

Feature ID: 010
Date created: 2026-05-01
Scope: Server-side session retention and cleanup policy only. No code changes applied in this session.
Audience: Future implementation agent(s), reviewers, maintainers, and operations.

## Purpose

Define how AuthSession records should be retained, revoked, and periodically purged so session storage remains bounded while preserving enough history for security review and operational debugging.

## Relationship To Other Features

- Depends on: [Server Login/Logout Session Architecture Placeholder](002-server-login-logout-session-architecture.md)
- Related to: [Per-Session Login Support Plan](001-per-session-login-support.md)
- Supports future security workflows in: [Authentication Abuse Protection and Lockout Policy (Draft)](008-authentication-abuse-protection-and-lockout-policy.md)

## PRD Status

- Stage: Proposed
- Code changes: Not started
- Decisions finalized: None yet
- Last updated: 2026-05-01

## Problem Statement

As multi-session login support is introduced, AuthSession rows will accumulate over time.
Without a retention policy and cleanup mechanism:

1. Session table size grows indefinitely.
2. Operational queries may degrade as stale records accumulate.
3. Security review needs conflict with uncontrolled long-term data retention.

## Proposed Starting Recommendation (Subject To Planning Discussion)

This recommendation is a baseline only and must be re-evaluated when this PRD moves from Proposed to Planning.

1. Do not hard-delete on normal logout.

- Set RevokedUtc for the active session.
- Preserve session rows for a defined retention window.

2. Run a periodic background cleanup worker.

- Frequency: daily (default) or weekly for low-volume environments.
- Purge policy defaults:
  - Revoked sessions: delete after 90 days.
  - Expired but never-revoked sessions: delete 30 days after ExpiresUtc.

3. Keep cleanup logic explicit and configurable.

- Configuration values should drive retention windows and schedule.
- Include a dry-run mode for validation before first destructive run.

4. Prefer soft lifecycle + periodic purge over immediate logout deletion.

- Preserves near-term investigation value.
- Keeps implementation simple and operationally safe.

## Draft Scope

In-scope (for eventual implementation):

1. Cleanup worker/service for expired/revoked AuthSession rows.
2. Retention configuration options.
3. Logging/metrics for deleted row counts and run outcome.
4. Basic test coverage for retention rule behavior.

Out-of-scope (initial pass):

1. Full audit-event archive subsystem.
2. User-facing session history UI.
3. Long-term compliance retention workflows.

## Open Product/Engineering Questions

1. Should retention windows differ between production and local/dev environments?
2. Is a manual admin-triggered cleanup endpoint/job required, or scheduled-only is sufficient?
3. Should cleanup be hard delete only, or archive-then-delete in first release?
4. What minimum operational telemetry is required to mark this feature complete?
5. Is daily cadence mandatory, or weekly cadence acceptable for low usage?

## Data and Performance Notes (Draft)

1. Query path for active session validation should always target non-revoked, non-expired rows.
2. Cleanup should run in bounded batches to avoid long locks.
3. Index strategy should prioritize active-session lookups and cleanup predicates.

## Security Notes (Draft)

1. Avoid keeping session rows forever without policy justification.
2. Avoid deleting immediately on logout if near-term incident review is expected.
3. Reconcile retention values with any future abuse-detection and audit requirements (Feature 008).

## Suggested Planning Entry Criteria

Before moving this PRD to Planning:

1. Confirm AuthSession schema and revocation semantics in Feature 002.
2. Confirm initial retention windows with product/security stakeholders.
3. Confirm required observability for worker execution.

## Suggested Acceptance Criteria (Planning-Level)

1. AuthSession growth remains bounded by policy.
2. Revoked/expired sessions are purged according to configured windows.
3. Cleanup execution is observable (success/failure + row counts).
4. Policy and defaults are documented and reviewed.

## Handoff Notes

When this PRD moves from Proposed to Planning, re-evaluate this baseline recommendation before implementation. Treat all values in this document as proposed defaults, not finalized decisions.

## Related Documents

- Future index: [Docs/Future/README.md](README.md)
- Session architecture: [Server Login/Logout Session Architecture Placeholder](002-server-login-logout-session-architecture.md)
- Client/server session umbrella: [Per-Session Login Support Plan](001-per-session-login-support.md)
- Abuse controls: [Authentication Abuse Protection and Lockout Policy (Draft)](008-authentication-abuse-protection-and-lockout-policy.md)
