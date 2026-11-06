# ADR review: what is still open (temporary)

**Status:** All review edits to `Docs/adr/` are applied. D-1 is in `79f48dd6`, the bulk in `a50d1c85`, and
the E2E authentication record in `fed146e7`; `git log -- Docs/adr` is the full record. This file now holds
only what the ADRs do not: the findings outside the ADRs. No review decision is still open; the last,
D-9, was resolved on 2026-11-06 by narrowing ADR-0050 and raising PRD-0035. The full report,
with the reason and evidence for every edit, is `git show 79f48dd6:ADR-REVIEW-EDITS.md`. Delete this file
once each item below has a home. A resolved item is removed rather than marked resolved, so this file only
ever holds what is still open; numbers are not reused, so the list may skip. ADR and PRD numbers here follow the renumbering of
2026-11-06; the commits named above, and the report in them, use the earlier numbers.

## Findings outside the ADRs

None of these is fixed. Each needs a DD brief, a PRD, or a W1 or W2 fix. **Verified** means re-checked
directly; the rest are as the reviewers reported.

### Security

- **F-1. A deployment-notes file is in pushed git history** (existence verified, contents not opened).
  - `Docs/Azure/Azure-Deployment-Blueprint.md` was tracked in commits `b2c4e1d8`, `05d046a7` and
    `e7e861f9` (2025-11-09 to 2025-11-16).
  - Those commits are reachable from `origin/v0.1`, `origin/v0.1-doc-update-plan` and `origin/v0.1-mcp`.
  - It contains password and connection-string steps.
  - **Check whether it holds a real value.** If it does, rotate that credential: rewriting history alone
    is not enough once the commits are pushed.
- **F-3. A completed password reset leaves existing sessions valid.**
  - Unlike a signed-in password change, it revokes nothing.
  - Nothing records whether this is intended.
- **F-4. Logout can leave the session active.**
  - The session is revoked only if the logout request carries a valid access token.
  - On the interceptor-failure path the client clears the token before it posts `/auth/logout`.
  - A replayed, already-rotated refresh token is refused but does not revoke the session, so there is no
    theft signal. PRD-0002 OQ-4 was never implemented.
- **F-5. Access tokens are accepted for up to about 20 minutes, not 15,** because the framework's default
  five-minute clock skew is kept.
- **F-6. Sign-up answers `UsernameTaken`,** which undermines the password reset's protection against
  username probing.
- **F-7. The three-attempt limit on one-time codes is not enforced across requests** (verified).
  - `VerificationServiceBase` increments `AttemptCount` and sets `Failed` on a wrong code.
  - `VerifySignupService` (`:92-97`) and `VerifyPasswordResetService` (`:90-92`) call `SaveAsync` only
    when the result is a success, so the increment is never persisted.
  - The per-user lock (`OtpService.HasReachedRateLimitAsync`) counts codes with `Failed` status, and that
    status is never persisted either, so the lock never trips.
  - The only brakes on guessing a six-digit code are the HTTP rate limit and the 15-minute expiry.
  - ADR-0024's Decision Outcome describes the intended behaviour ("fails after three wrong attempts"), not
    the effective behaviour. Check its wording as you review it.
  - No test references `AttemptCount` or the too-many-attempts result.
  - This weakens the reasoning, in ADR-0024 and in DD-08 (answered 2026-11-06), that a leaked code's
    impact is contained. PRD-0035 removes codes from the logs, but a code can still be guessed.
  - The HTTP rate limit is also weaker than configured: its counters are per replica, and the API allows
    up to ten (ADR-0032, "Known scale-out risks"). PRD-0008 records that risk.

### Correctness and code

- **F-8. API failures never reach the query library.** Hooks return failures as values, so TanStack
  Query's error state, retries and error boundaries never see them, and a failed query is cached as a
  success-shaped value. The same mechanism means 429s are never retried. ADR-0053 now makes that a rule,
  but nothing configures or tests it.
- **F-9. `authClient` sets no timeout** (`src/api/authClient.ts:19-20`), so a refresh can wait
  indefinitely.
- **F-10. Cross-site import may fail** (unverified). The `RowId` unique index spans all sites, while the
  import lookup is filtered by site, so importing one site's export into another site may hit the
  constraint.
- **F-11. The role-assignment transaction does not protect the last-admin check** under the default Read
  Committed isolation level, and it wraps only one save.
- **F-12. The OpenAPI contract declares `429` on no endpoint,** although every grouped endpoint can
  return it.
- **F-13. The prodlike Playwright config has drifted from the dev config.** It lacks two of the dev
  config's `testIgnore` entries, including `quickActions`, which the dev config says collides when run on
  two projects. It also sets no `expect.timeout`, so it uses 5 s instead of 10 s.
- **F-14. A vestigial index.** `IX_Setting_Global_Category_Key` has matched no row since settings became
  site-specific. `IX_Setting_SiteId_Category_Key` is partial in the database but unfiltered in the model
  snapshot.

### Documentation drift (for W1 and W2)

- **F-15. `DECISIONS-TO-DISCUSS.md`:**
  - DD-02 says six Etag update paths; there are nine. The three Users paths are missing.
  - The forwarded-headers follow-up cites rows 0074 and 0036; the trust model is row 0083.
- **F-16. Documents that say a 429 always carries `Retry-After`:** PRD-0014:22-30, PRD-0030 B8,
  `Source/Server/DEVELOPER.md:244` and `:3633`. An authenticated 429 carries none.
- **F-17. The client's "features never import features" rule is false in practice.**
  `Source/Client/pot-react/DEVELOPER.md:260` states it, and the client instruction says shared layers
  "must never depend on features"; both are contradicted by the code. See DD-13.
- **F-18. `Docs/AUTHENTICATION.md:2101`** says invalid platform-admin GUIDs are "ignored". They fail
  start-up.
- **F-19. Stale code comments:**
  - `playwright.config.ts:93-95` and `playwright.prod.config.ts:40-42` say "no mail is delivered".
  - `e2e/approvals/approvals.test.ts:17-23` says "no delete-user API".
  - `RateLimiterPolicy.cs:11,15,79` mention concurrency limiting and a 10 s timeout; the client timeout
    is 30 s.
  - `src/hooks/useLocalStorage.ts:24-61` describes the legacy `pot-` prefix.
- **F-20. PRD-0031 D1** says a canary covers "every destructive action". `IsolationFixture` covers three
  routes.
