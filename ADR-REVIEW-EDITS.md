# ADR review: proposed edits (temporary)

**Status:** awaiting owner approval. Written 2026-11-05 against HEAD `a93229b4` (branch
`v0.1-doc-update-plan`). Disposable: delete it once the approved edits are applied.

This is the second, independent review of all 47 ADRs in `Docs/adr/`. Eight reviewers worked in parallel,
one per ADR family. Each checked every claim against the code, and then against the guides, PRDs,
instruction files, skills, Docker and compose files, the Azure diagram, and the owner-confirmed facts in
`DOCS-WORK-LISTS.md`. The private `Docs/Azure/Azure-Deployment-Notes.md` is **not on this machine**, so
Azure facts rest only on what `DOCS-WORK-LISTS.md` records (SQ-35, SQ-37, SQ-38, rows 0083 and 0084) and on
the tracked diagram.

**Result:** 291 proposed edits. Their severities: consistency 59, format 29, factual 112, unsupported-history 27, confirmation 39, omission 25. MAJOR (the decision statement or summary
is factually wrong): ADR-0014, ADR-0041, ADR-0044, ADR-0045.

**House style:** 33 edits were reworded, because the reviewers had reintroduced "this record is
backfilled" paragraphs, "the owner recalls … (SQ-nn)" attributions and `DOCS-WORK-LISTS.md` citations. You
removed those in `8e74098d` ("ADR cleanup"), and FQ-04 records that you asked for them gone. The reworded
text keeps each correction and states the reason directly. 2 edits contained nothing but a
backfill paragraph and were dropped: E-0015-02, E-0006-01. Each reworded edit is marked "Reworded for house style".

**Checked mechanically:**
- Every `old` block matches its ADR exactly once when the edits are applied in order.
- No two edits overlap.
- With every edit applied to a copy of the repo, `docs-check -Update` followed by `docs-check` passes.
- Preview copies of all 47 edited files are in
  `C:\Users\malcolms\AppData\Local\Temp\claude\ed57caeb7ae7f936\79aa3f9d-6984-47f8-bdcc-121fc5bc4c58\scratchpad\adr-review\applied-final\`,
  so you can diff any of them against `Docs/adr/`.

## How to approve

- **Per edit:** put `Y` or `N` in the `Decision: [ ]` box under each edit in "Edits in full", or write the
  change you want.
- **In bulk:** answer the owner decisions (D-1 to D-9) below. Each one approves or rejects a whole tagged
  set. Then write, for example, "approve all others except E-0012-03".
- Every edit carries tags. Use them to approve a category at once.

## How to apply (for the agent that processes this file)

1. **Which edits to apply:** those marked `Y`, plus those covered by an approved bulk decision. Skip
   `DROPPED` blocks.
2. **Order:** within each ADR file, apply in ascending edit number.
3. **Line endings:** the ADR files are CRLF. Normalise to LF to match `old`, then write the file back as
   CRLF.
4. **Matching:** `old` must occur exactly once. If it does not, stop and report the edit. Do not improvise.
5. **Kinds:**
   - `replace` and `front-matter`: replace `old` with `new`.
   - `insert-after`: insert `new` immediately after `old`. `new` carries its own leading newlines.
   - `delete`: remove `old`.
6. **Dependencies:** an edit tagged `DEPENDS-ON` is anchored on text that an earlier edit adds. Apply it
   only if that earlier edit was approved. Otherwise report it. Edits with this tag: E-0026-03 (E-0026-02).
7. **Check:** run `pwsh ./.github/scripts/docs-check/docs-check.ps1 -Update`, then run it again without
   `-Update`. Both must pass.
8. **Stop:** never stage or commit. Offer a commit message and stop.

## Owner decisions

**D-1. Decision dates (tag `DATE`).**
- The 45 new records all say `date: 2026-11-04`. That is the day they were written; the template says
  "when the decision was made".
- These edits propose dates taken from PRDs and commits: E-0003-01, E-0004-01, E-0005-01, E-0007-01, E-0008-01, E-0009-01, E-0010-01, E-0011-01, E-0012-01, E-0013-01, E-0014-01, E-0015-01, E-0016-01, E-0032-01, E-0035-01, E-0036-01, E-0038-01, E-0039-01, E-0040-01, E-0041-01, E-0042-01, E-0043-01, E-0044-02, E-0045-02, E-0046-01.
- The other groups left the date alone and asked you to decide. ADRs with no date edit: ADR-0006, ADR-0017, ADR-0018, ADR-0019, ADR-0020, ADR-0021, ADR-0022, ADR-0023, ADR-0024, ADR-0025, ADR-0026, ADR-0027, ADR-0028, ADR-0029, ADR-0030, ADR-0031, ADR-0033, ADR-0034, ADR-0037, ADR-0047.
- Many of the proposed dates come from commits that exist only on `v0.1*` branches, not on `main`. The
  date is still a valid value, but the commit cited in a `Why` line may not survive a squash merge.
- **Options:**
  - A: apply the proposed dates, and have the remaining records dated the same way in a follow-up.
  - B: reject them all and keep `2026-11-04` as the series convention. FQ-04 would then need to record
    that the convention changed.
- **Recommendation:** A.

**D-2. ADR-0043 to ADR-0046 supersede ADR-0001 (tag `STATUS-LINK`: E-0001-01, E-0043-03, E-0044-04, E-0045-04, E-0046-03).**
- `DOCS-WORK-LISTS.md` W4 says the bounded E2E records replace ADR-0001, but no file declares it.
- Superseding ADR-0001 strands two of its sections: the fresh login for every test (forced by
  refresh-token rotation), and the dedicated seeded identities such as `e2e_pwchange`. No current record
  would hold them.
- **Options:**
  - A: apply the supersession, and write one more bounded ADR for the E2E authentication strategy.
  - B: reject all five edits as a set, and leave ADR-0001 `accepted` for now.
- **Recommendation:** A, but only together with the new record. Otherwise B.

**D-3. Retitles of ADR-0044 and ADR-0045 (tag `TITLE-AREA`: E-0043-02, E-0044-01, E-0044-03, E-0044-05, E-0045-01, E-0045-03, E-0045-05, E-0046-02).**
- Both titles are false.
- 0044 says each test states its class "at the top of the file". No rule says so, and 22 of 46 spec
  files don't.
- 0045 says the suite "runs on every change". The skills make it opt-in, and there is no CI.
- The same tag also moves 0043 to 0046 from `area: frontend` to `area: cross-cutting`.
- After the retitle the file slugs no longer match the titles. docs-check does not compare them.
- **Options:**
  - A: apply, and keep the file names.
  - B: apply, and rename both files. The links added in E-0043-07, E-0044-15 and E-0045-10 must then
    follow.
- **Recommendation:** B.

**D-4. `related-prds` front matter (tag `RELATED-PRDS`: E-0014-01, E-0025-01, E-0028-01, E-0032-01, E-0039-01, E-0041-01, E-0043-02, E-0044-03, E-0045-03, E-0046-02, E-0047-01).**
- The skill says to fill it when a decision came from a PRD.
- Doing so obliges each named PRD to list the ADR in its "Decisions Carried Into This Document" table.
  That is a PRD edit, and it is not in this file.
- The auth reviewer deliberately cited PRDs in the body text instead, for that reason, so the series
  would be inconsistent either way.
- **Options:**
  - A: apply the edits, and update the PRD tables in a follow-up.
  - B: reject them.
- **Recommendation:** A.

**D-5. Records whose status runs ahead of an open decision.**
- **ADR-0035** (hand-run builds, no CI) is `accepted`, which in effect answers DD-05 with option A before
  you have. Either answer DD-05 as A, or set the record to `proposed` until DD-05 is decided. No edit is
  proposed.
- **ADR-0045:** the edits record both of DD-11's rules and leave the choice open. When DD-11 is decided,
  this record needs a clarification or a superseding record.
- **ADR-0047** is `proposed`, and ADR-0002 is `superseded` by it. So no binding record states the
  never-auto-retry-a-429 rule today. Accept ADR-0047, or say why not.

**D-6. Records that bundle several decisions.**
- ADR-0013: password hashing, and code-based reset.
- ADR-0027: enum-by-name storage, and database-enforced uniqueness.
- ADR-0029: a separate migration process, running before the API, and running on every start.
- ADR-0031: in-process queueing, and Razor templating.

The reviewers proposed no splits. Splitting them, or not, is a structural choice for you.

**D-7. Editing accepted records in place.**
- `Docs/adr/README.md` allows an `accepted` record to change only for a typo, a link, status, or a dated
  clarification.
- Most edits here rewrite text in records 0003 to 0047, as the earlier "Correct the … record" commits
  did, on the basis that these records were written this week and are not yet on `main`.
- ADR-0001 and ADR-0002 are edited only by dated clarifications.
- Confirm that this basis still holds. If it does not, every non-clarification edit must become a
  superseding record instead.

**D-8. Facts only the Azure portal can settle.**
- **Certificate issuer (E-0003-02).** SQ-38 and the diagram say Let's Encrypt. Azure's documentation
  names DigiCert for managed Container Apps certificates. If it is DigiCert, change the `new` text to
  "Azure-managed certificates" and correct the diagram label.
- **Where `/api` reaches the API (row 0074, still `Ver`).** The build implies the API app's own hostname
  (`api.payontime.com.au`). The diagram and the Docker guide say ingress path routing. E-0036 records the
  discrepancy, not an answer.
- **Readiness probe and replica count** (FQ-03, DD-04): cannot be confirmed from the repo.

**D-9. ADR-0032's title against the running system.**
- "Logs … never submitted values" is the decision (PRD-030), but it is not what the code does today:
  verification codes and the raw refresh token are logged at `Information` (see F-2).
- The edits keep the decision and list the exceptions honestly.
- If DD-08 is answered "keep", the record has to be narrowed, which means superseding it.

## Findings outside the ADRs (no edit here; each needs a DD brief, a PRD or a W1/W2 fix)

Marked **verified** where I re-checked it myself; the rest are as reported by the reviewers.

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
  - ADR-0013's description of the lock, even as corrected by its edits, describes intended behaviour
    rather than effective behaviour. Check its wording when you approve those edits.
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
  - DD-11 cites `ADR-0001:147-165` and `:402`, which are now lines 170-188 and 424-426.
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
- **F-21. PRD-030** should name ADR-0014, ADR-0032 and ADR-0039 in its "Decisions Carried" table if D-4
  is approved.


## Per-ADR summary

| ADR | File | Verdict | Edits | Lines after |
| --- | --- | --- | --- | --- |
| ADR-0001 | 0001-playwright-e2e-testing-infrastructure.md | EDITS | 4 | 670 |
| ADR-0002 | 0002-retry-after-client-cooldown-handling.md | EDITS | 1 | 138 |
| ADR-0003 | 0003-public-tls-terminates-at-the-container-apps-ingress.md | EDITS | 6 | 107 |
| ADR-0004 | 0004-the-api-trusts-the-caller-address-and-scheme-from-the-ingress.md | EDITS | 4 | 117 |
| ADR-0005 | 0005-cross-origin-access-is-limited-to-configured-browser-origins.md | EDITS | 3 | 116 |
| ADR-0006 | 0006-optimistic-concurrency-is-checked-in-the-application.md | EDITS | 4 | 118 |
| ADR-0007 | 0007-each-request-resolves-the-callers-permissions-from-the-database.md | EDITS | 6 | 132 |
| ADR-0008 | 0008-access-tokens-are-short-lived-and-backed-by-a-server-side-session.md | EDITS | 8 | 136 |
| ADR-0009 | 0009-access-tokens-are-rejected-once-their-session-or-user-version-changes.md | EDITS | 8 | 127 |
| ADR-0010 | 0010-platform-administration-is-granted-by-deployment-configuration.md | EDITS | 8 | 124 |
| ADR-0011 | 0011-every-endpoint-is-authenticated-unless-explicitly-anonymous.md | EDITS | 7 | 110 |
| ADR-0012 | 0012-one-chained-rate-limit-policy-partitions-by-caller.md | EDITS | 5 | 105 |
| ADR-0013 | 0013-passwords-are-hashed-with-the-framework-hasher-and-reset-by-single-use-code.md | EDITS | 10 | 134 |
| ADR-0014 | 0014-every-failure-is-written-as-a-problem-details-document.md | MAJOR | 7 | 127 |
| ADR-0015 | 0015-expected-business-failures-are-returned-as-results.md | EDITS | 6 | 109 |
| ADR-0016 | 0016-validation-failures-answer-400-with-an-errors-map.md | EDITS | 7 | 112 |
| ADR-0017 | 0017-only-guid-row-identifiers-are-exposed.md | EDITS | 3 | 100 |
| ADR-0018 | 0018-the-openapi-document-is-generated-at-build-time-and-committed.md | EDITS | 5 | 109 |
| ADR-0019 | 0019-endpoints-are-minimal-api-slices-with-their-own-models.md | EDITS | 4 | 116 |
| ADR-0020 | 0020-the-server-is-layered-with-feature-slices-mirrored-across-layers.md | EDITS | 6 | 112 |
| ADR-0021 | 0021-entity-framework-core-is-the-persistence-mechanism.md | EDITS | 4 | 97 |
| ADR-0022 | 0022-postgresql-is-the-relational-datastore.md | EDITS | 8 | 99 |
| ADR-0023 | 0023-services-access-data-through-feature-repositories.md | EDITS | 4 | 119 |
| ADR-0024 | 0024-transactions-are-explicit-and-only-where-writes-must-be-one-unit.md | EDITS | 3 | 113 |
| ADR-0025 | 0025-tenant-confinement-is-structural-query-filters-and-site-scoped-reads.md | EDITS | 6 | 122 |
| ADR-0026 | 0026-request-shape-is-validated-at-the-edge-and-rules-in-check-chains.md | EDITS | 8 | 114 |
| ADR-0027 | 0027-enumerations-are-stored-by-name-and-uniqueness-is-enforced-by-the-database.md | EDITS | 5 | 116 |
| ADR-0028 | 0028-cascades-are-restricted-and-deletion-is-an-explicit-ordered-sweep.md | EDITS | 6 | 118 |
| ADR-0029 | 0029-migrations-run-in-a-separate-process-before-the-api-starts.md | EDITS | 5 | 112 |
| ADR-0030 | 0030-background-work-runs-inside-the-api-process.md | EDITS | 6 | 113 |
| ADR-0031 | 0031-email-is-queued-in-process-and-rendered-from-razor-components.md | EDITS | 6 | 111 |
| ADR-0032 | 0032-logs-record-request-metadata-and-never-submitted-values.md | EDITS | 5 | 116 |
| ADR-0033 | 0033-tests-are-per-layer-unit-projects-plus-one-hosted-integration-project.md | EDITS | 6 | 111 |
| ADR-0034 | 0034-secret-values-live-outside-the-repository.md | EDITS | 4 | 106 |
| ADR-0035 | 0035-builds-and-deployments-are-run-by-hand.md | EDITS | 5 | 105 |
| ADR-0036 | 0036-production-runs-two-container-apps.md | EDITS | 6 | 113 |
| ADR-0037 | 0037-the-client-is-organised-by-feature-over-shared-layers.md | EDITS | 4 | 112 |
| ADR-0038 | 0038-server-state-lives-in-a-query-cache-over-one-http-client.md | EDITS | 7 | 115 |
| ADR-0039 | 0039-api-hooks-return-results-and-one-interceptor-normalises-failures.md | EDITS | 4 | 106 |
| ADR-0040 | 0040-the-access-token-lives-in-memory-with-proactive-and-reactive-refresh.md | EDITS | 7 | 119 |
| ADR-0041 | 0041-browser-storage-is-keyed-by-environment-and-user.md | MAJOR | 9 | 116 |
| ADR-0042 | 0042-the-pwa-precaches-the-shell-only-and-enforces-updates.md | EDITS | 10 | 118 |
| ADR-0043 | 0043-end-to-end-tests-run-on-one-shared-stack.md | EDITS | 7 | 110 |
| ADR-0044 | 0044-every-end-to-end-test-declares-whether-it-may-run-beside-others.md | MAJOR | 15 | 118 |
| ADR-0045 | 0045-the-end-to-end-matrix-runs-on-every-change-with-a-stricter-release-profile.md | MAJOR | 11 | 113 |
| ADR-0046 | 0046-tenants-are-provisioned-and-destroyed-by-the-products-own-routes-in-tests.md | EDITS | 10 | 122 |
| ADR-0047 | 0047-a-rate-limited-caller-is-told-when-to-retry.md | EDITS | 8 | 113 |

## Index of every edit

| Edit | Kind | Severity | Tags | Why (short) |
| --- | --- | --- | --- | --- |
| E-0001-01 | front-matter | consistency | STATUS-LINK | W4 records that the bounded end-to-end records supersede this one, and its decision statements on test classes, |
| E-0001-02 | replace | format |  | The README allows an accepted record to change only by a dated clarification. Only the first entry carries a date. |
| E-0001-03 | insert-after | factual |  | The body has more errors that the clarifications do not cover: the Vite version; the "platform admin" presented as |
| E-0001-04 | replace | factual |  | The inline note says PRD-030 made the 422 → 400 change. The change was made in commit `bec69a48` on 2026-10-16, |
| E-0002-01 | replace | factual |  | The clarification says the 429 body holds those four members "only"; it also carries `traceId`, which the framework writer adds to every problem body. |
| E-0003-01 | front-matter | format | DATE | `date` is the writing date. The earliest record of the choice in code is the commit that explains why `UseHttpsRedirection()` is left out, one day aft… |
| E-0003-02 | replace | consistency |  | The issuer is recorded. The owner-confirmed deployment record (SQ-38) and the tracked diagram both name Let's Encrypt, so "not recorded" is false. (Se… |
| E-0003-03 | replace | factual |  | The image does not set the listening address. Only the compose file sets `ASPNETCORE_URLS=http://+:5241`; the Dockerfile only `EXPOSE`s 5241. The depl… |
| E-0003-04 | replace | unsupported-history |  | The owner's recollection of why this option was chosen is recorded, and the record leaves it out. The first correction pass removed "it is the platfor… |
| E-0003-05 | replace | factual |  | As configured, `UseHttpsRedirection()` would not redirect. No HTTPS port is set anywhere: compose and launchSettings are HTTP-only and there is no `HT… |
| E-0003-06 | replace | consistency |  | This bullet states network confinement as fact. The Consequences section of the same record (and SQ-38) treats it as an assumption the repo cannot sho… |
| E-0004-01 | front-matter | format | DATE | The decision was made and implemented in one dated commit. |
| E-0004-02 | replace | unsupported-history |  | "Unreachable except through the ingress" goes further than the evidence. The record confirms only that the ingress is the sole public route; it does n… |
| E-0004-03 | replace | consistency | TEMP-DOC-CITE | "Needs no change" settles a follow-up that is still open, and it hides the local effect: every anonymous caller shares nginx's rate-limit partition. |
| E-0004-04 | replace | confirmation |  | The fixture tests more than the scheme. It also proves the address half, including the driver "a caller must not be able to choose its own address", a… |
| E-0005-01 | front-matter | format | DATE | Configured origins with credentials arrived in November 2025, so `2026-11-04` is not the decision date. (Later refinements are listed in Notes.) |
| E-0005-02 | replace | factual |  | Local development is not cross-origin. The compose client's nginx proxies `/api/` to the server, and the Vite dev server is configured with an `/api` … |
| E-0005-03 | replace | confirmation |  | The splitting, trimming and de-duplication rules are tested in `CorsConfigurationFixture`, which the record does not name. "Validated on start" is pro… |
| E-0006-02 | replace | unsupported-history |  | The owner gave this reason: database independence, and unfamiliarity with database tokens made the application check the simpler choice. The claim tha… |
| E-0006-03 | replace | factual |  | A 409 body carries the problem `type` and `detail` only. The property name goes into the response only for a 400 (the `errors` map), so no `etag` fiel… |
| E-0006-04 | replace | factual |  | Leaving the tag check out of a path does not leave that path with an empty chain. Its other checks still run (Accounts, for example, also has `CheckDe… |
| E-0006-05 | replace | omission |  | The existing `DbUpdateConcurrencyException` mapping answers 409 with the `data-conflict` type, not `stale-entity-tag`. Adopting the token would theref… |
| E-0007-01 | front-matter | consistency | DATE | `date` is the writing date. The handler that resolves permissions from the database per requirement dates from the first JWT authorization work. PRD-0… |
| E-0007-02 | insert-after | omission |  | The record gives no evidence for its rationale, yet a completed PRD states it in so many words. The owner's recollection (SQ-29) is the first of those… |
| E-0007-03 | replace | format |  | The template requires "Chosen option: n, because …". The record gives no reason, though the evidence above supplies one. |
| E-0007-04 | replace | factual |  | The cost is one read per declared requirement, not per request. The handler creates a scope and calls `GetPermissionsAsync` for each `PermissionRequir… |
| E-0007-05 | replace | factual | TEMP-DOC-CITE | (1) The grant changes when configuration changes and the API restarts, which is not necessarily a redeploy. The options are bound once into a singleto… |
| E-0007-06 | replace | confirmation |  | The gap claims the matrix covers resolution, which it does not. The matrix takes each caller's expected permission set from `PermissionService` itself… |
| E-0008-01 | front-matter | consistency | DATE | `date` is the writing date. The server-side session this record decides (one AuthSession row per sign-in) was delivered by PRD-001 and PRD-002. The sh… |
| E-0008-02 | replace | factual |  | Bearer validation does not set `ClockSkew`, so the framework's default five-minute skew applies and a token is accepted up to about twenty minutes aft… |
| E-0008-03 | insert-after | unsupported-history |  | A shipped design was replaced: one refresh token on the user row. It is the evidenced status quo this decision replaced, and the record does not menti… |
| E-0008-04 | insert-after | unsupported-history |  | Adds the real former design as an option (see E-0008-03). |
| E-0008-05 | insert-after | unsupported-history |  | Pros and cons for option 5, taken from the symptom PRD-002 records. |
| E-0008-06 | replace | omission |  | For a security record, "cannot read it" alone misleads. Script on the page can still use the cookie, because the browser attaches it to the refresh PO… |
| E-0008-07 | insert-after | omission |  | The code shows three material consequences. (1) Session expiry slides on every refresh, with no absolute lifetime and no purge. (2) The raw refresh to… |
| E-0008-08 | replace | confirmation |  | No fixture asserts the fifteen-minute lifetime or the claim set, which are the parts the Review rule protects. |
| E-0009-01 | front-matter | consistency | DATE | `date` is the writing date. The per-session half, which completes the decision in the title, was added on 2026-10-17 with no PRD. The global token ver… |
| E-0009-02 | insert-after | unsupported-history |  | The evidenced history is missing. The record does not mention that global revocation came first or that per-session rejection was added later to close… |
| E-0009-03 | replace | factual |  | On an `AllowAnonymous` route (logout, refresh), failed token validation does not produce `401`; the request continues unauthenticated. |
| E-0009-04 | replace | factual |  | A password change also revokes every unrevoked session. Without that, a refresh would mint a token carrying the new version, so revoking the sessions … |
| E-0009-05 | insert-after | omission |  | Two consequences the code shows are missing. (1) A completed password reset neither bumps the version nor revokes sessions, so the Context claim ("A p… |
| E-0009-06 | insert-after | unsupported-history |  | Adds the real former design (version only) as an option. It is the evidenced status quo before 2026-10-17. |
| E-0009-07 | insert-after | unsupported-history |  | Pros and cons for option 5. |
| E-0009-08 | replace | confirmation |  | `LogoutFixture` directly asserts per-session revocation and the other device's survival, so the record should cite those tests. No fixture presents a … |
| E-0010-01 | front-matter | format | DATE | The configured platform-admin list arrived with the sign-up approval flow. |
| E-0010-02 | replace | factual |  | The summary presents recovering a locked-out site as a need this authority meets. Today `platform:manage` guards only the two approvals routes. Recove… |
| E-0010-03 | insert-after | omission |  | Without this, a reader would believe the two cases listed are handled today. |
| E-0010-04 | replace | factual |  | There are four refusals, not two. None protects a platform administrator in general; they protect only the last *enabled* one. "A site holding one can… |
| E-0010-05 | replace | factual |  | The recovery case does not exist yet, so "the recovery case it exists for" overstates what the code does. |
| E-0010-06 | replace | factual |  | The bullet contradicts itself ("must know them before the first sign-in" vs "discovered ... after the account exists"). The actual bootstrap also need… |
| E-0010-07 | replace | confirmation |  | The record should name the actual fixtures. The "parsing" gap is too broad: integration hosts already parse multi-id lists (for example `DeleteUserFix… |
| E-0010-08 | replace | consistency |  | The record refers to the permission-resolution decision without linking it, although ADR-0007 links back to this one. The planned cross-tenant scope (… |
| E-0011-01 | front-matter | consistency | DATE | `date` is the writing date. The fallback and the endpoint test were decided in PRD-026 §4.3. That PRD is Complete and was last updated 2026-10-14; the… |
| E-0011-02 | replace | factual |  | The public set also includes password reset and logout. |
| E-0011-03 | insert-after | unsupported-history |  | The record argues hypothetically for a failure that actually happened and is documented. SQ-28 cites the incident. |
| E-0011-04 | replace | factual |  | In Development, the OpenAPI document and the Scalar page are also `AllowAnonymous()`, so "nine" holds only outside Development. |
| E-0011-05 | replace | factual |  | The test requires each endpoint to declare its own `IAuthorizeData` and does not accept reliance on the fallback. So in the test host no route relies … |
| E-0011-06 | replace | factual |  | "the profile and user routes" overstates. Only the three `/api/me` routes and `PUT /api/users/{id}` use `AuthenticatedUser`; the other user routes nee… |
| E-0011-07 | replace | confirmation |  | The bullet omits the first assertion, which enforces the rule directly. The theory checks "contains", not "exact", on 8 routes. The host runs as Produ… |
| E-0012-01 | front-matter | consistency | DATE | `date` is when the decision was made; the policy, its partitions and its limits arrived in one commit. |
| E-0012-02 | replace | unsupported-history |  | Says lockout "is the wrong one here", but the owner says lockout was deferred, not rejected (as the record's own More Information says). The record al… |
| E-0012-03 | replace | consistency |  | The driver says limits "should be configurable", which contradicts the record's own Review rule and the code: the production values are constants, and… |
| E-0012-04 | replace | factual |  | "a Problem Details body that names the wait" is false for an authenticated rejection, whose detail is untimed, and "carries the detail without the hea… |
| E-0012-05 | replace | factual |  | With `QueueLimit = 0`, every rejection returns at once, so it cannot be reported as a timeout. The client timeout is 30 s, not tied to the window. The… |
| E-0013-01 | front-matter | consistency | DATE | `date` is the writing date. The hasher dates from 2025-09-12. The single-use code entity, which completes the decision in the title, dates from 2025-1… |
| E-0013-02 | replace | factual |  | A reset request names a username, not an address. The handler and service take `Username` only. |
| E-0013-03 | replace | factual |  | No Identity package is referenced. `PasswordHasher<T>` ships in the ASP.NET Core shared framework, which the Web SDK project gets implicitly. |
| E-0013-04 | replace | factual |  | Three wrong attempts fail that code, and the caller must request a new one. The five-minute "too many attempts" refusal applies only once three reques… |
| E-0013-05 | replace | omission |  | The hasher is constructed with default options, so a framework release that raises the default iteration count would produce `SuccessRehashNeeded` for… |
| E-0013-06 | replace | factual |  | The sweep does not clean anything up; it only flips `Active` codes to `Expired` every five minutes. Rows are deleted only when their user is deleted. |
| E-0013-07 | insert-after | omission | TEMP-DOC-CITE | (1) The reset's enumeration safety is undermined elsewhere: a sign-up request answers `UsernameTaken`. (2) The plaintext-and-logged code state belongs… |
| E-0013-08 | replace | confirmation |  | "The authentication fixtures" names no fixture. The unit fixture substitutes the hasher, so the real hash round trip is exercised only by the integrat… |
| E-0013-09 | delete | format |  | E-0013-07 moves this into Consequences with specifics. A "Known gap" about storage is not a confirmation mechanism. |
| E-0013-10 | replace | omission |  | A reader would assume a reset ends sessions as a password change does. It does not (see E-0009-05). |
| E-0014-01 | front-matter | consistency | DATE, RELATED-PRDS | `date` should be when the decision was made. Option 1 ("written by the application even where the framework would not write one") became true in `50ff… |
| E-0014-02 | replace | factual |  | No error code is on the wire, a `400` has no `detail`, and POT publishes a `type` only for conflicts (a `429` has none). |
| E-0014-03 | insert-after | format |  | The record is backfilled and must say so in Context and name its sources. |
| E-0014-04 | replace | factual |  | The driver asks for a type "per failure kind". Only the conflict categories get one, and the outcome says so. |
| E-0014-05 | replace | factual |  | The outcome lists `type`, `title` and `detail` as on every failure, and says a validation failure and a refusal are told apart by `detail`. A `400` ha… |
| E-0014-06 | replace | factual |  | The client can branch on `type` only for conflicts. It reads `type` only to recognise the stale tag. |
| E-0014-07 | replace | confirmation |  | The validation-body test is unnamed; the real guard is the shared helper. `EndpointResponseMetadataFixture` pins only the shared failure declarations,… |
| E-0015-01 | front-matter | consistency | DATE | `date` should be when services began returning `EnrichedResult`. |
| E-0015-03 | replace | factual |  | The failure is not always an `ApiDetailError`. `ApiBasicError` and `ApiDetailErrorCollection` are also used. The error does not carry the status it sh… |
| E-0015-04 | replace | omission |  | The record says refusals are values and faults are exceptions. Two kinds of refusal arrive as exceptions and are converted centrally, which is the pat… |
| E-0015-05 | replace | omission |  | The structural distinction holds only for refusals a feature defines. A race the database detects is a refusal that arrives as an exception. |
| E-0015-06 | replace | consistency |  | "cannot be forgotten" contradicts the record's own Bad consequence ("an ignored result is a silent success path"). Nothing forces a caller to handle t… |
| E-0015-07 | replace | unsupported-history |  | "no measurement of it was taken, so it is not offered as a reason" claims an absence the record cannot show, and it sets aside a reason the owner gave… |
| E-0016-01 | front-matter | consistency | DATE | The decision has an exact date: the commit that made the switch. |
| E-0016-02 | insert-after | format |  | The record is backfilled and must say so and name its sources. It should also say what the prior state was (the commit proves it) and that no rational… |
| E-0016-03 | delete | unsupported-history |  | No evidence (commit, PRD, interview) supports proxy or tooling handling as a reason for the switch. |
| E-0016-04 | replace | factual |  | A `400` carries no `detail`. A refusal's message goes into `errors` under its field, or the empty key. The error code was removed by PRD-030 (B9) a we… |
| E-0016-05 | replace | factual |  | "the `detail` is the only signal" is false, because a `400` has no `detail`. |
| E-0016-06 | replace | confirmation |  | "the validation fixtures" is unnamed. The real guards are the shared helper, its callers, and `EnrichedErrorExtensionsFixture`, which pins refusals in… |
| E-0016-07 | replace | factual |  | A refusal cannot carry its meaning in `detail`, which a `400` never has. |
| E-0017-01 | insert-after | format |  | A backfilled record must say so in Context and name its sources. The owner's reason (SQ-14) is available. |
| E-0017-02 | replace | factual |  | `RowId` is not always generated on insert. An import creates rows with the `RowId` from the export file, so linked incomes and expenses resolve. Token… |
| E-0017-03 | insert-after | confirmation |  | The only mechanical guards are the instruction rule and the `:guid` route constraint. The record names neither. |
| E-0018-01 | insert-after | format |  | Backfilled with no statement. The owner's account (SQ-25) contradicts the record's implied need, so it belongs in Context. |
| E-0018-02 | replace | unsupported-history |  | "The clearest description of the contract that exists" is an inferred reason. The instructions and the owner state different ones. |
| E-0018-03 | replace | factual |  | A missing diff is not visible. Committing the regenerated file is manual and unenforced, as the record's own Known gap and the instructions say. |
| E-0018-04 | replace | confirmation |  | `EndpointResponseMetadataFixture` does not pin which failures each endpoint declares. It asserts shared rules over every endpoint, plus the success sh… |
| E-0018-05 | delete | factual |  | Client types are hand-written, and the document generates nothing for the client. Per-endpoint declarations therefore cannot affect which bodies the c… |
| E-0019-01 | insert-after | format |  | Backfilled with no statement. The owner's reasons (SQ-03, SQ-04, SQ-05) are recorded and match the drivers. |
| E-0019-02 | replace | factual |  | Entity checks are not in the operation's endpoint folder. The host folder holds the handler, request, response, request mapping and validator. The che… |
| E-0019-03 | replace | factual |  | Only the standard failures (400 validation problem and 500) and the rate-limit policy are declared per group. Each operation declares its own success … |
| E-0019-04 | replace | confirmation |  | The stated reason is wrong (the operation makes its own failure declarations). An existing test mechanically checks per-operation models, and the reco… |
| E-0020-01 | insert-after | format |  | Backfilled with no statement. The owner's account (SQ-01, SQ-02) is recorded. |
| E-0020-02 | replace | factual |  | Request validation is not in the application project. All 24 `RequestValidator` classes live in the host beside their endpoints, which is what ADR-002… |
| E-0020-03 | replace | factual |  | The host references the data project directly, and its auth services use repositories without going through the application. A reader would otherwise … |
| E-0020-04 | replace | unsupported-history |  | The "because" is an inferred reason. No evidence records why authentication was placed in the host. |
| E-0020-05 | replace | unsupported-history |  | "Deliberate" is not evidenced. The direct data access (E-0020-03) is the material part of this exception. |
| E-0020-06 | replace | confirmation |  | The first bullet is half wrong and contradicts the record's own Consequence ("enforced by project references"). A reference from a lower project to a … |
| E-0021-01 | replace | unsupported-history |  | "familiarity matters more than the last increment of control" is an inferred trade-off. The owner's account (SQ-40) gives prior experience, built-in m… |
| E-0021-02 | replace | factual |  | The four query filters are not a central convention. Each is declared per entity in `PotDbContext`. The central conventions in `DbContextBase` are tab… |
| E-0021-03 | replace | omission | TEMP-DOC-CITE | The in-memory provider ignores `citext`, unique indexes and SQL translation. Whether this split is the intended test strategy is still open (DD-12d), … |
| E-0021-04 | replace | consistency |  | The "separate decisions" exist as ADRs and should be linked. |
| E-0022-01 | insert-after | unsupported-history |  | The record is backfilled but does not say so. The owner's reason (SQ-39) is cost and open source. The schema features are presented as the drivers of … |
| E-0022-02 | delete | unsupported-history |  | Nothing in the owner's account or the repository shows "managed hosting" as a driver, and the repository cannot confirm that the production server is … |
| E-0022-03 | replace | unsupported-history |  | Same reason as E-0022-02. "Managed server" states an Azure setting that cannot be confirmed from the repository. |
| E-0022-04 | replace | unsupported-history |  | The rationale credits `citext` for the choice, but the owner's reason was cost. The claim that the index replaces "application comparison" is false: a… |
| E-0022-05 | replace | factual |  | The schema has no partial (filtered) indexes. Uniqueness is not enforced "without application code", because application checks come first. The "manag… |
| E-0022-06 | replace | factual |  | 17.6 comes only from a diagram label, which the brief requires to be attributed. "Pinned" overstates `SetPostgresVersion`, which is a compatibility ta… |
| E-0022-07 | replace | factual |  | The schema uses no partial indexes, so "partial-index behaviours the schema uses" is false. |
| E-0022-08 | replace | consistency | TEMP-DOC-CITE | The open item is DD-06, and the uniqueness decision is ADR-0027. Both should be linked. "Rather than only by application checks" also misstates SQ-18 … |
| E-0023-01 | insert-after | format |  | The record is backfilled but does not say so. The owner's reasons are recorded and should be cited, and the other options have no evidence of being we… |
| E-0023-02 | replace | factual |  | The claim holds only for users and sites. The account, expense, income and role repositories expose a queryable that services compose. The Decision Ou… |
| E-0023-03 | replace | confirmation |  | The cited test does not enforce the rule. Nothing fails if a service injects the public `PotDbContext`. Four `Pot.App.Tests` fixtures use the in-memor… |
| E-0023-04 | replace | consistency |  | The "separate decisions" exist as ADRs and should be linked. |
| E-0024-01 | insert-after | format |  | The record is backfilled but does not say so. The owner's reason (SQ-12) should be cited, and the other options have no evidence of being weighed. |
| E-0024-02 | replace | factual |  | As written, the statement is false. Two operations save twice without a transaction, because their first save is independent housekeeping (expiring st… |
| E-0024-03 | replace | confirmation |  | The "Test:" bullet describes a test that does not exist. The gap line also says the import transaction is "covered through the version gate", which is… |
| E-0025-01 | front-matter | consistency | RELATED-PRDS | The users-and-sites half of this decision, the refusal contract and the rejection of a `User`/`Site` filter all come from PRD-026, which is Complete. |
| E-0025-02 | insert-after | format |  | The record is backfilled but does not say so. PRD-026 is real evidence for part of the history and should be cited. Option 2 was in fact considered an… |
| E-0025-03 | replace | factual |  | Not every cross-tenant read is "named as such". `GetByUsernameAsync`, `GetByUsernameWithSiteAsync` and `GetSecurityStateAsync` have neutral names and … |
| E-0025-04 | insert-after | omission |  | The PRD itself bounds the guarantee to the repository layer. `PotDbContext` is public and exposes `Users`. A reader of "isolation that a feature canno… |
| E-0025-05 | replace | confirmation |  | Two claims overstate. First, `CrossTenantAuthorizationFixture` covers only user and site routes (its own summary says so), and the normalised body com… |
| E-0025-06 | replace | consistency |  | The "separate decision" exists as an ADR and should be linked. |
| E-0026-01 | replace | factual |  | Checks do need the database. For example, `CheckDescriptionDoesNotExist` queries the repository. Read literally, "refusals that do not need a database… |
| E-0026-02 | replace | factual |  | A refused data-dependent rule of type BadRequest or Constraint is written as a `400` with the same `errors` map as a shape failure. Only 404 and 409 d… |
| E-0026-03 | insert-after | format | DEPENDS-ON E-0026-02 | Backfilled with no statement. The owner's account (SQ-08) supports the data-annotations option. |
| E-0026-04 | replace | factual |  | Authentication (session and token-version check) and permission resolution read the database on every authorised request before the handler runs. So "… |
| E-0026-05 | replace | factual |  | Five requests have no validator, and not all of them carry only "an identifier or a flag". One is an uploaded file (import), and two carry an enumerat… |
| E-0026-06 | replace | factual |  | Same as E-0026-04. A malformed authenticated request still costs the auth and permission reads. |
| E-0026-07 | replace | confirmation |  | The "handler fixtures" do not exist. `Pot.AspNetCore.Tests` has one handler fixture (Export), and it does not test validation. The 400 is asserted by … |
| E-0026-08 | replace | consistency |  | "Testing a rule needs a repository substitute" applies equally to the chosen option. Checks take repositories, and their fixtures use the in-memory pr… |
| E-0027-01 | replace | format |  | Backfilled record does not say so or name its sources (adr skill hard rule). |
| E-0027-02 | replace | factual |  | The indexes are not all per-site: expense/income descriptions are unique per account, site names and usernames are unique across the database. |
| E-0027-03 | replace | factual |  | An enriched enum carries one name (member name by default, or explicit), not separate display and storage names; and roles/permissions are not "the ta… |
| E-0027-04 | replace | factual |  | There is no longer an "unscoped case" for settings: `SiteId` was made required, so the global partial index can match no row. The setting key is case-… |
| E-0027-05 | replace | confirmation |  | Confirmation names no fixture; the cited check fixture asserts property name and message, not an error code (PRD-030 removed `errorCode`), and runs on… |
| E-0028-01 | front-matter | consistency | RELATED-PRDS | The hard-delete, no-soft-delete and sweep decisions were made in PRD-022 (Complete), which carries "Removal type: Permanent hard delete" and lists sof… |
| E-0028-02 | replace | format |  | Backfilled record does not say so or name its sources. |
| E-0028-03 | replace | factual |  | (a) Restrict is applied by convention to every FK on an entity type, not only owner FKs, and that convention is the mechanism worth naming. (b) Accoun… |
| E-0028-04 | replace | omission |  | The sweep materialises and tracks every dependent row before deleting it, which is the real cost for a large site. |
| E-0028-05 | replace | confirmation |  | `DeletionSweepFixture` is correctly cited but under-described; `IsolationFixture` (with `OrphanSweep` and model-derived tables) is the mechanical guar… |
| E-0028-06 | replace | factual |  | The sweeps use no explicit transaction; atomicity comes from the single `SaveAsync`, as this record's own Decision and Gap say. ADR-0024 covers when a… |
| E-0029-01 | replace | format |  | Backfilled record does not say so or name its sources; the owner's stated reasons (SQ-19, SQ-20) are absent. |
| E-0029-02 | replace | factual |  | The driver as written says the opposite of what is meant. |
| E-0029-03 | replace | omission |  | The record presents the additive-step rule as practised without exception; history shows two column drops shipped in the same change as the code, and … |
| E-0029-04 | replace | factual |  | The code answers the conditional: the runner catches every exception, logs it and sets exit code -1, which the AllOverIt console host returns; `set -e… |
| E-0029-05 | replace | confirmation |  | Name the fixture, say what it proves (EF Core refuses `MigrateAsync` on pending model changes, observed in PRD-021's delivery) and what it does not (t… |
| E-0030-01 | replace | factual |  | The OTP worker updates status to Expired (it does not delete), mail is drained as it arrives rather than on a schedule, and the cadences are known. |
| E-0030-02 | replace | format |  | Backfilled record does not say so or name its sources. |
| E-0030-03 | replace | factual |  | "The single-replica assumption is what makes the scheduling safe" is false for the reminder: it duplicates on a restart inside the trigger hour and mi… |
| E-0030-04 | replace | factual |  | An async hosted service does not hold a "request thread pool slot"; it shares the thread pool and the database connection pool with requests while it … |
| E-0030-05 | replace | confirmation |  | Neither worker fixture asserts "does not run when the host is stopping"; the drainer is not "covered through the channel's tests" — no fixture exercis… |
| E-0030-06 | replace | unsupported-history |  | PRD-019 keeps the workers in the API process ("No ... new deployables"), so it would not supersede in-process hosting; the owner said PRD-019 "does no… |
| E-0031-01 | replace | factual |  | Bodies are not type-checked: values pass to the component as a name-keyed dictionary at run time, and the plain-text body is a `.text` template with s… |
| E-0031-02 | replace | factual |  | The implementation does not make a wrong property a build error (see E-0031-01), so this cannot be a driver the decision satisfies, and no evidence re… |
| E-0031-03 | replace | factual |  | The subject is a constant per type, and only the HTML body comes from a Razor component. |
| E-0031-04 | replace | factual |  | A renamed property does not break the build. A dictionary key the component does not declare makes `SetParametersAsync` throw at render, which the cha… |
| E-0031-05 | replace | confirmation |  | The cited "channel's fixtures" do not exist — there is no `Pot.EmailSender` test project and no test references `SendEmailChannel`. Name the tests tha… |
| E-0031-06 | replace | consistency |  | PRD-019 leaves OD-03 pending (scheduled-only vs all types), so the outbox may replace the channel only for scheduled mail; "would supersede this recor… |
| E-0032-01 | front-matter | consistency | DATE, RELATED-PRDS | The decision was made in PRD-030 (Complete) and delivered on 2026-10-22; the record carries neither. |
| E-0032-02 | replace | format |  | Backfilled record does not say so or name its sources; it omits that option 2 was effectively the status quo until PRD-030 (a raw body logger ran outs… |
| E-0032-03 | replace | consistency |  | "defects to be removed, not an accepted exception" presents DD-08 option B as decided; DD-08 is open ("Your call" blank). The list is also imprecise: … |
| E-0032-04 | replace | confirmation |  | Removal of the level entry is not silent to the suite: `RequestLoggingFixture` asserts it. |
| E-0032-05 | replace | confirmation |  | Name the enricher fixture and the level assertion, and record the gap that lets the call-logging exceptions exist. |
| E-0033-01 | insert-after | format |  | Backfilled with no statement. The owner's account (SQ-26) is recorded. |
| E-0033-02 | replace | omission |  | Two facts would change a reader's picture. Some application fixtures use real repositories over the in-memory provider, not substitutes. Every hosted … |
| E-0033-03 | replace | factual |  | The cost is a container start and full migration per test, not one per run. |
| E-0033-04 | replace | consistency |  | There are two problems. First, the Etag is not a database concurrency token (ADR-0006, DD-02), so concurrency does not differ between the in-memory an… |
| E-0033-05 | replace | confirmation |  | Name the metadata fixtures the bullet relies on, so the confirmation is checkable. |
| E-0033-06 | replace | confirmation |  | Existing unit tests legitimately assert status codes on the classes that map failures to statuses, so the review rule as written condemns correct test… |
| E-0034-01 | replace | factual |  | The template does not hold "every key the compose file substitutes". `POSTGRES_DB` (and `COMPOSE_PROJECT_NAME`) come from the tracked `.env`, and `IMA… |
| E-0034-02 | replace | omission |  | The tracked evidence for the production store (the diagram) should be named, and the record should state what the repo cannot show. The API has no Key… |
| E-0034-03 | replace | confirmation |  | The named "Test" does not exist: nothing runs the compose instructions. Real mechanical enforcement does exist (the ignore rule and startup validation… |
| E-0034-04 | replace | consistency |  | The record refers to the consuming deployment without linking it. |
| E-0035-01 | front-matter | format | DATE | The hand-run build-and-push tasks for Azure date from the first Azure build. |
| E-0035-02 | replace | unsupported-history |  | Nothing in the repo records cost as "the deciding factor": there is no SQ interview row and no dated note. The deployment plan tracked in history from… |
| E-0035-03 | insert-after | confirmation |  | The Confirmation section has no mechanical rule at all. Adding a pipeline file is the observable event that undoes this decision. |
| E-0035-04 | replace | consistency | TEMP-DOC-CITE | (1) "Expected to be revisited as the product grows" is unsupported; the actual status is an open, undecided brief (DD-05). (2) "A change to this recor… |
| E-0035-05 | replace | consistency |  | The record refers to the deployment topology without linking it. |
| E-0036-01 | front-matter | format | DATE | The two-app shape is fixed by the first Azure client build: `nginx.azure.conf` with no `/api` proxy, plus an absolute API base URL. |
| E-0036-02 | insert-after | omission |  | W4 row 0074 is still `Ver` because the tracked documents disagree. The diagram and the Docker guide say the ingress routes `/api/*` to the API contain… |
| E-0036-03 | replace | omission |  | The code shows why `SameSite` is load-bearing. The refresh cookie is `Lax`, which works cross-origin only because the client and API hosts are the sam… |
| E-0036-04 | insert-after | omission | TEMP-DOC-CITE | The record misses an important cost of independent deployment. Two separate tasks deploy the two apps, so the client and API versions can be skewed, a… |
| E-0036-05 | replace | confirmation |  | The named test does not exercise this arrangement. `playwright.prod.config.ts` builds the client and serves it with `vite preview`, whose proxy forwar… |
| E-0036-06 | replace | consistency |  | Both "separate decisions" exist as ADRs and should be linked. The build-and-publish decision is related too. |
| E-0037-01 | insert-after | format |  | The record is backfilled but does not say so or name its sources (skill hard rule). |
| E-0037-02 | replace | factual |  | The current text gets the dependency graph wrong (DD-13): |
| E-0037-03 | replace | factual |  | Removing a screen means deleting its folder and also editing the route table and the sidebar menu that import it. |
| E-0037-04 | replace | consistency |  | - "The existing ones are debt to be removed" treats DD-13 option C as decided, but DD-13 is open. |
| E-0038-01 | front-matter | consistency | DATE | `date` is the writing date. TanStack Query and the shared axios hooks first appear together in `7bb53c3b`. |
| E-0038-02 | insert-after | format |  | Backfilled record with no backfill statement or sources. |
| E-0038-03 | replace | factual |  | - The transport is the global default axios instance, configured through `axios.defaults`. It is not an instance made with `axios.create()`. |
| E-0038-04 | replace | factual |  | - Invalidation follows one central dependency table. Mutations do not each declare the full set of affected keys. |
| E-0038-05 | replace | factual |  | False as written. No query ever rejects, so TanStack Query never retries. There is also an important consequence the record leaves out: a failure is c… |
| E-0038-06 | replace | factual |  | Invalidation takes a typed union, `CacheKey`, so a typo there is a compile error. The silent miss is between a query's untyped `string[]` key and that… |
| E-0038-07 | replace | confirmation |  | "The mutation hooks are tested" is broader than the truth. Only a few mutation flows have invalidation fixtures, and the account, expense and income m… |
| E-0039-01 | front-matter | consistency | DATE, RELATED-PRDS | `date` is the writing date. PRD-030 (Complete) sets this decision's current requirements (E1–E8), and row 0058 cites it. |
| E-0039-02 | insert-after | format |  | Backfilled record with no backfill statement or sources. |
| E-0039-03 | replace | factual |  | "The typed value" suggests validation, but the body is only cast. That runtime check is the open item DD-07. |
| E-0039-04 | replace | factual |  | A caller cannot "read further into the result". A failure carries only `type`, `code` and `description`. A 400's field messages are joined into one st… |
| E-0040-01 | front-matter | consistency | DATE | `date` is the writing date. The memory-only token provider and the cookie-based refresh arrived together. |
| E-0040-02 | replace | omission |  | The record gives no timing for the proactive refresh, and it omits that the interceptor never retries `/auth/` routes. |
| E-0040-03 | replace | factual |  | The single-flight belongs to the interceptor only. "One logout path" is stated loosely: every sign-out reaches the `logoutManager` callback, which cal… |
| E-0040-04 | replace | factual |  | The start-up refresh has no unit fixture, but every authenticated E2E test starts a page with only the refresh cookie (a storage state from a fresh lo… |
| E-0040-05 | insert-after | omission |  | The timer and the start-up refresh post to `/auth/refresh` independently of the interceptor's guard, and each tab has its own guard. Because the serve… |
| E-0040-06 | replace | confirmation |  | The bullet names no fixture and overstates "ends the session through the same logout path": the interceptor test asserts `clearTokens`, and a separate… |
| E-0040-07 | replace | consistency |  | As written, the rule makes the deletion flows (E-0040-03) defects. The rule should target a sign-out that leaves a live server session behind. |
| E-0041-01 | front-matter | consistency | DATE, RELATED-PRDS | The decision came from PRD-001 and was implemented on 2026-05-03. |
| E-0041-02 | replace | factual |  | - Browser storage is already separated by origin, so a local dev server and a deployed site never share keys. The env marker only separates dev-mode a… |
| E-0041-03 | replace | unsupported-history |  | Option 2 is not hypothetical. It was the shipped shape that PRD-001 replaced, and the record should say so. |
| E-0041-04 | replace | factual |  | The decision statement is wrong. It says keys are env-scoped and not user-scoped, and that most settings persist. In the code, user scoping is the nor… |
| E-0041-05 | replace | factual |  | Separating a local environment from a deployed one comes from the origin, not from the key. The user separation applies once a user is signed in. |
| E-0041-06 | replace | consistency |  | The bullet contradicts the record's own later bullet: storage does hold the signed-in identity, which is server data, and its permission list. |
| E-0041-07 | replace | factual |  | - False as written: persisted preferences are user-scoped. |
| E-0041-08 | replace | confirmation |  | The lifetime of each setting does have fixtures: each consumer's test asserts its key and backend. The record should name them. |
| E-0041-09 | replace | factual |  | Filters *are* stored, in session storage. The URL is the render-time source, and the stored copy feeds the sidebar links. |
| E-0042-01 | front-matter | consistency | DATE | The decision took its current form, an enforced update, on 2026-09-22. The shell-only precache dates from 2026-03-30. |
| E-0042-02 | replace | factual |  | The reload is enforced, at 45 seconds without input or 5 minutes at the latest. It does not wait for the user to reload. |
| E-0042-03 | insert-after | format |  | Backfilled record with no backfill statement or sources. The git history shows a deferrable prompt that shipped and was later replaced. |
| E-0042-04 | insert-after | unsupported-history |  | The approach that shipped and was then dropped, a prompt the user could postpone, belongs in Considered Options (README: "record that in the Considere… |
| E-0042-05 | replace | factual |  | - The "two channels" are not the build id and the version manifest. Those are two halves of one channel. The other channel is the service worker's own… |
| E-0042-06 | replace | factual |  | The notice cannot be dismissed at all, not merely "not indefinitely". The real cost is a lost form, and nothing tracks unsaved changes. |
| E-0042-07 | replace | factual |  | The precache manifest is generated from the build output on every build, so it cannot drift from the build. The real gap is file types that `globPatte… |
| E-0042-08 | replace | confirmation |  | - The Gap names the wrong mechanism. Nothing excludes the API; it is uncached because there are no runtime routes. |
| E-0042-09 | replace | factual |  | Without a service worker, repeat loads would still reuse the hashed bundle, because nginx marks `/assets/` as immutable. What would be lost is install… |
| E-0042-10 | insert-after | unsupported-history |  | Pros and cons for the option added in E-0042-04, citing the evidence that it shipped and was replaced. |
| E-0043-01 | front-matter | consistency | DATE | `date` is when the decision was made. The shared stack is PRD-012 R1, approved and implemented by 2026-09-02, |
| E-0043-02 | front-matter | consistency | RELATED-PRDS, TITLE-AREA | W4 files this group as cross-cutting, and the stack spans the client, the API and the database. The decision comes |
| E-0043-03 | replace | consistency | STATUS-LINK | This is the other half of E-0001-01 (W4: this group supersedes ADR-0001). |
| E-0043-04 | replace | unsupported-history |  | "Starts in seconds" is not supported. Before the first test, setup starts two containers, runs migrations through |
| E-0043-05 | replace | factual |  | The setup order is misstated: the baseline SQL runs before the readiness wait, and only the import comes after it. |
| E-0043-06 | replace | confirmation |  | The first bullet is labelled "Test" but describes setup behaviour without naming it, and nothing listed |
| E-0043-07 | replace | consistency |  | The neighbouring decisions are named but not linked, and the record cites no source. |
| E-0044-01 | front-matter | factual | TITLE-AREA | The title states a practice that is neither required nor followed. W4's statement of the decision is |
| E-0044-02 | front-matter | consistency | DATE | The decision date is PRD-012 R6 (2026-09-02). PRD-031 later added the destructive class. |
| E-0044-03 | front-matter | consistency | RELATED-PRDS, TITLE-AREA | W4 files the group as cross-cutting. The two-class model comes from PRD-012 and the third class from PRD-031. |
| E-0044-04 | replace | consistency | STATUS-LINK | This is the other half of E-0001-01. Row 0071 is sourced from ADR-0001 §7. |
| E-0044-05 | replace | factual | TITLE-AREA | The H1 must match the title (E-0044-01). |
| E-0044-06 | replace | factual |  | The summary states the unsupported "stated at the top of the file" rule. It defines parallel-safe differently from |
| E-0044-07 | replace | factual |  | The suite is not a gate on every change. It is run deliberately, and the skills make it opt-in. |
| E-0044-08 | replace | factual |  | The destructive class covers identities as well as tenants. Account deletion shares the seeded site and runs in |
| E-0044-09 | replace | consistency |  | Option 1 omits the destructive class that the outcome then describes. |
| E-0044-10 | replace | factual |  | This replaces the "stated at the top of the file" rule with what the instructions and code actually do, and gives |
| E-0044-11 | replace | factual |  | "Says so" is true for fewer than half the files. What every writing file does show is its structure. |
| E-0044-12 | replace | factual |  | Every run starts from a fresh container, so data piles up only within a run. Two suites also keep their rows by |
| E-0044-13 | replace | confirmation |  | No instruction requires a header statement. The instruction file and AUTHORING are what define the classes and |
| E-0044-14 | replace | consistency |  | The heading and its bullets repeat the "declared" framing that E-0044-01 corrects. |
| E-0044-15 | replace | consistency |  | Neighbours are named but not linked; sources not cited. |
| E-0045-01 | front-matter | factual | TITLE-AREA | The title contradicts the skills and records an undecided trigger as decided. |
| E-0045-02 | front-matter | consistency | DATE | The two-profile decision is PRD-012 R7 and R8, approved 2026-09-02. The prodlike config was "validated |
| E-0045-03 | front-matter | consistency | RELATED-PRDS, TITLE-AREA | W4 files the group as cross-cutting; the source is PRD-012. |
| E-0045-04 | replace | consistency | STATUS-LINK | This is the other half of E-0001-01. Row 0072 is sourced from ADR-0001 §10 and §13. |
| E-0045-05 | replace | factual | TITLE-AREA | The H1 must match the title (E-0045-01). |
| E-0045-06 | replace | factual |  | The cost the record accepts is misstated. The skills treat prodlike as a gate, and what is actually accepted is |
| E-0045-07 | replace | consistency |  | Option 1 says "before a release" while the outcome says "completion gate". It should state the chosen shape |
| E-0045-08 | replace | factual |  | (a) None of the per-project exclusions is engine-specific. Two files mutate shared data and run on chromium only, |
| E-0045-09 | replace | factual |  | Prodlike runs the same four projects. Its exclusions are also not a subset of the dev ones: it drops |
| E-0045-10 | replace | confirmation |  | The skill step and the instruction lines are the mechanisms that apply this decision. The current timeout bullet |
| E-0045-11 | replace | consistency |  | Neighbours are named but not linked. "A separate decision" for CI is ADR-0035. |
| E-0046-01 | front-matter | consistency | DATE | The decision is PRD-031's. Its "Decisions Carried Into This Document" were committed on 2026-10-27, and the PRD was |
| E-0046-02 | front-matter | consistency | RELATED-PRDS, TITLE-AREA | The decision spans the browser suite and the server integration suite. W4 files it as cross-cutting, and the |
| E-0046-03 | replace | consistency | STATUS-LINK | This is the other half of E-0001-01. It reverses ADR-0001's "OTP / email flows deferred indefinitely" and the |
| E-0046-04 | replace | factual |  | The site-deletion journey reads the sign-up message (its one-time code and temporary password), not an |
| E-0046-05 | insert-after | omission |  | The Context leaves out the force PRD-031 and the instruction file both name: a seeded identity can be destroyed |
| E-0046-06 | replace | factual |  | (a) The sign-up message is read as well as the invitation. (b) The E2E check is narrower than "the seeded |
| E-0046-07 | replace | factual |  | The canary check covers three identity- and tenant-removing operations. Account, expense and income deletes are |
| E-0046-08 | replace | factual |  | The sink starts first in global setup and throws if it cannot start or its ports are taken, so the whole run |
| E-0046-09 | replace | confirmation |  | Name the fixture and the instruction files that actually enforce this. "Each destructive operation" overstates |
| E-0046-10 | replace | consistency |  | Neighbours are named but not linked, and the source PRD is not cited. |
| E-0047-01 | front-matter | consistency | RELATED-PRDS | The decision comes from PRD-014, as 0002 did. The skill says to set `related-prds` when a decision came from a PRD. PRD-014 is still a draft, which ma… |
| E-0047-02 | replace | factual |  | The server sends `Retry-After` only for the fixed window; an authenticated (sliding-window) 429 has none. |
| E-0047-03 | replace | factual |  | "it says for how long" is true only for an anonymous caller. |
| E-0047-04 | insert-after | omission |  | The record does not name its sources. It also leaves open ADR-0002's choice of the header as the canonical source (D-3), and it should say so. |
| E-0047-05 | replace | factual |  | "the first two implementations below" points to a list that does not exist. The OTP cooldown is not a `429`: it comes back on a successful response an… |
| E-0047-06 | replace | factual |  | The wait is in both places, or in neither: header and detail come from the same value, and an authenticated rejection has neither. |
| E-0047-07 | insert-after | omission |  | The no-retry property comes from how hooks are written, not from configuration. If a query function threw instead of resolving a `FailResult`, TanStac… |
| E-0047-08 | replace | confirmation |  | The interceptor fixtures do not assert that no retry happens. They assert only the mapping to `RateLimitedError` and the fixed copy. The server-side g… |

## Edits in full, by ADR

## ADR-0001 — 0001-playwright-e2e-testing-infrastructure.md

Verdict: EDITS

### E-0001-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: STATUS-LINK
- Severity: consistency
- Why: W4 records that the bounded end-to-end records supersede this one, and its decision statements on test classes,
  email flows and setup data are no longer true. Pairs with E-0043-03, E-0044-04, E-0045-04 and E-0046-03.
- Evidence: `DOCS-WORK-LISTS.md:94`, `:515`; README "Supersede when the decision statement is no longer true"; this
  record's own Clarifications (three classes; email and OTP flows no longer deferred).
- old:
~~~
status: accepted
date: 2026-09-02
area: cross-cutting
related-prds: [PRD-012]
~~~
- new:
~~~
status: superseded
date: 2026-09-02
area: cross-cutting
related-prds: [PRD-012]
superseded-by: [ADR-0043, ADR-0044, ADR-0045, ADR-0046]
~~~

### E-0001-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: format
- Why: The README allows an accepted record to change only by a dated clarification. Only the first entry carries a date.
- Evidence: `Docs/adr/README.md:80-81`; ADR-0001 lines 14–27 (one dated bullet, seven undated).
- old:
~~~
Recorded as the design has moved, rather than by rewriting the sections below.
~~~
- new:
~~~
Recorded as the design has moved, rather than by rewriting the sections below. The undated entries were recorded on
2026-11-04 with the first; later entries carry their own date.
~~~

### E-0001-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: factual
- Why: The body has more errors that the clarifications do not cover: the Vite version; the "platform admin" presented as
  a separate identity; a missing fixture; a §2 constraint that the provisioning helpers and fixture-managed suites now
  contradict; the sink product; and the npm scripts that the run-tests skill now requires.
- Evidence: `package.json:96` (`"vite": "^8.2.2"`); `e2e/seed/baseline.sql:48` (row id `58d1a6b2-…` is `e2e_admin`) and
  `playwright.config.ts:91` (`--PlatformAdmin:UserIds=58d1a6b2-…`); `e2e/fixtures/auth.ts:127` (`quickActionsTest`);
  `e2e/helpers/provisioning.ts:105-228` and `e2e/tests/approvals/approvals.test.ts:12-15` (setup data created through
  the API); `e2e/helpers/mailSink.ts:12-14` (`pot-e2e-mail-sink`, `maildev/maildev:2.2.1`); `package.json:33-37`;
  `.github/skills/run-tests/SKILL.md:48-50`.
- old:
~~~
- `e2e/tests/users/`, the account and site deletion journeys, and the newer helpers (`mail`, `mailSink`,
  `provisioning`, `radix`, `api`, `dates`) exist and are not listed in the directory tree below.
~~~
- new:
~~~
- 2026-11-05: The client uses Vite 8 (`vite` `^8.2.2` in `package.json`), not Vite 6 (§3).
- 2026-11-05: The platform administrator named in §3 and §5 is not a separate identity. `58d1a6b2-bf25-40c1-b3d6-656ccf4a68b5`
  is `e2e_admin`'s row id (`e2e/seed/baseline.sql`), and the `--PlatformAdmin:UserIds` argument in both Playwright
  configs makes that user a platform administrator.
- 2026-11-05: `e2e/fixtures/auth.ts` also exports `quickActionsTest` (`e2e_quickactions`), which §6.2 does not list.
- 2026-11-05: Tests create data through the API during setup. Fixture-managed suites create their own rows, and the
  destructive journeys provision a tenant by sign-up, approval or invitation (`e2e/helpers/provisioning.ts`). The §2
  constraint against ad-hoc API data creation in setup no longer holds.
- 2026-11-05: The mail sink is maildev (`maildev/maildev:2.2.1`, container `pot-e2e-mail-sink`), not MailPit
  (Appendix A, item 8).
- 2026-11-05: `package.json` also defines `e2e:all:dev:log`, `e2e:preflight`, `e2e:clean` and `e2e:scripts`, which
  §10.1 does not list. The run-tests skill runs `e2e:preflight` before every E2E run.
~~~

### E-0001-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The inline note says PRD-030 made the 422 → 400 change. The change was made in commit `bec69a48` on 2026-10-16,
  without a PRD, and PRD-030 was not added until 2026-10-20. Correcting a wrong link is a permitted change to an accepted
  record.
- Evidence: `DOCS-WORK-LISTS.md:637-643` ("ADR-0001:373 wrongly attributes the 422 change to PRD-030");
  `git show bec69a48` ("Refactored API to return 400 instead of 422", 2026-10-16); `git log --diff-filter=A` on PRD-030
  → `278a7777` 2026-10-20.
- old:
~~~
records `422`, which [PRD-030](../Future/PRD-030-problem-details-consolidation.md) replaced when every
validation failure became a `400`). API output
~~~
- new:
~~~
records `422`, which commit `bec69a48` (2026-10-16, made without a PRD) replaced when every
validation failure became a `400`). API output
~~~

### Notes (no edit proposed)
- **Supersession leaves gaps.** Sections 5 and 6 are not carried into any of 0043–0046: the fresh login for every test,
  forced by refresh-token rotation, and the dedicated seeded identities such as `e2e_pwchange`. W4 rows 0070–0073 do not
  map them either. If the owner supersedes ADR-0001, that rationale survives only in this superseded record and in
  `e2e/README.md`. The alternatives are a fifth bounded record for the authentication strategy, or keeping ADR-0001
  `accepted` until one exists. In that case, reject E-0001-01 and the matching `supersedes` edits together.
- **Length.** The record is 656 lines against a ceiling of about 150. This is known (FQ-04) and is not something a
  clarification can fix.
- **Stale line references elsewhere.** DD-11 cites `ADR-0001:147-165` and `:402` for the prodlike text, which is now at
  lines 170–188 and 424–426. `DOCS-WORK-LISTS.md:643` cites `:373` for text that is now at 385.
- **Stale code comments, not ADR text.** `playwright.config.ts:93-95` and `playwright.prod.config.ts:40-42` still say
  "Nothing listens on that host … so no mail is delivered", but the mail sink listens on exactly that host and port.
- **Config drift that ADR-0001 §13.6 does not cover.** The prodlike config does not carry the dev config's `testIgnore`
  of `dashboard/quickActions.test.ts` (Edge, both phones) or `rendering/shortLandscapeScroll.test.ts` (both phones). It
  also sets no `expect.timeout`, so it uses Playwright's 5 s default where the dev config uses 10 s. The dev config says
  that running quickActions on two projects "would let the two instances sweep each other's rows mid-test", so the
  prodlike full matrix may be exposed to that collision. Owner to check the code; I have not run it.

---

---

## ADR-0002 — 0002-retry-after-client-cooldown-handling.md

Verdict: EDITS

Supersession check: symmetric (`superseded-by: [ADR-0047]` here, `supersedes: [ADR-0002]` in 0047; status
`superseded`). Body changes since the legacy file (`git diff 479420ad:Docs/Future/ADR-014-... HEAD:Docs/adr/0002-...`):
front matter replacing the Status/Date lines, the H1, two relative links, and the dated clarification
(`f4313370`). A "Legacy record" banner added at renumbering (`f3a043d6`) was removed again in `8e74098d`.
All within what the README allows for a record that was `proposed` and is now `superseded`.

### E-0002-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The clarification says the 429 body holds those four members "only"; it also carries `traceId`, which the framework writer adds to every problem body.
- Evidence: `UnconditionalProblemDetailsWriter.cs:184-189` delegates to the framework writer (which applies `ProblemDetailsDefaults`, adding `traceId`); `CorrelationAndProblemDetailsFixture.cs:203-204` asserts `traceId` on a failure body; `Source/Server/DEVELOPER.md:3936-3938`.
- old:
~~~
> `correlationId` and `instance` only (`ProblemDetailsOptionsSetup.cs`), and `Retry-After` is written
~~~
- new:
~~~
> `correlationId`, `traceId` and `instance` only (`ProblemDetailsOptionsSetup.cs`), and `Retry-After` is written
~~~

### Notes (no edit proposed)
- E-0002-01 corrects the 2026-11-04 clarification itself, not the legacy text. If the owner reads the
  README as forbidding any edit to an existing clarification, add a second dated clarification line instead.
- 0002 is superseded by a record that is itself only `proposed` (0047). The README does not forbid this,
  but it means no binding record currently states the never-retry rule. Owner decision.
- PRD-014 still names ADR-0002 as its companion record (`PRD-014:89`), not ADR-0047. PRD-014:22-30 and
  PRD-030 B8 both say a 429 always carries `Retry-After`, which the sliding window does not (the PRD's own
  OQ-6 asks this). `Source/Server/DEVELOPER.md:244` and `:3633` make the same overclaim. These are drift
  in other documents, for W1/W2, not ADR edits.

---

---

## ADR-0003 — 0003-public-tls-terminates-at-the-container-apps-ingress.md

Verdict: EDITS

### E-0003-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE
- Severity: format
- Why: `date` is the writing date. The earliest record of the choice in code is the commit that explains why `UseHttpsRedirection()` is left out, one day after the first Azure build tasks.
- Evidence: `git log -S UseHttpsRedirection` → `bebba703 2025-11-08 Add a comment why UseHttpsRedirection() is not being used`; `e45caf59 2025-11-07 Docker file updates for local vs azure builds` (first Azure tasks).
- old:
~~~
date: 2026-11-04
~~~
- new:
~~~
date: 2025-11-08
~~~

### E-0003-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why: The issuer is recorded. The owner-confirmed deployment record (SQ-38) and the tracked diagram both name Let's Encrypt, so "not recorded" is false. (See Notes for a possible doubt about the issuer.)
- Evidence: `DOCS-WORK-LISTS.md` SQ-38 ("Azure-managed (Let's Encrypt) certificates"); `Docs/Azure/POT-Azure-Diagram.mmd` (`Ingress["...HTTPS Termination<br/>Let's Encrypt SSL"]`).
- old:
~~~
The apex, www and api domains are served over HTTPS with Azure-managed certificates; which authority
issues them is not recorded.
~~~
- new:
~~~
The apex, www and api domains are served over HTTPS with Azure-managed certificates, which the
deployment record and `Docs/Azure/POT-Azure-Diagram.mmd` give as Let's Encrypt; the issuer does not
affect this decision.
~~~

### E-0003-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The image does not set the listening address. Only the compose file sets `ASPNETCORE_URLS=http://+:5241`; the Dockerfile only `EXPOSE`s 5241. The deployed value must therefore come from the container app's environment, which the repo does not record.
- Evidence: `Source/Docker/Server/Dockerfile` (no `ENV ASPNETCORE_URLS`/`HTTP_PORTS`, `EXPOSE 5241`); `Source/Docker/docker-compose-client-server.yml` (`ASPNETCORE_URLS=http://+:5241`); `Source/Server/Pot.AspNetCore/Properties/launchSettings.json` (`http://localhost:5242`).
- old:
~~~
The application itself contains no HTTPS configuration. In the container Kestrel listens on
`http://+:5241`; run outside Docker, the API listens on `http://localhost:5242`.
~~~
- new:
~~~
The application itself contains no HTTPS configuration. The image sets no listening address (its
Dockerfile only exposes 5241); the compose file sets `ASPNETCORE_URLS=http://+:5241`, and the deployed
container app's environment, which the repository does not record, must supply the equivalent for the
ingress target port to match. Run outside Docker, the API listens on `http://localhost:5242`.
~~~

### E-0003-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: unsupported-history
- Why: The owner's recollection of why this option was chosen is recorded, and the record leaves it out. The first correction pass removed "it is the platform default"; SQ-38 supports that wording.
- Reworded for house style: Removed "the owner's recollection"; "platform default" is now stated as the reason, as the cleanup diff had it.
- Evidence: `DOCS-WORK-LISTS.md` SQ-38 ("Owner recollection: landing on ingress-terminated TLS was accepting the platform default; encrypting the ingress-to-container hop was not weighed"); `git show d9f2fb00`.
- old:
~~~
Chosen option: 1, because it keeps certificate handling out of the application and matches the
configuration the deployment runs under.
~~~
- new:
~~~
Chosen option: 1, because it is the platform default, keeps certificate handling out of the
application and matches the configuration the deployment runs under.
~~~

### E-0003-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: As configured, `UseHttpsRedirection()` would not redirect. No HTTPS port is set anywhere: compose and launchSettings are HTTP-only and there is no `HTTPS_PORT`/`ASPNETCORE_HTTPS_PORTS`. Without one, `HttpsRedirectionMiddleware` logs "Failed to determine the https port for redirect" and passes the request through. The harm described only happens if a port is configured. No test enforces the absence.
- Evidence: `Program.cs:71-72` (commented out, with the reason); `git grep -i "HTTPS_PORT|https_ports"` → no hits; compose `ASPNETCORE_URLS=http://+:5241`; FQ-04 (b) ("its enforcement is a review rule plus an absent-code check").
- old:
~~~
- Code check: `UseHttpsRedirection()` must stay out of the pipeline. It runs after the forwarded-header
  middleware, so forwarded traffic already reports `https`; the redirect would fire on requests that
  arrive without the header — a direct or local call — and send them to an address nothing serves.
~~~
- new:
~~~
- Code check: `UseHttpsRedirection()` must stay out of the pipeline (`Program.cs` keeps it commented out
  with the reason). It would add nothing: placed after the forwarded-header middleware, forwarded traffic
  already reports `https`, and with no HTTPS port configured the middleware finds no redirect target and
  passes requests through. If an HTTPS port were ever configured, it would redirect requests that arrive
  without the header — a direct or local call — to an address nothing serves. No test fails if it is
  re-added.
~~~

### E-0003-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why: This bullet states network confinement as fact. The Consequences section of the same record (and SQ-38) treats it as an assumption the repo cannot show.
- Evidence: ADR-0003 Consequences ("assumed never to leave the environment's own network — an assumption about the platform"); SQ-38 ("Not recorded: whether another workload in the same environment could reach the API container directly").
- old:
~~~
- Bad: more configuration to keep working, for a hop that is already confined to a private network.
~~~
- new:
~~~
- Bad: more configuration to keep working, for a hop assumed to stay inside the environment's own network.
~~~

### Notes (no edit proposed)
- **Certificate issuer.** SQ-38 and the diagram say Let's Encrypt. As far as I know, Azure's own
  documentation names DigiCert as the issuer of free Container Apps managed certificates, which may be
  why the first correction pass removed "Let's Encrypt". The owner can confirm it in the portal. If it is
  DigiCert, change E-0003-02 to "Azure-managed certificates" and correct the diagram label. Either way,
  "not recorded" is wrong.
- Confirmation still names `Docs/Azure/Azure-Deployment-Notes.md`, which is ignored. It is a backticked
  path, not a link, so it does not break the link rule. The owner removed the Docker guide's links to the
  same file because they can never resolve, so for consistency they may want "the private deployment
  record (not in the repository)" here instead.
- Verified: ingress type `HTTP` from anywhere, three domains, PostgreSQL SSL required (SQ-38); target port
  5241 (SQ-37 probe port, Docker guide "Ingress ports"); `DATABASE__SSLMODE=Disable` in compose; the
  `ForwardedHeadersFixture` Location tests (`Should_Return_An_Https_Location_When_The_Ingress_Forwards_Https`,
  `..._Http_Location_When_There_Is_No_Forwarded_Proto`). 100 lines.

---

---

## ADR-0004 — 0004-the-api-trusts-the-caller-address-and-scheme-from-the-ingress.md

Verdict: EDITS

### E-0004-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE
- Severity: format
- Why: The decision was made and implemented in one dated commit.
- Evidence: `ceb57419 2026-11-02 Honour the forwarded headers set by the Container Apps ingress` (adds `ForwardedHeadersOptionsSetup`, both fixtures and the `Program.cs` change).
- old:
~~~
date: 2026-11-04
~~~
- new:
~~~
date: 2026-11-02
~~~

### E-0004-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: unsupported-history
- Why: "Unreachable except through the ingress" goes further than the evidence. The record confirms only that the ingress is the sole public route; it does not say whether another workload in the environment can reach the container. "Deliberate" is the owner's recollection and should be attributed as such.
- Reworded for house style: Removed "(owner recollection)"; the narrowed "only public route" claim and the unrecorded-reachability sentence are kept.
- Evidence: SQ-35 ("Owner recollection: trusting any sender was a deliberate decision ... the deployment record does not state whether another workload in the same environment could reach the API container directly"); SQ-38 ("Not recorded ... and no IaC exists to check it").
- old:
~~~
Trusting any sender is deliberate: the ingress address cannot be enumerated, the container is
unreachable except through the ingress (`Source/Docker/DEVELOPER.md`, "How Azure differs from local"),
and the limit of one means only the entry the ingress wrote is used.
~~~
- new:
~~~
Trusting any sender is deliberate: the ingress address cannot be enumerated, the ingress is the
container's only public route (`Source/Docker/DEVELOPER.md`, "How Azure differs from local"), and the
limit of one means only the entry the ingress wrote is used. The deployment record does not say whether
another workload in the same environment could reach the container directly, and no
infrastructure-as-code exists to check it.
~~~

### E-0004-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: TEMP-DOC-CITE
- Severity: consistency
- Why: "Needs no change" settles a follow-up that is still open, and it hides the local effect: every anonymous caller shares nginx's rate-limit partition.
- Evidence: `DECISIONS-TO-DISCUSS.md` "Follow-up: forwarded headers (fixed in `ceb57419`)", item 2, `> **Your call (item 2):**` unanswered; `Source/Docker/Client/nginx.conf:38-50` (no `X-Forwarded-For`).
- old:
~~~
- Neutral, because the local compose proxy (`nginx.conf`) sets `Host`, `Upgrade` and `Connection` only, so
  local calls through it present the client container's address and always report `http`. That is correct
  for a local stack and needs no change.
~~~
- new:
~~~
- Neutral, because the local compose proxy (`nginx.conf`) sets `Host`, `Upgrade` and `Connection` only, so
  local calls through it present the client container's address and always report `http`; locally every
  anonymous caller therefore shares one rate-limit partition. Whether to forward `X-Forwarded-For` there
  is an open follow-up in `DECISIONS-TO-DISCUSS.md` (forwarded headers, item 2).
~~~

### E-0004-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: The fixture tests more than the scheme. It also proves the address half, including the driver "a caller must not be able to choose its own address", and the record does not mention it.
- Evidence: `Pot.AspNetCore.Integration.Tests/Pipeline/ForwardedHeadersFixture.cs` — `Should_Give_Each_Forwarded_Caller_Its_Own_Anonymous_Rate_Limit`, `Should_Record_The_Forwarded_Address_On_The_Login_Session`, `Should_Record_Only_The_Last_Hop_So_A_Caller_Cannot_Choose_Its_Own_Address`.
- old:
~~~
- Test: `ForwardedHeadersFixture` drives real requests through the pipeline, so the unit test cannot pass
  while the middleware is unused: a forwarded scheme produces an `https` location for a created resource,
  an absent one produces `http`.
~~~
- new:
~~~
- Test: `ForwardedHeadersFixture` drives real requests through the pipeline, so the unit test cannot pass
  while the middleware is unused: a forwarded scheme produces an `https` location for a created resource
  and an absent one `http`; two forwarded addresses get separate anonymous rate-limit partitions; a login
  records the forwarded address on its session; and with two entries in `X-Forwarded-For` only the
  rightmost is recorded, so a caller cannot choose its own address.
~~~

### Notes (no edit proposed)
- `DECISIONS-TO-DISCUSS.md` follow-up item 1 (confirm that production `AuthSessions.IpAddress` holds real
  client addresses after the next deploy) is still unconfirmed. The record does not claim otherwise.
- That follow-up says the trust model is "recorded in backlog rows 0074 and 0036". The W4 inventory puts
  it in row 0083 (row 0036 is rate limiting). That is a stale pointer in DECISIONS-TO-DISCUSS, not in
  this ADR.
- Verified: `ForwardedHeadersOptionsSetup.cs` (flags, both lists cleared, `ForwardLimit` untouched);
  `Program.cs:31,55`; all seven `ForwardedHeadersOptionsSetupFixture` tests; the "Gap" about
  `X-Forwarded-Host` is accurate. 111 lines.

---

---

## ADR-0005 — 0005-cross-origin-access-is-limited-to-configured-browser-origins.md

Verdict: EDITS

### E-0005-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE
- Severity: format
- Why: Configured origins with credentials arrived in November 2025, so `2026-11-04` is not the decision date. (Later refinements are listed in Notes.)
- Evidence: `4d0856f3 2025-11-07 Load allowed origins from config`; `61a9c403 2025-11-07 Cors config / setup update` (adds `CorsConfiguration`, `CorsConfigurationSetup`, `CorsOptionsSetup`).
- old:
~~~
date: 2026-11-04
~~~
- new:
~~~
date: 2025-11-07
~~~

### E-0005-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: Local development is not cross-origin. The compose client's nginx proxies `/api/` to the server, and the Vite dev server is configured with an `/api` proxy. Only the deployed client calls an absolute API origin.
- Evidence: `Source/Docker/Client/nginx.conf:38-50`; `Source/Client/pot-react/vite.config.ts:87-94`; `Source/Docker/DEVELOPER.md:538-539` ("The browser only ever talks to port 5175"); `.vscode/tasks.json:108` (the Azure build alone passes an absolute `VITE_API_BASE_URL`).
- old:
~~~
- Bad: the client and API would have to be served under one host, which changes the deployment shape
  rather than the policy, and would still leave local development cross-origin.
~~~
- new:
~~~
- Bad: the client and API would have to be served under one host, which changes the deployment shape
  rather than the policy. Locally that shape already exists — the compose client's nginx proxies `/api`,
  and the Vite dev server is configured to — so only the deployed system is cross-origin.
~~~

### E-0005-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: The splitting, trimming and de-duplication rules are tested in `CorsConfigurationFixture`, which the record does not name. "Validated on start" is proven by `OptionsStartupValidationFixture`, not by the setup fixture.
- Evidence: `Pot.AspNetCore.Tests/Concerns/Cors/Configuration/CorsConfigurationFixture.cs` (comma, semicolon, trim, case-insensitive duplicates); `CorsConfigurationSetupFixture.cs:50-71` (empty, `*`, path, trailing slash, query, fragment, no scheme, `ftp`, user info); `Pot.AspNetCore.Tests/Extensions/OptionsStartupValidationFixture.cs:29,52` (`Cors:AllowedOrigins` empty → startup fails).
- old:
~~~
- Test: `CorsOptionsSetupFixture` and `CorsConfigurationSetupFixture` assert the policy, the delimiter
  handling and the startup validation, including rejection of `*`.
~~~
- new:
~~~
- Test: `CorsOptionsSetupFixture` asserts the policy; `CorsConfigurationFixture` the splitting on `,` and
  `;`, trimming and case-insensitive de-duplication; `CorsConfigurationSetupFixture` the validation rules,
  including rejection of an empty list, `*`, a path or trailing slash, user info and a non-HTTP scheme; and
  `OptionsStartupValidationFixture` that an empty `Cors:AllowedOrigins` fails startup.
~~~

### Notes (no edit proposed)
- The policy grew in steps: configured origins and setup on 2025-11-07; startup validation of origin
  shape (`7647c017`), the ten-minute preflight cache (`3dce3560`) and `Retry-After` exposure (`479420ad`)
  on 2026-10-19; correlation-id exposure (`50ff0372`) on 2026-10-21. If the owner prefers the date the
  policy reached its current shape, use 2026-10-21 instead of E-0005-01's value.
- Verified against `CorsOptionsSetup.cs`, `CorsConfiguration.cs`, `CorsConfigurationSetup.cs` and
  `CorsFixture.cs` (600 s max-age, credentials on preflight and on a 400 response, both delimiters, four
  exposed headers, no ACAO for a disallowed origin). SQ-34 confirms every setting and the owner's approval.
  The client reads none of `Location`/`Retry-After`/`X-Correlation-Id` from a response; the correlation id
  is read from the request config (`axiosInterceptors.ts:79,103`). Every client request carries the
  non-safelisted `X-Correlation-ID` (`apiHelpers.ts:103-104`), so "blocked before the request is sent"
  holds. 112 lines.

---

---

## ADR-0006 — 0006-optimistic-concurrency-is-checked-in-the-application.md

Verdict: EDITS

The count is nine, and the ADR is right. There are nine `CheckHasSameEtag.cs` files:
`Pot.App/Features/{Accounts/Update, Approvals/UpdateStatus, Expenses/Update, Incomes/Update, Settings/Upsert, Sites/Update, Users/Update, Users/UpdateRoles, Users/UpdateStatus}/EntityChecks/Checks/CheckHasSameEtag.cs`.
Nine input models carry `Etag`, and nine routes call `.ProducesStaleEntityTagConflict()`
(`Pot.AspNetCore/Features/*/Extensions/RouteGroupBuilderExtensions.cs`; Users has three). DD-02's list of
six leaves out the three Users paths, so DD-02 is the stale document.

### E-0006-01 — DROPPED (house style)
- Was: The record is backfilled but does not say so, and does not name its sources (skill hard rule).
- Reason dropped: Only a backfilled/sources paragraph; DD-02 is already cited in More Information.

### E-0006-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: unsupported-history
- Why: The owner gave this reason: database independence, and unfamiliarity with database tokens made the application check the simpler choice. The claim that the token approach "would make each update path responsible for setting the original value" is an inference presented as the reason. Nothing shows that `xmin` or last-write-wins were weighed.
- Reworded for house style: Removed "The owner recalls" and the `DOCS-WORK-LISTS.md` SQ-15 citation; the SQ-15 reasons and the xmin/last-write-wins caveat are kept.
- Evidence: `DOCS-WORK-LISTS.md` SQ-15: "wanted database-independent concurrency; unfamiliarity with database tokens made the ETag approach simpler, with an acceptable race risk given parallel edits are very unlikely".
- old:
~~~
The aim is concurrency handling that does not depend on the database. The database-token approach
would make each update path responsible for setting the original value, and the residual race is
accepted because two people editing the same record at the same moment is very unlikely in this
application.
~~~
- new:
~~~
The aim is concurrency handling that does not depend on the database, and the application check was the
simpler choice because database concurrency tokens were unfamiliar. The remaining race is accepted
because two people editing the same record at the same moment is very unlikely in this application. The
record does not show whether `xmin` or last-write-wins were weighed; they are listed because they are the
realistic alternatives.
~~~

### E-0006-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: A 409 body carries the problem `type` and `detail` only. The property name goes into the response only for a 400 (the `errors` map), so no `etag` field name appears in a stale-tag conflict. The other conflicts are `duplicate-value` (rule) and `data-conflict` (database).
- Evidence: `Pot.AspNetCore/Extensions/EnrichedErrorExtensions.cs:17-27` (409 sets `Type` only) and `:29-45` (field map only for 400); `Pot.AspNetCore/Concerns/ProblemDetails/ProblemTypes.cs:12-18,47-55`; `DatabaseExceptionHandler.cs:25-27` (all database conflicts use `ErrorCodes.Conflict`, which maps to `data-conflict`).
- old:
~~~
carrying the
  `stale-entity-tag` problem type and the `etag` property name, so it is distinguishable from the conflicts
  raised by unique and reference constraints, which use their own types.
~~~
- new:
~~~
carrying the
  `urn:pot:problem:stale-entity-tag` problem type. That type sets it apart from a duplicate value
  (`duplicate-value`) and from the conflicts the database raises (`data-conflict`). The body names no
  field, because only a `400` lists fields.
~~~

### E-0006-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: Leaving the tag check out of a path does not leave that path with an empty chain. Its other checks still run (Accounts, for example, also has `CheckDescriptionDoesNotExist`).
- Evidence: `Pot.App/Features/Accounts/Update/EntityChecks/PreUpdateChecker.cs:19-21` (chain built from all `IPreUpdateCheck`); `Accounts/Update/EntityChecks/Checks/` holds two checks.
- old:
~~~
Checks are bound by scanning for the
  interface, so a path that omits one runs an empty chain and nothing fails
~~~
- new:
~~~
Checks are bound by scanning for the
  interface, so a path whose chain has no tag check runs its other checks and nothing fails
~~~

### E-0006-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: omission
- Why: The existing `DbUpdateConcurrencyException` mapping answers 409 with the `data-conflict` type, not `stale-entity-tag`. Adopting the token would therefore change the problem type a client sees unless a new mapping is added. A reader would otherwise assume no contract change.
- Evidence: `Pot.AspNetCore/Concerns/ExceptionHandlers/DatabaseExceptionHandler.cs:27`; `ProblemTypes.cs:53`.
- old:
~~~
- Good: the guard moves into the `UPDATE`, so the check and the write are one atomic step, and the
  existing `DbUpdateConcurrencyException` mapping already answers `409`.
~~~
- new:
~~~
- Good: the guard moves into the `UPDATE`, so the check and the write are one atomic step, and the
  existing `DbUpdateConcurrencyException` mapping already answers `409`. That mapping uses the
  `data-conflict` type, so keeping `stale-entity-tag` would need a mapping of its own.
~~~

### Notes (no edit proposed)
- `DECISIONS-TO-DISCUSS.md` DD-02 should be corrected from six update paths to nine. The ADR's closing
  sentence already says DD-02 is the stale one.
- `date: 2026-11-04` is the date the record was written, not the date of the decision. `Etag` is in the
  `20250923095549_Init` migration. Every backfilled ADR uses `2026-11-04`, so the owner should decide this
  once for the whole series.
- Several mutating routes carry no tag at all: deletes, toggle-exclude, renew and resend-invite. The record
  is scoped to "update paths", so this is not an error. The owner may want it stated (PRD-026 I4 already
  notes that resend-invite has no concurrency token).
- Both client facts check out: `src/data/role.ts:7` uses `z.number()` and the others use `z.bigint()`. A
  second number-typed declaration exists at `src/api/types/identity.ts:3` (`eTag: number`), which is a TS
  type, not a schema. No edit is needed.

---

---

## ADR-0007 — 0007-each-request-resolves-the-callers-permissions-from-the-database.md

Verdict: EDITS

### E-0007-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE
- Severity: consistency
- Why: `date` is the writing date. The handler that resolves permissions from the database per requirement dates from the first JWT authorization work. PRD-026 §4.6 (Complete, 2026-10-14) later restated the policy.
- Evidence: `git log -S"PermissionAuthorizationHandler" --reverse` → `8db2d00f 2025-09-12 Initial jwt auth effort`; `Docs/Future/PRD-026-cross-tenant-authorization-hardening.md:463-469`.
- old:
~~~
date: 2026-11-04
~~~
- new:
~~~
date: 2025-09-12
~~~

### E-0007-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: omission
- Why: The record gives no evidence for its rationale, yet a completed PRD states it in so many words. The owner's recollection (SQ-29) is the first of those reasons.
- Evidence: PRD-026 §4.6 (`:463-469`): "a claim is readable by anyone holding the token"; "Claims go stale". DOCS-WORK-LISTS SQ-29.
- old:
~~~
anywhere", not "what may this user do here". Site scoping is enforced later, by the data layer.
~~~
- new:
~~~

[PRD-026](../Future/PRD-026-cross-tenant-authorization-hardening.md) §4.6 records why permission and
site claims are kept out of the token: a JWT payload is encoded, not encrypted, so a claim is readable by
anyone holding the token; and a claim goes stale while the token stays valid.
~~~

### E-0007-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: format
- Why: The template requires "Chosen option: n, because …". The record gives no reason, though the evidence above supplies one.
- Evidence: `.github/skills/adr/references/adr.template.md` ("Chosen option: <n>, because …"); PRD-026 §4.6.
- old:
~~~
Chosen option: 1. `PermissionService` loads the caller's roles and their permissions — reference data, not
~~~
- new:
~~~
Chosen option: 1, because a permission carried as a claim would be readable by whoever holds the token
and would stay in force until the token expired. `PermissionService` loads the caller's roles and their
permissions — reference data, not
~~~

### E-0007-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The cost is one read per declared requirement, not per request. The handler creates a scope and calls `GetPermissionsAsync` for each `PermissionRequirement` and nothing memoises the result. The accruals route declares three requirements.
- Evidence: `Pot.AspNetCore/Concerns/Auth/PermissionAuthorizationHandler.cs:17-37`; `Pot.AspNetCore/Features/Accruals/Extensions/RouteGroupBuilderExtensions.cs:9` (`RequireAuthorization("account:view", "expense:view", "income:view")`).
- old:
~~~
- Bad, because every authorized request costs at least one database read of the caller's roles and
  their permissions, in a scope created in the handler.
~~~
- new:
~~~
- Bad, because the read is repeated per requirement, not per request: the handler resolves the set
  afresh, in a scope it creates, for each permission an endpoint declares, so the accruals route, which
  declares three, pays three reads on top of the session check.
~~~

### E-0007-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: TEMP-DOC-CITE
- Severity: factual
- Why: (1) The grant changes when configuration changes and the API restarts, which is not necessarily a redeploy. The options are bound once into a singleton. (2) A consequence the code documents is missing: an unknown policy name turns into a requirement nobody holds. DD-09 is undecided, so the record should state the current behaviour and leave the outcome open.
- Evidence: `Pot.AspNetCore/Extensions/WebApplicationBuilderExtensions.cs:116-121` (`AddSingletonFromOptions<PlatformAdminOptions>`); `Pot.AspNetCore/Concerns/Auth/PermissionAuthorizationPolicyProvider.cs:22-34`; `Pot.AspNetCore/Concerns/Auth/AuthorizationPolicies.cs:7-13`; `DECISIONS-TO-DISCUSS.md` DD-09 ("Your call" empty).
- old:
~~~
- Bad, because platform administration is decided from deployment configuration rather than from the
  same role data, so a permission answer can change only by redeploying.
~~~
- new:
~~~
- Bad, because platform administration is decided from deployment configuration rather than from the
  same role data, so that grant changes only when the configuration changes and the API restarts: the
  options are bound once into a singleton.
- Bad, because a policy name is not checked against the permissions that exist: the policy provider turns
  any name it does not recognise into a requirement, so a misspelt name builds, is held by no one and
  answers `403` to every caller. Whether to close that is open (`DECISIONS-TO-DISCUSS.md`, DD-09).
~~~

### E-0007-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: The gap claims the matrix covers resolution, which it does not. The matrix takes each caller's expected permission set from `PermissionService` itself, so a flattening error would move the expectation with it. Its callers are a tenant Admin and a Viewer, so the configuration-added grant is exercised elsewhere.
- Evidence: `Pot.AspNetCore.Integration.Tests/Pipeline/AuthorizationMatrixFixture.cs:259-283` (`GetCallerPermissionsAsync` → `IPermissionService.GetPermissionsAsync`); `Features/Approvals/UpdateStatusFixture.cs:32` (`PlatformAdminApiWebApplicationFactory`); no test references `PermissionService` otherwise.
- old:
~~~
- Gap: no unit fixture covers the permission flattening or the configuration-added platform permission;
all of that rests on the hosted matrix.
~~~
- new:
~~~
- Gap: no fixture checks the resolution itself. The matrix takes each caller's expected permissions from
`PermissionService`, so it proves enforcement against the resolved set, not the flattening; its callers
are a tenant `Admin` and a `Viewer`, so it proves the platform routes refuse them, while the
configuration-added grant is exercised only by fixtures that start `PlatformAdminApiWebApplicationFactory`
(for example the approvals `UpdateStatusFixture`).
~~~

### Notes (no edit proposed)
- SQ-29: the owner is unsure this is best practice and asked for the trade-off to be challenged. FQ-02
  (evaluate claims or caching) is still "To assess". "More Information" says the question is open, which
  is consistent.
- Nothing mechanical fails if permissions move into claims; the matrix would still pass. The Review
  rule is the only guard on the decision itself.
- The client persists the `/me` user info, including permissions, in a Zustand `persist` store
  (`src/stores/useUserStore.ts:18`). So the interface's copy can be stale until `/me` refetches. That
  belongs to ADR-0041; it does not affect enforcement.
- Verified as accurate: claims (`JwtService.cs:27-33`), the cross-tenant comment (`PermissionService.cs:27-28`),
  ordering of `UseAuthorization` before `UsePotMiddleware` (`Program.cs:112-116`), `/me` reusing the
  service (`Features/Me/Get/Handler.cs:24`), and the 8 pinned routes in `EndpointAuthorizationMetadataFixture`.

---

---

## ADR-0008 — 0008-access-tokens-are-short-lived-and-backed-by-a-server-side-session.md

Verdict: EDITS

### E-0008-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE
- Severity: consistency
- Why: `date` is the writing date. The server-side session this record decides (one AuthSession row per sign-in) was delivered by PRD-001 and PRD-002. The short JWT and the HttpOnly cookie are older (2025-11-23).
- Evidence: `Docs/Future/PRD-002-server-login-logout-session-architecture.md` "PRD Status" (Implemented; Last updated 2026-05-03); `git log -S"class AuthSessionEntity"` → 2026-05-02.
- old:
~~~
date: 2026-11-04
~~~
- new:
~~~
date: 2026-05-03
~~~

### E-0008-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: Bearer validation does not set `ClockSkew`, so the framework's default five-minute skew applies and a token is accepted up to about twenty minutes after issue. Only the refresh-time parse sets skew to zero. The token also carries the registered issuer, audience and time claims, so "nothing else" overstates.
- Evidence: `Pot.AspNetCore/Concerns/Auth/Configuration/JwtBearerOptionsSetup.cs:24-39` (no `ClockSkew`); `Services/JwtService.cs:38-45, 66`; `TokenValidationParameters.DefaultClockSkew` = 300 s.
- old:
~~~
Signing in mints two credentials. The access token is a JWT signed with HMAC-SHA512, expiring fifteen
minutes after issue, and it carries the user identifier (`sub`), a token identifier (`jti`), the user's
token version and the identifier of the session it was issued for — and nothing else.
~~~
- new:
~~~
Signing in mints two credentials. The access token is a JWT signed with HMAC-SHA512, expiring fifteen
minutes after issue — and accepted for up to five minutes more, because bearer validation keeps the
framework's default clock skew. It carries the user identifier (`sub`), a token identifier (`jti`), the
user's token version and the identifier of the session it was issued for; beyond the issuer, audience
and validity times, nothing else.
~~~

### E-0008-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: unsupported-history
- Why: A shipped design was replaced: one refresh token on the user row. It is the evidenced status quo this decision replaced, and the record does not mention it. The README requires it to appear among the options.
- Evidence: PRD-002 "Current State Summary (Verified)" and "Known User-Visible Symptom"; PRD-002 "PRD Status" ("legacy RefreshToken/RefreshTokenExpiryUtc columns removed"); PRD-001:18.
- old:
~~~
Session state is readable on every request, which is what makes revocation possible at all: a
self-contained token cannot be taken back once issued.
~~~
- new:
~~~

Until [PRD-001](../Future/PRD-001-per-session-login-support.md) and
[PRD-002](../Future/PRD-002-server-login-logout-session-architecture.md), delivered together, the user
row held a single refresh token, so a sign-in or refresh on one device replaced the token another device
held. A session row per sign-in replaced it.
~~~

### E-0008-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: unsupported-history
- Why: Adds the real former design as an option (see E-0008-03).
- Evidence: As E-0008-03.
- old:
~~~
4. A short access token plus a refresh token kept in browser storage.
~~~
- new:
~~~
5. A short access token plus one refresh token stored on the user row, so one session per user.
~~~

### E-0008-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: unsupported-history
- Why: Pros and cons for option 5, taken from the symptom PRD-002 records.
- Evidence: PRD-002 "Known User-Visible Symptom".
- old:
~~~
- Bad: any script on the page can read and exfiltrate it, which is the attack this design avoids.
~~~
- new:
~~~

### One refresh token per user

- Good: no session table to keep.
- Bad: a sign-in or refresh on one device replaces the token another device holds, so that device is
  signed out at its next refresh — the symptom PRD-002 records for the design this one replaced.
~~~

### E-0008-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: omission
- Why: For a security record, "cannot read it" alone misleads. Script on the page can still use the cookie, because the browser attaches it to the refresh POST.
- Evidence: `Pot.AspNetCore/Features/Auth/Refresh/Handler.cs:90-115` (anonymous, cookie-driven, returns a new access token); client `src/api/authClient.ts` (withCredentials).
- old:
~~~
- Good, because the browser never exposes the refresh token to script, so an injection bug cannot read
  it.
~~~
- new:
~~~
- Good, because the browser never exposes the refresh token to script, so an injection bug cannot read
  it — though script running on the page can still make the browser send it to the refresh endpoint and
  receive a fresh access token for as long as the page is open.
~~~

### E-0008-07
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: omission
- Why: The code shows three material consequences. (1) Session expiry slides on every refresh, with no absolute lifetime and no purge. (2) The raw refresh token is logged at `Information`, so "never stored in clear" holds only for the database; ADR-0032 already records this. (3) Logout revokes the session row only when the request also carries a valid access token.
- Evidence: `Concerns/Auth/Services/AuthService.cs:327-332` and `AuthSessionService.cs:432-439` (expiry reset on rotate); no AuthSession worker in `Pot.AspNetCore/Features/Workers/`; `Docs/Future/PRD-010-auth-session-retention-and-cleanup-policy.md` (Stage: Proposed); `AuthService.cs:229` (`_logger.LogCall(this, new { refreshToken })`), `appsettings.json:4` (Default `Information`), ADR-0032:53-55; `Features/Auth/Logout/Handler.cs:136-143` (revokes only if `GetMeInfoAsync` resolves the bearer `sub`), `Features/Auth/Extensions/RouteGroupBuilderExtensions.cs:178-181` (`AllowAnonymous`).
- old:
~~~
- Bad, because the refresh token is renewed on use, so a lost response from a refresh can leave a
  client holding a token the server has already replaced.
~~~
- new:
~~~
- Bad, because the session's expiry slides: every refresh moves it thirty days on, there is no absolute
  lifetime, and nothing purges revoked or expired session rows
  ([PRD-010](../Future/PRD-010-auth-session-retention-and-cleanup-policy.md), a draft, proposes a policy).
- Bad, because the raw refresh token is written to the log at `Information` when a refresh is handled, so
  "never stored in clear" holds for the database only.
- Bad, because logout revokes the session only when the request also carries a valid access token: the
  route is anonymous and identifies the caller from the bearer token, so a sign-out sent after the access
  token has expired clears the cookie but leaves the session row active until it expires.
~~~

### E-0008-08
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: No fixture asserts the fifteen-minute lifetime or the claim set, which are the parts the Review rule protects.
- Evidence: Grep of `Pot.AspNetCore.Tests` and `Pot.AspNetCore.Integration.Tests` for `ValidTo`/expiry assertions finds none; no `JwtService` fixture exists.
- old:
~~~
- Gap: the `Secure` attribute and the thirty-day maximum age are configuration defaults and no test
  asserts them on a set cookie, because the fixture runs with `SecureOnly` disabled.
~~~
- new:
~~~
- Gap: the `Secure` attribute and the thirty-day maximum age are configuration defaults and no test
  asserts them on a set cookie, because the fixture runs with `SecureOnly` disabled. Nothing asserts the
  fifteen-minute access-token lifetime or the claim set either.
~~~

### Notes (no edit proposed)
- The cookie's `MaxAgeDays` is configurable (`AuthenticationOptions.cs:24`), but the session row's
  thirty days is a constant rounded to local midnight (`AuthService.cs:112, 327-332`). A configured
  value other than 30 makes cookie and session lifetimes diverge.
- PRD-002 open question 4 (refresh-token reuse detection) was not implemented. A replayed, already
  rotated refresh token is refused (`RefreshFixture.cs:58-61`) but does not revoke the session, so there
  is no theft signal. That is worth a decision, not an edit.
- SQ-31 confirms the four security motivations in Decision Drivers, so they are owner-confirmed.
- Verified: HMAC-SHA512 and 15 min (`JwtService.cs:14, 36, 41`); SHA-256 hash with unique index
  (`AuthSessionService.cs:463-469`, `AuthSessionEntity.cs:8`); cookie attributes and domain clearing
  (`RefreshTokenHelper.cs:10-63`); all four named fixtures exist and assert what is claimed.

---

---

## ADR-0009 — 0009-access-tokens-are-rejected-once-their-session-or-user-version-changes.md

Verdict: EDITS

### E-0009-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE
- Severity: consistency
- Why: `date` is the writing date. The per-session half, which completes the decision in the title, was added on 2026-10-17 with no PRD. The global token version dates from 2025-11-22.
- Evidence: `DOCS-WORK-LISTS.md` "Decisions with no PRD" ("binding access tokens to their session, in commit `31c96313` (2026-10-17)"); `git log -S"TokenVersion" --reverse` → 2025-11-22.
- old:
~~~
date: 2026-11-04
~~~
- new:
~~~
date: 2026-10-17
~~~

### E-0009-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: unsupported-history
- Why: The evidenced history is missing. The record does not mention that global revocation came first or that per-session rejection was added later to close a known gap. The commit message states the gap.
- Evidence: commit 31c96313 message ("so signing out ends that device's access token instead of leaving it usable until it expires. TokenVersion is unchanged and still covers global revocation on password change"); PRD-002 "PRD Status" (password change is global revoke); SQ-32.
- old:
~~~
Neither can be expressed inside the token, because a token cannot be edited after it is issued. They
can only be detected by comparing the token against current state.
~~~
- new:
~~~

Global revocation by a per-user token version came first, with the password-change goal recorded in
[PRD-002](../Future/PRD-002-server-login-logout-session-architecture.md). Per-session rejection was added
on 2026-10-17, when each access token began to carry its session's identifier; before that, logging out
stopped the session's refresh but left its access token usable until it expired.
~~~

### E-0009-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: On an `AllowAnonymous` route (logout, refresh), failed token validation does not produce `401`; the request continues unauthenticated.
- Evidence: `Concerns/Auth/Configuration/JwtBearerEventsSetup.cs:51-105` (`context.Fail`); `Features/Auth/Logout/Handler.cs:137-143`; `RouteGroupBuilderExtensions.cs:175-201`.
- old:
~~~
unrevoked and be unexpired, and the account must still be enabled. Any mismatch fails authentication,
so the caller receives `401`.
~~~
- new:
~~~
unrevoked and be unexpired, and the account must still be enabled. Any mismatch fails authentication,
so a protected route answers `401`; an anonymous route carries on with no signed-in caller.
~~~

### E-0009-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: A password change also revokes every unrevoked session. Without that, a refresh would mint a token carrying the new version, so revoking the sessions is what stops refresh.
- Evidence: `Concerns/Auth/Services/AuthService.cs:305-316` (`RevokeAllSessionsForUserAsync` then `TokenVersion++`); `LogoutFixture.Should_Revoke_All_Sessions_When_Password_Changed`.
- old:
~~~
A password change increments the user's version, which invalidates every token issued before it. Logging
out revokes one session row, which invalidates the tokens that name it.
~~~
- new:
~~~
A password change increments the user's version and revokes every unrevoked session row, so the tokens
issued before it fail on both counts and no refresh token can mint a replacement. Logging out revokes
one session row, which invalidates the tokens that name it.
~~~

### E-0009-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: omission
- Why: Two consequences the code shows are missing. (1) A completed password reset neither bumps the version nor revokes sessions, so the Context claim ("A password change must end every existing session") holds only for the signed-in change. (2) The version is bumped only on the path that also revokes every session, so today it never rejects a token the session check would accept.
- Evidence: `Pot.App/Features/Auth/PasswordReset/Verify/VerifyPasswordResetService.cs:78-95` (sets `PasswordHash` only; no `TokenVersion` or `AuthSession` use anywhere in `Pot.App`); `AuthService.cs:70-74` ("incremented only on global-revoke paths … Any future logout-all operation").
- old:
~~~
- Bad, because the failure is a plain `401` with no distinction between "never valid" and "revoked".
~~~
- new:
~~~
- Bad, because only the signed-in password change revokes anything: completing a password reset
  replaces the password hash and touches neither the version nor the sessions, so sessions opened with
  the old password keep working after a reset.
- Bad, because the version is incremented only on the path that also revokes every session, so today it
  never rejects a token the session check would accept; it is held for a future sign-out-everywhere.
~~~

### E-0009-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: unsupported-history
- Why: Adds the real former design (version only) as an option. It is the evidenced status quo before 2026-10-17.
- Evidence: As E-0009-02.
- old:
~~~
4. Cache the validation result per user for a short window.
~~~
- new:
~~~
5. Validate the user's token version only, so logging out ends a session's refresh but not its access
   token.
~~~

### E-0009-07
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: unsupported-history
- Why: Pros and cons for option 5.
- Evidence: As E-0009-02.
- old:
~~~
- Bad: reintroduces exactly the delay the design exists to remove, for a read that is small at this
  scale.
~~~
- new:
~~~

### Token version only

- Good: the token names no session, and the check reads the user row alone.
- Bad: a signed-out device's access token keeps working until it expires — the gap closed on 2026-10-17.
~~~

### E-0009-08
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: `LogoutFixture` directly asserts per-session revocation and the other device's survival, so the record should cite those tests. No fixture presents a stale-version token. Because a password change also revokes every session, removing the version comparison would leave the suite green.
- Evidence: `Pot.AspNetCore.Integration.Tests/Features/Auth/LogoutFixture.cs:115-198`; `SessionBoundAccessTokenFixture.cs:139-170` (crafted tokens copy the current `token_version`); grep for a stale-version case finds none.
- old:
~~~
- Test: the logout fixture asserts a password change increments the token version and that logging out
  does not, which are the two behaviours this record rests on.
~~~
- new:
~~~
- Test: `LogoutFixture` asserts that a signed-out device's access token is refused while another
  device's still works, that a password change ends every session's refresh, and that a password change
  increments the token version while logging out does not.
- Gap: no fixture presents a token whose version is stale and expects it to be refused; because a
  password change also revokes every session, removing the version comparison would leave the suite green.
~~~

### Notes (no edit proposed)
- **Needs a decision:** a completed password reset leaves every existing session, and its access
  tokens, valid. Neither `DECISIONS-TO-DISCUSS.md` nor `DOCS-WORK-LISTS.md` records this. If it is a
  defect, it needs a PRD; if it is intended, E-0009-05 records it honestly.
- A disabled user can still refresh (`RefreshAsync` does not check status). Each new token is then
  refused by the status check, which matches the record's "work again if re-enabled".
- Verified: the single projection read (`UserRepository.cs:99-123`; `SessionValidationRoundTripFixture`),
  the session-must-belong-to-user rule (`SessionBoundAccessTokenFixture.Should_Reject_A_Token_That_Names_Another_Users_Session`),
  and the disabled-account gap (no fixture).

---

---

## ADR-0010 — 0010-platform-administration-is-granted-by-deployment-configuration.md

Verdict: EDITS

### E-0010-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE
- Severity: format
- Why: The configured platform-admin list arrived with the sign-up approval flow.
- Evidence: `git log -S PlatformAdmin` → `e09c5394 2025-11-09 Updated signup / approval flow` (adds `PlatformAdminOptions.cs`).
- old:
~~~
date: 2026-11-04
~~~
- new:
~~~
date: 2025-11-09
~~~

### E-0010-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The summary presents recovering a locked-out site as a need this authority meets. Today `platform:manage` guards only the two approvals routes. Recovery and other cross-tenant operations are PRD-027, which is blocked and not delivered.
- Evidence: `git grep PlatformManage` → used only in `Features/Approvals/Extensions/RouteGroupBuilderExtensions.cs:12,26`; `PRD-027-platform-administration.md` header ("Blocked") and Purpose ("Approvals is the only cross-tenant surface in the product today"); PRD-027:56 ("The only remedy reachable today is destructive").
- old:
~~~
authority that can act across tenants and recover a site whose administrators are locked out, we
~~~
- new:
~~~
authority above every tenant (today it approves sign-ups from every site), we
~~~

### E-0010-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: omission
- Why: Without this, a reader would believe the two cases listed are handled today.
- Evidence: as E-0010-02.
- old:
~~~
- An operation that spans tenants, which by definition is not a tenant's business.
~~~
- new:
~~~

Today `platform:manage` guards only the approvals routes: listing sign-ups awaiting approval in every
site and deciding them. Repairing a site with no usable administrator, and the other cross-tenant
operations, are planned in PRD-027 and not delivered.
~~~

### E-0010-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: There are four refusals, not two. None protects a platform administrator in general; they protect only the last *enabled* one. "A site holding one cannot be deleted" is wrong: only a site holding the last enabled one is refused. The owner's recorded reason for the decision (SQ-30) is missing.
- Reworded for house style: Removed "The owner's reason ..." attribution; the four-guard correction is kept and the SQ-30 reason is stated plainly.
- Evidence: `Pot.App/Features/Users/UpdateStatus/EntityChecks/Checks/CheckTargetIsNotPlatformAdmin.cs:38` (disable); `Users/Delete/.../CheckTargetIsNotPlatformAdmin.cs:47` (delete another); `Me/Delete/.../CheckIsNotPlatformAdmin.cs:41-43` (delete own account); `Sites/Delete/.../CheckSiteHasNoPlatformAdmin.cs:10-17` ("holds the last live platform administrator ... not enabled do not block"); PRD-027 header correction; SQ-30.
- old:
~~~
are unchanged, apart from two guards that protect an administrator from being removed by accident — the
last platform administrator cannot be disabled, and a site holding one cannot be deleted.
~~~
- new:
~~~
are unchanged, apart from guards that keep at least one enabled platform administrator: the last one
cannot be disabled or deleted by another user, cannot delete their own account, and the site that holds
them cannot be deleted. A platform administrator who is not the last enabled one has no protection.

The authority is kept out of tenant data because platform administration belongs to the product's
operator, and a tenant is a consumer of the product.
~~~

### E-0010-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The recovery case does not exist yet, so "the recovery case it exists for" overstates what the code does.
- Evidence: as E-0010-02.
- old:
~~~
- Good, because it works whatever state a site's roles are in, which is the recovery case it exists
  for.
~~~
- new:
~~~
- Good, because it holds whatever state a site's roles are in, so the cross-tenant recovery PRD-027 plans
  has an authority to rest on; no recovery operation exists yet.
~~~

### E-0010-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The bullet contradicts itself ("must know them before the first sign-in" vs "discovered ... after the account exists"). The actual bootstrap also needs a restart and a direct database update, because sign-up waits for platform-admin approval.
- Evidence: `Docs/FIRST-TIME-SETUP.md` Step 3 ("For the first user: Since there are no platform admins yet, you need to approve yourself manually") and Step 4 (read `RowId`, set `PLATFORM_ADMIN_USERIDS`, restart, `UPDATE "User" SET "Status" = 'Enabled'`).
- old:
~~~
- Bad, because the configuration holds user identifiers, so the operator must know them before the
  first sign-in; the identifier is discovered from the database after the account exists.
~~~
- new:
~~~
- Bad, because the configuration holds user identifiers that exist only once the account does: the first
  platform administrator signs up, reads their `RowId` from the database, adds it to configuration and
  restarts the API, and, because sign-up waits for a platform administrator's approval, enables their
  own account directly in the database (`Docs/FIRST-TIME-SETUP.md`, step 4).
~~~

### E-0010-07
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: The record should name the actual fixtures. The "parsing" gap is too broad: integration hosts already parse multi-id lists (for example `DeleteUserFixture:271`). What is missing is a unit fixture for `PermissionService` and `PlatformAdminOptions`, which matches ADR-0007's own gap.
- Evidence: `Pot.AspNetCore.Tests/Extensions/OptionsStartupValidationFixture.cs:28,51`; `Pipeline/AuthorizationMatrixFixture.cs:186-188` (403 when the caller lacks the policy; callers are Admin/Viewer); `Host/PlatformAdminApiWebApplicationFactory.cs:36-42` used by `Features/Approvals/UpdateStatusFixture.cs:32`, `Sites/DeleteSiteFixture.cs:228,255`, `Users/DeleteUserFixture.cs:243,271`, `Me/DeleteAccountFixture.cs:155`, `Security/CrossTenantAuthorizationFixture.cs:470-607`; no `PermissionServiceFixture` or `PlatformAdminOptionsFixture` (`git ls-files`); ADR-0007:81.
- old:
~~~
- Test: the startup-validation fixture asserts a malformed list fails the start, and the hosted matrix
  asserts that a caller without the permission is refused. The grant side is exercised by the fixtures
  that run against a host configured with a platform administrator.
- Gap: no fixture asserts that a configured user holds the platform permission in addition to their role
  permissions, and nothing covers the parsing of the setting itself.
~~~
- new:
~~~
- Test: `OptionsStartupValidationFixture` asserts a malformed list fails the start, and
  `AuthorizationMatrixFixture` asserts that an Admin or Viewer without the permission is refused. The grant
  side is exercised through `PlatformAdminApiWebApplicationFactory`, which configures the list for
  `Approvals/UpdateStatusFixture`, `DeleteSiteFixture`, `DeleteUserFixture`, `DeleteAccountFixture` and
  `CrossTenantAuthorizationFixture`.
- Gap: no unit fixture covers `PermissionService` adding the permission on top of role permissions, or
  `PlatformAdminOptions` parsing the list.
~~~

### E-0010-08
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why: The record refers to the permission-resolution decision without linking it, although ADR-0007 links back to this one. The planned cross-tenant scope (PRD-027) should also be linked.
- Evidence: `Docs/adr/0007-...md:113-114`; `Docs/Future/PRD-027-platform-administration.md`.
- old:
~~~
- Platform authority is added to the caller's ordinary permissions on each request, which is the
  permission-resolution decision recorded separately.
~~~
- new:
~~~
- Platform authority is added to the caller's ordinary permissions on each request, which is the
  permission-resolution decision recorded in
  [ADR-0007](0007-each-request-resolves-the-callers-permissions-from-the-database.md).
- The cross-tenant operations this authority is meant to carry (deleting a user or site in another
  tenant, and administrator recovery) are specified in
  [PRD-027](../Future/PRD-027-platform-administration.md), which is not delivered.
~~~

### Notes (no edit proposed)
- `Docs/AUTHENTICATION.md:2101` says invalid GUIDs in the list are "ignored". `PlatformAdminOptionsSetup`
  makes them fail startup, which is what this ADR correctly states. That is a guide error for W2, not an
  ADR issue.
- After the edits the record is 125 lines: over the 120-line target, under the ~150 ceiling.
- Every `old` in this file was checked by script: each matches the current ADR exactly once, in order,
  with the earlier edits applied. Resulting lengths: 0003 107, 0004 117, 0005 116, 0010 125, 0034 106,
  0035 106, 0036 115.

---

---

## ADR-0011 — 0011-every-endpoint-is-authenticated-unless-explicitly-anonymous.md

Verdict: EDITS

### E-0011-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE
- Severity: consistency
- Why: `date` is the writing date. The fallback and the endpoint test were decided in PRD-026 §4.3. That PRD is Complete and was last updated 2026-10-14; the implementation followed on 2026-10-15.
- Evidence: `Docs/Future/PRD-026-cross-tenant-authorization-hardening.md:15-17, 435-437`; `git log -S"FallbackPolicy"`.
- old:
~~~
date: 2026-11-04
~~~
- new:
~~~
date: 2026-10-14
~~~

### E-0011-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The public set also includes password reset and logout.
- Evidence: `Features/Auth/Extensions/RouteGroupBuilderExtensions.cs:160-261`; `EndpointAuthorizationMetadataFixture.cs:32-44`.
- old:
~~~
reachable by anyone. That is a safe default for a public site and an unsafe one for an API whose every
route but sign-in, sign-up, refresh and the health probes expects a caller.
~~~
- new:
~~~
reachable by anyone. That is a safe default for a public site and an unsafe one for an API whose every
route but sign-in, sign-up, password reset, refresh, logout and the health probes expects a caller.
~~~

### E-0011-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: unsupported-history
- Why: The record argues hypothetically for a failure that actually happened and is documented. SQ-28 cites the incident.
- Evidence: PRD-026:38 ("There is no global fallback authorization policy"), :88-90 (I1, `PUT /api/users/{id}` open to an anonymous caller), :437 (§4.3); DOCS-WORK-LISTS SQ-28.
- old:
~~~
The failure is silent in both directions. A new protected endpoint that forgets its policy is
immediately public, and nothing in a normal review catches it.
~~~
- new:
~~~
That happened: [PRD-026](../Future/PRD-026-cross-tenant-authorization-hardening.md) found that the API
had no fallback policy and that `PUT /api/users/{id}` declared no requirement, so an anonymous caller
could edit any user's details (§2.1, issue I1); its §4.3 introduced the fallback and the endpoint test.
~~~

### E-0011-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: In Development, the OpenAPI document and the Scalar page are also `AllowAnonymous()`, so "nine" holds only outside Development.
- Evidence: `Pot.AspNetCore/Extensions/WebApplicationExtensions.cs:29-39`.
- old:
~~~
unless it explicitly opts out. Nine routes are reachable without a session: the seven authentication
operations — including logout — and the two health probes, each carrying `AllowAnonymous()` explicitly.
~~~
- new:
~~~
unless it explicitly opts out. Outside Development, nine routes are reachable without a session: the
seven authentication operations — including logout — and the two health probes, each carrying
`AllowAnonymous()` explicitly. Development adds two more, the OpenAPI document and the Scalar reference
page, also marked `AllowAnonymous()`.
~~~

### E-0011-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The test requires each endpoint to declare its own `IAuthorizeData` and does not accept reliance on the fallback. So in the test host no route relies on the fallback alone, and a reader should know the fallback is the net, not the mechanism.
- Evidence: `EndpointAuthorizationMetadataFixture.cs:46-58` (`GetOrderedMetadata<IAuthorizeData>().Count == 0` fails).
- old:
~~~
A test enumerates the application's endpoints and requires each one to be either authorized or
explicitly anonymous, so a new route cannot arrive without that decision having been made.
~~~
- new:
~~~
A test enumerates the application's endpoints and requires each one to declare its own authorization
requirement or be explicitly anonymous, so a new route cannot arrive without that decision having been
made. No route therefore relies on the fallback alone in the test host; the fallback is the net for a
route the test does not see, such as one mapped only in another environment.
~~~

### E-0011-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: "the profile and user routes" overstates. Only the three `/api/me` routes and `PUT /api/users/{id}` use `AuthenticatedUser`; the other user routes need `user:view` or `user:manage`. They declare the policy explicitly rather than rely on the fallback.
- Evidence: `Features/Me/Extensions/RouteGroupBuilderExtensions.cs:12,26,41`; `Features/Users/Extensions/RouteGroupBuilderExtensions.cs:12,28,45-112`.
- old:
~~~
  do this, so most protected routes still need their own permission policy — the profile and user routes
  are the exception, and they require authentication alone.
~~~
- new:
~~~
  do this, so most protected routes still need their own permission policy — the three profile routes and
  `PUT /api/users/{id}` are the exception, and they declare the `AuthenticatedUser` policy explicitly.
~~~

### E-0011-07
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: The bullet omits the first assertion, which enforces the rule directly. The theory checks "contains", not "exact", on 8 routes. The host runs as Production, so the Development-only anonymous routes are outside it.
- Evidence: `EndpointAuthorizationMetadataFixture.cs:46-111`; `IntegrationFixtureBase.cs:73-75` → `ProductionApiWebApplicationFactory` (`UseEnvironment("Production")`).
- old:
~~~
- Test: `EndpointAuthorizationMetadataFixture` asserts that the set of anonymous routes equals the
expected set, so an unexpected `AllowAnonymous()` fails as well as a missing one, and a separate theory
pins the exact policy on named routes, which is what distinguishes authentication from authority.
~~~
- new:
~~~
- Test: `EndpointAuthorizationMetadataFixture` asserts that every endpoint declares a requirement or is
anonymous, and that the set of anonymous routes equals the expected nine, so an unexpected
`AllowAnonymous()` fails as well as a missing one; a separate theory requires the expected policy on
eight named routes, which is what distinguishes authentication from authority. The fixture hosts the API
as Production, so the Development-only anonymous routes are outside it.
~~~

### Notes (no edit proposed)
- PRD-026 is not the origin of the record's Considered Options 3 and 4 (group convention, path-list
  middleware). They are presented as options, not as history, which is acceptable.
- Verified: fallback at `WebApplicationBuilderExtensions.cs:135`; `AuthenticatedUser` policy at `:130`;
  probes `ShortCircuit()` plus `AllowAnonymous()` at `Program.cs:82-98`.

---

---

## ADR-0012 — 0012-one-chained-rate-limit-policy-partitions-by-caller.md

Verdict: EDITS

### E-0012-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE
- Severity: consistency
- Why: `date` is when the decision was made; the policy, its partitions and its limits arrived in one commit.
- Evidence: `git show 1896f3e4` (2025-11-08, "Add rate limiting") adds `RateLimiterPolicy.cs` with `ChainedPolicy`, `user:`/`ip:` partitions, sliding 50 / fixed 15, and `RequireRateLimiting` on every group.
- old:
~~~
status: accepted
date: 2026-11-04
area: backend
~~~
- new:
~~~
status: accepted
date: 2025-11-08
area: backend
~~~

### E-0012-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: unsupported-history
- Why: Says lockout "is the wrong one here", but the owner says lockout was deferred, not rejected (as the record's own More Information says). The record also does not say it is backfilled or what it was reconstructed from.
- Reworded for house style: Removed "the owner recalls" and the whole backfilled/commits/sources paragraph; the "deferred, not rejected" correction is kept.
- Evidence: DOCS-WORK-LISTS.md SQ-33 ("temporary throttling is enough for now, with account lockout deferred until a requirement justifies added complexity"); ADR-0012:101-102; `git log -S ChainedPolicy`, `git log -S RateLimiting -- Source/Server` (`1bb1e37e`, 2026-05-16).
- old:
~~~
An account lockout is the traditional answer to guessing, and it is the wrong one here: it converts a
guessing attempt into a denial of service against the account's real owner, and it needs a recovery
path that is itself an attack surface.
~~~
- new:
~~~
An account lockout is the traditional answer to guessing. It has not been built: temporary throttling is
enough for now, and a lockout is deferred until a requirement justifies its added complexity. A lockout's
known costs are set out under the options below.
~~~

### E-0012-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why: The driver says limits "should be configurable", which contradicts the record's own Review rule and the code: the production values are constants, and the configuration keys exist only for test runs.
- Evidence: `WebApplicationBuilderExtensions.cs:170-179` ("must NOT be added to any configuration file ... exclusively for E2E test runs"); `RateLimiterDefaults.cs:3-9`; ADR-0012:70-71.
- old:
~~~
- Limits should be configurable, and the code's defaults should be a sound starting point.
~~~
- new:
~~~
- The production limits should be fixed in code, while a test run can raise them without a code change.
~~~

### E-0012-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: "a Problem Details body that names the wait" is false for an authenticated rejection, whose detail is untimed, and "carries the detail without the header" implies a timed detail. Also, despite its name, the policy chains nothing; a reader could take "chained" to mean that both limiters apply.
- Evidence: `ApiDetailErrorFactory.cs:81-83`; `HeadersAndRateLimitFixture.cs:99-102` (`Detail.ShouldBe("Too many requests. Please wait and try again.")`, no `Retry-After`); `RateLimiterPolicy.cs:10-12,23-87` (one partition returned per request; comment mentions concurrency limiting that does not exist).
- old:
~~~
queueing in either. Rejection is `429` with a Problem Details body that names the wait. `Retry-After` is
written where the limiter exposes the retry metadata, which the fixed window does and the sliding window
does not, so an authenticated rejection carries the detail without the header.
~~~
- new:
~~~
queueing in either. Despite the name `ChainedPolicy`, no limiters are chained: each request is counted by
exactly one partition. Rejection is `429` with a Problem Details body. `Retry-After` is written, and the
`detail` names the wait, only where the limiter exposes the retry metadata, which the fixed window does
and the sliding window does not, so an authenticated rejection carries neither the header nor a timed
message.
~~~

### E-0012-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: With `QueueLimit = 0`, every rejection returns at once, so it cannot be reported as a timeout. The client timeout is 30 s, not tied to the window. The real cost is that a signed-in user is not told the wait. The "Good" bullet gives the wrong cause for a fast rejection: speed comes from no queue, while the window sets the length of the wait.
- Evidence: `RateLimiterDefaults.cs:46-51`; `RateLimiterPolicy.cs:57,84`; `Source/Client/pot-react/.env:2` (`VITE_API_TIMEOUT_MS=30000`); `HeadersAndRateLimitFixture.cs:99-102`.
- old:
~~~
- Good, because the short anonymous window means a rejected anonymous request returns quickly, and can
  be reported as a cooldown rather than a hang.
- Bad, because the authenticated window is as long as the client's request timeout, so a rejected
  authenticated request can be reported as a timeout rather than as a refusal.
~~~
- new:
~~~
- Good, because with no queue a rejected request returns at once, and the short anonymous window means
  the wait it names is at most ten seconds.
- Bad, because an authenticated rejection names no wait, so a signed-in user is not told how long to
  wait, which can be up to the thirty-second window.
~~~

### Notes (no edit proposed)
- DECISIONS-TO-DISCUSS.md, forwarded-headers follow-up item 2 (still "Your call"): the local prodlike
  `Source/Docker/Client/nginx.conf:38-50` sets no `X-Forwarded-For`, so locally every anonymous caller
  shares nginx's partition. The existing consequence ("forwarded header trust is part of this control
  working at all") covers it. Do not record a fix as decided.
- Stale code comments: `RateLimiterPolicy.cs:11` (concurrency limiting) and `:15`, `:79` ("frontend timeout
  (10s)"; it is 30 s). These are W1 source-comment drift, not ADR text.
- Nothing mechanical enforces "every group opts into the policy". The Review rule is the only guard, and
  `HeadersAndRateLimitFixture` exercises `/api/auth/logout` and `/api/me` only. An endpoint-metadata
  assertion would make that rule mechanical. Owner's call.
- The OpenAPI contract declares `429` nowhere (`contracts/openapi/v1.json`), although every grouped
  endpoint can return it. Raised under ADR-0014 E-0014-07.

---

---

## ADR-0013 — 0013-passwords-are-hashed-with-the-framework-hasher-and-reset-by-single-use-code.md

Verdict: EDITS

### E-0013-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE
- Severity: consistency
- Why: `date` is the writing date. The hasher dates from 2025-09-12. The single-use code entity, which completes the decision in the title, dates from 2025-10-10. No PRD covers either.
- Evidence: `git log -S"PasswordHasher<UserEntity>" --reverse` → 2025-09-12; `git log -S"OtpCode" --reverse` → `400590f6 2025-10-10 Add OneTimePassword entity`.
- old:
~~~
date: 2026-11-04
~~~
- new:
~~~
date: 2025-10-10
~~~

### E-0013-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: A reset request names a username, not an address. The handler and service take `Username` only.
- Evidence: `Features/Auth/PasswordReset/Send/Handler.cs:17, 34-40`; `Pot.App/Features/Auth/PasswordReset/Request/RequestPasswordResetService.cs:48-54`.
- old:
~~~
A reset request is also the most obvious way to ask whether an address is registered, so its answer
must not depend on the truth.
~~~
- new:
~~~
A reset request names a username, so it is also the most obvious way to ask whether an account exists,
and its answer must not depend on the truth.
~~~

### E-0013-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: No Identity package is referenced. `PasswordHasher<T>` ships in the ASP.NET Core shared framework, which the Web SDK project gets implicitly.
- Evidence: No `Identity` entry in any `*.csproj` or `Directory.Packages.props`; `Pot.AspNetCore.csproj:1` (`Microsoft.NET.Sdk.Web`); `Concerns/Auth/UserPasswordHasher.cs:1, 41`.
- old:
~~~
Chosen option: 1. Passwords are hashed with `PasswordHasher<UserEntity>` from the ASP.NET Core Identity
package, used without adopting Identity's user store, schema or endpoints: the hasher is a
~~~
- new:
~~~
Chosen option: 1. Passwords are hashed with `PasswordHasher<UserEntity>` from ASP.NET Core Identity — it
ships in the ASP.NET Core shared framework, so no package is referenced — used without adopting
Identity's user store, schema or endpoints: the hasher is a
~~~

### E-0013-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: Three wrong attempts fail that code, and the caller must request a new one. The five-minute "too many attempts" refusal applies only once three requests for the username have failed within five minutes (by creation time), and only to wrong codes.
- Evidence: `Pot.App/Features/Auth/VerificationServiceBase.cs:147-150, 230-261`; `Features/Otp/OtpService.cs:21-23, 65-75`; `Pot.Data/Repositories/Otp/OtpRepository.cs:45-54`.
- old:
~~~
count failures and invalidate the previous active code for a username. A code is six digits, expires
fifteen minutes after issue, allows three attempts and then locks verification for five minutes. A new
request invalidates the earlier active code for that username.
~~~
- new:
~~~
count failures and invalidate the previous active code for a username. A code is six digits, expires
fifteen minutes after issue and fails after three wrong attempts, after which a new request is needed;
once three requests for a username have failed within five minutes, further wrong codes are refused as
too many attempts. A new request invalidates the earlier active code for that username.
~~~

### E-0013-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: omission
- Why: The hasher is constructed with default options, so a framework release that raises the default iteration count would produce `SuccessRehashNeeded` for every stored hash. That is the likelier trigger than a deliberate change.
- Evidence: `Concerns/Auth/UserPasswordHasher.cs:41` (`new()`), `:55-57` (`== PasswordVerificationResult.Success`).
- old:
~~~
  is treated as a failure, and nothing rehashes after a successful verification, so raising the work
  factor would lock users out rather than upgrade them.
~~~
- new:
~~~
  is treated as a failure, and nothing rehashes after a successful verification, so raising the work
  factor — including a future framework default, since the hasher runs with default options — would lock
  users out rather than upgrade them.
~~~

### E-0013-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The sweep does not clean anything up; it only flips `Active` codes to `Expired` every five minutes. Rows are deleted only when their user is deleted.
- Evidence: `Pot.AspNetCore/Features/Workers/ExpiredOtpCleanupWorker.cs:12, 49-61`; `OtpService.cs:40-62`; `Pot.Data/Repositories/Users/UserRepository.cs:184-190` (the only delete).
- old:
~~~
- Bad, because code state is another table to clean up, which is why a background sweep exists.
~~~
- new:
~~~
- Bad, because code rows are never purged: a background worker only marks lapsed codes `Expired` every
  five minutes, and rows are deleted only with the user they belong to.
~~~

### E-0013-07
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: TEMP-DOC-CITE
- Severity: omission
- Why: (1) The reset's enumeration safety is undermined elsewhere: a sign-up request answers `UsernameTaken`. (2) The plaintext-and-logged code state belongs under Consequences, stated precisely and with DD-08 shown as open, not as a vague "Known gap" under Confirmation (deleted in E-0013-09).
- Evidence: `Pot.App/Features/Auth/Signup/Request/RequestSignupService.cs:51`; `Signup/Request/Models/OutputStatus.cs` (`UsernameTaken`); `Pot.Data/Entities/OneTimePasswordEntity.cs:51-59, 65-66`; `VerificationServiceBase.cs:230`; `VerifyPasswordResetService.cs:63`, `VerifySignupService.cs:78`, `Pot.AspNetCore/Features/Auth/PasswordReset/Verify/Handler.cs:16`, `Signup/Complete/Handler.cs:17`; `DECISIONS-TO-DISCUSS.md` DD-08 (undecided).
- old:
~~~
- Bad, because a temporary password is a credential the user did not choose and must change.
~~~
- new:
~~~
- Bad, because the reset's enumeration safety is local: a sign-up request answers `UsernameTaken` for a
  username that exists, so usernames can be probed there instead.
- Bad, because the verification code is stored as issued and compared as plain text, and the sign-up and
  reset verification handlers and services log it at `Information` with the username and reference code.
  The temporary password is stored only as a hash, so a code alone signs nobody in; whether to stop
  logging codes, or to hash them, is open (`DECISIONS-TO-DISCUSS.md`, DD-08).
~~~

### E-0013-08
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: "The authentication fixtures" names no fixture. The unit fixture substitutes the hasher, so the real hash round trip is exercised only by the integration host's seeded sign-ins. The temporary passwords that are actually signed in with come from sign-up and invitation, not reset. Nothing exercises a reset end to end.
- Evidence: `Pot.AspNetCore.Integration.Tests/Host/IntegrationAuthFixtureBase.cs:194-205, 306-313` (real `IUserPasswordHasher`); `Scenarios/ScenarioProvisioner.cs:133, 188`; `Pot.AspNetCore.Tests/Concerns/Auth/Services/AuthServiceFixture.cs:83-103` (`_passwordHasherFake`); `Pot.App.Tests/Features/Auth/PasswordReset/Request/RequestPasswordResetServiceFixture.cs:61-71`; no `VerifyPasswordResetService` or `VerificationServiceBase` fixture.
- old:
~~~
- Test: the authentication fixtures assert that a sign-in with the wrong password fails and that a
  temporary password is accepted.
- Gap: no fixture asserts that a used or expired code is refused, and none asserts that a temporary
  password stops working, because nothing implements that.
~~~
- new:
~~~
- Test: the integration host seeds every user with the real `UserPasswordHasher` and signs in through
  the API (`IntegrationAuthFixtureBase`), and `ScenarioProvisioner` signs in with the sign-up and
  invitation temporary passwords read from captured mail, so a hash that stops verifying fails the suite.
- Test: `AuthServiceFixture` asserts that a failed password check refuses sign-in without creating a
  session; the hasher is substituted there.
- Gap: nothing exercises a password reset end to end, and no fixture asserts that a used, expired or
  failed code is refused, or that a reset for an unknown username answers as a known one does over HTTP;
  `RequestPasswordResetServiceFixture` asserts only that no email is sent. None asserts that a temporary
  password stops working, because nothing implements that.
~~~

### E-0013-09
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: delete
- Tags: none
- Severity: format
- Why: E-0013-07 moves this into Consequences with specifics. A "Known gap" about storage is not a confirmation mechanism.
- Evidence: As E-0013-07.
- old:
~~~
- Known gap: codes are stored and logged in a form that a reader of the data can use; a change here
  belongs to a new record.
~~~
- new:
~~~
~~~

### E-0013-10
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: omission
- Why: A reader would assume a reset ends sessions as a password change does. It does not (see E-0009-05).
- Evidence: `VerifyPasswordResetService.cs:78-95`.
- old:
~~~
- Which sessions are ended by a password change is covered by the revocation decision.
~~~
- new:
~~~
- Which sessions are ended by a password change is covered by the revocation decision. Completing a
  reset ends none: it replaces the password hash only.
~~~

### Notes (no edit proposed)
- **One decision per file:** this record bundles at least two independent choices: the password-hashing
  primitive, and code-based verification and reset with an enumeration-safe answer. W4 row 0033 bundles
  them the same way. The owner should decide whether to split them; I have not proposed a split.
- DD-08 is undecided ("Your call" empty). E-0013-07 states the current state without choosing an option.
  PRD-032 (Planning) may amend the temporary-password lifecycle for invitations.
- The reset email carries the reference code, the verification code and the temporary password together
  (`RequestPasswordResetService.cs:69-80`). So "proves control of the address" means one mailbox read,
  not two factors.
- No SQ interview covers row 0033. The record's rationale bullets are presented as consequences, not
  as history, which is acceptable.

---

---

## ADR-0014 — 0014-every-failure-is-written-as-a-problem-details-document.md

Verdict: MAJOR (the Summary says every failure carries a stable type, a user-safe detail and an error code.
No failure carries an error code, a `400` carries no `detail`, and only conflicts get a POT type. The
Decision Outcome says refusals are distinguished by `detail`, which a `400` never has.)

### E-0014-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE, RELATED-PRDS
- Severity: consistency
- Why: `date` should be when the decision was made. Option 1 ("written by the application even where the framework would not write one") became true in `50ff0372`. The record comes from PRD-030 (Complete), which front matter should name.
- Evidence: `git show -s 50ff0372` (2026-10-21, "Write every error as Problem Details and echo the correlation id"); PRD-030 status Complete, scope line; DOCS-WORK-LISTS.md W4 row 0022 (main source PRD-030).
- old:
~~~
date: 2026-11-04
area: backend
---
~~~
- new:
~~~
date: 2026-10-21
area: backend
related-prds: [PRD-030]
---
~~~

### E-0014-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: No error code is on the wire, a `400` has no `detail`, and POT publishes a `type` only for conflicts (a `429` has none).
- Evidence: PRD-030 B1, B9 and its table ("`errorCode` | Removed from the wire"); `ProblemDetailsOptionsSetup.cs:63-83`; `EnrichedErrorExtensions.cs:23-46`; `HttpResponseMessageExtensions.cs:103,112`; `HeadersAndRateLimitFixture.cs:56`.
- old:
~~~
no body at all, we decided that every failure response carries an RFC 9457 Problem Details document
with a stable type, a user-safe detail and an error code, to achieve one shape the client can read
whatever produced it,
~~~
- new:
~~~
no body at all, we decided that every failure response carries an RFC 9457 Problem Details document
with a correlation identifier and a user-safe message — in `detail`, or for a `400` in an `errors` map
keyed by field — and a stable `type` for each conflict category, to achieve one shape the client can read
whatever produced it,
~~~

### E-0014-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: format
- Why: The record is backfilled and must say so in Context and name its sources.
- Reworded for house style: Removed backfilled statement, commits, source list and SQ-24 citation; the client-predictability reason and PRD-030 link are kept.
- Evidence: `.github/skills/adr/SKILL.md` hard rules; PRD-030; commits `50ff0372`, `d837c060`; DOCS-WORK-LISTS.md SQ-24.
- old:
~~~
A client that receives no body cannot say anything useful, so it invents a generic message; a support
request that quotes only a status code cannot be traced to a request at all.
~~~
- new:
~~~

A consistent shape is what lets the client interpret and handle every error predictably;
[PRD-030](../Future/PRD-030-problem-details-consolidation.md) consolidated the responses into one.
~~~

### E-0014-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The driver asks for a type "per failure kind". Only the conflict categories get one, and the outcome says so.
- Evidence: `ProblemTypes.cs:107-114,143-152`; PRD-030 scope ("every conflict names its category in a stable `type`") and non-goal "machine-readable error codes beyond the conflict types".
- old:
~~~
- A stable, machine-readable type per failure kind, so behaviour can be tested without matching prose.
~~~
- new:
~~~
- A stable, machine-readable type wherever a caller must tell failures of one status apart, so it need
  not match prose.
~~~

### E-0014-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The outcome lists `type`, `title` and `detail` as on every failure, and says a validation failure and a refusal are told apart by `detail`. A `400` has no `detail`; a `429` has no `type`. A refusal and a validation failure differ only in the messages in `errors`.
- Evidence: `ProblemDetailsOptionsSetup.cs:49-59,63-83`; `EnrichedErrorExtensions.cs:29-46`; `HttpResponseMessageExtensions.cs:112`; `HeadersAndRateLimitFixture.cs:56,92`; `CorrelationAndProblemDetailsFixture.cs:203-204`.
- old:
~~~
Chosen option: 1. Failures are written as Problem Details carrying a `type`, a `title`, an HTTP-status-bearing
`status`, a user-safe `detail`, a correlation identifier, the framework trace identifier, the request path
as `instance`, and an `errors` map when the failure is a validation one. A bespoke `urn:pot:*` type is
published for the three conflict categories, which is where a caller needs to tell one conflict from
another; every other status keeps the framework's per-status type, so a validation failure and a refused
business rule share a type and are distinguished by their `detail`.
~~~
- new:
~~~
Chosen option: 1. Every failure body carries `status`, a correlation identifier (`correlationId`), the
framework trace identifier (`traceId`) and the request path as `instance`. A `400` is always a validation
problem: the framework's validation title, an `errors` map from field to messages (a message for the whole
request under the empty key), and no `detail`. Every other status carries no `errors` and a user-safe
`detail`, the application's own or a per-status default. A bespoke `urn:pot:*` type is published for the
three conflict categories, which is where a caller needs to tell one conflict from another; every other
status keeps the framework's per-status `type` where the framework defines one, and a `429` has none. No
error code is serialised, so a validation failure and a refused business rule share one shape and differ
only in the messages in `errors`.
~~~

### E-0014-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The client can branch on `type` only for conflicts. It reads `type` only to recognise the stale tag.
- Evidence: `Source/Client/pot-react/src/api/errors/apiErrorResponse.ts:122-135`; `EnrichedErrorExtensions.cs:23-27`.
- old:
~~~
- Good, because the client parses one document and can branch on `type` rather than on prose, and on the
  conflict family the type distinguishes the cases.
~~~
- new:
~~~
- Good, because the client parses one document, and on a conflict can branch on `type` rather than on
  prose.
~~~

### E-0014-07
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: The validation-body test is unnamed; the real guard is the shared helper. `EndpointResponseMetadataFixture` pins only the shared failure declarations, not "which failures each endpoint declares, so the contract matches the behaviour", and no endpoint declares the 429 every rate-limited group can return. "the readiness probe" should be both probes, since liveness is short-circuited too.
- Evidence: `HttpResponseMessageExtensions.cs:93-115`; `EndpointResponseMetadataFixture.cs:12-52,55-71,135-185`; `grep -c '"429"' contracts/openapi/v1.json` = 0; `Program.cs:82-99` (both `/_health` and `/_health/ready` `.ShortCircuit()`); `HeadersAndRateLimitFixture.cs:49-57`.
- old:
~~~
- Test: a validation failure's body is asserted to carry no submitted value and no error-code member,
  which is the evidence for the driver above.
- Test: `EndpointResponseMetadataFixture` pins which failures each endpoint declares, so the contract
  matches the behaviour.
- Outside this decision: the readiness probe is short-circuited and answers in plain text, and a request
  aborted by the client gets no body. Both are deliberate exceptions rather than gaps.
~~~
- new:
~~~
- Test: `ShouldBeValidationProblemAsync`
  (`Pot.AspNetCore.Integration.Tests/Host/Extensions/HttpResponseMessageExtensions.cs`) asserts every
  validation failure it checks carries no submitted value, no error-code member and no `detail`, with a
  non-empty `errors` map.
- Test: `HeadersAndRateLimitFixture` asserts the `429` body carries `detail`, `status` and
  `correlationId`, and no `type` or `errors`.
- Test: `EndpointResponseMetadataFixture` pins the failures every endpoint shares — `400` as the validation
  problem, `500` as Problem Details, a declared `409` as Problem Details, `413` and `408` on routes that read
  a body, and no `422`. It does not compare each endpoint's declared set with its behaviour, and no
  endpoint declares the `429` that every rate-limited group can return.
- Outside this decision: both health probes are short-circuited and answer in plain text, and a request
  aborted by the client gets no body. Both are deliberate exceptions rather than gaps.
~~~

### Notes (no edit proposed)
- Date: Problem Details has been the error format since well before 2026-10-21 (`AddProblemDetails` is
  already in `bd5ec890`, 2025-03-06). 2026-10-21 is when "every failure, even where the framework writes
  none" became true. If the owner reads the decision as "use Problem Details at all", the date is earlier
  and cannot be fixed exactly from history.
- The `429` that no endpoint declares in OpenAPI is a contract gap (every grouped endpoint can return it).
  Whether to declare it is an owner decision (it touches ADR-0018 and ADR-0019 as well).
- With E-0014-03 the file is about 125 lines, still under the ~150 ceiling.
- PRD-030 should name this ADR in its "Decisions Carried Into This Document" table (skill rule). This is a
  PRD edit, not an ADR edit.

---

---

## ADR-0015 — 0015-expected-business-failures-are-returned-as-results.md

Verdict: EDITS

### E-0015-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE
- Severity: consistency
- Why: `date` should be when services began returning `EnrichedResult`.
- Evidence: `git log --reverse -S EnrichedResult` → `89bf905b` (2025-01-24, "Refactoring updates while working out architecture"), which adds `Task<EnrichedResult<AccountEntity>> CreateAccountAsync` and `EnrichedResult.Fail`.
- old:
~~~
status: accepted
date: 2026-11-04
area: backend
~~~
- new:
~~~
status: accepted
date: 2025-01-24
area: backend
~~~

### E-0015-02 — DROPPED (house style)
- Was: The record is backfilled and must say so in Context and name its sources.
- Reason dropped: Only a backfilled/sources paragraph; the start date is carried by E-0015-01 and the SQ-07 reasons are already the drivers (the only residual fact is that `EnrichedResult` comes from AllOverIt).

### E-0015-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The failure is not always an `ApiDetailError`. `ApiBasicError` and `ApiDetailErrorCollection` are also used. The error does not carry the status it should produce: the status comes from the `ErrorType` in `ProblemTypes.ToStatusCode`.
- Evidence: `Pot.App/Errors/ApiErrorBase.cs`, `ApiBasicError.cs` (ErrorCode, ErrorMessage), `ApiDetailError.cs` (+PropertyName), `ApiDetailErrorCollection.cs`; `ApiDetailErrorFactory.cs:47-54` returns `ApiBasicError`; `EnrichedErrorExtensions.cs:15,33-41`; `ProblemTypes.cs:122-135`.
- old:
~~~
Chosen option: 1. Service methods return `EnrichedResult<Output>`: a success carries the value, a
failure carries an `ApiDetailError` naming the kind, the error code, the user-safe message and the
status it should produce. Handlers check the result and convert it to a response; the conversion from
error kind to status code exists once.
~~~
- new:
~~~
Chosen option: 1. Service methods return `EnrichedResult<Output>`: a success carries the value, a
failure carries an `ApiErrorBase` — an `ApiBasicError`, an `ApiDetailError` that also names the field, or
an `ApiDetailErrorCollection` — whose `ErrorType` names the kind, with an error code and a user-safe
message. The error does not carry a status. Handlers check the result and convert it with
`ToProblemDetails()`; the conversion from error kind to status code exists once, in
`ProblemTypes.ToStatusCode`.
~~~

### E-0015-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: omission
- Why: The record says refusals are values and faults are exceptions. Two kinds of refusal arrive as exceptions and are converted centrally, which is the pattern the record rejects as option 2. A reader would not otherwise know that.
- Evidence: `DatabaseExceptionHandler.cs:21-28` (unique, reference and concurrency exceptions → 409 `urn:pot:problem:data-conflict`); `BadRequestExceptionHandler.cs:18-50` (`BadHttpRequestException` → its status, a 400 validation problem).
- old:
~~~
Faults remain exceptions and reach the exception handler, so "the caller may not do this" and "the
server broke" never look the same.
~~~
- new:
~~~
Faults remain exceptions and reach the exception handler, so "the caller may not do this" and "the
server broke" never look the same. Two exception families are refusals rather than faults and are
converted centrally: a unique, reference or concurrency failure from the database becomes a `409`
(`DatabaseExceptionHandler`), and an unreadable request a `400` (`BadRequestExceptionHandler`).
~~~

### E-0015-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: omission
- Why: The structural distinction holds only for refusals a feature defines. A race the database detects is a refusal that arrives as an exception.
- Evidence: as E-0015-04; `DatabaseExceptionHandler.cs:21-23` comment ("two requests racing past an 'already exists' check").
- old:
~~~
- Good, because the distinction between a refusal and a fault is structural: one is a value, the other
  is an exception.
~~~
- new:
~~~
- Good, because for the refusals a feature defines the distinction from a fault is structural: one is a
  value, the other is an exception. A refusal the database discovers — a race past an "already exists"
  check — still arrives as an exception and is converted by the database exception handler.
~~~

### E-0015-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why: "cannot be forgotten" contradicts the record's own Bad consequence ("an ignored result is a silent success path"). Nothing forces a caller to handle the result; no analyzer exists.
- Evidence: ADR-0015:59-60; no analyzer or rule on unused `EnrichedResult` in the repo (`git grep -n "EnrichedResult" -- "*.editorconfig" "*.props"` returns nothing).
- old:
~~~
- Good: failure is part of the signature and cannot be forgotten by the compiler's silence — a caller
  has a result to handle.
~~~
- new:
~~~
- Good: failure is part of the signature, so a caller can see it has a result to handle, though nothing
  forces it to.
~~~

### E-0015-07
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: unsupported-history
- Why: "no measurement of it was taken, so it is not offered as a reason" claims an absence the record cannot show, and it sets aside a reason the owner gave and the guide still lists.
- Reworded for house style: Removed "the owner recalls" and the SQ-07 citation; the cost reason, the server guide's performance reason and the no-measurement caveat are kept.
- Evidence: DOCS-WORK-LISTS.md SQ-07 ("concern about the cost/performance impact of raising and catching exceptions was also a factor"); `Source/Server/DEVELOPER.md:2232-2238` ("Better performance (no stack unwinding)").
- old:
~~~
- Cost was a consideration in keeping exceptions for faults only; no measurement of it was taken, so it
  is not offered as a reason.
~~~
- new:
~~~
- The cost of raising and catching exceptions was a further reason to keep them for faults, and the
  server guide lists performance among its reasons; no measurement of that cost is recorded.
~~~

### Notes (no edit proposed)
- `.github/instructions/server.instructions.md:16` states the rule ("Business failures flow via
  `EnrichedResult` plus Problem Details mapping"). Once accepted, it could be cited as an Instruction line
  in Confirmation (the skill's step 4 would append `(ADR-0015)` there).

---

---

## ADR-0016 — 0016-validation-failures-answer-400-with-an-errors-map.md

Verdict: EDITS (the decision statement and Summary are true; the Outcome, a consequence and a Review rule
describe a `detail` that a `400` never carries)

### E-0016-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE
- Severity: consistency
- Why: The decision has an exact date: the commit that made the switch.
- Evidence: `git show -s bec69a48` (2026-10-16, "Refactored API to return 400 instead of 422"); DOCS-WORK-LISTS.md "Decisions with no PRD".
- old:
~~~
status: accepted
date: 2026-11-04
area: backend
~~~
- new:
~~~
status: accepted
date: 2026-10-16
area: backend
~~~

### E-0016-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: format
- Why: The record is backfilled and must say so and name its sources. It should also say what the prior state was (the commit proves it) and that no rationale was recorded, since the drivers are inferred.
- Reworded for house style: Removed backfilled statement, "the owner has not been asked", "not recalled" and the source list; the prior-422 state, the commit and the no-recorded-reason caveat are kept.
- Evidence: `git show bec69a48 -- .../ValidationResultExtensions.cs .../EnrichedErrorExtensions.cs .../axiosInterceptors.ts` (`Status422UnprocessableEntity` → `Status400BadRequest`; `Constraint`/`UnprocessableEntity` 422 → 400; client `case 422: ValidationError` → `case 400: BadRequestError`); commit message has no body; no SQ row covers it; PRD-026:428-433 records the earlier 422 refusal rule.
- old:
~~~
`422` was introduced for a request that is syntactically valid but semantically wrong — which describes
most validation rules here. Either could be defended on its own.
~~~
- new:
~~~

Until commit `bec69a48`, request validation and the constraint and unprocessable-entity refusals
answered `422` — PRD-026 records the earlier rule for refusals — and the client had no `400` branch; that
commit moved them all to `400`. Its message gives no reason, so the drivers below are read from the code
and PRD-030 rather than from a recorded rationale.
~~~

### E-0016-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: delete
- Tags: none
- Severity: unsupported-history
- Why: No evidence (commit, PRD, interview) supports proxy or tooling handling as a reason for the switch.
- Evidence: `git show -s --format=%B bec69a48` (subject only); DOCS-WORK-LISTS.md has no SQ row for 0024.
- old:
~~~
- Proxies, gateways and tooling handle the conventional status more consistently.
~~~
- new:
~~~
~~~

### E-0016-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: A `400` carries no `detail`. A refusal's message goes into `errors` under its field, or the empty key. The error code was removed by PRD-030 (B9) a week after this switch, so it is not this decision's reason.
- Evidence: `ProblemDetailsOptionsSetup.cs:63-83` ("Every 400 is a validation problem ... and no detail"); `EnrichedErrorExtensions.cs:29-46`; `ProblemTypes.cs:129-130`; `HttpResponseMessageExtensions.cs:112`; PRD-030 B1, B9.
- old:
~~~
Chosen option: 1. Validation failures answer `400` with a Problem Details body carrying the `errors` member,
which maps each field to its messages. Refusals from business rules answer `400` as well and are written
into the same shape, differing in the `detail` they carry; no `type` or error-code member distinguishes
them, because a client-visible error code was deliberately removed from the wire.
~~~
- new:
~~~
Chosen option: 1. Validation failures answer `400` with a Problem Details body carrying the `errors` member,
which maps each field to its messages, and no `detail`. Refusals from business rules that map to `400`
(`BadRequest` and `Constraint`) are written into the same shape: the message sits in `errors` under the
field it names, or under the empty key when it belongs to the whole request. No `type`, `detail` or
error-code member distinguishes a refusal from a validation failure; PRD-030 later removed the error code
from the wire (B9).
~~~

### E-0016-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: "the `detail` is the only signal" is false, because a `400` has no `detail`.
- Evidence: as E-0016-04.
- old:
~~~
- Bad, because `400` now covers two situations — the request was malformed, and the request was
  understood but refused — so the status alone does not tell the client which, and the `detail` is the
  only signal, which is prose rather than something to branch on.
~~~
- new:
~~~
- Bad, because `400` now covers two situations — the request was malformed, and the request was
  understood but refused — so nothing in the body tells the client which; the messages in `errors` are
  prose rather than something to branch on.
~~~

### E-0016-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: "the validation fixtures" is unnamed. The real guards are the shared helper, its callers, and `EnrichedErrorExtensionsFixture`, which pins refusals into the same shape.
- Evidence: `HttpResponseMessageExtensions.cs:93-115`; callers: `Features/Expenses/CreateUpdateFixture.cs`, `Expenses/RenewFixture.cs`, `Incomes/CreateUpdateFixture.cs`, `Incomes/RenewFixture.cs`, `Users/SiteAdministrabilityFixture.cs`, `Pipeline/CorrelationAndProblemDetailsFixture.cs`; `Pot.AspNetCore.Tests/Extensions/EnrichedErrorExtensionsFixture.cs:78-116` (`Should_Describe_A_Bad_Request_As_A_Validation_Problem`, `Should_File_A_Whole_Request_Refusal_Under_The_General_Key`, `BadRequest`/`Constraint` → 400).
- old:
~~~
- Test: the validation fixtures assert the `400` shape and the `errors` map for a request that fails a
  rule on one field.
~~~
- new:
~~~
- Test: `ShouldBeValidationProblemAsync`
  (`Pot.AspNetCore.Integration.Tests/Host/Extensions/HttpResponseMessageExtensions.cs`), used by the
  expense, income, user and pipeline fixtures, asserts the `400` status, the validation title, the
  expected field keys in `errors`, and no `detail`.
- Test: `EnrichedErrorExtensionsFixture` asserts `BadRequest` and `Constraint` map to `400`, a refusal is
  written as a validation problem, and a whole-request refusal is filed under the general key.
~~~

### E-0016-07
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: A refusal cannot carry its meaning in `detail`, which a `400` never has.
- Evidence: as E-0016-04.
- old:
~~~
- Review: `422` in a new endpoint is a defect; a refusal that needs its own meaning carries it in the
  `detail`, not in a new type or code.
~~~
- new:
~~~
- Review: `422` in a new endpoint is a defect; a refusal that needs its own meaning carries it in its
  message in `errors`, not in a new type or code.
~~~

### Notes (no edit proposed)
- ADR-0026:24-25 says "a data-dependent refusal is a business failure with its own type". This conflicts
  with 0016 and the code: a `400` refusal shares the validation shape and has no type. Flag it to whoever
  reviews ADR-0026.
- PRD-030's scope leaves open "whether state-dependent refusals should answer `409` rather than `400`".
  This is an open question adjacent to this decision, not yet a DD item.

---

---

## ADR-0017 — 0017-only-guid-row-identifiers-are-exposed.md

Verdict: EDITS

### E-0017-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: format
- Why: A backfilled record must say so in Context and name its sources. The owner's reason (SQ-14) is available.
- Reworded for house style: Removed backfilled statement, source list and "owner's account given during the documentation review"; the SQ-14 security reason is stated plainly.
- Evidence: `.github/skills/adr/SKILL.md` (Hard rules); `DOCS-WORK-LISTS.md` SQ-14: "a security measure intended to make routes harder to guess than incrementing identifiers".
- old:
~~~
Two uses want different things from an identifier, so the design answers both rather than compromising.
~~~
- new:
~~~

The public identifier is a security measure, to make routes harder to guess than incrementing identifiers
would. The record does not show whether the other options below were weighed.
~~~

### E-0017-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: `RowId` is not always generated on insert. An import creates rows with the `RowId` from the export file, so linked incomes and expenses resolve. Token claims also carry `RowId`s.
- Evidence: `Pot.App/Features/Accounts/Create/Models/Input.cs:5-6` ("Provided when importing"); `CreateAccountService.cs:51-53`; `AccountsImporter.cs:63-76`; `DbContextBase.cs:128-134` (`GuidValueGenerator`, `ValueGeneratedOnAdd`); `EntityBase.cs:5` (unique index); `Pot.AspNetCore/Concerns/Auth/Services/JwtService.cs:30-32` (`sub` and session claims are RowIds).
- old:
~~~
Chosen option: 1. `RowId` is a GUID generated on insert, indexed, and used as the identifier in routes,
request and response payloads and export files.
~~~
- new:
~~~
Chosen option: 1. `RowId` is a random GUID assigned when a row is added, or carried over from the export
file when an import creates the row. It sits under a unique index and is the identifier in routes, request
and response payloads, token claims and export files.
~~~

### E-0017-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: confirmation
- Why: The only mechanical guards are the instruction rule and the `:guid` route constraint. The record names neither.
- Evidence: `.github/instructions/server.instructions.md` Architecture ("never expose internal persistence ids"). All 16 identifier segments in `Pot.AspNetCore/Features/*/*Endpoints.cs` are `{id:guid}`. `contracts/openapi/v1.json` has every `id` path parameter as `format: uuid` and no integer id property.
- old:
~~~
- Review: a payload that carries an integer key, and a route that accepts one, are defects.
~~~
- new:
~~~
- Instruction: `.github/instructions/server.instructions.md` (Architecture): public contracts use `RowId`
  and `Etag`, and never expose internal persistence ids.
- Review: every identifier route segment is declared `{id:guid}`, so a route that accepts an integer key
  would have to change that constraint, which is the change to challenge.
~~~

### Notes (no edit proposed)
- `date: 2026-11-04` is the day the record was written, not the day the decision was made. `RowId` is in `EntityBase` from the earliest history (`git log -S"public Guid RowId"` reaches `bd5ec890`, 2025-03-06, a folder move). The real date is unknown. The same applies to all six ADRs in this group and needs an owner decision on how backfilled records are dated.
- Possible consequence, not verified by running it: the `RowId` unique index covers every site (`EntityBase.cs:5`), while the import lookup is site-filtered (`PotDbContext.cs:60-61`). Importing one site's export into a different site while the original rows still exist would then hit a unique-constraint failure (`AccountsImporter.cs:53-55` anticipates `UniqueConstraintException`). If that is intended, the export/import record should say so.

---

---

## ADR-0018 — 0018-the-openapi-document-is-generated-at-build-time-and-committed.md

Verdict: EDITS

### E-0018-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: format
- Why: Backfilled with no statement. The owner's account (SQ-25) contradicts the record's implied need, so it belongs in Context.
- Reworded for house style: Removed backfilled statement, source list and "owner's account"; the SQ-25 "no current use, added for completeness" reason is stated plainly.
- Evidence: `DOCS-WORK-LISTS.md` SQ-25: "no current requirement or client use; included for completeness and could help a future consumer such as an MCP server". `git log -S OpenApiGenerateDocumentsOnBuild` gives `7a7d85c8` (2026-10-19).
- old:
~~~
an artefact of the change rather than of the running process.
~~~
- new:
~~~

No requirement or API client uses the document today. It exists for completeness, and because a future
consumer such as an MCP server could be built on it. The record does not show whether the other options
below were weighed.
~~~

### E-0018-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: unsupported-history
- Why: "The clearest description of the contract that exists" is an inferred reason. The instructions and the owner state different ones.
- Reworded for house style: Removed "By the owner's account"; the "no consumer today" point now lives in E-0018-01's Context, so this keeps the two instruction-file reasons and the future consumer.
- Evidence: `.github/instructions/server.instructions.md` OpenAPI contract, bullet 2 (read without installing .NET; the change shows in the PR); `Pot.AspNetCore.csproj:8-9`; SQ-25.
- old:
~~~
It is committed because it is the clearest
description of the contract that exists, and because a consumer that is not the React client — a
generator, or an assistant-facing server — can be built on it without the API having to change.
~~~
- new:
~~~
It is committed so that a client developer can read
it from the repository without installing .NET, and so that a contract change appears in the pull request
beside the code that caused it. A consumer other than the React client, such as an assistant-facing
server, could be built on it later.
~~~

### E-0018-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: A missing diff is not visible. Committing the regenerated file is manual and unenforced, as the record's own Known gap and the instructions say.
- Evidence: `.github/instructions/server.instructions.md` OpenAPI contract, bullet 3 ("Nothing enforces this yet").
- old:
~~~
- Good, because a shape change produces a diff in the same commit, and a shape change with no document
  change is visible as a missing diff.
~~~
- new:
~~~
- Good, because a shape change produces a diff of the document when the regenerated file is committed
  with it; committing it is a manual step, so an omitted document is noticed only by a reviewer.
~~~

### E-0018-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: `EndpointResponseMetadataFixture` does not pin which failures each endpoint declares. It asserts shared rules over every endpoint, plus the success shape of 7 named routes. ADR-0019's Confirmation already says it does not assert an endpoint's own failure set, so the two records contradict each other. The closest per-endpoint pin is in `OpenApiSecurityFixture`.
- Evidence: `Pot.AspNetCore.Integration.Tests/Pipeline/EndpointResponseMetadataFixture.cs:12-182` (500, single 400 validation problem, no 422, 409 shape, success not Problem Details, 413/408 only for body-accepting endpoints, named success shapes). It reads `EndpointDataSource`, not the document. `OpenApiSecurityFixture.cs:460-531` pins the nine operations that can answer a stale-entity-tag 409.
- old:
~~~
- Test: `EndpointResponseMetadataFixture` pins which failures each endpoint declares, which is what the
  document records, and `OpenApiSecurityFixture` checks the served document's security and schema shape.
~~~
- new:
~~~
- Test: `EndpointResponseMetadataFixture` asserts the declarations every endpoint shares — the `500`, a
  single validation-problem `400`, no `422`, and `413` and `408` only where a body is read — and the
  success shape of named routes. `OpenApiSecurityFixture` reads the served document, checks its security,
  schemas and problem descriptions, and pins which operations can answer a stale-entity-tag `409`. Neither
  pins every endpoint's full failure set.
~~~

### E-0018-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: delete
- Tags: none
- Severity: factual
- Why: Client types are hand-written, and the document generates nothing for the client. Per-endpoint declarations therefore cannot affect which bodies the client can read. Every failure is Problem Details (ADR-0014), normalised by one interceptor (ADR-0039).
- Evidence: this ADR's own line 47-48 ("not used to generate the client"); `DECISIONS-TO-DISCUSS.md` DD-07 (no codegen; responses are cast).
- old:
~~~
- Per-endpoint failure declarations are also what keeps the client able to read any body it receives.
~~~
- new:
~~~
~~~

### Notes (no edit proposed)
- DD-07 (hand-written client types) is undecided. The record states the current state honestly and does not choose an option. Consistent.
- Undated record date: see the ADR-0017 note. The earliest evidence of build-time generation is `7a7d85c8` (2026-10-19). It is not on `main` (it is on `v0.1`), so per the skill it should not be used as the date if a squash merge may drop it.

---

---

## ADR-0019 — 0019-endpoints-are-minimal-api-slices-with-their-own-models.md

Verdict: EDITS

### E-0019-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: format
- Why: Backfilled with no statement. The owner's reasons (SQ-03, SQ-04, SQ-05) are recorded and match the drivers.
- Reworded for house style: Removed backfilled statement, source list and "owner's account"; the SQ-03/04/05 reasons and the not-weighed caveat are kept.
- Evidence: `DOCS-WORK-LISTS.md` SQ-03, SQ-04, SQ-05.
- old:
~~~
Minimal APIs make the operation the unit: its route, its metadata and its handler are declared together.
~~~
- new:
~~~

Minimal APIs suit a pure API with no views. Independent handlers inject only what they need, are easier
to test, and can be configured individually for concerns such as Problem Details and rate limiting.
Operation slices map well to the client's calls. Per-operation models and mapping keep each operation
independently changeable, and only genuinely common models are shared. The record does not show whether
returning entities or generated handlers were weighed.
~~~

### E-0019-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: Entity checks are not in the operation's endpoint folder. The host folder holds the handler, request, response, request mapping and validator. The checks sit in the mirrored folder in `Pot.App`.
- Evidence: `Pot.AspNetCore/Features/Accounts/Create/` (Handler, Request, RequestMapping, RequestValidator, Response); `Pot.App/Features/Accounts/Create/` (service, `Models/Input`/`Output`, `Mappings`, `EntityChecks`).
- old:
~~~
Chosen option: 1. Each operation has its own folder containing its handler, its request and response
models, its mapping, its validation and its entity checks.
~~~
- new:
~~~
Chosen option: 1. Each operation has its own folder in the host containing its handler, its request and
response models, its request mapping and its request validator, and a folder of the same name in the
application layer holding its service, input and output models, mapping and entity checks.
~~~

### E-0019-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: Only the standard failures (400 validation problem and 500) and the rate-limit policy are declared per group. Each operation declares its own success response and its own 401/404/409/408/413. The current text contradicts ADR-0018 ("Each endpoint declares the failures its own rules can produce").
- Evidence: `Pot.AspNetCore/Extensions/RouteGroupBuilderExtensions.cs:14-19` (`ProducesStandardProblems`); `Features/Accounts/Extensions/WebApplicationExtensions.cs` (group: tags, standard problems, `RateLimiterPolicy.Chained`); `Features/Accounts/Extensions/RouteGroupBuilderExtensions.cs:20-95` (per-operation `Produces`, `ProducesProblem(401/404/409)`, `ProducesStaleEntityTagConflict`, `ProducesRequestBodyProblems`). The same group shape appears in all 13 feature groups.
- old:
~~~
Response metadata and the rate-limit policy are declared once per feature group, not per operation, so a
new route in a feature inherits them.
~~~
- new:
~~~
The standard failures (the validation-problem `400` and the `500`) and the rate-limit policy are declared
once per feature group, so a new route in a feature inherits them. Each operation declares its own success
response, its authorization and the failures specific to it, such as `401`, `404`, `409` and the
request-body `408` and `413`.
~~~

### E-0019-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: The stated reason is wrong (the operation makes its own failure declarations). An existing test mechanically checks per-operation models, and the record does not name it.
- Evidence: `EndpointResponseMetadataFixture.cs` (no per-endpoint failure assertions); `Pot.AspNetCore.Integration.Tests/Pipeline/OpenApiSecurityFixture.cs:91-183` (`Auth_Login_Request` vs `Expenses_Create_Request`; no bare `Request`/`Response` schema; `Expenses_Get_AccountModel` differs from `Incomes_Get_AccountModel`).
- old:
~~~
It does not
  assert a single endpoint's own failure set, because those declarations are made per feature group.
~~~
- new:
~~~
It does not
  assert an operation's own failure set, which each operation declares for itself.
- Test: `OpenApiSecurityFixture.Should_Use_Distinct_Schemas_For_Feature_Specific_Requests_And_Responses`
  asserts that operations publish feature-specific request and response schemas rather than one shared
  `Request` or `Response`.
~~~

### Notes (no edit proposed)
- Verified: 47 handlers, all `public static Invoke`. The single untyped result is `Auth/Logout/Handler.cs:10` (`Task<IResult>`). 22 of 32 `Response` classes derive from `Pot.AspNetCore/Models/ResponseBase.cs` (`RowId`, `Etag`). No mapping library appears in `Directory.Packages.props` or any csproj. 13 `Add*Endpoints()` are chained in `Program.cs:120-132`.

---

---

## ADR-0020 — 0020-the-server-is-layered-with-feature-slices-mirrored-across-layers.md

Verdict: EDITS

### E-0020-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: format
- Why: Backfilled with no statement. The owner's account (SQ-01, SQ-02) is recorded.
- Reworded for house style: Removed backfilled statement, source list and "owner's account"; the SQ-01/02 reasons and the not-weighed caveat are kept.
- Evidence: `DOCS-WORK-LISTS.md` SQ-01 ("past-project experience"), SQ-02.
- old:
~~~
Grouping by feature instead means the same name identifies the
feature everywhere.
~~~
- new:
~~~

Separating HTTP, business logic and data follows experience on past projects. Feature folders are
mirrored across the layers to separate cross-cutting concerns from features and to make code predictable
to find. The record does not show whether the other options below were weighed.
~~~

### E-0020-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: Request validation is not in the application project. All 24 `RequestValidator` classes live in the host beside their endpoints, which is what ADR-0026 calls "at the edge". `Pot.App` holds only the validation registrar. It also holds the calculators.
- Evidence: 27 files deriving from a validator base are in `Pot.AspNetCore`, and none in `Pot.App` except `Pot.App/Extensions/ServiceCollectionExtensions.cs` (the registration filter). See also `Pot.App/Concerns/Validation/` and `Pot.App/Calculators/`.
- old:
~~~
application project holds orchestration, validation and entity checks; the data project holds entities,
~~~
- new:
~~~
application project holds orchestration, entity checks and calculations, while request-shape validators sit
beside each endpoint in the host; the data project holds entities,
~~~

### E-0020-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The host references the data project directly, and its auth services use repositories without going through the application. A reader would otherwise believe the host reaches data only through the application.
- Evidence: `Pot.AspNetCore/Pot.AspNetCore.csproj:107-108`. `using Pot.Data.Repositories` appears in `Concerns/Auth/Services/AuthService.cs`, `AuthSessionService.cs`, `PermissionService.cs` and `Concerns/Auth/Configuration/JwtBearerEventsSetup.cs`. `Pot.App.csproj:13-14` references Data and EmailSender only, and `Pot.EmailSender.csproj:10-11` references Shared.
- old:
~~~
Dependencies point host → application → data, and shared is reached through data rather than referenced
directly by the application.
~~~
- new:
~~~
Dependencies point host → application → data → shared. The host also references the data project
directly, and its authentication services read users, sessions and permissions through repositories
without passing through the application. The application reaches shared only through data and the email
sender, never by a direct reference.
~~~

### E-0020-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: unsupported-history
- Why: The "because" is an inferred reason. No evidence records why authentication was placed in the host.
- Evidence: W4 row 0037 and SQ-01/SQ-02 record the placement but not a reason.
- old:
~~~
Authentication lives
in the host because everything it touches — tokens, cookies, claims, the pipeline — is transport, but it is
not purely transport:
~~~
- new:
~~~
Authentication lives
in the host, beside the tokens, cookies, claims and pipeline it works with; the record does not show why it
was placed there rather than in the application. It is not purely transport:
~~~

### E-0020-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: unsupported-history
- Why: "Deliberate" is not evidenced. The direct data access (E-0020-03) is the material part of this exception.
- Evidence: as E-0020-03 and E-0020-04.
- old:
~~~
- Bad, because the host is not purely transport: it also owns authentication rules, which is a deliberate
  exception rather than a clean separation.
~~~
- new:
~~~
- Bad, because the host is not purely transport: it also owns authentication rules and reads data directly
  for them, an exception to a clean separation.
~~~

### E-0020-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: The first bullet is half wrong and contradicts the record's own Consequence ("enforced by project references"). A reference from a lower project to a higher one closes a cycle, and the build refuses that. A reference that skips a layer is not caught. The second bullet is a review heuristic, not a test.
- Evidence: current references form an acyclic chain (`*.csproj` above); MSBuild rejects circular project references; no architecture-test package (`NetArchTest`/`ArchUnit`) is referenced.
- old:
~~~
- Review: a project reference that points backwards is a defect, because the compiler alone will not
  prevent it appearing.
- Test: the unit projects are per layer, so a test that needs the host is evidence the rule leaked into
  the wrong layer.
~~~
- new:
~~~
- Build: a reference from a lower project to a higher one closes a cycle, which the build refuses.
- Review: a new project reference that skips a layer, or that points a support project at a layer, is a
  defect the build does not catch; the host's direct reference to the data project already exists and is
  used by authentication.
- Instruction: `.github/instructions/server.instructions.md` (Architecture) states the layering direction.
- Review: the unit projects are per layer, so a unit test that needs the host is evidence the rule leaked
  into the wrong layer.
~~~

### Notes (no edit proposed)
- Verified: migrations live in `Pot.Data/Migrations`, and `Pot.Data.Migrations` is the runner. Data repositories are grouped by feature (`Pot.Data/Repositories/{Accounts,…}`), while entities and specifications are flat. `Workers` appears only in the host, and `Notifications`/`Otp` only in the application, which is consistent with "features appear in some layers and not others".

---

---

## ADR-0021 — 0021-entity-framework-core-is-the-persistence-mechanism.md

Verdict: EDITS

### E-0021-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: unsupported-history
- Why: "familiarity matters more than the last increment of control" is an inferred trade-off. The owner's account (SQ-40) gives prior experience, built-in migrations and LINQ composition. The record is also backfilled but does not say so, and nothing shows that Dapper, a micro-ORM or ADO.NET were weighed.
- Reworded for house style: Removed "The owner recalls", the SQ-40 citation and the backfilled/sources paragraph; the three reasons and the not-weighed caveat are kept.
- Evidence: `DOCS-WORK-LISTS.md` SQ-40.
- old:
~~~
The team writing it is small, and familiarity matters more than the last increment of control.
~~~
- new:
~~~
Entity Framework Core was chosen for prior experience with it, its built-in migration support, and how
easily LINQ queries compose. The record does not show whether the other options below were weighed.
~~~

### E-0021-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The four query filters are not a central convention. Each is declared per entity in `PotDbContext`. The central conventions in `DbContextBase` are table naming, `RowId` generation, enriched enums and restricted cascades. Identity resolution applies to tracked-shape entity instances, not to DTO projections.
- Evidence: `Pot.Data/DbContextBase.cs:58-66` (`NoTrackingWithIdentityResolution`), `:78-94` (naming, cascade, RowId), `:136-140` (enriched enum); `Pot.Data/PotDbContext.cs:55-76` (four `HasQueryFilter` calls).
- old:
~~~
Conventions are applied centrally — entity naming, value generation, enriched enum storage, query filters
— rather than per entity. Reads do not track by default, with identity resolution enabled so a projection
still returns one instance per key.
~~~
- new:
~~~
Conventions are applied centrally in `DbContextBase` rather than per entity: entity naming, `RowId`
value generation, enriched enum storage and restricted cascades. The four tenant query filters are
declared per entity in `PotDbContext`. Reads do not track by default. Identity resolution is enabled, so
a query that reaches the same row twice materialises one instance of it.
~~~

### E-0021-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: TEMP-DOC-CITE
- Severity: omission
- Why: The in-memory provider ignores `citext`, unique indexes and SQL translation. Whether this split is the intended test strategy is still open (DD-12d), so the record should not present it as settled.
- Evidence: `DECISIONS-TO-DISCUSS.md` DD-12d; `grep UseInMemoryDatabase` finds 15 files in `Pot.Data.Tests` and 4 in `Pot.App.Tests`; `Pot.AspNetCore.Integration.Tests/IntegrationFixtureBase.cs:96` (`postgres:13` Testcontainer).
- old:
~~~
- Test: the data-layer fixtures run against the in-memory provider, so repository rules are covered without
  a container; queries that depend on provider translation are covered only by the hosted suite, which runs
  against a real PostgreSQL instance.
~~~
- new:
~~~
- Test: the data-layer fixtures run against the in-memory provider, so repository logic is covered
  without a container. That provider ignores `citext`, unique indexes and SQL translation, so those are
  covered only by the hosted suite, which runs against a real PostgreSQL container. Whether that split is
  the intended test strategy is still open (`DECISIONS-TO-DISCUSS.md`, DD-12d).
~~~

### E-0021-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why: The "separate decisions" exist as ADRs and should be linked.
- Evidence: `Docs/adr/0022-…`, `0023-…`, `0024-…` exist; ADR-0023's Summary covers untracked reads.
- old:
~~~
- No-tracking reads and explicit transactions are separate decisions.
- The database engine choice is a separate decision.
~~~
- new:
~~~
- No-tracking reads are part of the repository access model,
  [ADR-0023](0023-services-access-data-through-feature-repositories.md). Explicit transactions are
  [ADR-0024](0024-transactions-are-explicit-and-only-where-writes-must-be-one-unit.md).
- The database engine choice is [ADR-0022](0022-postgresql-is-the-relational-datastore.md).
~~~

### Notes (no edit proposed)
- The no-tracking default appears in both this record's Decision Outcome and ADR-0023's decision. E-0021-02
  keeps it here only as a description of how the context is configured. If the owner wants strict
  one-decision-per-file, the sentence can be removed from this record.
- The Confirmation is weak for a decision of this kind. Nothing mechanical fails if a second persistence
  mechanism is introduced. This is acceptable for an accepted ORM choice, but the owner should know.
- Package versions are correct: EF Core `[10.0.14,11.0.0)` and Npgsql `[10.0.3,11.0.0)` in
  `Directory.Packages.props:29,42`.

---

---

## ADR-0022 — 0022-postgresql-is-the-relational-datastore.md

Verdict: EDITS

### E-0022-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: unsupported-history
- Why: The record is backfilled but does not say so. The owner's reason (SQ-39) is cost and open source. The schema features are presented as the drivers of the choice, but no evidence shows that.
- Reworded for house style: Removed backfilled statement, source list, "owner's recollection" and the SQ-39 citation; the cost/open-source reason and the caveat that the schema features were not shown to drive the choice are kept.
- Evidence: `DOCS-WORK-LISTS.md` SQ-39: "open-source and cost-effective for a small application that was not expected to grow very large".
- old:
~~~
Cost matters,
because this is a personal project with a single operator.
~~~
- new:
~~~


The engine was chosen because it is open source and cost-effective for a small application that is not
expected to grow very large. The schema features above are what the schema relies on now; the record does
not show whether they, or the other options below, were weighed when the engine was chosen.
~~~

### E-0022-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: delete
- Tags: none
- Severity: unsupported-history
- Why: Nothing in the owner's account or the repository shows "managed hosting" as a driver, and the repository cannot confirm that the production server is a managed offering.
- Evidence: SQ-39 names only cost and open source. `Docs/Azure/POT-Azure-Diagram.mmd:18` says only "PostgreSQL / pot-postgres-prod / PostgreSQL 17.6". There is no IaC (`DOCS-WORK-LISTS.md` line 382).
- old:
~~~
- A managed hosting option, because the deployment is a single operator's.
~~~
- new:
~~~
~~~

### E-0022-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: unsupported-history
- Why: Same reason as E-0022-02. "Managed server" states an Azure setting that cannot be confirmed from the repository.
- Evidence: as E-0022-02.
- old:
~~~
1. PostgreSQL, run in a container locally and as a managed server in production.
~~~
- new:
~~~
1. PostgreSQL, run in a container locally and as a hosted server in production.
~~~

### E-0022-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: unsupported-history
- Why: The rationale credits `citext` for the choice, but the owner's reason was cost. The claim that the index replaces "application comparison" is false: application checks exist and the index is a second line of defence. "Managed" is unconfirmed. SSL is owner-confirmed.
- Reworded for house style: Removed "owner-confirmed", the SQ-38/SQ-39 and `DOCS-WORK-LISTS.md` citations and the "repository does not record" aside; the cost reason is now "Chosen option: 1, because …", "managed" becomes "hosted" (as E-0022-03), and "per-site" becomes "case-insensitive" (see E-0027-02).
- Evidence: SQ-39; SQ-18 ("application checks provide clearer business-rule errors; database constraints are a second defense"); `Pot.App/Features/{Accounts,Expenses,Incomes}/{Create,Update}/EntityChecks/Checks/CheckDescriptionDoesNotExist.cs`; SQ-38 ("PostgreSQL connections require SSL").
- old:
~~~
Chosen option: 1. PostgreSQL runs as a container for local development and for tests, and as a managed
server in production with SSL required on connections.

The engine's case-insensitive text type is what makes per-site uniqueness of names implementable as an
index rather than as application comparison, and the same engine can be run locally and hosted.
~~~
- new:
~~~
Chosen option: 1, because it is open source and cost-effective for a small application that is not
expected to grow very large. PostgreSQL runs as a container for local development and for tests, and in
production as a hosted server in Azure, with SSL required on connections.

The schema has since come to rely on `citext`, which lets a unique index back the case-insensitive
uniqueness of names alongside the application's own check. The same engine runs locally and hosted.
~~~

### E-0022-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The schema has no partial (filtered) indexes. Uniqueness is not enforced "without application code", because application checks come first. The "managed option" claim is unconfirmed.
- Evidence: `grep -rn "HasFilter" Source/Server/Pot.Data` finds nothing, and the model snapshot has no `filter:`. Unique indexes are declared by attribute (`AccountEntity.cs:7`, `ExpenseEntity.cs:8`, `IncomeEntity.cs:8`, `SettingEntity.cs:6`, `SiteEntity.cs:7`, `UserEntity.cs:9`). `CheckDescriptionDoesNotExist` checks exist; SQ-18.
- old:
~~~
- Good, because there is no licence cost at any environment, and the managed option removes most of the
  operational work.
- Good, because `citext` and partial indexes express the uniqueness rules without application code.
~~~
- new:
~~~
- Good, because there is no licence cost at any environment.
- Good, because `citext` columns under composite unique indexes enforce case-insensitive uniqueness in
  the database, as a second line of defence behind the application checks.
~~~

### E-0022-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: 17.6 comes only from a diagram label, which the brief requires to be attributed. "Pinned" overstates `SetPostgresVersion`, which is a compatibility target, not a package or engine pin.
- Evidence: `Docs/Azure/POT-Azure-Diagram.mmd:18`; `Pot.Data/Extensions/DbContextOptionsBuilderExtensions.cs:11`; `Source/Docker/Postgres/Dockerfile:16` (`postgres:13`); `IntegrationFixtureBase.cs:96` (`postgres:13`); `playwright.globalSetup.ts:516` (`postgres:16`); DD-06.
- old:
~~~
the local container and the integration
  fixtures run 13, the end-to-end stack runs 16 and the deployed server runs 17.6, and the provider is
  pinned to the oldest of them.
~~~
- new:
~~~
the local container and the integration
  fixtures run 13, and the end-to-end stack runs 16. The deployment diagram labels the production server
  17.6, which nothing else in the repository confirms. EF is told to target 13.6 (`SetPostgresVersion`).
~~~

### E-0022-07
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The schema uses no partial indexes, so "partial-index behaviours the schema uses" is false.
- Evidence: as E-0022-05.
- old:
~~~
- Bad: historically weaker support for the case-insensitive-text and partial-index behaviours the schema
  uses, and no advantage over PostgreSQL here.
~~~
- new:
~~~
- Bad: no `citext` equivalent, because case-insensitivity comes from column collations instead, and no
  advantage over PostgreSQL here.
~~~

### E-0022-08
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: TEMP-DOC-CITE
- Severity: consistency
- Why: The open item is DD-06, and the uniqueness decision is ADR-0027. Both should be linked. "Rather than only by application checks" also misstates SQ-18 (both mechanisms are used).
- Evidence: `DECISIONS-TO-DISCUSS.md` DD-06; `Docs/adr/0027-enumerations-are-stored-by-name-and-uniqueness-is-enforced-by-the-database.md`.
- old:
~~~
- The version target and the drift between environments is a separate, open item.
- Uniqueness being enforced by the database rather than only by application checks is a separate decision.
~~~
- new:
~~~
- The version target and the drift between environments are a separate, open item:
  `DECISIONS-TO-DISCUSS.md` (DD-06).
- Enforcing uniqueness in the database as well as by application checks is
  [ADR-0027](0027-enumerations-are-stored-by-name-and-uniqueness-is-enforced-by-the-database.md).
~~~

### Notes (no edit proposed)
- Option 2's wording, "SQL Server, with its developer and small-worker licensing", is unclear. Its con,
  "licensing for a personal project", ignores the free Express edition. This is alternative analysis
  rather than a claim about the system, so no edit is proposed. The owner may want to reword it.
- DD-06 is undecided. The record states the drift honestly and does not present option B as decided.
- `date: 2026-11-04` is the date the record was written (see the note under ADR-0006).
- ADR-0027, the uniqueness target, bundles enum storage and uniqueness. That belongs to another reviewer's
  group, but it affects where E-0022-08 points.

---

---

## ADR-0023 — 0023-services-access-data-through-feature-repositories.md

Verdict: EDITS

Verified: no service in `Pot.App` or `Pot.AspNetCore` injects `PotDbContext`. Its only uses are
registration and the health check (`WebApplicationBuilderExtensions.cs:56,532`). `IUserRepository` and
`ISiteRepository` expose no `IQueryable`. The account, expense, income and role repositories each expose
one. `GetAccountService`, `GetExpenseService` and `GetIncomeService` take persistable interfaces while
only reading, so the "split is not applied uniformly" claim is correct.

### E-0023-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: format
- Why: The record is backfilled but does not say so. The owner's reasons are recorded and should be cited, and the other options have no evidence of being weighed.
- Reworded for house style: Removed the backfill statement, the source list and the "owner's recollections"/SQ-09..11 citations; the three reasons are stated directly, and the "not weighed" line follows the cleanup wording.
- Evidence: `DOCS-WORK-LISTS.md` SQ-09, SQ-10, SQ-11.
- old:
~~~
The cost is that a new query shape is a new method.
~~~
- new:
~~~


Named repository methods make service code easier to read and let a service test mock the database
calls. The split between read-only and persistable interfaces follows the Interface Segregation
Principle, and reads do not track, for performance. The alternatives below record what each would have
cost; the record does not show whether they were weighed before the choice.
~~~

### E-0023-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The claim holds only for users and sites. The account, expense, income and role repositories expose a queryable that services compose. The Decision Outcome says so, and this consequence contradicts it.
- Evidence: `IAccountRepository.Accounts`, `IExpenseRepository.Expenses`, `IIncomeRepository.Incomes`, `IRoleRepository.Roles`; composed at `Pot.App/Features/Accounts/Create/EntityChecks/Checks/CheckDescriptionDoesNotExist.cs:42`, `Roles/GetAll/GetAllRolesService.cs:31`, `Users/Invite/EntityChecks/Checks/CheckValidRoles.cs:31`.
- old:
~~~
- Bad, because a caller cannot refine a query after the fact — the repository must anticipate the shapes
  it needs, and one that guesses wrong is extended.
~~~
- new:
~~~
- Bad, because a caller of the user or site repository cannot refine a query after the fact. The
  repository must anticipate the shapes it needs, and one that guesses wrong is extended. The account,
  expense, income and role repositories avoid this by exposing a queryable that callers compose.
~~~

### E-0023-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: The cited test does not enforce the rule. Nothing fails if a service injects the public `PotDbContext`. Four `Pot.App.Tests` fixtures use the in-memory provider rather than substitutes. A real fixture and an instruction do enforce the tracking default, and neither is cited.
- Evidence: `UseInMemoryDatabase` in `Pot.App.Tests/Features/Accounts/{Create,Update}/EntityChecks/Checks/CheckDescriptionDoesNotExistFixture.cs`, `Expenses/Delete/DeleteExpenseServiceFixture.cs`, `Projections/ProjectionsServiceFixture.cs`. `Pot.Data.Tests/Extensions/DbContextExtensionsFixture.cs:60-106` asserts `NoTrackingWithIdentityResolution` and the nested scopes. `.github/instructions/server.instructions.md:33`. There is no architecture-test package in the solution.
- old:
~~~
- Test: the application-layer fixtures substitute the interfaces, so a repository dependency reaching for
  the database would fail the test rather than pass.
~~~
- new:
~~~
- Test: most application-layer fixtures build services with substituted repository interfaces, so
  business rules run without a database. Four use the EF in-memory provider instead: the account
  description checks, the expense delete fixture and the projections fixture. Nothing fails if a service
  injects `PotDbContext`, which is public, so that part of the rule rests on review.
- Test: `DbContextExtensionsFixture` asserts that the context defaults to
  `NoTrackingWithIdentityResolution` and that nested tracking scopes restore it.
- Instruction: `.github/instructions/server.instructions.md` (Persistence: default reads to no-tracking;
  opt into tracking only on write paths).
~~~

### E-0023-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why: The "separate decisions" exist as ADRs and should be linked.
- Evidence: `Docs/adr/0024-…`, `0025-…`.
- old:
~~~
- Tenant confinement by query filters and site-scoped reads builds on this access model and is a separate
  decision.
- Explicit transactions are used where several repository calls must be one unit, which is a separate
  decision.
~~~
- new:
~~~
- Tenant confinement by query filters and site-scoped reads builds on this access model:
  [ADR-0025](0025-tenant-confinement-is-structural-query-filters-and-site-scoped-reads.md).
- Explicit transactions are used where several repository calls must be one unit:
  [ADR-0024](0024-transactions-are-explicit-and-only-where-writes-must-be-one-unit.md).
~~~

### Notes (no edit proposed)
- One Consequences bullet, "Good, because untracked reads…", sits between "Bad" bullets. That is ordering
  only, so no edit is proposed.
- `date: 2026-11-04` is the date the record was written (see the note under ADR-0006).

---

---

## ADR-0024 — 0024-transactions-are-explicit-and-only-where-writes-must-be-one-unit.md

Verdict: EDITS

There are exactly two transactions, both through `IPotTransactionFactory`:
`Pot.App/Features/Maintenance/Import/ImportDataService.cs:89` (three importers save once each) and
`Pot.App/Features/Users/UpdateRoles/UpdateUserRolesService.cs:46` (one save). The decision statement
"more than one save opens a transaction" has exceptions. Signup and password-reset verification save twice
without one: `VerificationServiceBase.cs:85` calls `OtpService.UpdateExpiredRequestsAsync`, which saves
(`OtpService.cs:56`), and then `VerifySignupService.cs:97` and `VerifyPasswordResetService.cs:92` save
again.

### E-0024-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: format
- Why: The record is backfilled but does not say so. The owner's reason (SQ-12) should be cited, and the other options have no evidence of being weighed.
- Reworded for house style: Removed the backfill statement, the source list and the "owner's recollection"/SQ-12 citation; the reason is stated directly, and the "not weighed" line follows the cleanup wording.
- Evidence: `DOCS-WORK-LISTS.md` SQ-12.
- old:
~~~
Wrapping everything in a transaction would be uniform, and it would hold locks for longer and couple
unrelated work into one unit for no reason.
~~~
- new:
~~~


An explicit transaction is for exactly the case where EF calls made across several services cannot
otherwise act as one atomic unit, so that a failure rolls all of them back. The alternatives below record
what each would have cost; the record does not show whether they were weighed before the choice.
~~~

### E-0024-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: As written, the statement is false. Two operations save twice without a transaction, because their first save is independent housekeeping (expiring stale codes). The record also should not suggest a reason for the role-assignment exception, since none is recorded. Naming the one real multi-save unit makes the rule concrete.
- Evidence: `VerificationServiceBase.cs:85`; `OtpService.cs:40-56`; `VerifySignupService.cs:97`; `VerifyPasswordResetService.cs:92`; `ImportDataService.cs:89-113`; `UpdateUserRolesService.cs:46-80`.
- old:
~~~
Chosen option: 1. An operation that writes through more than one save opens a transaction around the
whole unit, and the early-return failure paths leave without committing, so disposing the transaction at
the end of the operation rolls the work back. An operation that saves once does not open one, with one
exception — a role assignment opens a transaction around its single save.
~~~
- new:
~~~
Chosen option: 1. An operation whose saves must succeed together opens a transaction around the whole
unit. Its early-return failure paths leave without committing, so disposing the transaction at the end
of the operation rolls the work back. Today the only such operation is the data import: its account,
expense and income importers each save once, inside one transaction. Signup and password-reset
verification also save twice without a transaction, because the first save only expires stale codes and
need not roll back with the second.

An operation that saves once does not open a transaction, with one exception: a role assignment opens
one around its single save. The record does not show why.
~~~

### E-0024-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: The "Test:" bullet describes a test that does not exist. The gap line also says the import transaction is "covered through the version gate", which is wrong: `ImportVersionGateFixture` rejects the package before the transaction is opened, with the factory substituted. An instruction that does state the rule is not cited.
- Evidence: `Pot.App.Tests/Features/Maintenance/ImportVersionGateFixture.cs:28-72`; the version gate in `ImportDataService.cs:73-80` returns before `:89`. `Pot.AspNetCore.Integration.Tests/Features/Maintenance/` holds only `Export`. `aspnetcore-integration-tests.instructions.md` says `POST /api/maintenance/import` is ignored by the authorization matrix. `.github/instructions/server.instructions.md:34`.
- old:
~~~
- Test: the failure path of a multi-write operation is exercised with the second write failing, and the
  first is asserted absent afterwards.
- Gap: no such test exists yet; the import path is the operation the rule exists for, and its transaction
  is only covered through the version gate.
- Review: a service that performs two saves without a transaction is a defect, and an operation that opens
  one for a single save is questioned.
~~~
- new:
~~~
- Instruction: `.github/instructions/server.instructions.md` (Persistence: keep transaction boundaries
  explicit for multi-step writes).
- Gap: no test exercises a rollback. The import is the operation the rule exists for, and nothing covers
  its transaction:
  - `ImportVersionGateFixture` rejects the package before the transaction is opened, with the transaction
    factory substituted;
  - no hosted fixture calls the import;
  - the authorization matrix ignores it.
- Review: a service whose saves must succeed together but run without a transaction is a defect, and an
  operation that opens one for a single save is questioned.
~~~

### Notes (no edit proposed)
- The role-assignment transaction wraps the reads and the last-admin check as well as the save. Under the
  default Read Committed isolation it does not serialise that check against a concurrent change. The
  owner may want to decide whether that transaction has a purpose.
- In `VerifySignupService`, the failed-attempt counter (`AttemptCount++` in `VerificationServiceBase`) is
  saved only on the success path. This is outside this ADR, but it was noticed while checking the saves.
- `date: 2026-11-04` is the date the record was written (see the note under ADR-0006).

---

---

## ADR-0025 — 0025-tenant-confinement-is-structural-query-filters-and-site-scoped-reads.md

Verdict: EDITS

Verified: the four `HasQueryFilter` calls are at `PotDbContext.cs:55-76`. `IUserRepository` and
`ISiteRepository` expose no queryable. The site-scoped reads take no site parameter (`UserRepository.cs`
`FindForCurrentSiteAsync`, `GetCurrentSiteId`). A foreign id returns `null`, the same as a missing one,
and is logged as a cross-tenant refusal.

### E-0025-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: RELATED-PRDS
- Severity: consistency
- Why: The users-and-sites half of this decision, the refusal contract and the rejection of a `User`/`Site` filter all come from PRD-026, which is Complete.
- Evidence: `Docs/Future/PRD-026-cross-tenant-authorization-hardening.md` §§4.1, 4.2, 4.7; `DOCS-WORK-LISTS.md` SQ-13 ("owner directed to PRD-026 §§4.1, 4.7").
- old:
~~~
area: backend
---
~~~
- new:
~~~
area: backend
related-prds: [PRD-026]
---
~~~

### E-0025-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: format
- Why: The record is backfilled but does not say so. PRD-026 is real evidence for part of the history and should be cited. Option 2 was in fact considered and rejected (PRD-026 §4.7), but nothing shows that options 3 and 4 were weighed.
- Reworded for house style: Removed the backfill statement, the closing source list and the "owner's direction"/SQ-13 citation; kept the PRD-026 §§4.1, 4.2, 4.7 history and the honest limits on what is known about the entity filters and options 3 and 4.
- Evidence: PRD-026 §§4.1, 4.2, 4.5, 4.7; SQ-13.
- old:
~~~
resolve which site the caller is in.
~~~
- new:
~~~


The site-scoped repository methods for users and sites, and the refusal contract, come from
[PRD-026](../Future/PRD-026-cross-tenant-authorization-hardening.md) (§§4.1, 4.2). That PRD also
considered a filter on `User` and `Site` and rejected it (§4.7). The four entity filters predate the PRD;
beyond what §4.7 says, the record does not show why they were chosen, or whether options 3 and 4 below
were weighed.
~~~

### E-0025-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: Not every cross-tenant read is "named as such". `GetByUsernameAsync`, `GetByUsernameWithSiteAsync` and `GetSecurityStateAsync` have neutral names and are marked only by a `Cross-tenant` comment. The complete list is in PRD-026 §4.5.
- Evidence: `Pot.Data/Repositories/Users/UserRepository.cs:84-97` (comments), `:99`, `:125`, `:142`, `:150`, `:160`; PRD-026 §4.5 table.
- old:
~~~
A small set of deliberately cross-tenant user reads exists and
is named as such, which is how authentication and recovery work before a site is known.
~~~
- new:
~~~
A small set of deliberately cross-tenant user reads exists, and
this is how authentication and recovery work before a site is known. Each read is marked in the
repository with a `Cross-tenant` comment and listed in PRD-026 §4.5. Some names say so, such as
`GetEnabledUsersForAllSitesAsync`, but `GetByUsernameAsync` and `GetSecurityStateAsync` do not.
~~~

### E-0025-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: omission
- Why: The PRD itself bounds the guarantee to the repository layer. `PotDbContext` is public and exposes `Users`. A reader of "isolation that a feature cannot forget" would be misled without this.
- Evidence: PRD-026 §4.1 ("The guarantee is bounded by that layer: `PotDbContext` is public and exposes `DbSet<UserEntity> Users`…"); `Pot.Data/PotDbContext.cs:8,13`.
- old:
~~~
- Bad, because there are two mechanisms, and a reader must know which entities use which.
~~~
- new:
~~~
- Bad, because the user and site confinement holds only for code that goes through the repositories.
  `PotDbContext` is public and exposes `Users`, so code that injects the context can still write an
  unconfined lookup (PRD-026 §4.1).
~~~

### E-0025-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: Two claims overstate. First, `CrossTenantAuthorizationFixture` covers only user and site routes (its own summary says so), and the normalised body comparison runs on one route, user status. Second, `EndpointAuthorizationMetadataFixture` asserts that endpoints are authenticated unless anonymous; the inventory sweep that covers new routes is `AuthorizationMatrixFixture`. The financial-data 404 and the destructive-action isolation have their own fixtures, and neither is cited.
- Evidence: `CrossTenantAuthorizationFixture.cs:17-27`, `:366-397`. `Pipeline/AuthorizationMatrixFixture.cs:120-145` (`Should_Answer_A_Foreign_Tenants_Row_As_Not_Found` for accounts, expenses, incomes) and `:166-205` (foreign-row expectations). `aspnetcore-integration-tests.instructions.md` ("a case naming another tenant's row when the path contains `{id}`", "fails the coverage check"). `Invariants/IsolationFixture.cs:1-12,29,67,105`. `Security/EndpointAuthorizationMetadataFixture.cs:47,61`.
- old:
~~~
- Test: `CrossTenantAuthorizationFixture` addresses another tenant's records across the endpoint set and
  asserts `404` with a body byte-identical to the missing-id response, so the answer leaks neither the
  row's existence nor its absence.
- Test: `EndpointAuthorizationMetadataFixture` keeps every endpoint in the authorization inventory, so a
  new route is covered by the same sweeps.
~~~
- new:
~~~
- Test: `AuthorizationMatrixFixture` generates a case naming another tenant's row for every documented
  operation whose path carries an id. It expects `404`, or, for an operation with a body, no success and
  no `401` or `403`. A path naming an unclassified resource fails the coverage check, so a new route is
  swept too. `Should_Answer_A_Foreign_Tenants_Row_As_Not_Found` covers the filtered account, expense and
  income reads.
- Test: `CrossTenantAuthorizationFixture` covers the user and site routes.
  `Should_Report_A_Foreign_Target_Indistinguishably_From_A_Missing_One` asserts that a foreign user and a
  missing user get the same status, content type and body once per-request identifiers are normalised.
- Test: `IsolationFixture` fingerprints a canary tenant before and after each destructive action, and
  fails if any of its rows changed.
~~~

### E-0025-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why: The "separate decision" exists as an ADR and should be linked.
- Evidence: `Docs/adr/0007-each-request-resolves-the-callers-permissions-from-the-database.md`.
- old:
~~~
- Which authority a signed-in caller has anywhere is a separate decision; this one is about which rows a
  query can see.
~~~
- new:
~~~
- Which authority a signed-in caller has anywhere is a separate decision,
  [ADR-0007](0007-each-request-resolves-the-callers-permissions-from-the-database.md). This one is about
  which rows a query can see.
~~~

### Notes (no edit proposed)
- The Summary and Decision Outcome group settings under "financial data". Settings are site
  configuration, but the Decision Outcome names the four entities explicitly, so no edit is proposed.
- Roles and permissions belong to no tenant. They are declared as `GlobalEntities` in
  `Pot.AspNetCore.Integration.Tests/Invariants/Support/GlobalEntities.cs`. The Context's "Every tenant's
  data lives in the same tables" is still accurate.
- `date: 2026-11-04` is the date the record was written. For the users-and-sites half, the decision date
  is PRD-026's (last updated 2026-10-14, status Complete). The owner may prefer that date.

---

## ADR-0026 — 0026-request-shape-is-validated-at-the-edge-and-rules-in-check-chains.md

Verdict: EDITS

### E-0026-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: Checks do need the database. For example, `CheckDescriptionDoesNotExist` queries the repository. Read literally, "refusals that do not need a database" is false. What the split actually achieves is rules expressed without HTTP (Decision, line 51-52).
- Evidence: `Pot.App/Features/Accounts/Create/EntityChecks/Checks/CheckDescriptionDoesNotExist.cs` (`_accountRepository.Accounts.AnyAsync`).
- old:
~~~
business refusals that do
not need a database to be described,
~~~
- new:
~~~
business refusals that are
expressed without HTTP,
~~~

### E-0026-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: A refused data-dependent rule of type BadRequest or Constraint is written as a `400` with the same `errors` map as a shape failure. Only 404 and 409 differ, and only 409 carries its own problem `type`. The current text contradicts ADR-0016 and the code.
- Evidence: `Pot.AspNetCore/Concerns/ProblemDetails/ProblemTypes.cs:26-38` (Constraint/BadRequest give 400, NotFound gives 404, Conflict gives 409); `Pot.AspNetCore/Extensions/EnrichedErrorExtensions.cs:15-45` (409 gets `ToConflictType`; 400 gets the validation `errors` map); ADR-0016 Decision Outcome.
- old:
~~~
The two also fail differently: a shape failure is a `400` with an `errors` map keyed by field, while a
data-dependent refusal is a business failure with its own type.
~~~
- new:
~~~
The two do not always look different on the wire: a shape failure is a `400` with an `errors` map keyed
by field, and a refused data-dependent rule is written in the same shape when it is a field-level refusal,
as a `404` when the record is missing, or as a `409` with its own problem `type` when it conflicts.
~~~

### E-0026-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: DEPENDS-ON E-0026-02
- Severity: format
- Why: Backfilled with no statement. The owner's account (SQ-08) supports the data-annotations option.
- Reworded for house style: Removed backfilled statement, source list and "owner's account"; the SQ-08 reason is kept, and the caveat now names options 2 and 3 because annotations were weighed.
- Evidence: `DOCS-WORK-LISTS.md` SQ-08.
- old:
~~~
as a `404` when the record is missing, or as a `409` with its own problem `type` when it conflicts.
~~~
- new:
~~~

Validators replaced data annotations because they are easier to test and to extend for complex rules;
the application's checks then enforce domain rules once the input is correctly formed. The record does
not show whether options 2 and 3 were weighed.
~~~

### E-0026-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: Authentication (session and token-version check) and permission resolution read the database on every authorised request before the handler runs. So "before any data is read" is not achievable or true. What the design guarantees is that the service never runs.
- Evidence: ADR-0007 (permissions resolved from the database per request); `Pot.AspNetCore/Concerns/Auth/PermissionAuthorizationHandler.cs`; `Concerns/Auth/Configuration/JwtBearerEventsSetup.cs` (uses repositories).
- old:
~~~
- A malformed request should be refused before any data is read.
~~~
- new:
~~~
- A malformed request should be refused before the service reads or changes any business data.
~~~

### E-0026-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: Five requests have no validator, and not all of them carry only "an identifier or a flag". One is an uploaded file (import), and two carry an enumerated status.
- Evidence: a `Request.cs` with no `RequestValidator.cs` exists in `Pot.AspNetCore/Features/Accruals/Status`, `Approvals/UpdateStatus`, `Maintenance/Import` (`IFormFile`), `Users/UpdateRoles` and `Users/UpdateStatus` (24 validators for 29 request models).
- old:
~~~
the
few requests whose input is an identifier or a flag have none.
~~~
- new:
~~~
the
five whose input is only identifiers, an entity tag and a status, or an uploaded file have none.
~~~

### E-0026-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: Same as E-0026-04. A malformed authenticated request still costs the auth and permission reads.
- Evidence: as E-0026-04.
- old:
~~~
- Good, because a malformed request costs no database read and cannot produce a half-evaluated rule.
~~~
- new:
~~~
- Good, because a malformed request is refused before the service runs, so it reads no business data and
  cannot produce a half-evaluated rule; the authentication and permission lookups that precede every
  authorized handler still run.
~~~

### E-0026-07
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: The "handler fixtures" do not exist. `Pot.AspNetCore.Tests` has one handler fixture (Export), and it does not test validation. The 400 is asserted by hosted endpoint fixtures, and nothing asserts "before any data is touched". "Each rule" is overstated: 17 of the 35 checks have a fixture, and none of the nine `CheckHasSameEtag` checks does.
- Evidence: `Pot.AspNetCore.Tests/Features/Maintenance/Export/HandlerFixture.cs` (filename and stream tests only); `Pot.AspNetCore.Integration.Tests/Features/Expenses/CreateUpdateFixture.cs:56-77` (`ShouldBeValidationProblemAsync("description")`); `find Pot.App.Tests -path "*Checks*" -name "*Fixture.cs"` returns 17 check fixtures and 4 checker fixtures; `CheckDescriptionDoesNotExistFixture.cs:99` uses `UseInMemoryDatabase`.
- old:
~~~
- Test: the check fixtures assert each rule in isolation with a constructed state.
- Test: the handler fixtures assert a shape failure answers `400` with `errors` before any data is
  touched.
~~~
- new:
~~~
- Test: the check fixtures in `Pot.App.Tests` assert a rule in isolation with a constructed state, and
  the checker fixtures assert the declared order of the delete chains. Not every check has one: 17 of the
  35 do. The duplicate-description checks run over the EF Core in-memory provider, which ignores `citext`.
- Test: hosted endpoint fixtures such as `Features/Expenses/CreateUpdateFixture` assert that a shape
  failure answers `400` with an `errors` entry for the field. Nothing asserts that no data was read first;
  that rests on each handler calling its validator before its service.
~~~

### E-0026-08
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why: "Testing a rule needs a repository substitute" applies equally to the chosen option. Checks take repositories, and their fixtures use the in-memory provider, so the stated disadvantage does not tell the options apart.
- Evidence: `CheckDescriptionDoesNotExist.cs` constructor (`IAccountRepository`); `CheckDescriptionDoesNotExistFixture.cs:97-100`.
- old:
~~~
- Bad: validators become data access, so testing a rule needs a repository substitute and the "validation
  is cheap" property is lost.
~~~
- new:
~~~
- Bad: validators become data access, so a malformed request can trigger a query before its shape is
  known, and the "validation is cheap" property is lost.
~~~

### Notes (no edit proposed)
- DD-12d (in-memory provider for check and repository tests) is undecided. E-0026-07 records the current state without choosing an option.
- `More Information` says "Authorization rules are enforced before either layer". Endpoint permission policies are, but data-dependent authorization rules (last site admin, platform-admin target, self-target) are entity checks. That is acceptable as written, because those rules are data-dependent. Flagged only in case the owner wants it explicit.

---

---

## ADR-0027 — 0027-enumerations-are-stored-by-name-and-uniqueness-is-enforced-by-the-database.md

Verdict: EDITS

### E-0027-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: format
- Why: Backfilled record does not say so or name its sources (adr skill hard rule).
- Reworded for house style: Removed the backfill statement, the source list, the December 2024 date and "the owner's recollection"; the reasons (readability, insert ordering, stored name differing from member name, clearer errors, index as second defence) are stated directly.
- Evidence: `Pot.Data/DbContextBase.cs:73,137-141`; `git log -S "AsName(maxLength"` → `a0c78b6d 2024-12-28`; owner recollections SQ-17, SQ-18, SQ-27 in `DOCS-WORK-LISTS.md`.
- old:
~~~
other writer — a migration fix, a support query, a future feature.
~~~
- new:
~~~
other writer — a migration fix, a support query, a future feature.

Names are stored to keep queries readable and to avoid ordering problems when a member is inserted. An
enriched type lets the stored name differ from the member name, which matters once the name is in the
data. The application check gives the caller a clear business-rule error, and the index is a second
defence against a coding error or direct database access.
~~~

### E-0027-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The indexes are not all per-site: expense/income descriptions are unique per account, site names and usernames are unique across the database.
- Evidence: `Pot.Data/Migrations/PotDbContextModelSnapshot.cs:191-192` (Expense `AccountId, Description`), `:252-253` (Income), `:487-488` (Site `Name`), `:556-557` (User `Username`), all `citext`.
- old:
~~~
1. Enriched enumerations persisted by name, with per-site case-insensitive unique indexes and an
   application check in front.
~~~
- new:
~~~
1. Enriched enumerations persisted by name, with case-insensitive unique indexes scoped to their owner and
   an application check in front.
~~~

### E-0027-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: An enriched enum carries one name (member name by default, or explicit), not separate display and storage names; and roles/permissions are not "the tables that hold" enumerations — enum columns live on many entities.
- Evidence: `Pot.Shared/Enumerations/Role.cs:9-14` (`CallerMemberName`), `Permission.cs:9` (`new(1, "site:manage")`); snapshot `character varying(50)` columns on Expense/Income `Frequency`, `AccrualPolicy`, OneTimePassword `Reason`/`Status`, Setting `Category`, User `Status` (snapshot lines 139, 164, 225, 295, 309, 423, 532).
- old:
~~~
Chosen option: 1. Enumerations are modelled as enriched types that carry their own display and storage
names, and the model is configured to persist them as text with a bounded length of fifty. A name in the
database is the name in the code, so an insert into the middle of a declaration cannot reinterpret
existing rows. The reference tables that hold them — roles and permissions — have plain, case-sensitive
unique indexes on those names, which are global rather than site-scoped.
~~~
- new:
~~~
Chosen option: 1. Enumerations are modelled as enriched types, each member carrying a name — the member
name by default, or an explicit one such as `site:manage` for a permission — and the model is configured
to persist every enriched type as that name, as text with a bounded length of fifty. A name in the
database is the name in the code, so an insert into the middle of a declaration cannot reinterpret
existing rows. The names are columns on the entities that use them (an expense's frequency, a user's
status, a setting's category, a one-time code's reason and status); the two reference tables, roles and
permissions, have plain, case-sensitive unique indexes on their names, which are global rather than
site-scoped.
~~~

### E-0027-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: There is no longer an "unscoped case" for settings: `SiteId` was made required, so the global partial index can match no row. The setting key is case-sensitive `text`, and the snapshot records the index without the partial filter the hand-written SQL created.
- Evidence: `Migrations/20251005015830_AddSettingEntity.cs:58-69` (two partial unique indexes via raw SQL); `Migrations/20260925093414_MakeSettingsSiteSpecific.cs` (`SiteId` nullable → false); snapshot `:434-454` (`Key` is `text`, index `SiteId, Category, Key` with no filter); `Entities/SettingEntity.cs:6`.
- old:
~~~
Uniqueness of user-entered text is a separate application of the same principle: user-facing names such
as an account's description are held in the database's case-insensitive text type with a composite unique
index scoped to the site, and a settings row is unique per site with a separate global index for the
unscoped case. An application check runs first where one exists, so the caller receives a business
message rather than a constraint violation, but the index is the guarantee.
~~~
- new:
~~~
Uniqueness of user-entered text is a separate application of the same principle: user-facing names are
held in the database's case-insensitive text type under a unique index scoped to their owner — an
account's description per site, an expense's or income's description per account, and a site's name and
a username across the whole database. A settings row is unique per site, category and key, and its key is
case-sensitive text. An application check runs first where one exists, so the caller receives a business
message rather than a constraint violation, but the index is the guarantee.

The settings index was created by hand as two partial indexes, one for rows with a site and one for
global rows with none. Settings have since been made site-specific, so the global index can match no row,
and the model snapshot records the site index without its filter.
~~~

### E-0027-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: Confirmation names no fixture; the cited check fixture asserts property name and message, not an error code (PRD-030 removed `errorCode`), and runs on the in-memory provider (no `citext`). Real fixtures exist and should be named. The review rule's "explicit storage names" would flag every existing enum, which uses the member name.
- Evidence: `Pot.App.Tests/Features/Accounts/Create/EntityChecks/Checks/CheckDescriptionDoesNotExistFixture.cs` (asserts `PropertyName`, `ErrorMessage`; DD-12d notes `UseInMemoryDatabase` at `:99`); `Pot.AspNetCore.Integration.Tests/Features/Accounts/CreateUpdateFixture.cs:17-39`, `Features/Expenses/CreateUpdateFixture.cs:130-175` (`ShouldBeEntityConflictAsync` → 409 `DuplicateValue`); `Pot.AspNetCore.Tests/Concerns/ExceptionHandlers/DatabaseExceptionHandlerFixture.cs:77` (`Should_Write_Generic_409_When_Exception_Is_A_Conflict`); no test grep-matches a case-only duplicate.
- old:
~~~
- Test: the service fixtures assert the application check answers the business failure, with its error
  code, before the constraint is reached — for the text fields that have such a check.
- Gap: no data fixture inserts a duplicate to show the database refusing it; the constraint is evidenced by
  the migrations that create the indexes, and the refusal path by the exception-handler fixture.
- Review: a new enumeration is added as an enriched type with explicit storage names; a raw `int` column
  is a defect.
~~~
- new:
~~~
- Test: `CheckDescriptionDoesNotExistFixture` asserts the account check refuses a description already used
  on the same site, with its message, and allows one used on another site. It runs on the in-memory
  provider, so it does not exercise case-insensitivity.
- Test: the hosted `Features/Accounts/CreateUpdateFixture` and `Features/Expenses/CreateUpdateFixture`
  assert that an exact duplicate answers 409 against PostgreSQL, and `DatabaseExceptionHandlerFixture`
  asserts that a unique-constraint violation reaching the database is answered 409.
- Gap: no test writes past the application check, or sends a duplicate that differs only by case, to show
  the index refusing it; the constraint is evidenced by the migrations that create the indexes.
- Review: a new enumeration is added as an enriched type, stored by name; a raw `int` column is a defect.
~~~

### Notes (no edit proposed)
- Bundles two decisions (see cross-cutting note).
- The vestigial `IX_Setting_Global_Category_Key` partial index and the snapshot/database mismatch on
  `IX_Setting_SiteId_Category_Key` (partial in the database, unfiltered in the snapshot) are harmless today
  but would surprise whoever next changes that index; the owner may want a tidy-up migration.

---

---

## ADR-0028 — 0028-cascades-are-restricted-and-deletion-is-an-explicit-ordered-sweep.md

Verdict: EDITS

### E-0028-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: RELATED-PRDS
- Severity: consistency
- Why: The hard-delete, no-soft-delete and sweep decisions were made in PRD-022 (Complete), which carries "Removal type: Permanent hard delete" and lists soft delete as out of scope.
- Evidence: `Docs/Future/PRD-022-user-and-site-deletion.md:3,114,144`.
- old:
~~~
date: 2026-11-04
area: backend
---
~~~
- new:
~~~
date: 2026-11-04
area: backend
related-prds: [PRD-022]
---
~~~

### E-0028-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: format
- Why: Backfilled record does not say so or name its sources.
- Reworded for house style: Removed the backfill statement, the source list, the December 2024 and October 2026 dates and "the owner's recollection"; kept the reason for restricting cascades and the PRD-022 hard-delete decision.
- Evidence: `git log -S DisableCascadeDelete` → `a0c78b6d 2024-12-28`; `git log -S DeleteCurrentSiteWithDependentsAsync` → `8a0bef58 2026-10-17`; server `DEVELOPER.md:4588-4607`; SQ-16 owner recollection.
- old:
~~~
are handled, which turns an unintended deletion into an error.
~~~
- new:
~~~
are handled, which turns an unintended deletion into an error.

Restricting cascades is meant to prevent accidental data loss through a coding error or direct database
access. [PRD-022](../Future/PRD-022-user-and-site-deletion.md) makes removing a user or a site a
permanent hard delete, and the user and site sweeps carry it out.
~~~

### E-0028-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: (a) Restrict is applied by convention to every FK on an entity type, not only owner FKs, and that convention is the mechanism worth naming. (b) Account deletion is not a sweep: it is refused while expenses or incomes exist. (c) The join tables cascade because the convention loop skips non-`EntityBase` types (its comment names `EnrichedEnum<>` as the target of the exclusion); "because those rows exist only as pairs" is unsupported rationale.
- Evidence: `Pot.Data/DbContextBase.cs:79-95,118-126` (`Where(entity => entity.ClrType.IsDerivedFrom(typeof(EntityBase)))`, comment "Exclude anything that is not an EntityBase, such as EnrichedEnum<>"); snapshot `:671-697` (join tables `Cascade`); `Pot.App/Features/Accounts/Delete/EntityChecks/Checks/CheckHasNoExpenses.cs`, `CheckHasNoIncomes.cs`; `DeleteAccountService.cs:33-53`; `SiteRepository.cs:25-96`; `UserRepository.cs:169-198`.
- old:
~~~
Chosen option: 1. Foreign keys from an entity to its owner restrict, and deleting an aggregate is a
sequence of named repository operations in dependency order — children, then the parent — inside one
save, so the whole removal either applies or does not. The join tables that link roles to permissions and
users to roles are the exception: their keys cascade, because those rows exist only as pairs and are never
addressed on their own.
~~~
- new:
~~~
Chosen option: 1. A model convention, `DbContextBase.DisableCascadeDelete`, sets every foreign key on an
entity type to restrict, so a new relationship restricts without being configured. Removing a user or a
site is a sequence of named repository operations in dependency order — children, then the parent —
inside one save, so the whole removal either applies or does not. An account is not swept: deleting one
that still has expenses or incomes is refused with a business error, and expenses and incomes are deleted
one row at a time.

The join tables that link roles to permissions and users to roles are the exception: they are not entity
types of their own, so the convention does not reach them and their keys keep the framework's default
cascade. The record does not show whether that exception was chosen or is a side effect of the
convention's scope.
~~~

### E-0028-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: omission
- Why: The sweep materialises and tracks every dependent row before deleting it, which is the real cost for a large site.
- Evidence: `Pot.Data/Repositories/PersistableRepository.cs:63-74` (`DeleteWhereAsync` → `ToListAsync` then `Delete` each); `SiteRepository.cs:27` (`WithTracking()`).
- old:
~~~
- Bad, because a large site's removal is many statements in one save, which holds its transaction longer
  than a cascade would.
~~~
- new:
~~~
- Bad, because a large site's removal loads and tracks every dependent row in memory and then issues many
  statements in one save, which holds its transaction longer than a cascade would.
~~~

### E-0028-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: `DeletionSweepFixture` is correctly cited but under-described; `IsolationFixture` (with `OrphanSweep` and model-derived tables) is the mechanical guard for "a new dependent table needs its sweep step" and is not named.
- Evidence: `Pot.AspNetCore.Integration.Tests/Repositories/DeletionSweepFixture.cs:26,89,126`; `Invariants/IsolationFixture.cs:29,67,105-148` (asserts `targetAfter.Tables.Values.ShouldAllBe(rows => rows.Count == 0)` and calls `OrphanSweep.FindAsync`); `.github/instructions/aspnetcore-integration-tests.instructions.md:53-60`.
- old:
~~~
- Test: `DeletionSweepFixture` deletes an aggregate and asserts that no dependent row remains and that the
  neighbouring site's rows are untouched.
~~~
- new:
~~~
- Test: `DeletionSweepFixture` runs the user and site sweeps against PostgreSQL and asserts the dependent
  counts, that no dependent row remains, and that another site's rows are untouched.
- Test: `IsolationFixture` removes a member, the caller's own account and a whole site through the API,
  and asserts that a removed tenant leaves no row in any site-scoped table, that a canary tenant is
  unchanged, and that `OrphanSweep` finds no row referencing a removed one. Its tables come from the model
  rather than a list, so a new dependent table the sweep misses fails it.
~~~

### E-0028-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The sweeps use no explicit transaction; atomicity comes from the single `SaveAsync`, as this record's own Decision and Gap say. ADR-0024 covers when an explicit transaction is used instead.
- Evidence: `SiteRepository.cs:91`, `UserRepository.cs:194` (one `SaveAsync`); no `IPotTransactionFactory` in `DeleteSiteService`, `DeleteUserService`, `DeleteOwnAccountService` (only `ImportDataService`, `UpdateUserRolesService`); `Docs/adr/0024-...md:13-19`.
- old:
~~~
- The explicit transaction that makes a sweep all-or-nothing is a separate decision.
~~~
- new:
~~~
- A sweep is all-or-nothing because it is one save, not because of an explicit transaction; when an
  explicit transaction is used instead is
  [ADR-0024](0024-transactions-are-explicit-and-only-where-writes-must-be-one-unit.md).
~~~

### Notes (no edit proposed)
- A constraint failure from an incomplete sweep is answered 409 Conflict by `DatabaseExceptionHandler`
  (`ReferenceConstraintException`), not 500 — worth knowing when reading "an omission is a constraint
  error".
- Sign-up one-time codes are linked by username, not by foreign key, so an omission there would leave rows
  rather than fail; the sweeps handle it explicitly (`SiteRepository.cs:66-75`).

---

---

## ADR-0029 — 0029-migrations-run-in-a-separate-process-before-the-api-starts.md

Verdict: EDITS

### E-0029-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: format
- Why: Backfilled record does not say so or name its sources; the owner's stated reasons (SQ-19, SQ-20) are absent.
- Reworded for house style: Removed the backfill statement, the source list (entrypoint, Dockerfile, runner, PRD-023, working agreement), the April 2025 date and "the owner's recollection"; the two reasons are stated directly.
- Evidence: `git log --diff-filter=A -- Source/Docker/Server/entrypoint.sh` → `e0139b0f 2025-04-01` (already runs migrations then API under `set -e`); `PRD-023 §5`; `.github/copilot-instructions.md:17`; SQ-19, SQ-20.
- old:
~~~
application binary, so a rolling deployment can have old and new code running against one schema.
~~~
- new:
~~~
application binary, so a rolling deployment can have old and new code running against one schema.

Migrations run in their own process for separation of concerns and to make a failed deployment easier to
mitigate. Destructive changes are split into separate steps, where possible, for safer rolling
deployments.
~~~

### E-0029-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The driver as written says the opposite of what is meant.
- Evidence: Context paragraph and the "Good, because additive steps keep old and new code compatible" consequence.
- old:
~~~
- Rolling deployments must not require the old code and the new schema to be incompatible.
~~~
- new:
~~~
- During a rolling deployment the old code must keep working against the new schema.
~~~

### E-0029-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: omission
- Why: The record presents the additive-step rule as practised without exception; history shows two column drops shipped in the same change as the code, and the one recorded two-step drop used a maintenance window, not a rolling deploy. Owner wording is "where possible".
- Reworded for house style: No attribution in the text; replaced "legacy refresh-token fields" with a plain description so the record carries no "legacy" wording. The correction is unchanged.
- Evidence: `git show --stat cb99ab2e` (2026-05-03: `UserEntity.cs` fields removed + `20260503111438_RemoveLegacyRefreshTokenFields.cs`); `git show --stat 81a0d863` (2026-09-24: `AccountEntity.cs` + `20260924111350_RemoveAccountBsbAndNumber.cs`); `Docs/Future/PRD-023-dynamic-accrual-calculation.md:316-324` ("Amended 2026-09-27 ... the rollout is not a rolling deploy ... Production is shut down for a maintenance window").
- old:
~~~
A change that removes or reinterprets existing data is delivered in steps: first a schema that both the
old and new code can use, then the code change, then the removal — each an append-only migration.
~~~
- new:
~~~
A change that removes or reinterprets existing data is delivered in steps: first a schema that both the
old and new code can use, then the code change, then the removal — each an append-only migration. The
rule is applied where possible rather than without exception: PRD-023's accrual storage was removed in
two releases, and its drop was applied with production stopped for a maintenance window, while a user's
stored refresh token and its expiry (May 2026) and the account BSB and number (September 2026) were
dropped in the same change as the code that stopped using them.
~~~

### E-0029-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The code answers the conditional: the runner catches every exception, logs it and sets exit code -1, which the AllOverIt console host returns; `set -e` then stops the entrypoint. The real cost is that neither is tested.
- Evidence: `Pot.Data.Migrations/App.cs:47-71` (`catch (Exception) { ... ExitCode = -1; }`); AllOverIt.GenericHost 9.4.0 XML docs (`ConsoleAppBase.ExitCode` "will be returned by the application when it exits"); `entrypoint.sh` `set -e`.
- old:
~~~
- Bad, because whether a failed migration stops the container depends on the runner returning a failing
  exit code rather than swallowing the exception.
~~~
- new:
~~~
- Bad, because the ordering rests on two things outside the application: the runner catches any
  exception, logs it and exits with code -1 (`Pot.Data.Migrations/App.cs`), and the entrypoint's `set -e`
  turns that into a stopped container. Neither is tested.
~~~

### E-0029-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: Name the fixture, say what it proves (EF Core refuses `MigrateAsync` on pending model changes, observed in PRD-021's delivery) and what it does not (the runner/entrypoint); name the instruction files that carry the person-generated rule.
- Evidence: `Pot.AspNetCore.Integration.Tests/IntegrationFixtureBase.cs:96,116` (`postgres:13`, `dbContext.Database.MigrateAsync()`); `PRD-021-remove-account-bsb-and-number.md:146` ("27 integration failures ... were all `PendingModelChangesWarning`"); `.github/copilot-instructions.md:17`; `.github/instructions/server.instructions.md:32`.
- old:
~~~
- Test: the hosted integration fixture brings its database up to date by running the migrations, so a
  broken or missing migration fails the test run rather than a deployment.
~~~
- new:
~~~
- Test: `IntegrationFixtureBase` applies every migration to a fresh PostgreSQL container with the
  framework's `MigrateAsync`, which also refuses a model change that has no migration, so a broken or
  missing migration fails the hosted suite rather than a deployment. It does not run the migration runner.
- Instruction: `.github/copilot-instructions.md` (never generate a migration unless asked) and
  `.github/instructions/server.instructions.md` (migration generation is a developer-reviewed step).
- Gap: nothing tests the entrypoint's ordering or the runner's failing exit code.
~~~

### Notes (no edit proposed)
- Bundles three decisions (see cross-cutting note).
- Whether production deployments ever overlap old and new code cannot be confirmed from the repo (no IaC;
  replica count is DD-04). PRD-019 AC-04 assumes Container Apps may overlap revisions; PRD-023 records a
  maintenance-window rollout. The "rolling deployment" premise should be confirmed by the owner.
- "concurrency of migrations depends on the runner being safe when it finds the schema already current"
  could not be verified: `PotDbMigrator` delegates to `AllOverIt.EntityFrameworkCore.Migrator`'s
  `DatabaseMigratorBase`, whose locking behaviour is not visible in the repo.
- SQ-37 confirms the Azure liveness probe has a 30-second initial delay "to allow startup migrations",
  which supports the liveness-delay consequence.

---

---

## ADR-0030 — 0030-background-work-runs-inside-the-api-process.md

Verdict: EDITS

### E-0030-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The OTP worker updates status to Expired (it does not delete), mail is drained as it arrives rather than on a schedule, and the cadences are known.
- Evidence: `Pot.App/Features/Otp/OtpService.cs:40-61` (`otp.Status = OtpStatus.Expired`); `ExpiredOtpCleanupWorker.cs:12,61` (5 minutes); `BudgetReminderEmailWorker.cs:89-90` (next UTC hour); `SendEmailWorker.cs:17-20`.
- old:
~~~
The work is scheduled and periodic rather than request-driven: send queued mail, sweep expired one-time
codes, run the budget reminder cycle.
~~~
- new:
~~~
The work runs outside requests: drain queued mail as it arrives, mark expired one-time codes as expired
every five minutes, and run the budget reminder cycle every hour.
~~~

### E-0030-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: format
- Why: Backfilled record does not say so or name its sources.
- Reworded for house style: Removed the backfill statement, the source list and "the owner's recollection"; kept only the reason, simplicity.
- Evidence: `Pot.AspNetCore/Features/Workers/*.cs`; `PRD-013`; `PRD-019` Problem Statement; SQ-21 ("currently kept in-process for simplicity").
- old:
~~~
the API — for a deployment operated by one person.
~~~
- new:
~~~
the API — for a deployment operated by one person.

The workers are kept in the API process for simplicity.
~~~

### E-0030-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: "The single-replica assumption is what makes the scheduling safe" is false for the reminder: it duplicates on a restart inside the trigger hour and misses a trigger hour with no process, even with one replica. The OTP update is idempotent.
- Evidence: `BudgetReminderEmailWorker.cs:35-98` (runs immediately after readiness, then each UTC hour); `BudgetReminderService.cs:65` (strict hour equality, no send record); `DECISIONS-TO-DISCUSS.md` DD-04 ("Duplicates happen with one replica too"); PRD-019 problems 2-3 and line 33.
- old:
~~~
The single-replica assumption is what makes the scheduling safe: a per-process timer would otherwise run
the same job once per replica. It is an assumption about the deployment rather than something the
application configures, and nothing in the repository sets a replica count.
~~~
- new:
~~~
The single-replica assumption removes only the scale-out duplicate: a per-process timer would otherwise
run the same job once per replica. It is an assumption about the deployment rather than something the
application configures, and nothing in the repository sets a replica count. It does not make the reminder
safe on its own: the cycle runs once at start-up and then at each UTC hour, decides to send only by
matching the configured local hour, and records nothing, so a restart inside the trigger hour sends that
hour's reminders again and a trigger hour with no process running sends none. The expired-code update is
idempotent, so a duplicate run of it is only redundant work.
~~~

### E-0030-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: An async hosted service does not hold a "request thread pool slot"; it shares the thread pool and the database connection pool with requests while it runs.
- Evidence: `BackgroundWorker` subclasses with `async ExecuteAsync` awaiting `WaitUntilUtcAsync` (`BudgetReminderEmailWorker.cs:29,97`; `ExpiredOtpCleanupWorker.cs:29,68`).
- old:
~~~
- Bad, because a long cycle holds a request thread pool slot and is not observable except through logs.
~~~
- new:
~~~
- Bad, because a cycle runs on the same thread pool and database connection pool as requests, and is not
  observable except through logs.
~~~

### E-0030-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: Neither worker fixture asserts "does not run when the host is stopping"; the drainer is not "covered through the channel's tests" — no fixture exercises `SendEmailChannel` at all; the hosted suite removes every hosted service; and the existing reminder job fails the review rule.
- Evidence: `Pot.AspNetCore.Tests/Features/Workers/BudgetReminderEmailWorkerFixture.cs:72,107` (`Should_Wait_For_Database_Health_Before_Starting_Its_First_Cycle`, `Should_Attempt_Remaining_Users_When_A_User_Fails`); `ExpiredOtpCleanupWorkerFixture.cs:65` (wait test only); no test file references `SendEmailChannel`/`ISendEmailChannelReader` (grep over `Pot.*Tests`); `Pot.AspNetCore.Integration.Tests/Host/ApiWebApplicationFactory.cs:141` (`services.RemoveAll<IHostedService>()`).
- old:
~~~
- Test: `BudgetReminderEmailWorkerFixture` and `ExpiredOtpCleanupWorkerFixture` assert that each waits for
database readiness before its first cycle and does not run when the host is stopping.
- Gap: the email drainer has no fixture of its own; its behaviour is covered through the channel's tests.
- Review: a new scheduled job must be written to be idempotent for its period, because the single-replica
  assumption is environmental rather than enforced.
~~~
- new:
~~~
- Test: `BudgetReminderEmailWorkerFixture` and `ExpiredOtpCleanupWorkerFixture` assert that each waits for
  the `database` health check before creating the scope for its first cycle; the reminder fixture also
  asserts that one user's failure does not stop the rest of the cycle.
- Gap: `SendEmailWorker` and the channel it drains have no fixtures, no fixture asserts behaviour at host
  shutdown, and the hosted integration suite removes every hosted service, so no test runs a worker inside
  a host.
- Review: a new scheduled job must be written to be idempotent for its period, because the single-replica
  assumption is environmental rather than enforced. The budget reminder does not meet this today.
~~~

### E-0030-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: unsupported-history
- Why: PRD-019 keeps the workers in the API process ("No ... new deployables"), so it would not supersede in-process hosting; the owner said PRD-019 "does not by itself decide the host for every worker". Nothing supports "expected to be superseded".
- Evidence: `PRD-019-email-outbox-durable-dispatch.md:7,13,78,80`; SQ-21.
- old:
~~~
- This record is expected to be superseded by a more scalable arrangement. The shape it would take is
  written up in `Docs/Future/PRD-019-email-outbox-durable-dispatch.md`: a database outbox drained with
  claim and lease, delivering email without a new deployable, and explicitly leaving the expired-code
  sweep out of scope.
~~~
- new:
~~~
- `Docs/Future/PRD-019-email-outbox-durable-dispatch.md` (Proposed) would make scheduled email safe under
  more than one replica and across restarts — a database outbox drained with claim and lease — while
  keeping the workers in the API process and adding no deployable, and it leaves the expired-code update
  out of scope. It would replace the single-replica assumption for email, not the in-process hosting; the
  record does not show that the hosting itself is expected to change.
~~~

### Notes (no edit proposed)
- DD-04 is open. Option A (pin Azure to one replica) would make the assumption true; the ADR correctly
  does not claim it is pinned. Replica count cannot be confirmed from the repo.

---

---

## ADR-0031 — 0031-email-is-queued-in-process-and-rendered-from-razor-components.md

Verdict: EDITS

### E-0031-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: Bodies are not type-checked: values pass to the component as a name-keyed dictionary at run time, and the plain-text body is a `.text` template with string replacement, not a Razor component. Nothing else renders these components. Also adds the missing backfill statement.
- Reworded for house style: Kept the template correction; removed the backfill statement, the source list and "the owner's recollection", stating the queue's reason directly and keeping the plain note that no reason for Razor is recorded.
- Evidence: `Pot.EmailSender/EmailSender.cs:114-140` (`config.ToPropertyDictionary()`, `RenderToHtmlAsync<TEmailComponent>(htmlDictionary)`, `PlainTextEmailTemplateLoader.Populate`); `Pot.RazorComponents/RazorComponentRenderer.cs:20-33` (`ParameterView.FromDictionary`); `PlainTextEmailTemplateLoader.cs:15-25` (`template.Replace($"{{{key}}}", ...)`); `Emails/*/*.text`; SQ-22.
- old:
~~~
The templates are content in code: the same Razor components used to build markup, rendered to a string
for the body. That keeps the body type-checked and shareable with anything else that renders the same
component.
~~~
- new:
~~~
Each message has two templates in `Pot.RazorComponents`: a Razor component for the HTML body, rendered to
a string outside any web request, and a plain-text file with `{Property}` placeholders for the text body.
Both are filled from the message's properties by name at send time.

The in-process queue is a deliberately simple first implementation that gets email working, and it is
expected to become more durable under PRD-019. The record does not show why Razor components were chosen
for the HTML body.
~~~

### E-0031-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace (removes the driver; the following line is included so no blank line is left in the list)
- Tags: none
- Severity: factual
- Why: The implementation does not make a wrong property a build error (see E-0031-01), so this cannot be a driver the decision satisfies, and no evidence records it as the reason.
- Evidence: as E-0031-01.
- old:
~~~
- Templates should be code, so a wrong property is a build error.
- No new infrastructure for a volume measured in messages per day.
~~~
- new:
~~~
- No new infrastructure for a volume measured in messages per day.
~~~

### E-0031-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The subject is a constant per type, and only the HTML body comes from a Razor component.
- Evidence: `EmailSender.cs:26-35` (subject constants), `:128-132`.
- old:
~~~
Chosen option: 1. A queued message carries only the email type and its configuration; the subject and both
bodies are rendered from a Razor component at send time, when the worker dequeues it. The channel is
unbounded and lives as long as the process.
~~~
- new:
~~~
Chosen option: 1. A queued message carries the email type and the message's values; nothing is rendered
until the worker dequeues it. The subject is a fixed string per type, the HTML body is rendered from the
type's Razor component, and the plain-text body is the type's text template with its placeholders
replaced. The channel is unbounded and lives as long as the process.
~~~

### E-0031-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: A renamed property does not break the build. A dictionary key the component does not declare makes `SetParametersAsync` throw at render, which the channel logs and drops; an unmatched text placeholder is sent literally.
- Evidence: `RazorComponentRenderer.cs:30-31`; `SendEmailChannel.cs:62,68-71`; `PlainTextEmailTemplateLoader.cs:19-22`; `Pot.App.Tests/Emails/EmailGreetingFixture.cs` (renders templates).
- old:
~~~
- Good, because templates are components: a renamed property breaks the build rather than producing a
  blank line in someone's inbox.
~~~
- new:
~~~
- Good, because the HTML layout is written as markup and rendered by the framework.
- Bad, because the values reach the templates by property name at run time, not through a typed contract:
  a value the component does not declare fails the render, and the message is then logged and dropped,
  while a placeholder the text template names but the values lack is sent as written. Nothing checks this
  at build time; `EmailGreetingFixture` renders some of the templates.
~~~

### E-0031-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: The cited "channel's fixtures" do not exist — there is no `Pot.EmailSender` test project and no test references `SendEmailChannel`. Name the tests that do exist.
- Evidence: test projects are `Pot.App.Tests`, `Pot.AspNetCore.Tests`, `Pot.AspNetCore.Integration.Tests`, `Pot.Data.Tests`, `Pot.Shared.Tests`; `RequestPasswordResetServiceFixture.cs:44`, `InviteUserServiceFixture.cs:40` (`_emailWriterFake.Received(1).SubmitAsync`); `Pipeline/MessageCaptureFixture.cs:25`; `Host/ApiWebApplicationFactory.cs:141-148`; `Pot.App.Tests/Emails/EmailGreetingFixture.cs:47-203`.
- old:
~~~
- Test: the channel's fixtures assert a queued item is dequeued and that a send failure is logged and
  swallowed rather than thrown back to the request.
- Gap: no worker fixture asserts end-to-end delivery, and none asserts what happens to messages still
  queued when the host stops.
~~~
- new:
~~~
- Test: the application fixtures for services that send mail (for example
  `RequestPasswordResetServiceFixture` and `InviteUserServiceFixture`) assert the message is submitted to
  the channel writer; the hosted suite replaces that writer with `CapturingEmailWriter`, proved by
  `MessageCaptureFixture`, so no request path reaches a mail server in tests.
- Test: `EmailGreetingFixture` renders several of the HTML and plain-text templates with the values the
  sender passes.
- Gap: `SendEmailChannel`, `EmailSender` and `SendEmailWorker` have no fixtures, so dequeueing, the
  swallowed send failure and messages still queued when the host stops are not asserted anywhere.
~~~

### E-0031-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why: PRD-019 leaves OD-03 pending (scheduled-only vs all types), so the outbox may replace the channel only for scheduled mail; "would supersede this record" may be partial.
- Evidence: `PRD-019-email-outbox-durable-dispatch.md:70,103,135`.
- old:
~~~
  `Docs/Future/PRD-019-email-outbox-durable-dispatch.md`; adopting it would supersede this record rather
  than edit it.
~~~
- new:
~~~
  `Docs/Future/PRD-019-email-outbox-durable-dispatch.md` (Proposed); adopting it would supersede this
  record rather than edit it. PRD-019 leaves open whether transactional messages move to the outbox or
  stay on the in-process channel, so the supersession may be partial.
~~~

### Notes (no edit proposed)
- Bundles two decisions (queueing and templating; see cross-cutting note).
- The queued payload holds verification codes and temporary passwords in memory until sent
  (`Models/EmailOtpInfo.cs`); not a defect, but relevant to DD-08.

---

---

## ADR-0032 — 0032-logs-record-request-metadata-and-never-submitted-values.md

Verdict: EDITS

### E-0032-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE, RELATED-PRDS
- Severity: consistency
- Why: The decision was made in PRD-030 (Complete) and delivered on 2026-10-22; the record carries neither.
- Evidence: `git show 59bbae8f` (2026-10-22, "Log requests with the framework's HTTP logging": deletes `RawRequestLoggingMiddleware`, adds `AddHttpLogging` and `RequestLoggingFixture`); `Docs/Future/PRD-030-problem-details-consolidation.md:3,64-68,153,204`.
- old:
~~~
date: 2026-11-04
area: backend
---
~~~
- new:
~~~
date: 2026-10-22
area: backend
related-prds: [PRD-030]
---
~~~

### E-0032-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: format
- Why: Backfilled record does not say so or name its sources; it omits that option 2 was effectively the status quo until PRD-030 (a raw body logger ran outside Production from 2026-03-03).
- Reworded for house style: Removed "backfilled from", the code source list and "the owner's recollection"; kept the pre-PRD-030 middleware history (close to option 2), PRD-030 A2 to A6 as the decision, and the reason.
- Evidence: `git log -S EnableBuffering` → `6f357781 2026-03-03 Add raw request logging middleware`, `59bbae8f 2026-10-22`; PRD-030:16 ("writes every request body ... whenever the server runs outside Production"); `RequestLoggingFixture.cs:70` comment ("Development is where the raw body logger ran"); SQ-36.
- old:
~~~
enters a log has left the controls that were designed around it.
~~~
- new:
~~~
enters a log has left the controls that were designed around it.

Before [PRD-030](../Future/PRD-030-problem-details-consolidation.md), a hand-written middleware wrote
every request body whenever the server ran outside Production and recorded no request metadata, which is
close to option 2 below. PRD-030 (requirements A2 to A6) replaced it with this decision, to keep
credentials and sensitive financial data out of logs.
~~~

### E-0032-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why: "defects to be removed, not an accepted exception" presents DD-08 option B as decided; DD-08 is open ("Your call" blank). The list is also imprecise: two services and two handlers log the verification code, and the refresh token is logged by `AuthService`, not a handler. All are at `Information`, the configured default, so they reach production logs.
- Evidence: `VerifyPasswordResetService.cs:63`, `VerifySignupService.cs:78`, `Features/Auth/PasswordReset/Verify/Handler.cs:16`, `Features/Auth/Signup/Complete/Handler.cs:17` (`new { Username, ReferenceCode, VerificationCode }`); `Concerns/Auth/Services/AuthService.cs:229` (`LogCall(this, new { refreshToken })`); `EmailSender.cs:50,57` (`ReferenceCode`); `appsettings.json:4`; `DECISIONS-TO-DISCUSS.md` DD-08.
- old:
~~~
- Bad, because the rule has exceptions in the current code: several authentication handlers log the
  username, a reference code or a verification code alongside their call, and one logs the refresh token.
  Those statements contradict this decision and are defects to be removed, not an accepted exception.
~~~
- new:
~~~
- Bad, because the rule has exceptions in the current code, all through call logging at `Information` and
  so in every environment: the sign-up and password-reset verification handlers and services log the
  username, reference code and verification code; `AuthService.RefreshAsync` logs the refresh token; and
  other handlers, services and the email sender log usernames and reference codes. They contradict this
  decision; whether and when they are removed is not yet decided.
~~~

### E-0032-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: Removal of the level entry is not silent to the suite: `RequestLoggingFixture` asserts it.
- Evidence: `Pot.AspNetCore.Integration.Tests/Pipeline/RequestLoggingFixture.cs:40-46` (`Should_Configure_The_Request_Log_Category_At_Information`); `appsettings.json:6`.
- old:
~~~
- Bad, because request logging itself is enabled by a single logging level entry, so removing it silently
  stops every entry while leaving the no-body rule intact.
~~~
- new:
~~~
- Bad, because request logging itself is enabled by a single logging level entry in `appsettings.json`,
  so removing it stops every entry with no sign at run time; `RequestLoggingFixture` asserts the entry is
  present.
~~~

### E-0032-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: Name the enricher fixture and the level assertion, and record the gap that lets the call-logging exceptions exist.
- Evidence: `RequestLoggingFixture.cs:40,49,68,95`; `Pot.AspNetCore.Tests/Concerns/Logging/CorrelationIdLogEnricherFixture.cs:23`; no test asserts `LogCall` argument content.
- old:
~~~
- Test: `RequestLoggingFixture` asserts the entry carries the method, the path and the status, that the
  health probes are not logged, and that a body sent in a request never appears; the correlation identifier
  in entries is asserted by the enricher fixture.
~~~
- new:
~~~
- Test: `RequestLoggingFixture` asserts the request-log category is configured at `Information`, that the
  entry carries the method, the path and the status, that the health probes are not logged, and that a
  body sent in a request never appears; the correlation identifier in entries is asserted by
  `CorrelationIdLogEnricherFixture`.
- Gap: no test asserts that call logging carries no credential, which is how the exceptions above exist.
~~~

### Notes (no edit proposed)
- **Title vs. current behaviour.** "Logs ... never submitted values" is the decision (PRD-030, owner intent),
  but it is not true of the running system: verification codes and the raw refresh token are written at
  `Information` in every environment. The edits keep the decision and record the exceptions honestly. If
  DD-08 is decided as option A (keep), the title and summary would need narrowing to the request log, which
  means superseding this record.
- **Refresh-token logging is not covered by any DD item.** DD-08 covers `VerificationCode` only.
  `AuthService.RefreshAsync` logging the refresh token (a 30-day credential) is a separate, arguably larger
  leak the owner should see.
- The request log includes the query string as supplied. Today the only query parameters are
  `StartDate`/`EndDate` on projections (`contracts/openapi/v1.json`), so nothing sensitive is logged, but
  the "never submitted values" rule relies on no sensitive value ever travelling in a query string.
- Verified as correct: fields `RequestMethod | RequestPath | RequestQuery | ResponseStatusCode | Duration`
  with `CombineLogs = true` (`WebApplicationBuilderExtensions.cs:377-386`), set in code; `UseHttpLogging`
  placed before the exception handler (`Program.cs:64-66`); correlation id from `TraceIdentifier` via
  `CorrelationIdLogEnricher`; correlation middleware runs before authentication, so 401/403/429 entries
  carry it; probes short-circuited.

---

## ADR-0033 — 0033-tests-are-per-layer-unit-projects-plus-one-hosted-integration-project.md

Verdict: EDITS

### E-0033-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: format
- Why: Backfilled with no statement. The owner's account (SQ-26) is recorded.
- Reworded for house style: Removed backfilled statement, source list and "owner's account"; the SQ-26 reasons and the not-weighed caveat are kept.
- Evidence: `DOCS-WORK-LISTS.md` SQ-26.
- old:
~~~
A single suite of unit tests would never show what the framework does with what the code returns.
~~~
- new:
~~~

Database-independent tests are fast and run often. The hosted tests are slower but verify whole
behaviour, such as a database error becoming Problem Details. The record does not show whether the other
options below were weighed.
~~~

### E-0033-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: omission
- Why: Two facts would change a reader's picture. Some application fixtures use real repositories over the in-memory provider, not substitutes. Every hosted test starts its own PostgreSQL 13 container and migrates it: `IntegrationFixtureBase` is the test class's `IAsyncLifetime`, and there is no class or collection fixture, so xUnit runs it per test.
- Evidence: `Pot.App.Tests` uses `UseInMemoryDatabase` in 4 files (for example `CheckDescriptionDoesNotExistFixture.cs:99`); `IntegrationFixtureBase.cs:13, 93-117` (`new PostgreSqlBuilder("postgres:13")`, `MigrateAsync`); no `IClassFixture`/`ICollectionFixture`/`AssemblyFixture` in the project.
- old:
~~~
testing that layer with its neighbours substituted, and one integration project that boots the
application against a PostgreSQL container and drives it through HTTP.
~~~
- new:
~~~
testing that layer with its neighbours substituted (a few application fixtures run real
repositories over the EF Core in-memory provider instead), and one integration project that boots the
application against a PostgreSQL container and drives it through HTTP. Each integration test starts its
own `postgres:13` container and applies every migration before it runs (`IntegrationFixtureBase`).
~~~

### E-0033-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The cost is a container start and full migration per test, not one per run.
- Evidence: as E-0033-02.
- old:
~~~
- Bad, because a change can need both suites, and the outer one costs a container start.
~~~
- new:
~~~
- Bad, because a change can need both suites, and the outer one starts a container and migrates a fresh
  database for every test.
~~~

### E-0033-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why: There are two problems. First, the Etag is not a database concurrency token (ADR-0006, DD-02), so concurrency does not differ between the in-memory and real providers. Second, DD-12d says the hosted suite covers only some of what the in-memory provider misses, but "covered only by the hosted suite" implies it is covered. The trailing clause is rhetoric with no evidence.
- Evidence: `DECISIONS-TO-DISCUSS.md` DD-12d ("InMemory ignores `citext` … unique indexes and SQL translation … The Testcontainers integration project covers some of this"); ADR-0006 title.
- old:
~~~
- Bad, because the data-layer tests use an in-memory provider, so query translation, constraints and
  concurrency are covered only by the hosted suite — the failures that reach production first are the ones
  furthest down the pyramid.
~~~
- new:
~~~
- Bad, because the data-layer tests use an in-memory provider, which ignores `citext`, unique indexes and
  SQL translation, so those are covered only where a hosted test happens to exercise them.
~~~

### E-0033-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: Name the metadata fixtures the bullet relies on, so the confirmation is checkable.
- Evidence: `Pot.AspNetCore.Integration.Tests/Security/EndpointAuthorizationMetadataFixture.cs`, `Pipeline/EndpointResponseMetadataFixture.cs`.
- old:
~~~
and the metadata
  fixtures are what make the architecture rules checkable rather than remembered.
~~~
- new:
~~~
and the metadata
  fixtures (`EndpointAuthorizationMetadataFixture`, `EndpointResponseMetadataFixture`) are what make the
  architecture rules checkable rather than remembered.
~~~

### E-0033-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: Existing unit tests legitimately assert status codes on the classes that map failures to statuses, so the review rule as written condemns correct tests. The instruction file that actually defines the split is not named.
- Evidence: `Pot.AspNetCore.Tests/Concerns/ExceptionHandlers/BadRequestExceptionHandlerFixture.cs:31-40`, `DatabaseExceptionHandlerFixture.cs`, `Extensions/EnrichedErrorExtensionsFixture.cs`; `.github/instructions/aspnetcore-integration-tests.instructions.md` ("Unit or integration?").
- old:
~~~
- Review: a rule tested only through the hosted suite is an expensive test for something that did not need
  a pipeline; a status code asserted in a unit test is a test in the wrong project.
~~~
- new:
~~~
- Instruction: `.github/instructions/aspnetcore-integration-tests.instructions.md` (Unit or integration?)
  defines which tests belong in the hosted project.
- Review: a rule tested only through the hosted suite is an expensive test for something that did not need
  a pipeline; an endpoint's status code or body asserted without booting the host is a test in the wrong
  project. A unit test of the class that maps a failure to a status, such as
  `EnrichedErrorExtensionsFixture`, is not.
~~~

### Notes (no edit proposed)
- DD-12d (keep the in-memory provider vs move `Pot.Data.Tests` to Testcontainers) and DD-06 (PostgreSQL 13 in tests vs 17.6 in Azure) are undecided. After the edits the record states the current state and does not pick an option.
- Verified: unit projects exist for App, Data, AspNetCore and Shared. There are none for EmailSender, RazorComponents or Data.Migrations. The integration project references `Testcontainers.PostgreSql`.

---

## ADR-0034 — 0034-secret-values-live-outside-the-repository.md

Verdict: EDITS

### E-0034-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The template does not hold "every key the compose file substitutes". `POSTGRES_DB` (and `COMPOSE_PROJECT_NAME`) come from the tracked `.env`, and `IMAGE_TAG` is set by the tasks. The record also leaves out the ignore rule that keeps the working file out, and the other ignored local file the API reads when run outside Docker.
- Evidence: `docker-compose-client-server.yml` (`POSTGRES_DB=${POSTGRES_DB}`, `image: pot-server:${IMAGE_TAG:-latest}`); `Source/Docker/.env` keys `COMPOSE_PROJECT_NAME`, `POSTGRES_DB`; `.gitignore:405-408` (`*.development*`, `*.production*`, `!Source/Docker/.env.development.template`); `git check-ignore -v Source/Server/Pot.AspNetCore/appsettings.Development.json` → `.gitignore:405`.
- old:
~~~
Chosen option: 1. The repository holds `.env.development.template`: every key the compose file
substitutes, with startup-safe values where a local default is harmless and marked placeholders where a
value must be supplied. Copying it produces a working local stack; the copy is ignored.
~~~
- new:
~~~
Chosen option: 1. The repository holds `Source/Docker/.env.development.template`: every per-developer
key the compose file substitutes, with startup-safe values where a local default is harmless and marked
placeholders where a value must be supplied. Copying it produces a working local stack; the copy is
ignored (`.gitignore` ignores `*.development*` and `*.production*`, with the template as the one
exception). The two non-secret keys, `COMPOSE_PROJECT_NAME` and `POSTGRES_DB`, are in the tracked `.env`.
The API run outside Docker reads its local values from `appsettings.Development.json`, which the same
rule ignores; no template is tracked for it.
~~~

### E-0034-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: omission
- Why: The tracked evidence for the production store (the diagram) should be named, and the record should state what the repo cannot show. The API has no Key Vault client, so values must arrive as configuration whose mapping lives only in the container app.
- Evidence: `Docs/Azure/POT-Azure-Diagram.mmd` (`KV["Key Vault<br/>kv-potapp-prod"]`, `MI -.->|Get Secrets| KV`, `KV -.->|Returns Values| APIContainer`); `git grep -i "KeyVault|Azure.Identity"` in `Source/Server` → no hits; `Source/Docker/DEVELOPER.md:711-712` ("connection details come from the container app's environment variables").
- old:
~~~
Production values come from the platform's secret store and reach the container as environment settings,
so the image contains no credential and a rotated value does not need a rebuild.
~~~
- new:
~~~
Production values come from the platform's secret store (Key Vault, read through a managed identity, in
`Docs/Azure/POT-Azure-Diagram.mmd`) and reach the container as environment settings, so the image
contains no credential and a rotated value does not need a rebuild. The API has no Key Vault client of its
own; how each value is mapped is container-app configuration that the repository does not record.
~~~

### E-0034-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: The named "Test" does not exist: nothing runs the compose instructions. Real mechanical enforcement does exist (the ignore rule and startup validation), and the gap should be stated.
- Evidence: `.gitignore:405-408`; `Pot.AspNetCore.Tests/Extensions/OptionsStartupValidationFixture.cs` (Jwt, PlatformAdmin, Cors, Smtp fail at start); no secret-scanning config or workflow in the repo.
- old:
~~~
- Test: the local compose instructions are executable as written — copying the template produces a stack
  that starts, which is the property the template exists for.
~~~
- new:
~~~
- Mechanism: `.gitignore` ignores `*.development*` and `*.production*` and re-includes only
  `Source/Docker/.env.development.template`, so an ordinary `git add` does not pick up a working copy.
- Test: `OptionsStartupValidationFixture` asserts that missing or malformed required settings fail startup,
  which is what makes an unsupplied value a failed start rather than a silent default.
- Gap: nothing scans tracked files for credentials, and no test checks that a copy of the template starts
  a stack; both are kept by review.
~~~

### E-0034-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why: The record refers to the consuming deployment without linking it.
- Evidence: `Docs/adr/0036-production-runs-two-container-apps.md`.
- old:
~~~
- The deployment that consumes the secret store is a separate decision.
~~~
- new:
~~~
- The deployment that consumes the secret store is
  [ADR-0036](0036-production-runs-two-container-apps.md).
~~~

### Notes (no edit proposed)
- **Tracked test-only values.** `playwright.config.ts` and `playwright.prod.config.ts` pass a JWT signing
  key and database credentials for the throwaway E2E stack on the command line, and the integration test
  hosts and `OptionsStartupValidationFixture` use literal test values. None is a real credential, but the
  Review rule ("a real credential in a tracked file ... is a defect") does not say test-only values are
  allowed. The owner may want a sentence that says so explicitly, so a reviewer does not flag them.
- `DOCS-WORK-LISTS.md` says "Database backups are kept outside the repository" folds into this decision
  (row 0079). The ADR does not mention it. `.gitignore:402,416-418` (`**/postgres-data/`, `Data/Azure`,
  `Data/**/Backups/`) and `Source/Docker/DEVELOPER.md:589-590` are the evidence, if the owner wants it
  folded in.
- See the cross-cutting note on the deployment blueprint in git history. If it holds a real value, that
  is a live instance of the failure this record exists to prevent.
- The `2026-11-04` date matches the owner's decision to track the template ("Decided 2026-11-04"), so no
  date edit is proposed.

---

---

## ADR-0035 — 0035-builds-and-deployments-are-run-by-hand.md

Verdict: EDITS

### E-0035-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE
- Severity: format
- Why: The hand-run build-and-push tasks for Azure date from the first Azure build.
- Evidence: `git show e45caf59 -- .vscode/tasks.json` adds `pot-server-build-and-deploy-azure` / `pot-client-build-and-deploy-azure` (2025-11-07); they were renamed to `azure-*-build-and-deploy` in `11f91299` (2026-05-01).
- old:
~~~
date: 2026-11-04
~~~
- new:
~~~
date: 2025-11-07
~~~

### E-0035-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: unsupported-history
- Why: Nothing in the repo records cost as "the deciding factor": there is no SQ interview row and no dated note. The deployment plan tracked in history from 2025-11-07 to 2025-11-16 says the opposite. It lists GitHub Actions as "Free (within included minutes)" and plans to "Implement GitHub Actions after successful manual deployment". "Consumption plan" is an Azure setting with no owner-confirmed record.
- Evidence: `DOCS-WORK-LISTS.md` (no SQ row for 0080; row 0080 status `Dec`); `git show e7e861f9^:Docs/Azure/Azure-Deployment-Blueprint.md` lines 70-73, 204-206, 266-268, 323-325 (not cited in the proposed text).
- old:
~~~
Cost is the deciding factor at this stage: a hosted pipeline and its minutes are a recurring cost for a
project whose deployment is a container app on a consumption plan.
~~~
- new:
~~~
No interview or dated note in the repository gives the reason there is no pipeline. The deployment plan
written when the hosted environment was first set up (November 2025) intended automation to follow a
manual first deployment; it has not followed.
~~~

### E-0035-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: confirmation
- Why: The Confirmation section has no mechanical rule at all. Adding a pipeline file is the observable event that undoes this decision.
- Evidence: `.github/workflows/` absent from `git ls-files` (it exists on disk, empty and untracked); `Docs/adr/README.md` "Supersede when the decision statement is no longer true".
- old:
~~~
- Known gap: no artefact identifies the commit it was built from, so provenance depends on the developer's
  record.
~~~
- new:
~~~
- Review: a pipeline definition added to the repository (for example under `.github/workflows/`)
  supersedes this record.
~~~

### E-0035-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: TEMP-DOC-CITE
- Severity: consistency
- Why: (1) "Expected to be revisited as the product grows" is unsupported; the actual status is an open, undecided brief (DD-05). (2) "A change to this record" contradicts the README, which says an accepted record is superseded, never rewritten.
- Evidence: `DECISIONS-TO-DISCUSS.md` DD-05 (`> **Your call:**` unanswered; recommendation B); `Docs/adr/README.md` "Amending and superseding".
- old:
~~~
- This decision is expected to be revisited as the product grows; a pipeline would then be a change to
  this record rather than an addition beside it.
~~~
- new:
~~~
- Whether to add a build-and-test workflow is open (`DECISIONS-TO-DISCUSS.md`, DD-05). A pipeline, if
  adopted, supersedes this record rather than sitting beside it.
~~~

### E-0035-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why: The record refers to the deployment topology without linking it.
- Evidence: `Docs/adr/0036-production-runs-two-container-apps.md`.
- old:
~~~
- The deployment target and its topology are separate decisions.
~~~
- new:
~~~
- The deployment target and its topology are
  [ADR-0036](0036-production-runs-two-container-apps.md).
~~~

### Notes (no edit proposed)
- **Status vs DD-05.** W4 row 0080 is `Dec` ("waits for its brief in `DECISIONS-TO-DISCUSS.md`"), and
  DD-05 is unanswered. The record is `accepted`, which in effect adopts DD-05 option A ("keep as-is,
  record honestly") before the owner has chosen. Either the owner answers DD-05 with A (and the record
  stands), or the record should stay `proposed` until DD-05 is decided. Nothing cites ADR-0035, so
  `proposed` would not break `docs-check`. This needs an owner decision, not a mechanical edit.
- "The running revision is switched by hand in the portal" has no tracked support or owner-confirmed row,
  but the 2025-11 deployment plan in git history describes exactly that ("Save as a new revision"). I
  left it in. The owner may want to confirm it in DOCS-WORK-LISTS so it has a citable source.
- If the owner prefers not to refer to the removed deployment plan at all, drop the last sentence of
  E-0035-02's new text.
- Verified: `.vscode/tasks.json:93-117` (`docker build --no-cache ... :latest; docker push`), the
  server test task (`server-run-test-coverage` → `code_coverage.ps1`), and the client `test` script
  (`vitest`). 101 lines.

---

---

## ADR-0036 — 0036-production-runs-two-container-apps.md

Verdict: EDITS

### E-0036-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE
- Severity: format
- Why: The two-app shape is fixed by the first Azure client build: `nginx.azure.conf` with no `/api` proxy, plus an absolute API base URL.
- Evidence: `e45caf59 2025-11-07 Docker file updates for local vs azure builds` (adds `Source/Docker/Client/nginx.azure.conf` and the client deploy task with `VITE_API_BASE_URL=https://api.payontime.com.au/api`).
- old:
~~~
date: 2026-11-04
~~~
- new:
~~~
date: 2025-11-07
~~~

### E-0036-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: omission
- Why: W4 row 0074 is still `Ver` because the tracked documents disagree. The diagram and the Docker guide say the ingress routes `/api/*` to the API container; the build and nginx config say the API has its own hostname. The record picks one side without saying so, and does not say whether the alternatives were weighed (no SQ interview for 0074).
- Evidence: `.vscode/tasks.json:108`; `Source/Docker/Client/nginx.azure.conf` (no `/api/` location); `Docs/Azure/POT-Azure-Diagram.mmd` (`Ingress -->|"/api/*"| APIContainer`); `Source/Docker/DEVELOPER.md:105-106,404-405,709`; `DOCS-WORK-LISTS.md` row 0074 ("Ver (docs disagree on whether ingress routes `/api`)").
- old:
~~~
The client is built with the API's absolute base URL, because there is no proxy to make a relative path
work.
~~~
- new:
~~~

Both are set in `.vscode/tasks.json` (`azure-client-build-and-deploy`): it builds with
`NGINX_CONFIG=nginx.azure.conf`, which has no `/api` proxy, and with the API's own hostname as the base
URL. The tracked diagram and `Source/Docker/DEVELOPER.md` instead describe the ingress routing `/api/*`
to the API container. This record follows the build, and the repository cannot settle the discrepancy.
The record does not show whether the single-origin options were weighed when the deployment was set up.
~~~

### E-0036-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: omission
- Why: The code shows why `SameSite` is load-bearing. The refresh cookie is `Lax`, which works cross-origin only because the client and API hosts are the same site. Moving the API to another registrable domain would break refresh.
- Evidence: `Source/Server/Pot.AspNetCore/Features/Auth/RefreshTokenHelper.cs:18,37` (`SameSite = SameSiteMode.Lax`); hosts `payontime.com.au`/`www.`/`api.` (diagram, tasks.json).
- old:
~~~
- Bad, because every call is cross-origin, so the CORS policy and the cookie's `SameSite` behaviour are
  load-bearing rather than incidental.
~~~
- new:
~~~
- Bad, because every call is cross-origin, so the CORS policy and the cookie's `SameSite` behaviour are
  load-bearing rather than incidental: the refresh cookie is `SameSite=Lax` (`RefreshTokenHelper.cs`),
  which works only because the client and API hostnames share one registrable domain.
~~~

### E-0036-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: TEMP-DOC-CITE
- Severity: omission
- Why: The record misses an important cost of independent deployment. Two separate tasks deploy the two apps, so the client and API versions can be skewed, and there is no compatibility policy for that window. That is open as DD-14.
- Evidence: `DECISIONS-TO-DISCUSS.md` DD-14 ("The server and client are built and pushed by two separate manual tasks (`.vscode/tasks.json:93-117`), so for a while the new server serves the old client"; unanswered).
- old:
~~~
- Bad, because two deployments and two ingress configurations must be kept in step.
~~~
- new:
~~~
- Bad, because the two images are built and pushed by separate tasks, so for a while after a deployment
  the new API serves the old client (and a cached PWA shell keeps calling it); no compatibility policy
  covers that window, which is the open question DD-14 in `DECISIONS-TO-DISCUSS.md`.
~~~

### E-0036-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: The named test does not exercise this arrangement. `playwright.prod.config.ts` builds the client and serves it with `vite preview`, whose proxy forwards a relative `/api`. The test is same-origin and never uses an absolute base URL, so it cannot catch a base-URL mistake.
- Evidence: `Source/Client/pot-react/playwright.prod.config.ts:59-60` (`npm run build && npm run preview`); `e2e/README.md:144-146` ("the preview server inherits `server.proxy`, so `/api` still reaches the API, and `VITE_API_BASE_URL=/api`"); `vite.config.ts:87-94`; `Pipeline/CorsFixture.cs`.
- old:
~~~
- Test: the production-like end-to-end configuration runs the built client against the API, which is the
  arrangement that catches a base-URL mistake.
~~~
- new:
~~~
- Gap: no test exercises the deployed arrangement. The production-like end-to-end configuration
  (`playwright.prod.config.ts`) serves the built client through `vite preview`, whose proxy forwards a
  relative `/api`, so it is same-origin and would not catch a wrong absolute base URL or a missing origin
  in the deployed CORS list. `CorsFixture` covers the API's side of the cross-origin boundary.
~~~

### E-0036-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why: Both "separate decisions" exist as ADRs and should be linked. The build-and-publish decision is related too.
- Evidence: `Docs/adr/0003-...md`, `Docs/adr/0005-...md`, `Docs/adr/0035-...md`.
- old:
~~~
- Where TLS terminates, and what the container receives, is a separate decision.
- The CORS policy that makes the cross-origin boundary work is a separate decision.
~~~
- new:
~~~
- Where TLS terminates, and what the container receives, is
  [ADR-0003](0003-public-tls-terminates-at-the-container-apps-ingress.md).
- The CORS policy that makes the cross-origin boundary work is
  [ADR-0005](0005-cross-origin-access-is-limited-to-configured-browser-origins.md).
- How the images are built and published is
  [ADR-0035](0035-builds-and-deployments-are-run-by-hand.md).
~~~

### Notes (no edit proposed)
- **Row 0074 is `Ver`, yet the record is `accepted`.** The owner should confirm how `/api` reaches the API
  in Azure: through the API app's own hostname (as the build implies) or through ingress path routing (as
  the diagram and Docker guide say). The diagram and Docker guide then need correcting under W2. The
  record's claim "each with its own ingress and managed certificate, on the custom domains" agrees with
  Container Apps having one ingress per app and with the owner-confirmed certificates for three domains,
  but the repo cannot confirm which app owns which hostname.
- DD-04 (replica count) does not touch this record. It states no replica count, and none can be
  confirmed from the repo.
- Drivers such as "a UI change should not redeploy the API" and "the API should be reachable by anything
  that can present a token" have no interview or note behind them. E-0036-02 states that the record does
  not show whether alternatives were weighed, rather than removing the drivers.
- Verified: the client Dockerfile is multi-stage with an `nginx:alpine` final stage; `/health` and the
  immutable `/assets/` cache headers are in `nginx.azure.conf`; the server entrypoint runs the migration
  runner and then the API; both images are pushed to `ghcr.io/mjfreelancing`; the diagram shows both apps
  in one environment (`cae-pot-prod`). About 115 lines after the edits.

---

## ADR-0037 — 0037-the-client-is-organised-by-feature-over-shared-layers.md

Verdict: EDITS

### E-0037-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: format
- Why: The record is backfilled but does not say so or name its sources (skill hard rule).
- Reworded for house style: Removed the backfill statement and the source list; kept only the plain statement that the record does not show whether the alternatives were weighed, in the cleanup's wording.
- Evidence: `.github/skills/adr/SKILL.md` ("A backfilled record says so where it matters"); `git log` shows `src/features/` from 2025-03-15 (`f6516773`) and `src/concerns/` from 2025-11-23 (`550b4681`). The ADR was written on 2026-11-04 (`6a4cbf5c`).
- old:
~~~
it, and promotion has to have a place to happen.
~~~
- new:
~~~

The alternatives below record what each would have cost; the record does not show whether they were
weighed when the layout was chosen.
~~~

### E-0037-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The current text gets the dependency graph wrong (DD-13):
  - The API hooks live in `src/api/hooks`, not in features.
  - `routes` is the composition root, not a layer that avoids features.
  - `contexts` is missing from the list of shared folders.
  - It leaves out the dominant edge: six features import `auth`.
  - "Kept its old imports" is a cause with no evidence behind it.
- Evidence:
  - Shared folders that import features: `src/api/hooks/useMe.ts:2`, `src/api/hooks/useSignup.ts:1`, `src/concerns/auth/logoutManager.ts:1`, `src/components/nav/AppSidebarMenus.tsx:16-18`, `src/components/user/UserMenu.tsx:13-14`.
  - Features that import `@/features/auth`: `accounts`, `dashboard`, `expenses`, `incomes`, `users`, `userSettings` (for example `src/features/accounts/AccountsPage.tsx:16`, `src/features/userSettings/delete/DeleteAccountAction.tsx:6-8`).
  - Dashboard renew helpers: `src/features/dashboard/actions/accruals/RenewExpensesAction.tsx:12`, `RenewIncomesAction.tsx:12`.
  - `features/shared`: `src/features/expenses/edit/EditExpenseSheet.tsx:10`.
  - Composition root: `src/routes/AppRoutes.tsx:5-49` and `src/App.tsx:15` import features.
  - The API hooks are all in `src/api/hooks/` (`useAccounts.ts`, `useExpenses.ts` and so on).
  - `src/contexts/ErrorContext.tsx` exists.
- old:
~~~
Chosen option: 1. Each feature holds its pages, its components, its forms, its contexts and its API hooks.
Below the features sit the shared folders — `api` for the HTTP client, interceptors and shared types;
`concerns` for cross-cutting behaviour such as caching, logging, token providers and session handling;
`lib` for utilities; and the shared `components`, `data`, `hooks`, `routes` and `stores` folders. A feature
depends on those layers, and the layers are meant not to depend on a feature.

In practice they do: shared hooks, the logout manager and the sidebar import from features, because a
piece promoted to a shared folder kept its old imports. Feature-to-feature imports also exist — the
dashboard composes other features' data — and one feature folder, `features/shared`, holds sheets that
several features use. Both are visible in the import graph rather than prevented by the structure, which
is the one place this record's rule is softer than the folder layout suggests.
~~~
- new:
~~~
Chosen option: 1. Each feature holds its pages, components, forms, schemas, contexts and the hooks that
orchestrate its mutations. Below the features sit the shared folders: `api` for the HTTP client,
interceptors, error types and the per-resource query and mutation hooks; `concerns` for cross-cutting
behaviour such as authentication tokens and logout, caching, logging, storage keys and the PWA; `lib` for
utilities; and the shared `components`, `contexts`, `data`, `hooks` and `stores` folders. A feature
depends on those layers, and the layers are meant not to depend on a feature. `App.tsx` and
`routes/AppRoutes.tsx` are the composition root and import features by design.

In practice the direction does not hold. Shared folders import features: the API hooks `useMe` and
`useSignup` and the logout manager import from `features/auth`, and the sidebar menus and the user menu
import from `auth`, `maintenance` and `userSettings`. Most feature-to-feature imports are into
`features/auth`: six features (accounts, dashboard, expenses, incomes, users, userSettings) import its
permission components, contexts, session notice or session-clearing hook, so `auth` already behaves as a
shared layer. The dashboard also imports the expense and income renew helpers, and one feature folder,
`features/shared`, holds sheet states that expenses and incomes use. None of this is prevented by the
structure or by lint, which is the one place this record's rule is softer than the folder layout suggests.
~~~

### E-0037-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: Removing a screen means deleting its folder and also editing the route table and the sidebar menu that import it.
- Evidence: `src/routes/AppRoutes.tsx:9-49` (lazy imports of each feature page and sheet); `src/components/nav/AppSidebarMenus.tsx:16-18`.
- old:
~~~
- Good, because a screen's code is in one folder, and its removal is one deletion.
~~~
- new:
~~~
- Good, because a screen's code is in one folder, so removing it is that folder plus its route and menu
  entries.
~~~

### E-0037-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why:
  - "The existing ones are debt to be removed" treats DD-13 option C as decided, but DD-13 is open.
  - "Recorded if it stays" names no place where it would be recorded.
  - Confirmation leaves out the instruction file that states the rule, and does not say that nothing enforces it.
- Evidence:
  - `DECISIONS-TO-DISCUSS.md` DD-13 (options A/B/C, `> **Your call:**` empty).
  - `.github/instructions/client.instructions.md` (Architecture: "shared/cross-cutting layers must never depend on features").
  - `eslint.config.js` has no `no-restricted-imports` or boundary rule (grep for `restricted|boundar|import/` finds nothing).
- old:
~~~
- Review: an import from a shared layer into a feature is a defect; the existing ones are debt to be
  removed, not a pattern to copy. A new feature-to-feature import is challenged, and recorded if it stays.
~~~
- new:
~~~
- Instruction: `.github/instructions/client.instructions.md` (Architecture) states the one-way rule.
- Review: a new import from a shared layer into a feature is a defect, and a new feature-to-feature import
  is challenged. Whether the existing ones are moved into shared layers or sanctioned is undecided; until
  it is, they are not a pattern to copy.
- Gap: nothing mechanical enforces the direction; `eslint.config.js` has no `no-restricted-imports` or
  import-boundary rule.
~~~

### Notes (no edit proposed)
- **Date.** `date: 2026-11-04` is the writing date. The layout built up over time: feature folders from
  2025-03-15 (`f6516773`), and the `concerns` layer from 2025-11-23 (`550b4681`, "Client http-cookie +
  some re-organisation"). The owner should choose the date. 2025-11-23 is when all three shared layers
  first existed.
- **DD-13 is open.** Recording the graph honestly is not a ruling on it. If DD-13 goes to option C (move
  `auth` into `concerns`), this record's second paragraph becomes historical, and the record should be
  superseded rather than edited.
- **Other docs are wrong.** `DEVELOPER.md:260` ("Features never import from other features") is false. The
  instruction file's "must never depend on features" is broken by five shared-layer files. Both are W2/W3
  documentation fixes, not ADR text.

---

---

## ADR-0038 — 0038-server-state-lives-in-a-query-cache-over-one-http-client.md

Verdict: EDITS

### E-0038-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE
- Severity: consistency
- Why: `date` is the writing date. TanStack Query and the shared axios hooks first appear together in `7bb53c3b`.
- Evidence: `git log --reverse -S "QueryClient" -- Source/Client/pot-react/src` → `7bb53c3b 2025-03-23 Start adding account api call` (it adds `src/api/hooks/useApi.ts`).
- old:
~~~
date: 2026-11-04
~~~
- new:
~~~
date: 2025-03-23
~~~

### E-0038-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: format
- Why: Backfilled record with no backfill statement or sources.
- Reworded for house style: Removed the backfill statement and the source list; kept only the plain statement that the record does not show whether the alternatives were weighed, in the cleanup's wording.
- Evidence: `.github/skills/adr/SKILL.md` hard rules.
- old:
~~~
they would diverge.
~~~
- new:
~~~

The alternatives below record what each would have cost; the record does not show whether they were
weighed before the choice.
~~~

### E-0038-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why:
  - The transport is the global default axios instance, configured through `axios.defaults`. It is not an instance made with `axios.create()`.
  - The second instance is used only for the token refresh, not for "the authentication calls": login and logout go through `usePost` on the default instance.
- Evidence:
  - `src/api/axios.ts:10-31` and `src/main.tsx:14-15`.
  - `src/api/authClient.ts:16-27`. Its only callers are `src/api/services/authService.ts:19-20` and `src/features/auth/accessTokenRefreshTimer.ts:56` (both `/auth/refresh`).
  - `src/api/hooks/useAuth.ts:6,16` (`usePost('/auth/login')`, `usePost('/auth/logout')`).
- old:
~~~
Transport is one
axios instance with its interceptors; a second instance exists only for the authentication calls that must
not run through the interceptor that depends on them.
~~~
- new:
~~~
Transport is the
default axios instance, configured once at start-up (`src/main.tsx`, `src/api/axios.ts`) with its
interceptors; a second instance, `authClient`, exists only for the token refresh, which must not run through
the authentication interceptor that depends on it. It shares the correlation and normalising interceptors.
~~~

### E-0038-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why:
  - Invalidation follows one central dependency table. Mutations do not each declare the full set of affected keys.
  - The current wording leaves out the table that row 0057 names.
  - It also leaves out the direct calls that bypass the table.
- Evidence:
  - `src/concerns/cache/cacheInvalidation.ts:19-36,86-96`: `INVALIDATION_DEPENDENCIES`, where expenses and incomes invalidate accounts, and accounts invalidate projections.
  - Direct `invalidateQueries` calls: `src/features/dashboard/actions/accruals/RenewExpensesAction.tsx:68`, `RenewIncomesAction.tsx:66` (`projections`), and `src/features/userSettings/sections/budgetReminders/BudgetRemindersForm.tsx:219` (`settings`).
  - `DOCS-WORK-LISTS.md` row 0057 ("invalidation follows a dependency graph").
- old:
~~~
Invalidation is explicit: an operation declares which query keys it affects, so a change to the accounts
list refreshes what derives from accounts and nothing else.
~~~
- new:
~~~
Invalidation is explicit: a mutation names the keys it changed, and one dependency table
(`src/concerns/cache/cacheInvalidation.ts`) adds what derives from them — expenses and incomes invalidate
accounts, and accounts invalidate projections. A few call sites bypass the table and call the query client
directly: the dashboard renew actions for projections, and the budget reminders form for settings.
~~~

### E-0038-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: False as written. No query ever rejects, so TanStack Query never retries. There is also an important consequence the record leaves out: a failure is cached as fresh data.
- Evidence:
  - `src/api/hooks/useApi.ts:47-56`: `performOperation` catches every error and *returns* the `FailResult`.
  - `useApi.ts:119-126`: every `useGet` `queryFn` returns `performOperation(...)`.
  - `src/api/hooks/useExports.ts:33-48` and `useImports.ts` do the same for their mutations.
  - TanStack Query retries only a `queryFn` that rejects.
  - `src/main.tsx:21`: `staleTime` is 5 minutes.
  - `src/api/hooks/useMe.ts:24` sets `retry: false`, which therefore has no effect.
- old:
~~~
- Bad, because retries are the library's default rather than a decision: a failing query is retried, and
  only one hook opts out, so a throttled or forbidden read is repeated before the user sees it.
~~~
- new:
~~~
- Bad, because the query hooks never reject: `useGet` resolves a failure as a `FailResult` value, so the
  library treats every failure as success. Its retries never run (the `retry: false` on `useMe` has no
  effect), `isError` is never set, and a failed read is held as fresh data for the five-minute stale time,
  so remounting a screen shows the cached failure rather than asking again.
~~~

### E-0038-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: Invalidation takes a typed union, `CacheKey`, so a typo there is a compile error. The silent miss is between a query's untyped `string[]` key and that union, and some query keys are not in the union at all.
- Evidence:
  - `src/concerns/cache/cacheInvalidation.ts:6-13` (`CacheKey` union).
  - `src/api/hooks/useApi.ts:108` (`queryKey: string[]`).
  - `src/api/hooks/useSettings.ts:7` (`['settings']`) and `useRoles.ts:9` (`['roles']`), neither in `CacheKey`.
- old:
~~~
- Bad, because query keys are strings with structure, so a typo is a silent miss rather than a compile
  error.
~~~
- new:
~~~
- Bad, because the keys a query is stored under are untyped `string[]` while invalidation accepts a typed
  union, so a query whose key is outside the union (`settings`, `roles`) can be invalidated only by a direct
  call, and a mismatch between the two is a silent miss rather than a compile error.
~~~

### E-0038-07
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: "The mutation hooks are tested" is broader than the truth. Only a few mutation flows have invalidation fixtures, and the account, expense and income mutation hooks have none. The record should also name the real fixtures and the instruction file.
- Evidence:
  - `tests/concerns/cache/cacheInvalidation.test.ts:8-75` (mocked `QueryClient`).
  - `tests/features/users/delete/hooks/useDeleteUser.test.tsx:67-94`, `tests/features/approvals/components/PendingApprovalMobileCard.test.tsx:93-156` and `tests/features/users/UsersPage.test.tsx:329-346` (`useCacheInvalidation` mocked).
  - No tests exist for `src/features/{accounts,expenses,incomes}/{create,edit,delete}/hooks/*`.
  - `.github/instructions/client.instructions.md` (State and data flow).
- old:
~~~
- Test: the invalidation helper and the mutation hooks are tested with the query client substituted, so the
  assertions are about which keys a mutation invalidates rather than about refetching from a real cache.
~~~
- new:
~~~
- Test: `tests/concerns/cache/cacheInvalidation.test.ts` asserts the dependency expansion against a mocked
  query client. `tests/features/users/delete/hooks/useDeleteUser.test.tsx`,
  `tests/features/approvals/components/PendingApprovalMobileCard.test.tsx` and
  `tests/features/users/UsersPage.test.tsx` assert the keys a mutation invalidates, with
  `useCacheInvalidation` mocked; the account, expense and income mutation hooks have no such fixture.
- Instruction: `.github/instructions/client.instructions.md` (State and data flow).
~~~

### Notes (no edit proposed)
- **Cached failures need a decision.** The no-reject behaviour in E-0038-05 comes from ADR-0039's choice to
  return results. The owner may want to decide whether a failed query should reject, or be excluded from
  caching, so that it is refetched. That is a code decision, not a wording fix.
- **`authClient` timeout.** `authClient` sets no `timeout`, while the default instance uses
  `VITE_API_TIMEOUT_MS` (`src/api/authClient.ts:19-20` against `src/api/axios.ts:12`), so a refresh can
  wait indefinitely. This is not an ADR edit; I note it for the owner.
- **Correction pass.** The retries bullet was not introduced by the first correction pass (`f0eb33d8`); it
  was there from the start.

---

---

## ADR-0039 — 0039-api-hooks-return-results-and-one-interceptor-normalises-failures.md

Verdict: EDITS

### E-0039-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE, RELATED-PRDS
- Severity: consistency
- Why: `date` is the writing date. PRD-030 (Complete) sets this decision's current requirements (E1–E8), and row 0058 cites it.
- Evidence:
  - `f4b9a68d 2025-05-10 "Api + test updates - WIP"` adds `src/api/interceptors/axiosInterceptors.ts`, which maps status to `FailResult`, and strips the mapping out of `useApi.ts`.
  - `53235fb4 2025-05-04` "Update useGet for result pattern".
  - `Docs/Future/PRD-030-problem-details-consolidation.md:3` (Status: Complete), section E.
  - `DOCS-WORK-LISTS.md` row 0058.
- old:
~~~
date: 2026-11-04
area: frontend
~~~
- new:
~~~
date: 2025-05-10
area: frontend
related-prds: [PRD-030]
~~~

### E-0039-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: format
- Why: Backfilled record with no backfill statement or sources.
- Reworded for house style: Removed the backfill statement, the "since May 2025" history (the date is E-0039-01's) and the source list; kept PRD-030 E1–E8 as what governs the interceptor, and the plain "not shown whether weighed" line.
- Evidence: `.github/skills/adr/SKILL.md` hard rules; commits above.
- old:
~~~
application answers with no body at all.
~~~
- new:
~~~

How the interceptor reads a body, and which message each status shows, are set by
[PRD-030](../Future/PRD-030-problem-details-consolidation.md) (E1–E8). The alternatives below record what
each would have cost; the record does not show whether they were weighed before the choice.
~~~

### E-0039-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: "The typed value" suggests validation, but the body is only cast. That runtime check is the open item DD-07.
- Evidence: `src/api/hooks/useApi.ts:52` (`new SuccessResult(response.data)`, where the type comes from the generic only); `DECISIONS-TO-DISCUSS.md` DD-07 ("Nothing in `src/` calls `.parse` or `.safeParse` on API data").
- old:
~~~
A successful call produces a successful result carrying the typed value;
~~~
- new:
~~~
A successful call produces a successful result carrying the response body, cast to the hook's type but not validated;
~~~

### E-0039-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: A caller cannot "read further into the result". A failure carries only `type`, `code` and `description`. A 400's field messages are joined into one string and the field names are thrown away. Five statuses always show fixed client copy.
- Evidence:
  - `src/lib/result.ts:1-7` (`FailResultBase` has `type`, `code`, `description` only).
  - `src/api/errors/apiErrorResponse.ts:137-146` (`Object.values(error.errors).flat().join('\n')`).
  - `apiErrorResponse.ts:102-120` (fixed copy for 401/403/404/405/429).
  - PRD-030 E4.
- old:
~~~
- Bad, because a caller that wants a nuance the taxonomy does not carry — a field-level message, for
  instance — has to read further into the result than the type alone.
~~~
- new:
~~~
- Bad, because a failure carries only a type, a code and one description: a `400`'s messages are joined
  into one string and their field names dropped, so a form cannot place a server message beside its
  field, and a `401`, `403`, `404`, `405` or `429` always shows fixed client copy, whatever `detail` the
  server sent.
~~~

### Notes (no edit proposed)
- **Verified.**
  - The ten error types match `src/api/errors/apiErrors.ts:10-21`.
  - The status switch, including the 502/503/504 "unreachable" case, matches `axiosInterceptors.ts:144-192`.
  - The auth interceptor is registered first, so it runs first (`src/api/axios.ts:23-26`), and it produces `FailResult(AuthenticationError('Token refresh failed'))` (`authInterceptor.ts:99-110`).
  - The root `ErrorBoundary` is at `src/App.tsx:89`.
  - The Confirmation fixtures exist: `tests/api/interceptors/axiosInterceptors.test.ts:133-479` and the "Error bodies the client cannot read" block at `:497`.
- **Third message source.** The proactive refresh timer (`src/features/auth/accessTokenRefreshTimer.ts:70-76`)
  also builds its own `AuthenticationError('Token refresh failed')`, outside any interceptor. "The one
  place where two interceptors can decide a message" is still true of interceptors, but there is a third
  source.
- **Consequence for ADR-0038.** Because hooks return results instead of rejecting, the query library's error
  state, retries and error boundaries never see an API failure. The consequence is recorded in ADR-0038
  (E-0038-05). The owner may want it stated here too.
- **Date.** The proposed 2025-05-10 is the commit that moved normalisation into one interceptor. If the
  owner prefers the date the decision took its current form, use PRD-030's completion date instead.

---

---

## ADR-0040 — 0040-the-access-token-lives-in-memory-with-proactive-and-reactive-refresh.md

Verdict: EDITS

### E-0040-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE
- Severity: consistency
- Why: `date` is the writing date. The memory-only token provider and the cookie-based refresh arrived together.
- Evidence: `git show 550b4681` (2025-11-23 "Client http-cookie + some re-organisation") adds `src/concerns/auth/authTokenProvider.ts` ("tokens live only in memory"); `git log -S"HttpOnly = true" --reverse` → `49bdb7a9 2025-11-23 Server http cookie implementation`.
- old:
~~~
date: 2026-11-04
~~~
- new:
~~~
date: 2025-11-23
~~~

### E-0040-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: omission
- Why: The record gives no timing for the proactive refresh, and it omits that the interceptor never retries `/auth/` routes.
- Evidence: `src/lib/jwt.ts:231-253` (`bufferMs = 5 * 60 * 1000`); `src/api/interceptors/authInterceptor.ts:73-78`.
- old:
~~~
Nothing is written to storage. A timer refreshes it before expiry, and a response interceptor refreshes it
on a `401` and replays the request.
~~~
- new:
~~~
Nothing is written to storage. A timer refreshes it five minutes before expiry, and a response interceptor
refreshes it on a `401` (except on `/auth/` routes) and replays the request.
~~~

### E-0040-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The single-flight belongs to the interceptor only. "One logout path" is stated loosely: every sign-out reaches the `logoutManager` callback, which calls the server and then `useClearSession`, but the account and site deletion flows call `useClearSession` directly. As written, the Review rule would call those flows defects.
- Evidence: `authInterceptor.ts:49-127`; `src/concerns/auth/logoutManager.ts`; `src/features/auth/contexts/AuthContext.tsx:79-97`; `src/features/auth/hooks/useClearSession.ts:7-25`; `src/features/userSettings/delete/DeleteAccountAction.tsx:56`, `DeleteSiteAction.tsx:67`.
- old:
~~~
Concurrent requests that fail together wait on one refresh rather than each starting their own, and a
single logout path clears the session, the token and the cached server data.
~~~
- new:
~~~
Concurrent requests that fail together wait on the interceptor's one refresh rather than each starting
their own. Every sign-out reaches one logout function, registered with `logoutManager`, which asks the
server to end the session and then clears the token, the cached user and the query cache through
`useClearSession`; the account and site deletion flows call `useClearSession` directly, because the
server session has already gone with the account.
~~~

### E-0040-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The start-up refresh has no unit fixture, but every authenticated E2E test starts a page with only the refresh cookie (a storage state from a fresh login) and no token in memory. So the reload path runs on every such test.
- Evidence: `e2e/fixtures/auth.ts:32-104` (storageState plus a separate `accessToken` fixture; no init script injects the token); `src/features/auth/contexts/AccessTokenContext.tsx:52-97`; no `AccessTokenContext` test under `tests/`.
- old:
~~~
- Bad, because the start-up refresh has no fixture of its own, so the path a reload takes is verified only
  by using the application.
~~~
- new:
~~~
- Bad, because the start-up refresh has no unit fixture of its own; it is exercised by the end-to-end
  suite, whose authenticated tests begin each page with the refresh cookie and no token in memory.
~~~

### E-0040-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: omission
- Why: The timer and the start-up refresh post to `/auth/refresh` independently of the interceptor's guard, and each tab has its own guard. Because the server rotates the refresh token, two overlapping refreshes can leave one presenting a token already rotated. Either failure path signs the user out.
- Evidence: `src/features/auth/accessTokenRefreshTimer.ts:132-158` (direct `authClient.post`; failure → `onRefreshError` → `logout`); `AccessTokenContext.tsx:80`; `authTokenProvider.ts:21-36`; server rotation `AuthService.cs:267-273` and `RefreshFixture.cs:58-61`.
- old:
~~~
  there shows as duplicated or lost requests rather than as an error.
~~~
- new:
~~~
- Bad, because the single-flight guard covers the interceptor only, and only in one tab: the timer and
  the start-up refresh call the refresh endpoint on their own, so a refresh that overlaps another can
  present a refresh token the other has just rotated, which the server refuses and the client treats as
  a failed refresh that signs the user out.
~~~

### E-0040-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: The bullet names no fixture and overstates "ends the session through the same logout path": the interceptor test asserts `clearTokens`, and a separate test asserts that `clearTokens` triggers the logout manager. The proactive timer has its own test, which the bullet omits.
- Evidence: `tests/api/interceptors/authInterceptor.test.ts:160-325`; `tests/concerns/auth/authTokenProvider.test.ts:67`; `tests/features/auth/accessTokenRefreshTimer.test.ts:88-160`.
- old:
~~~
- Test: the interceptor fixtures assert a `401` triggers one refresh, that queued requests are replayed
  after it, and that a failed refresh ends the session through the same logout path.
~~~
- new:
~~~
- Test: `tests/api/interceptors/authInterceptor.test.ts` asserts a `401` triggers a refresh and a retry,
  that a concurrent `401` is queued and replayed after it, that `/auth/` routes are not retried, and that
  a failed refresh clears the tokens; `tests/concerns/auth/authTokenProvider.test.ts` asserts that
  clearing the tokens triggers the logout manager.
- Test: `tests/features/auth/accessTokenRefreshTimer.test.ts` asserts the timer refreshes when it fires
  and reports a failure to its error callback.
~~~

### E-0040-07
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why: As written, the rule makes the deletion flows (E-0040-03) defects. The rule should target a sign-out that leaves a live server session behind.
- Evidence: As E-0040-03.
- old:
~~~
- Review: a token written to storage, or a logout that does not go through the single path, is a defect.
~~~
- new:
~~~
- Review: a token written to storage, or a sign-out that bypasses `logoutManager` while the server
  session still exists, is a defect.
~~~

### Notes (no edit proposed)
- On the interceptor-failure path, `clearTokens` empties the provider before `logoutManager.logout()`
  posts `/auth/logout`, so that request carries no bearer token. The server then clears the cookie
  without revoking the session (see E-0008-07). The session is usually already unusable on that path,
  but not always, for example after a rotation race (E-0040-05).
- The Summary's "every path out of the session goes through one logout" is slightly loose given the
  deletion flows. E-0040-03 explains it in the body, so I left the Summary alone.
- Verified: the provider is the authoritative copy (`authTokenProvider.ts:10`), the context mirrors it
  (`AccessTokenContext.tsx:45-50`), the stale-context consequence (the reactive refresh writes the
  provider only, `authTokenProvider.ts:29`), and the start-up refresh (`AccessTokenContext.tsx:73-93`).
  No token is written to `localStorage` or `sessionStorage`; `sessionNotice.ts` stores only a message.

---

## ADR-0041 — 0041-browser-storage-is-keyed-by-environment-and-user.md

Verdict: MAJOR

The decision statement says keys are "environment-scoped rather than user-scoped". That is the reverse of
the code and of PRD-001. Every feature setting and the signed-in theme use
`pot:{env}:user:{userId}:{feature}`. Only device-level state uses `pot:{env}:{feature}`. The first
correction pass (`a84609c0`) introduced this error, and four consequence bullets and the Confirmation now
follow from it.

### E-0041-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE, RELATED-PRDS
- Severity: consistency
- Why: The decision came from PRD-001 and was implemented on 2026-05-03.
- Evidence: `git show c63f92fb` (2026-05-03, "add per-user, per-environment storage scoping infrastructure"); `Docs/Future/PRD-001-per-session-login-support.md:20-25` (Stage: Implemented, last updated 2026-05-03) and decisions D001–D007 (`:126-160`).
- old:
~~~
date: 2026-11-04
area: frontend
~~~
- new:
~~~
date: 2026-05-03
area: frontend
related-prds: [PRD-001]
~~~

### E-0041-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why:
- Reworded for house style: Kept both corrected paragraphs; removed "This record is backfilled from" and the source list, keeping the pre-PRD-001 flat-key history as a plain statement that cites the PRD.
  - Browser storage is already separated by origin, so a local dev server and a deployed site never share keys. The env marker only separates dev-mode and production-mode builds on the same origin.
  - No column widths or column layout are stored.
  - The record has no backfill statement.
  - Before PRD-001 the keys were flat and unscoped. That is evidence, not inference.
- Evidence:
  - `src/concerns/storage/storageEnv.ts:9-11` (`prod` for MODE production, else `dev`).
  - `package.json` scripts `dev` and `preview` both use `--port 5175`.
  - Stored settings: `src/features/dashboard/hooks/useDashboardStorage.ts:8-15` (sections and periods), the filters in `use{Account,Expense,Income}Storage.ts`, and the theme in `src/App.tsx:47-50`. A grep for `columnSizing|columnWidth` finds nothing.
  - `PRD-001:50-65` (the legacy flat `pot-*` keys).
- old:
~~~
A preference is chosen in an environment, by a person. A local development server and a deployed
environment share a browser, and two users can share a machine. A key without either dimension means one
user's column widths and filters appear for another, in the wrong environment, where they may not even be
valid.

Some preferences are also not preferences at all: a filter or a transient view state should not survive
the session, while a theme or a column layout should.
~~~
- new:
~~~
A preference is chosen in an environment, by a person. Two users can share a browser profile, and a key
without a user segment means one user's filters, dashboard layout and theme appear for the other. Browser
storage is already separated by origin, so a deployed site and a local server on another origin never see
each other's keys; what can share an origin is a development-mode and a production-mode build of the same
client (`npm run dev` and `npm run preview` both serve on port 5175).

Some preferences are also not preferences at all: a filter or the selected account is working state that
should not outlive the tab, while a theme or the dashboard's open sections should.

Until [PRD-001](../Future/PRD-001-per-session-login-support.md) (decisions D001–D007), every setting used
a flat, unscoped `localStorage` key shared by every user and environment.
~~~

### E-0041-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: unsupported-history
- Why: Option 2 is not hypothetical. It was the shipped shape that PRD-001 replaced, and the record should say so.
- Evidence: `PRD-001:64` ("All items used flat, unscoped localStorage keys shared across all users and environments"); `c63f92fb` added a temporary migration from the legacy keys.
- old:
~~~
2. Unscoped keys, one per setting.
~~~
- new:
~~~
2. Unscoped keys, one per setting (the shape before PRD-001).
~~~

### E-0041-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The decision statement is wrong. It says keys are env-scoped and not user-scoped, and that most settings persist. In the code, user scoping is the norm, and most feature settings are session-scoped.
- Evidence:
  - `src/concerns/storage/storageKeyBuilder.ts:21-37`.
  - User-scoped: `src/App.tsx:48-50` (theme), `src/features/accounts/hooks/useAccountStorage.ts:23-33`, `src/features/expenses/hooks/useExpenseStorage.ts:25-35`, `src/features/incomes/hooks/useIncomeStorage.ts`, `src/features/dashboard/hooks/useDashboardStorage.ts:44-53`, `src/features/projections/hooks/useProjectionStorage.ts:94-112`, `src/hooks/useFeatureFilterHref.ts:43-48`.
  - Env-scoped: `src/stores/useUserStore.ts:25` (identity), `src/features/auth/sessionNotice.ts:3` (session notice), and `src/App.tsx:50` (theme before sign-in).
  - `resolveStorageBackend` has no caller outside `concerns/storage` and its tests (grep).
- old:
~~~
Chosen option: 1. A storage helper builds every key from an environment marker and a feature or setting
name, so the same setting holds different values per environment. Keys are environment-scoped rather than
user-scoped, which is what makes them usable before anyone is signed in; the one value that follows the
user is the signed-in identity the interface keeps so it can render before the first request completes.

Most settings are persisted and some are session-scoped, decided per setting when it is added; the helper
does not enforce the choice, so a setting that should be session-scoped is one declaration away from
being persistent.
~~~
- new:
~~~
Chosen option: 1. Two builders in `src/concerns/storage/storageKeyBuilder.ts` make every scoped key. A
setting that belongs to a person uses `pot:{env}:user:{userId}:{feature}`. State that belongs to the
device rather than to a person uses `pot:{env}:{feature}`: the theme before sign-in, the one-shot session
notice, and the persisted signed-in identity (`src/stores/useUserStore.ts`), which cannot be keyed by the
user it identifies. `{env}` is `prod` for a production build and `dev` for every other mode. While no one
is signed in, the feature storage hooks return their defaults and write nothing.

The lifetime is chosen per setting where its storage hook is written: list filters and the selected
account (accounts, expenses, incomes, and the sidebar links built from them) are session-scoped; the
theme and the dashboard's sections and periods persist; projection settings are session-scoped with a
persistent copy that seeds a new tab. The helper does not enforce the choice: `resolveStorageBackend`
exists, but each hook passes its backend directly.
~~~

### E-0041-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: Separating a local environment from a deployed one comes from the origin, not from the key. The user separation applies once a user is signed in.
- Evidence: as E-0041-02 and E-0041-04.
- old:
~~~
- Good, because two users on one machine do not see each other's preferences, and a local environment
  cannot corrupt a deployed one.
~~~
- new:
~~~
- Good, because two users of one browser profile do not see each other's filters, layout or theme once
  signed in, and a development build cannot read what a production build wrote on the same origin.
~~~

### E-0041-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why: The bullet contradicts the record's own later bullet: storage does hold the signed-in identity, which is server data, and its permission list.
- Evidence: `src/stores/useUserStore.ts:13-28` (persists the `/me` `User`, which has `permissions` at `src/data/user.ts:14`).
- old:
~~~
- Good, because storage is not a shadow cache of server data, so there is no second copy to invalidate.
~~~
- new:
~~~
- Good, because, apart from the signed-in identity, storage holds no server data, so there is no second
  copy to invalidate.
~~~

### E-0041-07
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why:
  - False as written: persisted preferences are user-scoped.
  - The real weakness of the env segment is that it has two values. Every production build writes `prod`, whether local Docker or the deployed site, so PRD-001's "production vs docker" separation comes from the origin, not from the key.
- Evidence:
  - `src/concerns/storage/storageEnv.ts:9-11`.
  - `Source/Docker/Client/Dockerfile:111-117` (`npm run build` = MODE production).
  - `PRD-001:31-35` (FR2 example "production vs docker").
- old:
~~~
- Bad, because the environment marker is the only isolation: two users of the same browser against the
  same environment share every persisted preference, and the identity kept for rendering is the only
  per-user value.
~~~
- new:
~~~
- Bad, because the environment segment has only two values: every production build — local Docker and the
  deployed site alike — writes `prod`, so the key alone does not separate those environments; their
  different origins do.
~~~

### E-0041-08
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: The lifetime of each setting does have fixtures: each consumer's test asserts its key and backend. The record should name them.
- Evidence:
  - `tests/concerns/storage/storageKeyBuilder.test.ts:14-111` and `storageEnv.test.ts:10-28`.
  - Key and backend tests: `tests/features/{accounts,expenses,incomes}/hooks/use*Storage.test.ts:32` ("wires scoped key and sessionStorage backend").
  - `tests/features/dashboard/hooks/useDashboardStorage.test.ts:31-55` and `tests/features/projections/hooks/useProjectionStorage.test.ts:59-203`.
  - `tests/app/App.test.tsx:136-184` (theme keys) and `tests/stores/useUserStore.test.ts:36,62`.
- old:
~~~
- Test: the storage helper's fixtures assert the key contains the environment segment and that the chosen
  backend is the one written to; the lifetime rule itself has no fixture, because the helper does not
  enforce it.
~~~
- new:
~~~
- Test: `tests/concerns/storage/storageKeyBuilder.test.ts` and `storageEnv.test.ts` assert both key
  formats and the environment segment. Each consumer's fixture asserts the key it builds and, where it
  chooses one, its backend — `tests/features/*/hooks/use*Storage.test.ts`, `tests/app/App.test.tsx` for the
  theme and `tests/stores/useUserStore.test.ts` for the identity — so moving a setting to another key or
  lifetime fails its fixture. No fixture stops a new setting from choosing the wrong lifetime.
~~~

### E-0041-09
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: Filters *are* stored, in session storage. The URL is the render-time source, and the stored copy feeds the sidebar links.
- Evidence:
  - `src/hooks/useFeatureFilterHref.ts:8-48`.
  - `src/features/expenses/hooks/useExpenseStorage.ts:6-35`.
  - `DOCS-WORK-LISTS.md` (client DEVELOPER finding: "the code uses the URL plus sessionStorage") and row 0060.
  - `Docs/Future/PRD-011-account-filter-navigation-url-state-consistency-prd.md`.
- old:
~~~
- Filters that belong in the URL are decided separately and are not stored at all.
~~~
- new:
~~~
- Filters are read from the URL at render time, which is decided separately (PRD-011); the session-scoped
  copy exists so the sidebar can rebuild a filtered link (`src/hooks/useFeatureFilterHref.ts`).
~~~

### Notes (no edit proposed)
- **PRD-001 FR2 is met by the origin, not the key.** FR2 asks for separation between "production vs
  docker". Both are production builds and both write `prod`. The PRD is Implemented, so this is a note for
  the owner, not a PRD amendment.
- **Stale comment.** The doc comment in `src/hooks/useLocalStorage.ts:24-61` describes the legacy `pot-`
  prefix and a direct `localStorage.getItem('pot-auth')` in `authTokenProvider.ts`. No such access exists
  any more, and the access token is memory-only (ADR-0040). The comment is out of date, and it contradicts
  ADR-0040 for a reader who finds it.
- **DD-12b.** `src/components/ui/sonner.tsx` reads `next-themes`, which has no mounted provider. It touches
  no storage, but toasts ignore the user-scoped theme this record describes. No edit is proposed while
  DD-12b is open.
- **Consistency with ADR-0040.** The edits keep ADR-0040 consistent: nothing token-related is stored, and
  `useClearSession` clears the identity store (`src/features/auth/hooks/useClearSession.ts:19-25`).

---

---

## ADR-0042 — 0042-the-pwa-precaches-the-shell-only-and-enforces-updates.md

Verdict: EDITS

### E-0042-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE
- Severity: consistency
- Why: The decision took its current form, an enforced update, on 2026-09-22. The shell-only precache dates from 2026-03-30.
- Evidence: `ab0b559e 2026-03-30 Add PWA support` (adds `navigateFallbackDenylist`); `edd85d43 2026-09-22 PWA update to enforce client updates instead of allowing deferral`; `3a8d0cbd 2026-09-22` (deployed-build detection).
- old:
~~~
date: 2026-11-04
~~~
- new:
~~~
date: 2026-09-22
~~~

### E-0042-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The reload is enforced, at 45 seconds without input or 5 minutes at the latest. It does not wait for the user to reload.
- Evidence: `src/concerns/pwa/pwaRuntime.ts:13-19,62-72`; `src/concerns/pwa/pwaUpdateEnforcement.ts:43-84`.
- old:
~~~
accepting that a cached shell can outlive the server it was built for until the user reloads.
~~~
- new:
~~~
accepting that a cached shell can outlive the server it was built for until its enforced update applies.
~~~

### E-0042-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: format
- Why: Backfilled record with no backfill statement or sources. The git history shows a deferrable prompt that shipped and was later replaced.
- Reworded for house style: Removed "This record is backfilled from" and the source list; kept the dated history of the precache, the postponable prompt and the enforced update as plain context for the problem.
- Evidence: `ab0b559e` (2026-03-30); `27d0f7c1` (2026-04-04, "Add prompt-based service worker update flow", with a `Later` action); `edd85d43` (2026-09-22).
- old:
~~~
still comes from the server — but the interface itself can be an old build.
~~~
- new:
~~~

The shell-only precache has been in place since PWA support was added on 2026-03-30. The first update
flow, added on 2026-04-04, let the user postpone an update, which left the client on an old build for as
long as the user kept postponing it; the enforced update replaced it on 2026-09-22.
~~~

### E-0042-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: unsupported-history
- Why: The approach that shipped and was then dropped, a prompt the user could postpone, belongs in Considered Options (README: "record that in the Considered Options of the ADR that replaced it").
- Evidence: `git show edd85d43 -- Source/Client/pot-react/DEVELOPER.md` removes "Cancel: `Later`" and the snooze behaviour.
- old:
~~~
4. Cache only on first navigation, with no update check.
~~~
- new:
~~~
5. Precache the shell, and prompt for an update that the user may postpone.
~~~

### E-0042-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why:
  - The "two channels" are not the build id and the version manifest. Those are two halves of one channel. The other channel is the service worker's own update check, every 30 minutes.
  - API calls are not "excluded from the service worker". `navigateFallbackDenylist` affects navigations only. API traffic goes uncached because `runtimeCaching` is empty, and in production the API is on another origin.
  - "Served with no-cache" is true of nginx; the client also fetches with `no-store`.
- Evidence:
  - `vite.config.ts:72-77`.
  - `.vscode/tasks.json:108` (production `VITE_API_BASE_URL` is a separate API host).
  - `src/concerns/pwa/pwaUpdateChecks.ts:96-148`.
  - `src/concerns/pwa/pwaVersionCheck.ts:21-128`.
  - `src/concerns/pwa/pwaRuntime.ts:5,8,15,19`.
  - `Source/Docker/Client/nginx.conf:25-26` and `nginx.azure.conf:25-26`.
  - `src/concerns/pwa/pwaUpdatePrompt.ts:199-201` (`dismissible: false`).
- old:
~~~
Chosen option: 1. The build generates a precache manifest of the shell and hashed assets. Requests to the
API are excluded from the service worker, so data is always the server's answer. A new build is detected
through two channels: a build identifier baked into the bundle, and a version manifest served with
no-cache and polled at start-up, on focus and on a timer. When a newer build exists the user is told, and
the application applies it and reloads once the interface has been idle for a quiet period, or after a
maximum deferral if it never is.
~~~
- new:
~~~
Chosen option: 1. The build generates a precache manifest of the built files that `workbox.globPatterns`
matches (scripts, styles, HTML, images and fonts). The service worker has no runtime caching routes
(`runtimeCaching: []`), so it caches nothing it was not built with and every API call goes to the network;
`navigateFallbackDenylist` also keeps a navigation to `/api` from being answered with the cached
`index.html`. A new build is detected through two channels: the service worker's own update check, at
start-up, on focus and every 30 minutes, and a comparison of the build identifier baked into the bundle
with `/version.json`, which nginx serves with `no-cache` and the client fetches with `no-store` at
start-up, on focus and every 5 minutes. Once a new worker is waiting, the user sees a notice that cannot
be dismissed, and the application applies the update and reloads after 45 seconds without input, or after
5 minutes regardless.
~~~

### E-0042-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The notice cannot be dismissed at all, not merely "not indefinitely". The real cost is a lost form, and nothing tracks unsaved changes.
- Evidence: `src/concerns/pwa/pwaUpdatePrompt.ts:199-201`; `tests/concerns/pwa/pwaUpdatePrompt.test.ts:131-142`; `DEVELOPER.md` (Service Worker Registration and Update UX: "The app has no global unsaved-changes tracking").
- old:
~~~
- Bad, because the reload is not entirely the user's: the notice cannot be dismissed indefinitely, and the
  interface reloads on its own after the deferral limit, which can interrupt work.
~~~
- new:
~~~
- Bad, because the reload is not the user's: the notice cannot be dismissed, and the interface reloads on
  its own, which can discard a half-typed form; nothing tracks unsaved changes across the app.
~~~

### E-0042-07
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The precache manifest is generated from the build output on every build, so it cannot drift from the build. The real gap is file types that `globPatterns` does not list.
- Evidence: `vite.config.ts:73` (`globPatterns: ['**/*.{js,css,html,svg,png,ico,woff,woff2}']`); `vite-plugin-pwa` `generateSW` writes the manifest at build time.
- old:
~~~
- Bad, because the precache manifest is another artefact whose entries drift from the build.
~~~
- new:
~~~
- Bad, because the precache covers only the file types in `globPatterns`, so an asset of a new type is
  fetched from the network each time and is unavailable offline.
~~~

### E-0042-08
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why:
  - The Gap names the wrong mechanism. Nothing excludes the API; it is uncached because there are no runtime routes.
  - The Review rule contradicts the code, which reloads automatically.
  - The Known limitation should state the current skew honestly, without settling DD-14, which is open.
  - The test asserting the notice cannot be dismissed should be named.
- Evidence:
  - `vite.config.ts:74-76`.
  - `src/concerns/pwa/pwaUpdateEnforcement.ts:40-83`.
  - `tests/concerns/pwa/pwaUpdatePrompt.test.ts:131`.
  - `DECISIONS-TO-DISCUSS.md` DD-14: no client build id reaches the server, there is no compatibility policy, the skew lasts about 10 minutes, and there are separate deploy tasks at `.vscode/tasks.json:93-117`.
  - `DOCS-WORK-LISTS.md` row 0019.
- old:
~~~
- Gap: no fixture asserts that the service worker leaves API routes alone; that exclusion is configuration
  only.
- Review: a route added to the service worker for API traffic is a defect; a change to the update prompt
  keeps the reload explicit rather than automatic behind a form.
- Known limitation: an old shell can outlive a breaking API change until the user reloads, which is a
  compatibility window the client does not otherwise provide.
~~~
- new:
~~~
- Test: `tests/concerns/pwa/pwaUpdatePrompt.test.ts` asserts the notice cannot be dismissed.
- Gap: no fixture asserts that `runtimeCaching` stays empty or that `/api` stays in
  `navigateFallbackDenylist`; both are configuration in `vite.config.ts` only.
- Review: a `runtimeCaching` entry that matches API traffic is a defect; a change to the update flow keeps
  the update enforced — not dismissible, and applied by the maximum deferral.
- Known limitation: no client build identifier reaches the server and no compatibility policy covers a
  breaking API change, so an old shell keeps calling a changed API until its enforced update applies —
  roughly ten minutes after a client deploy for an open tab, plus any gap between the separate server and
  client deploy tasks (`.vscode/tasks.json`). Whether that skew is accepted or closed is not yet decided.
~~~

### E-0042-09
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: Without a service worker, repeat loads would still reuse the hashed bundle, because nginx marks `/assets/` as immutable. What would be lost is installability and loading when there is no network.
- Evidence: `Source/Docker/Client/nginx.conf:33-34` and `nginx.azure.conf:33-34` (`Cache-Control "public, max-age=31536000, immutable"`).
- old:
~~~
- Bad: every load downloads the bundle, and there is no installable app.
~~~
- new:
~~~
- Bad: there is no installable app, and the shell cannot load without the network; hashed assets would
  still come from the HTTP cache, which nginx marks `immutable` for `/assets/`.
~~~

### E-0042-10
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: unsupported-history
- Why: Pros and cons for the option added in E-0042-04, citing the evidence that it shipped and was replaced.
- Evidence: `27d0f7c1` (2026-04-04, `Later` cancel action); `edd85d43` (2026-09-22, "instead of allowing deferral"); `DEVELOPER.md` ("a stale client can never keep running indefinitely").
- old:
~~~
- Bad: the user is pinned to the build they first loaded, and the only signal is a bug report.
~~~
- new:
~~~

### Prompt the user may postpone

- Good: the user chooses when to reload, so no form is lost.
- Bad: a postponed update leaves the client on the old build for as long as the user keeps postponing it.
  This shipped on 2026-04-04, with a `Later` action that snoozed the prompt, and was replaced by
  enforcement on 2026-09-22 (commit `edd85d43`).
~~~

### Notes (no edit proposed)
- **DD-14 is open.** E-0042-08 records the current skew without choosing option A, B or C. If the owner
  picks A, the backlog row 0019 ADR should carry the full statement, and this record can link to it.
- **Guide drift.** `DEVELOPER.md:3122` promises a "backward-compatibility policy" that does not exist
  (DD-14). The ADR must not repeat it, and with E-0042-08 it does not.
- **Decision driver.** "Repeat loads should not download the whole bundle again" is also met by HTTP
  caching of hashed assets. The driver is not false, but the service worker's distinct value is
  installability and loading without a network. The owner may want the driver reworded. No edit is
  proposed because it is a matter of emphasis.
- **Unrecorded alternative.** `registerType: 'prompt'` was chosen over `autoUpdate` for a stated reason
  (`DEVELOPER.md`, "Why `prompt` matters"). This is a documented alternative the record could mention, but
  leaving it out does not mislead.
- **Verified.**
  - 30-minute service-worker check, 5-minute version check, 45-second quiet period and 5-minute maximum deferral: `src/concerns/pwa/pwaRuntime.ts`.
  - `/version.json` is fetched with `no-store`, and the test asserts it: `tests/concerns/pwa/pwaVersionCheck.test.ts:138`.
  - Enforcement fixtures: `tests/concerns/pwa/pwaUpdateEnforcement.test.ts:54-121`.
  - Tabs enforce independently: `pwaRuntimeState` is per tab.

---

## ADR-0043 — 0043-end-to-end-tests-run-on-one-shared-stack.md

Verdict: EDITS

### E-0043-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE
- Severity: consistency
- Why: `date` is when the decision was made. The shared stack is PRD-012 R1, approved and implemented by 2026-09-02,
  which is also ADR-0001's date. The first shared-stack config dates from `d5838735` (2026-05-11).
- Evidence: `Docs/Future/PRD-012-playwright-e2e-testing-infrastructure.md:4-5`, `:88-95`; ADR-0001 front matter.
- old:
~~~
date: 2026-11-04
~~~
- new:
~~~
date: 2026-09-02
~~~

### E-0043-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: RELATED-PRDS, TITLE-AREA
- Severity: consistency
- Why: W4 files this group as cross-cutting, and the stack spans the client, the API and the database. The decision comes
  from PRD-012.
- Evidence: `DOCS-WORK-LISTS.md:515`, `:519` (row 0070); PRD-012 R1, R2, R10.
- old:
~~~
area: frontend
~~~
- new:
~~~
area: cross-cutting
related-prds: [PRD-012]
~~~

### E-0043-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: STATUS-LINK
- Severity: consistency
- Why: This is the other half of E-0001-01 (W4: this group supersedes ADR-0001).
- Evidence: `DOCS-WORK-LISTS.md:515`, `:519` (row 0070, sourced from ADR-0001 §3–4, §8–9, App. A).
- old:
~~~
---

# ADR-0043:
~~~
- new:
~~~
supersedes: [ADR-0001]
---

# ADR-0043:
~~~

### E-0043-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: unsupported-history
- Why: "Starts in seconds" is not supported. Before the first test, setup starts two containers, runs migrations through
  `dotnet run`, seeds, polls readiness for up to about two minutes, imports, renews and warms seven routes in a browser.
  The API web server is allowed 180 s to start. PRD-012 claims only "predictable startup".
- Evidence: `playwright.globalSetup.ts:450-592`, `:293`; `playwright.config.ts:108`; PRD-012 R1 rationale (line 94).
- old:
~~~
a suite that starts in seconds and exercises the real pipeline
~~~
- new:
~~~
a suite that pays its setup cost once per run and exercises the real pipeline
~~~

### E-0043-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The setup order is misstated: the baseline SQL runs before the readiness wait, and only the import comes after it.
  The seed inserts role assignments (`UserRole`), not roles. "One on CI" reads as if a CI run exists, but none does; it
  is the `CI` environment variable.
- Evidence: `playwright.globalSetup.ts:456`, `:516-531`, `:574-579`; `e2e/seed/baseline.sql:36`, `:46`, `:58` (only
  `Site`, `User`, `UserRole`); `playwright.config.ts:31`; ADR-0035; DD-05 ("There is no `.github/workflows/` directory").
- old:
~~~
Chosen option: 1. The Playwright configuration starts the API and the client as web servers, and global
setup starts the PostgreSQL container and the mail sink, waits for the application to report readiness and
applies the baseline: canonical sites, users and roles inserted as SQL, with the financial data loaded
through the product's own import and renewal routes. The run then shares that stack, in two workers
locally and one on CI, and tests are written to isolate themselves within it by owning the data they
create.
~~~
- new:
~~~
Chosen option: 1. The Playwright configuration starts the API and the client as web servers. Global setup
then starts the mail sink and the PostgreSQL container, runs the migrations, inserts the canonical sites,
users and role assignments as SQL (`e2e/seed/baseline.sql`), and waits for the API to report readiness on
`/_health/ready`. Only then does it load the financial data through the product's own import and renewal
routes. The run shares that stack, in two workers locally or one when the `CI` variable is set (no pipeline
sets it; see [ADR-0035](0035-builds-and-deployments-are-run-by-hand.md)). Tests are written to isolate
themselves within it by owning the data they create.
~~~

### E-0043-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: The first bullet is labelled "Test" but describes setup behaviour without naming it, and nothing listed
  enforces the shared stack itself. The readiness gate, the configs and the instruction file are the actual mechanisms.
- Evidence: `playwright.globalSetup.ts:288-326` (`waitForDatabaseReady`, 60 attempts, then throws); `playwright.config.ts:47`,
  `:106`, `:121`; `playwright.prod.config.ts:19`, `:53`, `:62`; `.github/instructions/e2e.instructions.md:38`.
- old:
~~~
- Test: global setup fails the run rather than letting tests start against an unready API, so a broken stack
  is one clear failure rather than a hundred misleading ones.
~~~
- new:
~~~
- Setup: `waitForDatabaseReady` in `playwright.globalSetup.ts` polls `/_health/ready` at most 60 times and then
  throws, and every other setup step throws on failure. A broken stack fails the run once instead of letting
  tests start against it.
- Config: `playwright.config.ts` and `playwright.prod.config.ts` each declare one `globalSetup`, one API and one
  client `webServer`, both with `reuseExistingServer: false`, so a run owns exactly one stack.
- Instruction: `.github/instructions/e2e.instructions.md` (Isolation classification: all workers share one API
  process on `5242` and one Testcontainers database on `55432`).
~~~

### E-0043-07
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why: The neighbouring decisions are named but not linked, and the record cites no source.
- Evidence: README "Related ADRs can be grouped as a decision family"; PRD-012 R1, R2, R10.
- old:
~~~
- How a test declares that it can run beside others is a separate decision.
- The execution policy — which configurations run, and when — is separate again.
~~~
- new:
~~~
- How a test is classified for running beside others:
  [ADR-0044](0044-every-end-to-end-test-declares-whether-it-may-run-beside-others.md).
- Which configurations run, and when:
  [ADR-0045](0045-the-end-to-end-matrix-runs-on-every-change-with-a-stricter-release-profile.md).
- How destructive journeys get a tenant of their own:
  [ADR-0046](0046-tenants-are-provisioned-and-destroyed-by-the-products-own-routes-in-tests.md).
- Source: [PRD-012](../Future/PRD-012-playwright-e2e-testing-infrastructure.md) R1, R2 and R10. The earlier combined
  design record is [ADR-0001](0001-playwright-e2e-testing-infrastructure.md).
~~~

### Notes (no edit proposed)
- Options 2–4 (a stack per file, a mocked API, snapshot restore) are listed as options rather than described as weighed
  at the time, which is acceptable. PRD-012 NG3 records only that per-test containers were ruled out; the record cannot
  show whether a mocked API or snapshot restore was ever considered.
- If the owner renames ADR-0044 or ADR-0045 (see their notes), the links in E-0043-07 must follow the new file names.

---

---

## ADR-0044 — 0044-every-end-to-end-test-declares-whether-it-may-run-beside-others.md

Verdict: MAJOR

The decision, as stated in the title, the summary and the outcome, is that the classification "is stated at the top of
the file". No instruction, guide or PRD says this. `e2e.instructions.md:33` says "Classify every test before writing it",
and AUTHORING says "Pick the test class FIRST": the class determines the file's structure, not a header. In the code, 22
of the 46 spec files name a class in a comment or a suite title, often not at the top (`modalDialogs.test.ts:50`); none
names itself destructive; and both deletion journeys, `siteDetails`, `userSettings` and `approvals` write without a label.
W4 row 0071 states the decision as "Every E2E test is classified …". There are also smaller errors: the destructive class
is not "whole tenant only", and `fullyParallel` runs tests within a file in parallel, not only whole files.

### E-0044-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: TITLE-AREA
- Severity: factual
- Why: The title states a practice that is neither required nor followed. W4's statement of the decision is
  "classified".
- Evidence: `DOCS-WORK-LISTS.md:520`; `.github/instructions/e2e.instructions.md:31-38`; `e2e/AUTHORING.md:16-27`; label
  count (grep for `parallel-safe|fixture-managed|destructive` across `e2e/tests/**/*.test.ts`).
- old:
~~~
title: Every end-to-end test declares whether it may run beside others
~~~
- new:
~~~
title: Every end-to-end test is classified by whether it may run beside others
~~~

### E-0044-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE
- Severity: consistency
- Why: The decision date is PRD-012 R6 (2026-09-02). PRD-031 later added the destructive class.
- Evidence: PRD-012 lines 4–5, 143–153; PRD-031 F1–F3; commit `5d9d9fad` (2026-10-29).
- old:
~~~
date: 2026-11-04
~~~
- new:
~~~
date: 2026-09-02
~~~

### E-0044-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: RELATED-PRDS, TITLE-AREA
- Severity: consistency
- Why: W4 files the group as cross-cutting. The two-class model comes from PRD-012 and the third class from PRD-031.
- Evidence: `DOCS-WORK-LISTS.md:515`, `:520-522`.
- old:
~~~
area: frontend
~~~
- new:
~~~
area: cross-cutting
related-prds: [PRD-012, PRD-031]
~~~

### E-0044-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: STATUS-LINK
- Severity: consistency
- Why: This is the other half of E-0001-01. Row 0071 is sourced from ADR-0001 §7.
- Evidence: `DOCS-WORK-LISTS.md:520`.
- old:
~~~
---

# ADR-0044:
~~~
- new:
~~~
supersedes: [ADR-0001]
---

# ADR-0044:
~~~

### E-0044-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: TITLE-AREA
- Severity: factual
- Why: The H1 must match the title (E-0044-01).
- Evidence: template ("title, matching the front matter").
- old:
~~~
# ADR-0044: Every end-to-end test declares whether it may run beside others
~~~
- new:
~~~
# ADR-0044: Every end-to-end test is classified by whether it may run beside others
~~~

### E-0044-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The summary states the unsupported "stated at the top of the file" rule. It defines parallel-safe differently from
  the instruction file, and it limits the destructive class to tenants when the class covers identities too.
- Evidence: `.github/instructions/e2e.instructions.md:35-37`; `e2e/tests/settings/accountDeletion.test.ts:10-13`
  (destroys an invited identity inside the shared site).
- old:
~~~
serialising everything and letting tests run in parallel, we decided that a suite is written as
parallel-safe — reading only data it did not create — or as fixture-managed — owning the records it
creates and removing them — or, where it must destroy a tenant, as destructive and provisioning its own,
and that the classification is stated at the top of the file, to achieve parallelism where it is safe
without hiding the tests that cannot have it, accepting that the classification is the author's assertion
rather than something the runner enforces.
~~~
- new:
~~~
serialising everything and letting tests run in parallel, we decided that every test is classified before
it is written. A parallel-safe test never calls a state-changing endpoint or asserts on shared mutable
state. A fixture-managed suite runs serially and owns the records it creates and removes them. A test that
destroys the identity or tenant it runs as is destructive and provisions that tenant itself. The class then
shapes how the file is written. The aim is parallelism where it is safe without hiding the tests that cannot
have it, accepting that the classification is the author's judgement rather than something the runner checks.
~~~

### E-0044-07
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The suite is not a gate on every change. It is run deliberately, and the skills make it opt-in.
- Evidence: `.github/skills/run-tests/SKILL.md:12` ("never as routine verification"), `:54` ("Opt-in");
  `.github/skills/prd-delivery/SKILL.md:64`; `playwright.config.ts:29-30` ("the local matrix takes ~6-7 min").
- old:
~~~
Parallelism is worth having: the suite is the slowest gate in a change. It is only safe if two tests
running together cannot observe each other.
~~~
- new:
~~~
Parallelism is worth having: the full matrix takes several minutes even on two workers (`playwright.config.ts`
records about six to seven), so it is run deliberately rather than on every change. It is only safe if two tests
running together cannot observe each other.
~~~

### E-0044-08
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The destructive class covers identities as well as tenants. Account deletion shares the seeded site and runs in
  every project, so "cannot share anything at all" is false.
- Evidence: `.github/instructions/e2e.instructions.md:37`; `playwright.config.ts:155-157`; `e2e/helpers/provisioning.ts:94-99`.
- old:
~~~
beside anything; the second must not collide with another test doing the same thing to the same records. A
third exists for the journeys that destroy a whole tenant, which cannot share anything at all.
~~~
- new:
~~~
beside anything; the second must not collide with another test doing the same thing to the same records. A
third exists for the journeys that destroy the identity or tenant they run as (account deletion, site
deletion). They cannot use a seeded identity, which can be destroyed only once per database, so they
provision their own.
~~~

### E-0044-09
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why: Option 1 omits the destructive class that the outcome then describes.
- Evidence: ADR-0044 line 141; `e2e/AUTHORING.md:22`.
- old:
~~~
1. Classify each test as parallel-safe or fixture-managed, declared in the test.
~~~
- new:
~~~
1. Classify each test as parallel-safe, fixture-managed or destructive, and write it to its class.
~~~

### E-0044-10
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: This replaces the "stated at the top of the file" rule with what the instructions and code actually do, and gives
  a measured account of how unevenly labels are used. It also corrects the Playwright behaviour: with `fullyParallel:
  true`, tests within a file run in parallel unless the suite is serial, which is why fixture-managed suites use
  `describe.serial`.
- Evidence: `playwright.config.ts:6`; `.github/instructions/e2e.instructions.md:33-37`; label counts across the 46 spec
  files (12 name parallel-safe, one of them `settings/siteDeletion.test.ts:21` for a single test; 10 name fixture-managed;
  none names destructive); `e2e/tests/settings/siteDetails.test.ts:7-9`.
- old:
~~~
Chosen option: 1. A fixture-managed suite creates its records with unique values, operates only on them,
and removes them, so it can run beside another of its kind; a parallel-safe one reads only seed data; a
destructive one provisions its own tenant through the product's routes and destroys that. The
classification is stated at the top of the file, so a reviewer sees it and a new file must choose.

It is a statement rather than a declaration the runner can check, and the current suite does not apply it
consistently — some suites mutate the shared site without a label. Playwright runs files in parallel and
the label is what tells a reader which ones may.
~~~
- new:
~~~
Chosen option: 1. A fixture-managed suite is a `test.describe.serial` block. It creates its records with
unique values, operates only on them and removes them in `afterEach`, so it can run beside another of its
kind. A parallel-safe test calls no state-changing endpoint. A destructive one provisions its own identity
or tenant through `e2e/helpers/provisioning.ts` and destroys that. The class is chosen before the test is
written (`.github/instructions/e2e.instructions.md`, `e2e/AUTHORING.md`). It shows in the file's shape: the
serial block with its `(fixture-managed)` title, or the provisioning call.

Nothing requires the class to be written down, and the suite is uneven. Twenty-two of the 46 spec files name
a class in a comment or suite title, and none names itself destructive. `settings/siteDetails.test.ts` edits
the shared site's description, and restores it, without a label. `fullyParallel: true` runs every test in
parallel, within a file as well as across files, unless the suite is declared serial.
~~~

### E-0044-11
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: "Says so" is true for fewer than half the files. What every writing file does show is its structure.
- Evidence: as E-0044-10.
- old:
~~~
- Good, because a test that writes says so, and its isolation is visible in what it creates.
~~~
- new:
~~~
- Good, because a test that writes shows it in a serial block, its cleanup or a provisioning call, and its
  isolation is visible in what it creates.
~~~

### E-0044-12
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: Every run starts from a fresh container, so data piles up only within a run. Two suites also keep their rows by
  design, not only after a crash.
- Evidence: `playwright.globalSetup.ts:475-531` (new container each run); `e2e/tests/users/userManagement.test.ts:12-14`;
  `e2e/tests/approvals/approvals.test.ts:17-23`.
- old:
~~~
- Bad, because a fixture-managed test that removes its data in a teardown can leave it behind if the test
  crashes the worker, so the shared database accumulates.
~~~
- new:
~~~
- Bad, because a fixture-managed test that removes its data in a teardown can leave it behind if the worker
  crashes, and some suites keep their rows by design (`users/userManagement.test.ts`,
  `approvals/approvals.test.ts`). The database therefore accumulates within a run. Each run starts from a new
  container, so nothing carries over to the next one.
- Bad, because a test that must change a seeded identity fits none of the three classes.
  `settings/userSettings.test.ts` changes `e2e_pwchange`, and `dashboard/quickActions.test.ts` sweeps its own
  seeded site. Both are kept safe only by running on the `chromium` project alone, through `testIgnore` in the
  configuration, which is a manifest outside the test.
~~~

### E-0044-13
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: No instruction requires a header statement. The instruction file and AUTHORING are what define the classes and
  their required structure.
- Evidence: `.github/instructions/e2e.instructions.md:31-38`; `e2e/AUTHORING.md:16-27`.
- old:
~~~
- Review: every new file states its classification in its header, and a suite that writes anything it did
  not create is not parallel-safe.
~~~
- new:
~~~
- Instruction: `.github/instructions/e2e.instructions.md` (Isolation classification) and `e2e/AUTHORING.md`
  ("Pick the test class FIRST") define the three classes and the structure each one requires.
- Review: a new file's class is evident from its structure or a comment, and a suite that writes anything it
  did not create is not parallel-safe.
~~~

### E-0044-14
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why: The heading and its bullets repeat the "declared" framing that E-0044-01 corrects.
- Evidence: as E-0044-01.
- old:
~~~
### Declared classification (chosen)

- Good: parallelism where safe, honesty where not.
- Bad: an unverified claim.
~~~
- new:
~~~
### Classification by the author (chosen)

- Good: parallelism where it is safe, and the tests that cannot have it are visible in their structure.
- Bad: an unverified judgement, unevenly written down.
~~~

### E-0044-15
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why: Neighbours are named but not linked; sources not cited.
- Evidence: PRD-012 R6; PRD-031 F1–F3.
- old:
~~~
- The shared stack this depends on is a separate decision.
- The classes of test that provision their own tenant are part of the scenario work and are separate again.
~~~
- new:
~~~
- The shared stack this depends on: [ADR-0043](0043-end-to-end-tests-run-on-one-shared-stack.md).
- How the destructive class provisions its own tenant:
  [ADR-0046](0046-tenants-are-provisioned-and-destroyed-by-the-products-own-routes-in-tests.md).
- Sources: [PRD-012](../Future/PRD-012-playwright-e2e-testing-infrastructure.md) R6 (two classes) and
  [PRD-031](../Future/PRD-031-automated-tenant-scenario-testing.md) F1–F3 (the destructive class).
~~~

### Notes (no edit proposed)
- **File name.** After E-0044-01 the slug still says "declares". docs-check does not compare the slug with the title. A
  rename touches only the generated README index and the links proposed in E-0043-07 and E-0045-10. Owner's choice.
- **Whether to record the convention after all.** If the owner does want every file to state its class in a header, that
  is a new rule. It belongs in `e2e.instructions.md` and AUTHORING first, and 24 files would need a label. It should not
  be backfilled into this record as if it were already decided.
- **Stale comment.** `approvals/approvals.test.ts:17-23` still says "there is no delete-user API", but `DELETE
  /api/users/{id}` exists (`Pot.AspNetCore/Features/Users/Extensions/RouteGroupBuilderExtensions.cs:94`, used by
  `users/userDeletion.test.ts`).

---

---

## ADR-0045 — 0045-the-end-to-end-matrix-runs-on-every-change-with-a-stricter-release-profile.md

Verdict: MAJOR

The title says the matrix "runs on every change". The skills say the opposite: E2E is opt-in and "never as routine
verification", and there is no CI. "Before release" names a trigger that does not exist (DD-11: "There are no formal
releases"). The outcome presents DD-11 option A (completion gate for client-affecting work) as decided, while the README,
PRD-012 R8 and ADR-0001 say on demand. DD-11 is still open ("Your call" is blank). Further errors: per-project exclusions
are described as engine-specific, but none is; and the prodlike profile is said to run "fewer projects", when it runs the
same four.

### E-0045-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: TITLE-AREA
- Severity: factual
- Why: The title contradicts the skills and records an undecided trigger as decided.
- Evidence: `.github/skills/run-tests/SKILL.md:12`, `:54`, `:69`, `:94`; `.github/skills/prd-delivery/SKILL.md:15`, `:64`;
  ADR-0035; `DECISIONS-TO-DISCUSS.md:318-336`.
- old:
~~~
title: The end-to-end matrix runs on every change, with a stricter profile before release
~~~
- new:
~~~
title: End-to-end tests run as a four-project development matrix, with a separately run production-build profile
~~~

### E-0045-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE
- Severity: consistency
- Why: The two-profile decision is PRD-012 R7 and R8, approved 2026-09-02. The prodlike config was "validated
  2026-09-01".
- Evidence: PRD-012 lines 4–5, 155–174; ADR-0001 §3.2 heading; `e2e/README.md:142`.
- old:
~~~
date: 2026-11-04
~~~
- new:
~~~
date: 2026-09-02
~~~

### E-0045-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: RELATED-PRDS, TITLE-AREA
- Severity: consistency
- Why: W4 files the group as cross-cutting; the source is PRD-012.
- Evidence: `DOCS-WORK-LISTS.md:515`, `:521`.
- old:
~~~
area: frontend
~~~
- new:
~~~
area: cross-cutting
related-prds: [PRD-012]
~~~

### E-0045-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: STATUS-LINK
- Severity: consistency
- Why: This is the other half of E-0001-01. Row 0072 is sourced from ADR-0001 §10 and §13.
- Evidence: `DOCS-WORK-LISTS.md:521`.
- old:
~~~
---

# ADR-0045:
~~~
- new:
~~~
supersedes: [ADR-0001]
---

# ADR-0045:
~~~

### E-0045-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: TITLE-AREA
- Severity: factual
- Why: The H1 must match the title (E-0045-01).
- Evidence: template.
- old:
~~~
# ADR-0045: The end-to-end matrix runs on every change, with a stricter profile before release
~~~
- new:
~~~
# ADR-0045: End-to-end tests run as a four-project development matrix, with a separately run production-build profile
~~~

### E-0045-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The cost the record accepts is misstated. The skills treat prodlike as a gate, and what is actually accepted is
  that nothing runs automatically and the trigger is unsettled.
- Evidence: `.github/skills/prd-delivery/SKILL.md:84` ("the production-build gate"); DD-11.
- old:
~~~
confidence without paying the strictest cost every time, accepting that the stricter profile is a discipline
rather than a gate.
~~~
- new:
~~~
confidence without paying the strictest cost every time, accepting that neither profile runs automatically
and that when the production-build profile must run is not yet settled.
~~~

### E-0045-07
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why: Option 1 says "before a release" while the outcome says "completion gate". It should state the chosen shape
  without settling DD-11.
- Evidence: ADR-0045 lines 36 and 47–48; DD-11.
- old:
~~~
1. A development matrix by default plus a production-like profile on demand and before a release.
~~~
- new:
~~~
1. A development matrix by default plus a production-like profile that is run separately.
~~~

### E-0045-08
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: (a) None of the per-project exclusions is engine-specific. Two files mutate shared data and run on chromium only,
  and two are desktop-layout files kept off the phones. (b) The completion-gate rule is DD-11 option A, presented here as
  decided; the record must report both rules and leave the choice open. (c) The dev matrix is itself opt-in, not run on
  every change.
- Evidence: `playwright.config.ts:142-202` (userSettings and quickActions off Edge and both phones; filters and
  shortLandscapeScroll off both phones; every comment gives shared state or layout as the reason); `e2e.instructions.md:13`;
  `prd-delivery/SKILL.md:64`, `:84`; `run-tests/SKILL.md:54`, `:57-58`, `:69`; `e2e/README.md:150-153`; PRD-012 R8
  ("on-demand prodlike gate"); `DECISIONS-TO-DISCUSS.md:318-336`.
- old:
~~~
Chosen option: 1. The default profile runs the suite across four projects — desktop Chromium, Edge, a mobile
Chromium device and a mobile WebKit device — against the API and the development client, with fixed
per-test timeouts and reporters configured in the project rather than on the command line, and with a few
suites excluded per project where they are known to be engine-specific. The production-like profile builds
the client, serves it from the local preview server and runs the suite against it; it is the completion gate
for a client-affecting change and is not run per change.
~~~
- new:
~~~
Chosen option: 1. The default profile (`playwright.config.ts`) runs the suite across four projects: desktop
Chromium, Edge, a Pixel 7 (Chromium) and an iPhone 14 (WebKit). It runs against the API and the Vite
development server, with a fixed 60-second per-test timeout and reporters configured in the file rather than
on the command line. Some files are kept off some projects, none for engine reasons. Two that change shared
data run on `chromium` only (`settings/userSettings.test.ts`, `dashboard/quickActions.test.ts`), and two
desktop-layout files skip the phone projects. The default profile is itself opt-in: the delivery skill asks
before running it, and `run-tests` never runs it as routine verification. The production-like profile
(`playwright.prod.config.ts`) builds the client, serves it from the local preview server and runs the same four
projects against it. Neither profile is run on every change. The documents give two rules for when the
production-like profile runs. The delivery skills and `e2e.instructions.md` make the full matrix the completion
gate for a PRD that changed client product code. `e2e/README.md`, PRD-012 R8 and ADR-0001 call it on demand,
when the production build changes and before releases. That disagreement is open as DD-11, and this record
does not settle it.
~~~

### E-0045-09
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: Prodlike runs the same four projects. Its exclusions are also not a subset of the dev ones: it drops
  `errorBoundary` everywhere, but it runs `quickActions` and `shortLandscapeScroll` on projects where the dev config
  excludes them.
- Evidence: `playwright.prod.config.ts:73-123` against `playwright.config.ts:135-203`.
- old:
~~~
- Bad, because the production-like profile runs fewer projects and a subset of the suites, so it is a
  different slice of the suite rather than the same one against a different build.
~~~
- new:
~~~
- Bad, because the two configurations exclude different files. The production-like one drops
  `feedback/errorBoundary.test.ts` on every project, but it does not repeat the default's exclusions of
  `dashboard/quickActions.test.ts` (Edge and both phones) or `rendering/shortLandscapeScroll.test.ts` (both
  phones). The two runs are therefore different slices of the suite, and nothing checks that the lists agree.
~~~

### E-0045-10
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: The skill step and the instruction lines are the mechanisms that apply this decision. The current timeout bullet
  partly contradicts the instruction, which says not to raise the timeout to chase flakes.
- Evidence: `prd-delivery/SKILL.md:84`, `:95`; `prd-delivery/references/stage-plan.template.md:46`; `run-tests/SKILL.md:69`,
  `:93`; `e2e.instructions.md:43`, `:49`.
- old:
~~~
- Review: a client-affecting change states that the production-like profile was run, or why not.
- Review: a test that needs a longer timeout changes the project's configuration deliberately rather than
  passing a flag.
~~~
- new:
~~~
- Instruction: `.github/skills/prd-delivery/SKILL.md` (step 4.6) runs `npm run e2e:all:prodlike` for a PRD that
  changed client product code, and the report must say so when it is skipped. `.github/skills/run-tests/SKILL.md`
  says not to run the prodlike suite per change.
- Instruction: `.github/instructions/e2e.instructions.md` (Fixtures and config; Reporting and process lifecycle).
  Do not raise `timeout` to chase flakes, and never pass `--reporter` from an npm script.
- Review: a change to a timeout is made in the configuration and justified there, never passed as a flag.
~~~

### E-0045-11
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why: Neighbours are named but not linked. "A separate decision" for CI is ADR-0035.
- Evidence: ADR-0035.
- old:
~~~
- The shared stack and the test classification are separate decisions.
- Continuous integration does not currently run the suite; that is a separate decision.
~~~
- new:
~~~
- The shared stack and the test classification: [ADR-0043](0043-end-to-end-tests-run-on-one-shared-stack.md) and
  [ADR-0044](0044-every-end-to-end-test-declares-whether-it-may-run-beside-others.md).
- There is no continuous integration, so nothing runs either profile automatically:
  [ADR-0035](0035-builds-and-deployments-are-run-by-hand.md).
- Source: [PRD-012](../Future/PRD-012-playwright-e2e-testing-infrastructure.md) R7 and R8.
~~~

### Notes (no edit proposed)
- **DD-11 is open.** When the owner decides it, this record needs a dated clarification, or a superseding record if the
  decision statement changes. Option A matches the skills and needs the README and PRD-012 R8 updated. Option B needs
  `e2e.instructions.md:13`, prd-delivery and run-tests changed.
- **File name.** The slug `runs-on-every-change-with-a-stricter-release-profile` stays false after E-0045-01. A rename is
  recommended; update the links in E-0043-07 and E-0044-15 if it happens.
- **Context line 24** ("would double the slowest gate") is reasoning, not history, but it also treats the suite as a gate
  on every change. I left it, because the outcome now says otherwise.
- **Prodlike drift.** See the ADR-0001 notes: the missing `testIgnore` entries and the missing `expect.timeout` are code
  facts the owner may want fixed in the config rather than recorded as a consequence.

---

---

## ADR-0046 — 0046-tenants-are-provisioned-and-destroyed-by-the-products-own-routes-in-tests.md

Verdict: EDITS

### E-0046-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: DATE
- Severity: consistency
- Why: The decision is PRD-031's. Its "Decisions Carried Into This Document" were committed on 2026-10-27, and the PRD was
  Complete on 2026-11-02.
- Evidence: `git log --diff-filter=A` on PRD-031 → `73416f13` 2026-10-27; PRD-031 lines 3–5, 186–200;
  `DOCS-WORK-LISTS.md:522` (row 0073).
- old:
~~~
date: 2026-11-04
~~~
- new:
~~~
date: 2026-10-27
~~~

### E-0046-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: RELATED-PRDS, TITLE-AREA
- Severity: consistency
- Why: The decision spans the browser suite and the server integration suite. W4 files it as cross-cutting, and the
  source PRD is not recorded.
- Evidence: `DOCS-WORK-LISTS.md:515`, `:522`.
- old:
~~~
area: frontend
~~~
- new:
~~~
area: cross-cutting
related-prds: [PRD-031]
~~~

### E-0046-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: STATUS-LINK
- Severity: consistency
- Why: This is the other half of E-0001-01. It reverses ADR-0001's "OTP / email flows deferred indefinitely" and the
  constraint against creating data through the API in setup.
- Evidence: ADR-0001 §1.3, §2, Appendix A item 8; `DOCS-WORK-LISTS.md:522` ("amends … 0070 and 0071").
- old:
~~~
---

# ADR-0046:
~~~
- new:
~~~
supersedes: [ADR-0001]
---

# ADR-0046:
~~~

### E-0046-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The site-deletion journey reads the sign-up message (its one-time code and temporary password), not an
  invitation.
- Evidence: `e2e/helpers/provisioning.ts:188-222`.
- old:
~~~
invitation mail from a mail sink, and assert that the seeded identities survive
~~~
- new:
~~~
sign-up or invitation mail from a mail sink, and assert that the seeded identities survive
~~~

### E-0046-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: omission
- Why: The Context leaves out the force PRD-031 and the instruction file both name: a seeded identity can be destroyed
  only once, so a destructive spec that consumes one cannot repeat, or run in a second project. That is what the
  "every project, twice in a row" rule rests on.
- Evidence: PRD-031 Problem Statement (line 18) and F1–F3; `.github/instructions/e2e.instructions.md:37`;
  `e2e/AUTHORING.md:24-27`.
- old:
~~~
The provisioning a real tenant needs — sign-up, approval, an invitation, an accepted password — is
several steps through the product, one of which is an email.
~~~
- new:
~~~

A seeded identity can also be destroyed only once per database. A destructive spec that consumes one cannot
run in a second browser project, or a second time against the same seed
([PRD-031](../Future/PRD-031-automated-tenant-scenario-testing.md), Problem Statement and F1–F3).
~~~

### E-0046-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: (a) The sign-up message is read as well as the invitation. (b) The E2E check is narrower than "the seeded
  identities": two usernames. (c) The integration suite reads messages through `CapturingEmailWriter`, which replaces
  the channel writer. That is the mechanism the record rejects as option 4, while calling the integration suite "the
  stronger form", and the record should say so. (d) The invariants cover three operations, not every destructive one.
- Evidence: `e2e/helpers/mailSink.ts:7-16`; `playwright.globalSetup.ts:456`; `e2e/tests/settings/siteDeletion.test.ts:120-127`
  and `accountDeletion.test.ts:73-80` (`e2e_admin`, `e2e_viewer` only);
  `Pot.AspNetCore.Integration.Tests/Host/CapturingEmailWriter.cs:6-23`; `aspnetcore-integration-tests.instructions.md:20`;
  `Invariants/IsolationFixture.cs:29`, `:67`, `:105` (the only three facts: `DELETE /api/users/{id}`, `DELETE /api/me`,
  `DELETE /api/sites/current`).
- old:
~~~
Chosen option: 1. A destructive journey provisions what it will destroy by calling the product's routes:
the site-deletion journey creates a site of its own, and account deletion invites a user into the shared
site. Invitation mail is read from a mail-development container the run starts and points the API at, so the
temporary password is a real one. After the journey the seeded identities are asserted to still exist, so
"only this tenant was touched" is a check rather than an assumption.

The server-side integration suite has the stronger form of the same idea: a capturing email writer inside
the host, and a fingerprint of every row of every tenant-scoped table compared before and after.
~~~
- new:
~~~
Chosen option: 1. A destructive journey provisions what it will destroy by calling the product's routes
(`e2e/helpers/provisioning.ts`). The site-deletion journey signs up and is approved, so it owns a site of its
own; account deletion invites a viewer into the shared site. The sign-up's code and password, and the
invitation's password, are read from a maildev container (`pot-e2e-mail-sink`). Global setup starts it on the
SMTP port the API is configured with, so each credential is the one the application generated. After the
journey, two seeded usernames (`e2e_admin`, `e2e_viewer`) are asserted to still exist, so "the seed survived"
is a check rather than an assumption.

The server-side integration suite applies the same idea more strongly. Its scenarios are provisioned through
the routes, and `Invariants/IsolationFixture.cs` fingerprints every tenant-scoped table of a canary tenant and
of the target tenant before and after three operations: removing a member, removing one's own account and
removing a site. It reads messages differently. The test host replaces the email channel writer with
`CapturingEmailWriter`, so a message is read as the model the application submitted, not as mail that was
rendered and sent. That is the mechanism option 4 describes.
~~~

### E-0046-07
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The canary check covers three identity- and tenant-removing operations. Account, expense and income deletes are
  not under a canary.
- Evidence: `Invariants/IsolationFixture.cs` (three `[Fact]`s); `Pot.AspNetCore/Features/{Accounts,Expenses,Incomes}/Extensions/RouteGroupBuilderExtensions.cs`
  (`MapDelete`).
- old:
~~~
- Good, because the canary tenant turns confinement into a check on every destructive action.
~~~
- new:
~~~
- Good, because in the integration suite the canary tenant turns confinement into a check on each action that
  removes a person or a tenant.
~~~

### E-0046-08
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The sink starts first in global setup and throws if it cannot start or its ports are taken, so the whole run
  depends on it, not only the destructive journeys.
- Evidence: `playwright.globalSetup.ts:454-456`, `:588-591`; `e2e/helpers/mailSink.ts:135-164`.
- old:
~~~
- Bad, because the run depends on a mail container, so the destructive journeys cannot run without it.
~~~
- new:
~~~
- Bad, because global setup starts the mail container before anything else and fails the run if it cannot.
  No end-to-end test runs unless Docker can start it and ports `1025` and `1080` are free, and that applies to
  every test, not only the destructive journeys.
~~~

### E-0046-09
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: Name the fixture and the instruction files that actually enforce this. "Each destructive operation" overstates
  what the fixture covers.
- Evidence: `Invariants/IsolationFixture.cs`; `run-tests/SKILL.md:74-76` (invariants are in the always-on tier);
  `e2e/tests/smoke/mailSink.test.ts`; `e2e.instructions.md:37`; `aspnetcore-integration-tests.instructions.md:27-60`.
- old:
~~~
- Test: the integration isolation invariants compare a full tenant fingerprint before and after each
  destructive operation.
~~~
- new:
~~~
- Test: `Pot.AspNetCore.Integration.Tests/Invariants/IsolationFixture.cs` compares the canary's and the target's
  fingerprints before and after member removal, own-account removal and site removal. It runs in the always-on
  integration suite, not an opt-in tier.
- Test: `e2e/tests/smoke/mailSink.test.ts` reads a sign-up code from the sink.
  `e2e/tests/settings/accountDeletion.test.ts` and `siteDeletion.test.ts` assert that the seeded usernames survive.
- Instruction: `.github/instructions/e2e.instructions.md` (Isolation classification: Destructive) and
  `.github/instructions/aspnetcore-integration-tests.instructions.md` (Provisioning a scenario; Isolation invariants).
~~~

### E-0046-10
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: consistency
- Why: Neighbours are named but not linked, and the source PRD is not cited.
- Evidence: ADR-0031; PRD-031.
- old:
~~~
- The shared stack and the parallel classification are separate decisions.
- Queued in-process email delivery is a separate decision; the sink is what makes that queue observable to
  a test.
~~~
- new:
~~~
- The shared stack and the parallel classification: [ADR-0043](0043-end-to-end-tests-run-on-one-shared-stack.md)
  and [ADR-0044](0044-every-end-to-end-test-declares-whether-it-may-run-beside-others.md).
- Queued in-process email delivery:
  [ADR-0031](0031-email-is-queued-in-process-and-rendered-from-razor-components.md). The sink is what makes that
  queue observable to a browser test.
- Source: [PRD-031](../Future/PRD-031-automated-tenant-scenario-testing.md) (A, B, D and F requirements).
~~~

### Notes (no edit proposed)
- **PRD-031 D1** says "for every destructive action, a canary tenant's fingerprint is unchanged", but `IsolationFixture`
  covers only the three identity- and tenant-removing routes. Owner to decide whether the PRD's wording or the coverage
  is the intended state. The record now states the coverage as built.
- The record's "no test-only server endpoints" position is supported: PRD-031 A5, and the integration instruction says
  `MessageCaptureFixture` asserts the shipped assembly offers exactly one writer.
- Option 4's "Bad" line is kept, because E-0046-06 now says plainly that the server tier uses that mechanism. The owner
  may prefer to reword option 4 as "rejected for the browser tier only".

---

## ADR-0047 — 0047-a-rate-limited-caller-is-told-when-to-retry.md

Verdict: EDITS

Supersession check: `supersedes: [ADR-0002]` matches 0002's `superseded-by: [ADR-0047]`.

### E-0047-01
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: front-matter
- Tags: RELATED-PRDS
- Severity: consistency
- Why: The decision comes from PRD-014, as 0002 did. The skill says to set `related-prds` when a decision came from a PRD. PRD-014 is still a draft, which matches the `proposed` status.
- Evidence: ADR-0002 front matter `related-prds: [PRD-014]`; `Docs/Future/PRD-014-retry-after-client-cooldown-handling.md:3`; `.github/skills/adr/SKILL.md` step 3.1.
- old:
~~~
area: frontend
supersedes: [ADR-0002]
~~~
- new:
~~~
area: frontend
related-prds: [PRD-014]
supersedes: [ADR-0002]
~~~

### E-0047-02
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The server sends `Retry-After` only for the fixed window; an authenticated (sliding-window) 429 has none.
- Evidence: `RateLimiterOptionsSetup.cs:32-38`; `HeadersAndRateLimitFixture.cs:99-102`.
- old:
~~~
**Summary:** In the context of a server that answers a throttled request with `429` and a `Retry-After`
value, facing
~~~
- new:
~~~
**Summary:** In the context of a server that answers a throttled request with `429` and, where its limiter
knows the wait, a `Retry-After` value, facing
~~~

### E-0047-03
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: "it says for how long" is true only for an anonymous caller.
- Evidence: as E-0047-02; `ApiDetailErrorFactory.cs:81-83`.
- old:
~~~
A `429` is not a transient fault: the server has decided that this caller may not proceed yet, and it says
for how long.
~~~
- new:
~~~
A `429` is not a transient fault: the server has decided that this caller may not proceed yet. For an
anonymous caller it says for how long, in `Retry-After` and in the `detail`; an authenticated rejection
names no wait, because the sliding window exposes none.
~~~

### E-0047-04
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: omission
- Why: The record does not name its sources. It also leaves open ADR-0002's choice of the header as the canonical source (D-3), and it should say so.
- Evidence: ADR-0002 §2 (D-1..D-5); ADR-0047:58-59 (the consequence leaves the source open); PRD-014:3.
- old:
~~~
The client cannot compute the wait itself either. The server owns the limit, the window and the partition,
so only the server knows when the caller may try again.
~~~
- new:
~~~

This record restates the direction of ADR-0002, which it supersedes, for PRD-014, still a discovery
draft. ADR-0002's choice of `Retry-After` as the canonical source of the wait (D-3) is left open here.
~~~

### E-0047-05
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: "the first two implementations below" points to a list that does not exist. The OTP cooldown is not a `429`: it comes back on a successful response and already has a countdown. The record does not say what happens when a `429` names no wait, which is every authenticated case and an open question in PRD-014.
- Evidence: `VerifySignupService.cs:36-41` (`EnrichedResult.Success` with `RetryMinutes`); `OtpVerificationForm.tsx:36-52`; PRD-014:76 (OQ-6); ADR-0047 has no list after line 50.
- old:
~~~
The same rule applies to the authentication flows that carry their own guidance, such as a one-time code
policy that reports a wait in minutes.

This record states the intended behaviour. The transport does not yet read the server's guidance: a `429`
is mapped to a rate-limited failure with fixed copy, and no countdown exists, so the first two
implementations below are what would make the decision true.
~~~
- new:
~~~
The same rule applies to the authentication flows that carry their own guidance: the one-time code policy
reports a wait in minutes on a successful response, not a `429`, and `OtpVerificationForm` already counts
it down. What the interface shows when a `429` names no wait — every authenticated rejection today — is
not decided here (PRD-014 OQ-6).

This record states the intended behaviour. The transport does not yet read the server's guidance: a `429`
is mapped to a rate-limited failure with fixed copy, and no countdown exists for a `429`, so reading the
wait in the interceptor and showing it are what would make the decision true.
~~~

### E-0047-06
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: factual
- Why: The wait is in both places, or in neither: header and detail come from the same value, and an authenticated rejection has neither.
- Evidence: `RateLimiterOptionsSetup.cs:30-40`; `HeadersAndRateLimitFixture.cs:63-67,99-102`.
- old:
~~~
- Bad, because the number is available in two places — the exposed header and the problem detail — so a
  future implementation has to choose one rather than assume it is only in the header.
~~~
- new:
~~~
- Bad, because where a wait is given it is in two places — the exposed header and the problem detail —
  so a future implementation has to choose one, and an authenticated rejection carries neither, so it
  must also handle no wait at all.
~~~

### E-0047-07
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: insert-after
- Tags: none
- Severity: omission
- Why: The no-retry property comes from how hooks are written, not from configuration. If a query function threw instead of resolving a `FailResult`, TanStack Query's default of three query retries would apply to `429`s, and nothing would catch it.
- Evidence: `Source/Client/pot-react/src/api/hooks/useApi.ts:47-56,119-126`; `src/main.tsx:18-25` (no `retry` set); TanStack Query v5 default `retry: 3` for queries on the client, `0` for mutations; `authInterceptor.ts:36-45` retries only after a 401 refresh.
- old:
~~~
- Bad, because the user must act again after the wait; there is no silent recovery.
~~~
- new:
~~~
- Bad, because nothing configures the absence of retries: `useGet` resolves a failure as a value, so
  TanStack Query's default of three retries for a failed query never fires. A query function that threw
  instead would start retrying every `429`, and no test would notice.
~~~

### E-0047-08
- Decision: [ ]  (Y = apply, N = reject, or write a change)
- Kind: replace
- Tags: none
- Severity: confirmation
- Why: The interceptor fixtures do not assert that no retry happens. They assert only the mapping to `RateLimitedError` and the fixed copy. The server-side guard this decision depends on is not named.
- Evidence: `Source/Client/pot-react/tests/api/interceptors/axiosInterceptors.test.ts:396-421,636-647`; `grep -rn -i retry tests/` (only `retry: false` wrappers); `HeadersAndRateLimitFixture.cs:61-67,97-102`; `Pot.AspNetCore.Tests/Concerns/Cors/Configuration/CorsOptionsSetupFixture.cs:52`.
- old:
~~~
- Test: the interceptor fixtures assert a `429` produces the rate-limited failure and that no retry is
  attempted. Nothing yet asserts a wait, because none is read.
~~~
- new:
~~~
- Test: `tests/api/interceptors/axiosInterceptors.test.ts` asserts a `429` produces a `RateLimitedError`
  with the fixed copy whatever the body. Nothing asserts that no retry is attempted, and nothing yet
  asserts a wait, because none is read.
- Test: `HeadersAndRateLimitFixture` asserts what the client would read: `Retry-After` exposed on both
  rejections, present with a matching timed `detail` on the anonymous one, and absent with an untimed
  `detail` on the authenticated one; `CorsOptionsSetupFixture` pins the exposed-header list.
~~~

### Notes (no edit proposed)
- The clock-change consequence (line 61-62) is speculative: `Retry-After` is delta-seconds, and the only
  timed wait is at most ten seconds. It is harmless, so no edit is proposed.
- Option 4 ("generic message") is the current behaviour, which the Decision Outcome acknowledges. It
  could be labelled as the status quo; the README requires that only for backfilled records.
- With E-0047-04, -05 and -07 the file is about 112 lines.

---

---

## Appendix: reviewer cross-cutting notes (verbatim)

### From group-D-errors-ratelimit.md: Cross-cutting notes for the owner

- All 45 backfilled ADRs carry `date: 2026-11-04`, the day they were written, and none says in Context that
  it is backfilled or what it was reconstructed from, as the skill's hard rules require. The date and
  "This record is backfilled" edits above fix this for 0012, 0014, 0015 and 0016 only. Other groups
  probably need the same treatment.
- The proposed backfill sentences cite owner recollections as `SQ-nn in DOCS-WORK-LISTS.md`. If that work
  list is retired after W4, change the citation to plain "the owner's recollection".
- No ADR in this group links to its neighbours ("a separate decision"). This is consistent with the rest of
  the series, so no link edits are proposed.

### From group-G-data-ops.md: Cross-cutting notes (apply to all six)

- **No backfill statement.** None of ADR-0003..0047 says it is backfilled or names its sources, which the
  `adr` skill's hard rules require. Each ADR below gets an edit adding that paragraph, citing the evidence
  found. Owner recollections are paraphrased inline rather than cited by `SQ-nn` row, because
  `DOCS-WORK-LISTS.md` is a disposable file.
- **`date: 2026-11-04` is the date the record was written, not the date of the decision.** Only ADR-0032 has
  a clear decision date (PRD-030 delivery, commit `59bbae8f`, 2026-10-22), so only that one gets a
  front-matter edit. For the others the earliest dated evidence is: enumerations stored by name and
  `DisableCascadeDelete` both appear in `a0c78b6d` (2024-12-28, "Adding initial postgres support"); the
  user/site sweeps in `8a0bef58` (2026-10-17); the migrate-then-start entrypoint in `e0139b0f` (2025-04-01);
  Razor rendering in `ee0d1e05` (2025-10-18); the email channel in `ef170a2d` (2025-10-26). The owner should
  set one policy for all backfilled records (decision date where known, otherwise earliest evidence)
  rather than each reviewer choosing.
- **One decision per file.** ADR-0027 records two decisions (enumeration storage, and database-backed
  uniqueness; its own Context says "two related storage questions"). ADR-0029 records three (the separate
  runner before the API, additive steps for destructive changes, and person-generated migrations; its
  Context says "two separate problems"). ADR-0031 records two (in-process queue; Razor/text templates).
  Splitting is a structural change for the owner to decide; the edits below do not split them.

---
