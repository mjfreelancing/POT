# ADR review: what is still open (temporary)

**Status:** All review edits are applied to `Docs/adr/`. D-1 is in `79f48dd6`; the other 270 edits are
uncommitted, for review in `git diff -- Docs/adr`. This file now holds only what the ADRs do not: the
decisions still open, and the findings outside the ADRs. The full report, with the reason and evidence for
every edit, is `git show 79f48dd6:ADR-REVIEW-EDITS.md`. Delete this file once each item below has a home.

## Decisions still open

**D-2. ADR-0043 to ADR-0046 now supersede ADR-0001 (applied). Resolved by ADR-0048.**

- Two of ADR-0001's sections had no other record: the fresh login for every test, which refresh-token
  rotation forces, and the dedicated seeded identities such as `e2e_pwchange`.
- **Resolved:** `Docs/adr/0048-no-end-to-end-test-shares-authentication-state-with-another.md` records both
  as one decision — a test owns its session and the account it signs in as — and states that it was found
  by trial and error rather than designed. ADR-0001's `superseded-by` lists it, and
  `.github/instructions/e2e.instructions.md` cites it.
- The supersession edits stand; ADR-0001 stays `superseded`.

**D-3. ADR-0044 and ADR-0045 are retitled (applied); their file names are not.**

- The file slugs no longer match the titles. docs-check does not compare them.
- **To do:** rename both files (recommended). The links added in ADR-0043, ADR-0044 and ADR-0045 must
  follow, and so must the index (`docs-check -Update`).

**D-4. `related-prds` front matter is filled in (applied).**

- Each PRD named there should list the ADR in its "Decisions Carried Into This Document" table. Those PRD
  edits have not been made. See F-21.
- The auth records cite their PRDs in the body text instead, so the series is not yet consistent.

**D-5. Records whose status runs ahead of an open decision.**

- **ADR-0035** (hand-run builds, no CI) is `accepted`, which in effect answers DD-05 with option A. Either
  answer DD-05 as A, or set the record to `proposed` until DD-05 is decided.
- **ADR-0045** records both of DD-11's rules and leaves the choice open. When DD-11 is decided, it needs a
  clarification or a superseding record.
- **ADR-0047** is `proposed`, and ADR-0002 is `superseded` by it, so no binding record states the
  never-auto-retry-a-429 rule today. Accept ADR-0047, or say why not.

**D-6. Records that bundle several decisions.** The reviewers proposed no splits. Splitting is your call.

- ADR-0013: password hashing, and code-based reset.
- ADR-0027: enum-by-name storage, and database-enforced uniqueness.
- ADR-0029: a separate migration process, running before the API, and running on every start.
- ADR-0031: in-process queueing, and Razor templating.

**D-7. Accepted records were edited in place.**

- `Docs/adr/README.md` allows an `accepted` record to change only for a typo, a link, status, or a dated
  clarification.
- The edits rewrite text in records 0003 to 0047, as the earlier "Correct the … record" commits did. The
  basis is that these records were written this week and are not yet on `main`.
- ADR-0001 and ADR-0002 were changed only by dated clarifications.
- **To do:** confirm that basis. If it does not hold, every non-clarification edit must become a
  superseding record instead.

**D-8. Facts only the Azure portal can settle.**

- **Certificate issuer.** ADR-0003 says the certificates are Azure-managed, and that the deployment record
  and the diagram give the issuer as Let's Encrypt. Azure's documentation names DigiCert for managed
  Container Apps certificates. If it is DigiCert, correct ADR-0003 and the diagram label.
- **Where `/api` reaches the API** (row 0074, still `Ver`). The build implies the API app's own hostname
  (`api.payontime.com.au`). The diagram and the Docker guide say ingress path routing. ADR-0036 records
  the discrepancy, not an answer.
- **Readiness probe and replica count** (FQ-03, DD-04) cannot be confirmed from the repository.

**D-9. ADR-0032's title against the running system.**

- "Logs … never submitted values" is the decision (PRD-030), but it is not what the code does today:
  verification codes and the raw refresh token are logged at `Information` (see F-2).
- The record keeps the decision and lists the exceptions.
- If DD-08 is answered "keep", the record has to be narrowed, which means superseding it.

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
- **F-2. The raw refresh token is logged at `Information`** (verified).
  - `Source/Server/Pot.AspNetCore/Concerns/Auth/Services/AuthService.cs:229` calls
    `_logger.LogCall(this, new { refreshToken })`.
  - It is a 30-day credential. DD-08 covers only the verification code, so this needs its own brief, or
    an extension of DD-08.
- **F-3. A completed password reset leaves existing sessions valid.**
  - Unlike a signed-in password change, it revokes nothing.
  - Nothing records whether this is intended.
- **F-4. Logout can leave the session active.**
  - The session is revoked only if the logout request carries a valid access token.
  - On the interceptor-failure path the client clears the token before it posts `/auth/logout`.
  - A replayed, already-rotated refresh token is refused but does not revoke the session, so there is no
    theft signal. PRD-002 OQ-4 was never implemented.
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
  - ADR-0013 (lines 52-56) describes the intended behaviour ("fails after three wrong attempts"), not
    the effective behaviour. Check its wording as you review it.
  - No test references `AttemptCount` or the too-many-attempts result.
  - This affects DD-08's reasoning that the impact is "contained" by three attempts.

### Correctness and code

- **F-8. API failures never reach the query library.** Hooks return failures as values, so TanStack
  Query's error state, retries and error boundaries never see them, and a failed query is cached as a
  success-shaped value. The same mechanism means 429s are never retried, but nothing configures or tests
  that.
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
  - DD-11 cites ADR-0001 by line number (`:147-165`, `:402`). Those lines have moved; cite the sections
    instead.
- **F-16. Documents that say a 429 always carries `Retry-After`:** PRD-014:22-30, PRD-030 B8,
  `Source/Server/DEVELOPER.md:244` and `:3633`. An authenticated 429 carries none. PRD-014:89 still names
  ADR-0002 as its companion record.
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
- **F-20. PRD-031 D1** says a canary covers "every destructive action". `IsolationFixture` covers three
  routes.
- **F-21. PRD-030** should name ADR-0014, ADR-0032 and ADR-0039 in its "Decisions Carried" table, now that
  their `related-prds` name it (D-4).
