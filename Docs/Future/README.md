# Future Features Index

Purpose: Track proposed but not-yet-implemented features and design tracks for POT.

Status legend:

- Proposed: idea captured, not yet designed in detail
- Planning: analysis and design in progress; requirements and approach being refined
- Planned: design documented and approved; ready for implementation planning
- In Progress: implementation started
- Complete: implemented and validated

## Feature Backlog

| ID  | Feature                                                    | Status      | Priority | Last Updated | Document                                                                                                                    | Summary                                                                                                                                                                   |
| --- | ---------------------------------------------------------- | ----------- | -------- | ------------ | --------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 001 | Per-session login support and session-aware client storage | Complete    | High     | 2026-05-03   | [Per-Session Login Support Plan](001-per-session-login-support.md)                                                          | Delivered independent multi-device sessions and user/session-scoped client storage with migration-safe rollout and validation coverage.                                   |
| 002 | Server login/logout multi-session architecture             | Complete    | High     | 2026-05-03   | [Server Login/Logout Session Architecture Placeholder](002-server-login-logout-session-architecture.md)                     | Implemented AuthSession-backed server sessions, per-session logout semantics, global-revoke password change behavior, and legacy refresh-token field cleanup.             |
| 003 | Expense accrual policy modes and UX simplification         | Complete    | High     | 2026-04-06   | [Expense Accrual Policy Modes Plan](003-expense-accrual-policy-modes.md)                                                    | Add server-driven accrual policy options (Automatic/None), simplify expense UX, and keep calculation behavior deterministic.                                              |
| 004 | Import preflight awareness                                 | Proposed    | High     | 2026-04-05   | [Import Preflight Awareness](004-import-preflight-awareness.md)                                                             | Add a pre-import compatibility/preflight check so users can see blocking issues and remediation guidance before import is executed.                                       |
| 005 | PWA update change awareness                                | Proposed    | Medium   | 2026-04-05   | [PWA Update Change Awareness](005-pwa-update-change-awareness.md)                                                           | Detect new app versions in the PWA and notify users about important feature and breaking behavior changes with actionable guidance.                                       |
| 006 | Login long-wait progress feedback                          | Complete    | High     | 2026-04-06   | [Login Long-Wait Progress Feedback Plan](006-login-long-wait-progress-feedback.md)                                          | Keep normal login behavior, but for rare long waits show non-blocking progress updates every 5 seconds until a 2 minute login-only timeout.                               |
| 007 | Docker build performance and selective local workflows     | Complete    | High     | 2026-05-01   | [Docker Build Performance and Selective Local Workflows PRD](007-docker-build-performance-and-selective-local-workflows.md) | Reduce Docker build times by excluding test graph from runtime image builds, enabling cache-first task defaults, and adding server-only/client-only local workflows.      |
| 008 | Authentication abuse protection and lockout policy         | Proposed    | High     | 2026-04-11   | [Authentication Abuse Protection and Lockout Policy (Draft)](008-authentication-abuse-protection-and-lockout-policy.md)     | Define post-session-architecture brute-force and credential-stuffing protections, lockout policy options, auditability, and operational response controls.                |
| 009 | Accrual dirty-flag rules and UI status accuracy            | Complete    | High     | 2026-04-25   | [Accrual Dirty Flag Rules And UI Status PRD](009-accrual-dirty-flag-rules-and-ui-status-prd.md)                             | Define explicit dirty-trigger semantics, reduce false update notices, and align status signals with account-level accrual recalculation requirements.                     |
| 010 | AuthSession retention and background cleanup policy        | Proposed    | Medium   | 2026-05-01   | [AuthSession Retention and Background Cleanup Policy (Draft)](010-auth-session-retention-and-cleanup-policy.md)             | Define retention windows and a background cleanup worker to bound AuthSession growth while preserving short-term security and operational investigation context.          |
| 011 | Account filter navigation and URL state consistency        | Complete    | High     | 2026-05-10   | [Account Filter Navigation and URL State Consistency PRD](011-account-filter-navigation-url-state-consistency-prd.md)       | Implemented URL-authoritative account filtering with dynamic sidebar link context, create/edit return-path preservation, and validated behavior across all ten scenarios. |
| 012 | Playwright E2E testing infrastructure                      | Planning    | High     | 2026-05-10   | [Playwright E2E Testing Infrastructure PRD](012-playwright-e2e-testing-infrastructure.md)                                   | Design comprehensive E2E test architecture with database fixtures, auth helpers, test data management, multi-user support, and CI integration patterns.                   |
| 013 | DB readiness health check and worker startup gate          | Backlog     | Low      | 2026-05-16   | [DB Readiness Health Check and Worker Startup Gate PRD](013-db-readiness-health-check-and-worker-startup-gate.md)           | Register a DB health check on `/_health` and gate `ExpiredOtpCleanupWorker` and `BudgetReminderEmailWorker` on DB readiness before entering their execution loops.        |
| 014 | E2E test coverage discovery                                | In Progress | High     | 2026-05-17   | [E2E Test Coverage Discovery](014-e2e-test-coverage-discovery.md)                                                           | Live browser exploration of all UI operations mapped to test classification (parallel-safe / fixture-managed / isolated-sequence), locator reference, and data strategy.  |

## Notes

- This folder is intentionally implementation-planning focused.
- Keep one row per feature and link to a dedicated detail document when available.
- Current date of this index update: 2026-05-16.
