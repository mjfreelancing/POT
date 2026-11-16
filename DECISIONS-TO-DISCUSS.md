# Decisions to discuss

Each brief is one undecided item from `DOCS-REORGANISATION-PLAN.md`: a `Dec` row in its section 8, or a code risk mapped in its section 11. It says what the code does today, why it matters, the options and a recommendation.

**Baseline:** commit `9024e1d2`. Paths are relative to the repo root, and `path:line` references are to that commit. Every reference was re-checked against the code at that commit; the availability check is recorded in `DOCS-WORK-LISTS.md` ("Files reported missing, and where they turned out to be").

**How to answer:** write your decision after `> **Your call:**` at the end of each brief. A short answer is fine ("Option B", "Keep, record honestly", "Defer until after PRD-0019"). Add any conditions on the same line or the lines below.

**What happens next:**

- Nothing changes in code until a decision is approved.
- **A decision that changes code:** a PRD is raised through the prd-discovery skill first.
- **The ADR** is written once the decision is made.
- **Migrations:** agents never generate EF migrations, so any option marked "DB migration" needs a human to generate it.
- **Once a brief is decided and recorded,** remove it from this file. Its outcome lives in the ADR, or in the PRD it raised.

**Before acting on a brief, re-check its evidence against the code at that time.** These briefs were written on a machine without the full repository. Several rely on something being absent:

- DD-06 and DD-16: no infrastructure-as-code;
- DD-06: the local data folders.

Run the availability check in `DOCS-WORK-LISTS.md` ("Files reported missing, and where they turned out to be") first; it has been run once, and its result is recorded there. Never copy the contents of a git-ignored file into a brief or an ADR.

The briefs are ordered by severity, highest first. A follow-up on an item already fixed is at the end.

## Summary

Every brief below has been decided, and each records the outcome after its `> **Your call:**` line.

- Decided on 2026-11-16: DD-02, DD-03, DD-06, DD-07, DD-09, DD-12, DD-14, DD-16.
- Resolved earlier: DD-10 and DD-13 (2026-11-12), and DD-12 item 12e (2026-11-12).
- The forwarded-headers follow-up at the end was closed on 2026-11-16, both of its items declined.
- DD-13 was raised as [PRD-0036](Docs/prd/PRD-0036-client-feature-import-boundaries-discovery.md); the other
  items that change code follow the route recorded in their brief.

---

### DD-02 — Etag checked in code but not a database concurrency token; client types it as `bigint`

- **Backlog row:** 0021 (`Acc + Ver`).
- **What the code does today:**
  - `EntityBase.Etag` is a plain `long` (`Pot.Data/Entities/EntityBase.cs:11`). It is indexed, but there is no `IsConcurrencyToken`, `[ConcurrencyCheck]` or `xmin` anywhere in `Pot.Data`.
  - The value is set on every insert and update to `DateTime.UtcNow.GetEtag()` (`Pot.Data/DbContextBase.cs:154`), which is Unix milliseconds (`Pot.Shared/Extensions/DateTimeExtensions.cs:13-18`). It bypasses `ITimeProvider`.
  - Six update paths compare it in application code before saving: `Features/{Accounts,Expenses,Incomes,Sites,Approvals,Settings}/…/CheckHasSameEtag.cs`, for example `Accounts/Update/EntityChecks/Checks/CheckHasSameEtag.cs:34`.
  - `DbUpdateConcurrencyException` is already mapped to 409 (`Pot.AspNetCore/Concerns/ExceptionHandlers/DatabaseExceptionHandler.cs:28`). Without a token, though, EF raises it only when a row is missing.
  - Client: `EtagSchema` is `z.bigint()` (`src/data/identity.ts:8`). The same applies in `approvals.ts:12,19`, `settings.ts:7,20` and `siteUser.ts:16,35,43`. But `role.ts:7` uses `z.number()`, and `src/api/types/identity.ts:3` uses `eTag: number`. The schemas are only used for `z.infer`, so at runtime the value is a JS number. Millisecond values (about 1.8e12) are well within `Number.MAX_SAFE_INTEGER`.
  - **The plan's claims are correct.**
- **Why it matters:** Two users who load the same expense and save within the window between the check and `SaveChanges` both succeed, and the last write wins silently. Separately, two saves of one entity in the same millisecond produce the same Etag, so a stale client tag can match. Both are rare at current usage. The `bigint` type is a latent bug: if anyone ever adds `.parse()` or does BigInt arithmetic, it breaks (`JSON.stringify` throws on a BigInt).
- **Options:**
  - **A. Keep as-is (record honestly).** The ADR states that the check is advisory, with a race window. Effort: none.
  - **B. Make Etag a concurrency token and the value monotonic.** Mark `Etag` with `IsConcurrencyToken()`. In each update path, set the entry's original `Etag` to the client's value, so the `UPDATE … WHERE "Etag" = @original` guard does the check atomically. Change the generator to `max(nowMs, previous + 1)`, read from `ITimeProvider`. Effort M. **Model-only change: a human generates the migration** (empty `Up`/`Down`, snapshot update). No API contract or client change.
  - **C. Fix the client type.** Use `z.number()` everywhere, and remove the duplicate `eTag` type. Effort S. Client only.
  - **D. Switch to PostgreSQL `xmin`.** Effort L. Contract change (the value becomes a 32-bit transaction id), client changes, DB migration.
- **Recommendation:** B and C together. They close the race with the 409 path that already exists, and they make the client type honest, with no contract change.
- **Route if changed:** raise a PRD via the prd-discovery skill. The migration is generated by a human.

> **Your call:** Option C only. Fix the client type; leave the race as ADR-0001 accepted it.

**Decided 2026-11-16:** option C. Option B was declined after reading
[ADR-0001](Docs/adr/ADR-0001-optimistic-concurrency-is-checked-in-the-application.md), which had already
weighed a concurrency token as its option 2 and rejected it, choosing the application check and accepting
the race window. That record stands and is not edited — superseding it was the alternative and was not
taken.

What changes is the client type only: `EtagSchema` becomes `z.number()` in `src/data/identity.ts`, and the
duplicate `eTag: number` declaration in `src/api/types/identity.ts` goes, leaving one honest type. Nothing
changes at runtime, because those schemas exist for `z.infer` and the value was already a JavaScript
number. Client only — no server, contract or migration change — so no PRD is raised.

The race window and the same-millisecond collision are accepted exactly as ADR-0001 records them. If either
starts to matter, the change is ADR-0001's option 2, which is recorded there rather than here.

---

### DD-03 — Money stored and calculated as `double`

- **Backlog row:** 0016 (`Dec`).
- **What the code does today:**
  - Stored money is `double` / `double precision`: `AccountEntity.cs:15-16` (Balance, Reserved), `ExpenseEntity.cs:25` and `IncomeEntity.cs:23` (Amount). The current model snapshot confirms `double precision` (`Pot.Data/Migrations/PotDbContextModelSnapshot.cs:34-46,147-148,208-209`).
  - Every input and output model, the CSV import/export rows and the export metadata use `double` (for example `Features/Accounts/Get/Models/Output.cs:8-16`, `Features/Maintenance/Import/Models/ExpenseCsvRow.cs:63`, `Metadata/Models/MetadataV1.cs:14-63`). Altogether, 116 server files mention `double`.
  - "Rounded to 2 dp away from zero" applies only to each expense's accrued amount (`Pot.App/Calculators/AccrualCalculator.cs:220`). The totals are summed unrounded (`:66-79`), and so is the daily rate (`:237`, `:241`). The derived `Available` is unrounded too (`Accounts/Get/Models/Output.cs:16`). The comment at `AccrualCalculator.cs:18` claims all monetary results are rounded.
  - The contract publishes money as `number|string`, `format: double` (`contracts/openapi/v1.json:6869-6876`). The client uses `MoneyValue = number` (`src/lib/types.ts:3`) and formats it with `Intl.NumberFormat`, AUD / en-AU by default (`src/lib/moneyUtils.ts:22-35`).
- **Why it matters:** Binary floating point cannot hold most cent values exactly. Sums of many amounts drift, for example 0.1 + 0.2 = 0.30000000000000004. Display rounding hides most of this. It shows up as a `-$0.00` available balance, a comparison against zero that goes the wrong way, CSV exports with long fractions, and tests that need tolerances. The cost of switching grows with every feature and every row of data.
- **Options:**
  - **A. Keep as-is (record honestly).** The ADR records double, the rounding rule as it really is, and the known drift. Effort: none.
  - **B. Keep double, but round at defined boundaries.** Round totals, rates and `Available` to 2 dp in the calculator and the outputs. Effort S. No migration or contract change. This reduces visible drift; it does not make values exact.
  - **C. Use `decimal` end to end.** C# `decimal`, PostgreSQL `numeric(18,2)` for the stored amounts (computed values are never stored, row 0009), with explicit rounding rules in the calculators. Effort L. **DB migration and data migration** (`USING round(col::numeric, 2)`, written by a human). Contract: `format` changes from `double` to `decimal` on about 30 properties, but the wire format stays a JSON number, so the client is unchanged apart from tests. Import/export parsing and tests change.
  - **D. Integer cents (`long`).** Effort L. DB and data migration, a breaking contract change, and client changes everywhere. Not recommended.
- **Recommendation:** C. It is the standard representation for money, the wire format does not change, and the data set is still small. Sequence it next to the per-site currency work (PRD-0018) so the rounding rules are decided once.
- **Route if changed:** raise a PRD via the prd-discovery skill. The schema and data migration are generated and written by a human.

> **Your call:** Option B. Round at the boundaries; keep `double`.

**Decided 2026-11-16:** option B. Option C was the recommendation in this brief and was withdrawn on the
evidence: stored amounts are 2 dp values, which `double` holds exactly far beyond any realistic balance;
the only division is `AccrualCalculator.cs:220`, which already rounds; and the CSV export writes stored
values rather than sums. No money comparison against zero exists, so the drift has no reachable wrong
answer today. The one reachable symptom is the unrounded `Available = Balance - Reserved - TotalCommitted`
(`Accounts/Get/Models/Output.cs:16`), which can land a fraction below zero and render as `-$0.00`. That is
a boundary-rounding defect, and B fixes it without a migration.

`decimal` was not declined for speed — decimal arithmetic and PostgreSQL `numeric` aggregation are slower,
but the workload is too small for that to matter. It was declined because the effort lands on EF mapping,
validators, CSV parsing, around thirty contract property formats and a data migration on live data, for no
defect the code can currently reach.

**What would reopen it:** multi-currency with exchange-rate multiplication chains, interest or compounding,
reconciliation against a bank statement to the cent, regulatory reporting, or one observed cent-level
mismatch. Record the trigger when it happens rather than pre-empting it.

**Route:** server-only, but the values in responses move by rounding, so the brief's route stands — a PRD
before the change.

---

### DD-06 — PostgreSQL 13 locally and in tests, 16 in E2E, 17.6 in Azure

- **Backlog row:** 0047 (`Dec`).
- **What the code does today:**
  - EF is told the server is 13.6: `options.SetPostgresVersion(new Version(13, 6))` (`Pot.Data/Extensions/DbContextOptionsBuilderExtensions.cs:11`).
  - These all run `postgres:13`:
    - the Docker stack (`Source/Docker/Postgres/Dockerfile:16`);
    - the standalone stacks (`Data/Local Docker/docker-compose.yml:23`, and `Data/Seed Docker/docker-compose.yml:25`, renamed from `Data/E2E Docker` in `2d1e23ec`);
    - the server integration tests (`Pot.AspNetCore.Integration.Tests/IntegrationFixtureBase.cs:96`, with a matching doc comment in `Host/ApiWebApplicationFactory.cs:49`).

    The compose file's comments also name it (`docker-compose-client-server.yml:82,92`).

  - Playwright E2E uses `postgres:16` (`Source/Client/pot-react/playwright.globalSetup.ts:516`), so there are three versions in use.
  - The only source for "Azure runs 17.6" is a diagram label (`Docs/reference/diagrams/pot_azure_diagram.mmd:18`). There is no infrastructure-as-code to confirm it.
  - PostgreSQL 13 reached end of life in November 2025.

- **Why it matters:** The integration tests prove behaviour on 13, but production runs 17. Differences in planner behaviour, `citext`, collation and new defaults would show up in production first. The 13.6 hint is harmless against a 17 server: EF just avoids newer SQL. The real risk is the drift between test and production, and running an unsupported version locally.
- **Options:**
  - **A. Keep as-is (record honestly).** The ADR records all three versions and the drift. Effort: none.
  - **B. Align everything on the Azure version.** First confirm the Azure server version in the portal. Then update the Dockerfile, both compose files, the integration-test image, the Playwright image and `SetPostgresVersion`. Effort S. No EF migration. **Local data:** the existing `Source/Docker/postgres-data` directory, and the `postgres-data` folders under `Data/Local Docker` and `Data/Seed Docker`, are version 13 clusters and will not start under 17. You would need to dump and restore it, or recreate it, yourself; agents never touch that folder.
  - **C. B plus a single source for the version.** Put the image tag in `Source/Docker/.env` and the test configuration, so the version cannot drift again. Effort M.
- **Recommendation:** B. It is a small change that makes the tests prove the production engine. C is worth it only if the version drifts again.
- **Route if changed:** raise a PRD via the prd-discovery skill. The local data rebuild is a manual step for you.

> **Your call:** Option B. Align everything on the production version.

**Decided 2026-11-16:** option B. Three versions were in use — `postgres:13` for the Docker stack, both
standalone stacks and the integration tests; `postgres:16` for Playwright; and a later major in production.
PostgreSQL 13 is past end of life, so the integration suite was proving behaviour on an unsupported engine.
Option C was declined: one source for the image tag guards against a drift that has already happened, it
does not fix the drift itself.

**The production version is 17.11, confirmed by the owner on 2026-11-16.** That corrects two places that
said 17.6 — this brief above, which quoted the only evidence the repository held, and
`Docs/reference/diagrams/pot_azure_diagram.mmd:18`. The diagram label and its PNG were both regenerated on
2026-11-16, so the source and the render agree. `Docs/adr/ADR-0005-postgresql-is-the-relational-datastore.md:57`
repeats the 17.6 claim and should be dealt with when the alignment lands, rather than edited in place while
the record is accepted.

**Two steps belong to the owner:** confirm the deployed version in the portal before the work starts, and
rebuild the three local data directories. `Source/Docker/postgres-data`, `Data/Local Docker/postgres-data`
and `Data/Seed Docker/postgres-data` are version 13 clusters and will not start on a later major version.
Agents never touch those folders.

---

### DD-07 — Client API types are hand-written

- **Backlog row:** 0026 (`Dec`). Related to 0025.
- **What the code does today:**
  - The contract is generated at build time and committed (`contracts/openapi/v1.json`; `.github/instructions/server.instructions.md:21-28`).
  - The client has no codegen dependency (`Source/Client/pot-react/package.json`). Its API types are Zod schemas in `src/data/*.ts`, used only through `z.infer`. Nothing in `src/` calls `.parse` or `.safeParse` on API data. Responses are `JSON.parse(text) as unknown` and then cast (`src/api/apiHelpers.ts:88`).
  - In the contract, numbers are typed `["number","string"]` with a pattern (for example `v1.json:6869-6876`), because the server accepts numeric strings when reading.
  - The Etag type mismatch in DD-02 is one drift this has already let through.
- **Why it matters:** A renamed or retyped server field compiles on both sides and fails only at runtime, usually as `undefined` in the UI. Nothing in the build compares the client with the contract.
- **Options:**
  - **A. Keep as-is (record honestly).** The ADR records hand-written types, and that the contract is a reference, not a source. Effort: none.
  - **B. Generate the types from the committed contract.** Use a types-only generator (for example `openapi-typescript`) as a dev dependency, with an npm script and a "generated file is current" check. The client response types are replaced by generated ones; form schemas stay as Zod. Effort M-L. No contract change on the wire. It does need a server-side schema transformer, so numbers are published as `number` rather than `number|string`; otherwise every generated number type is a union.
  - **C. Validate at runtime.** Call `.parse` on responses with the existing Zod schemas. Effort M. Client only. Catches drift in use rather than at build. The Etag schema has to be fixed first, or every parse fails.
- **Recommendation:** B, after CI exists (PRD-0034, raised from the decided DD-05) so the "generated file is current" check runs on every change. It turns contract drift into a compile error.
- **Route if changed:** raise a PRD via the prd-discovery skill (client plus a small server schema change).

> **Your call:** Option A. Keep the hand-written types; the contract is a reference, not a source.

**Decided 2026-11-16:** option A, recorded as
[ADR-0059](Docs/adr/ADR-0059-the-client-api-types-are-hand-written-and-the-committed-contract-is-a-reference.md).
Options B and C were both declined. B needs the currency check that only CI could give, and C (runtime
validation with the existing schemas) is a behaviour change in the client's request path rather than a type
decision; the ADR keeps both alive as the alternatives it was chosen over.

The rule the decision leaves behind: when the contract and the client disagree about a field's type, the
contract wins and the client is corrected. Adding a contract field needs no client change, but renaming or
retyping one needs a client change that nothing will fail until the reading code runs — which is exactly how
the Etag mismatch shipped.

---

### DD-14 — No policy for API/client version skew while an old PWA shell is cached

- **Backlog row:** 0019 (`Acc`: "Client and server release together; breaking API changes need no compatibility window").
- **What the code does today:**
  - `Source/Client/pot-react/DEVELOPER.md:3122` says detection has no client/server version handshake, "so a breaking API change still relies on the backward-compatibility policy". **No such policy exists anywhere in the repo.** The only related statements are the four PRD lines behind row 0019 (`PRD-0021:169`, `PRD-0025:261`, `PRD-0028:73`, `PRD-0030:157`), and they say the opposite: no compatibility period is needed.
  - "Release together" is not true in practice:
    - The server and client are built and pushed by two separate manual tasks (`.vscode/tasks.json:93-117`), so for a while the new server serves the old client.
    - An open or cached PWA shell keeps calling the API until it updates.
  - That update window is bounded. The client checks `/version.json` at start-up, on focus and every 5 minutes while visible (`src/concerns/pwa/pwaRuntime.ts:8`); the service-worker check runs every 30 minutes (`:5`). The update is then enforced after 45 seconds of inactivity, deferred by at most 5 minutes (`:15`, `:19`; guide `DEVELOPER.md:3089-3102`).
  - Nothing sends a client build id to the server (no version header in `src/api/axios.ts`, nothing in `Pot.AspNetCore`).
- **Why it matters:** For about 10 minutes after a deploy, plus the gap between the two deploy tasks, an old client can call a changed endpoint. The usual results are a 400 on a write (a missing or renamed field) or a blank value in the UI (a removed response field). Silent data damage is unlikely, because the server validates every input. The guide currently promises a safeguard that does not exist.
- **Options:**
  - **A. Keep as-is (record honestly).** The ADR for row 0019 states that there is no compatibility window, that skew is accepted, and how long it can last. Correct `DEVELOPER.md:3122` to say so. Optionally, run both deploy tasks together. Effort S. Docs only.
  - **B. "Expand, then contract" for breaking changes.** The server accepts both shapes for one deploy, and removes the old one in a later deploy. This supersedes row 0019. Effort S per breaking PRD (process cost on every one). No migration.
  - **C. A version handshake.** The client sends its build id in a header. The server answers a dedicated Problem Details type when the client is older than a minimum, and the client forces the update immediately. Effort M. Server, client and contract changes (new header, new problem type). Infra: none.
- **Recommendation:** A. The enforced PWA update already bounds the skew to minutes, and there is one client and one operator. Revisit C if a breaking change causes real failures.
- **Route if changed:** A is a docs change made with the ADR. B and C each need a PRD via the prd-discovery skill.

> **Your call:** Option A. Record the accepted skew honestly and correct the guide.

**Decided 2026-11-16:** option A, recorded as
[ADR-0060](Docs/adr/ADR-0060-the-client-and-server-deploy-separately-and-api-version-skew-is-accepted-for-minutes.md).
The ADR states that no compatibility window exists, that the skew is accepted, and what bounds it: the build
check at startup, on focus and every 5 minutes while visible, the 30-minute service-worker check, and an
update applied after 45 seconds of quiet with a 5-minute hard cap. It records the cost too — a backgrounded
tab checks only when it regains focus, so a user can meet the stale client on their first action.

`Source/Client/pot-react/DEVELOPER.md` no longer claims a backward-compatibility policy that never existed;
it now states the accepted skew and points at the ADR. Options B and C are kept in the record as the
alternatives it was chosen over, with C named as the revisit path if a breaking change causes real failures.

---

### DD-09 — Permission policies are string literals

- **Backlog row:** 0029 (`Acc`); no `Dec` row.
- **What the code does today:**
  - 34 endpoint registrations pass literals such as `.RequireAuthorization("account:view")`, for example `Pot.AspNetCore/Features/Accounts/Extensions/RouteGroupBuilderExtensions.cs:11`. Accruals passes three literals, and all three must pass (`Features/Accruals/Extensions/RouteGroupBuilderExtensions.cs:9`).
  - The permissions already exist as an enriched enum (`Pot.Shared/Enumerations/Permission.cs:9-20`, 12 values). Approvals uses a constant (`AuthorizationPolicies.PlatformManage`).
  - `PermissionAuthorizationPolicyProvider.cs:22-34` turns any unknown policy name into a permission requirement, so a typo is never rejected; it just never matches (`PermissionAuthorizationHandler.cs:34`).
  - `EndpointAuthorizationMetadataFixture` checks that every endpoint is protected or on the anonymous allow-list, and pins the exact policy on 8 routes only.
  - `AuthorizationMatrixFixture` exercises every documented operation anonymously, as Admin and as Viewer. **It does not catch a typo.** It reads each endpoint's declared policies (`Pipeline/Support/EndpointRequirements.cs`) and derives the expected answer from them (`EndpointRequirement.Allows`). A misspelt policy is therefore expected to answer 403 to everyone, and it does. It does, though, already enumerate every endpoint's policies, which is exactly the input option B's test needs.
  - A typo fails closed (403), so it is a defect, not a security hole.
- **Why it matters:** A misspelt policy on a new endpoint ships, and every user gets a 403 on that endpoint until someone notices. Today only 8 routes would catch this in a test.
- **Options:**
  - **A. Keep as-is (record honestly).** Effort: none.
  - **B. Use names from `Permission` (or constants generated from it) at every endpoint, and add one fixture test.** The test asserts that every policy on every endpoint is a known `Permission` name or an `AuthorizationPolicies` constant. It can be a single assertion over `EndpointRequirements`, beside the matrix. Effort S. No migration, contract or client change.
  - **C. Make the provider reject unknown names.** It returns `null`, so ASP.NET Core throws. This is loud, but only at runtime on that endpoint. Effort S.
- **Recommendation:** B. The test alone catches every typo at build time, and the constants make typos unlikely in the first place.
- **Route if changed:** raise a PRD via the prd-discovery skill (server only).

> **Your call:** Option B. Names from `Permission` at every endpoint, plus one fixture test.

**Decided 2026-11-16:** option B. Every endpoint names its permission from `Permission` (or an
`AuthorizationPolicies` constant) rather than a literal, and one fixture asserts that every policy on every
endpoint is a known permission name or a known constant. That fixture reads the same `EndpointRequirements`
input the authorization matrix already uses, so it needs no inventory of its own.

**No ADR is raised.** The rule constrains one declaration style, its whole rationale is that a typo matches
nobody and fails closed — dead code rather than a hole — and the fixture is the enforcement that would
otherwise have to be described in prose. It becomes a convention with a guard, which belongs with the tests
that implement it.

**Route:** server only, and no observable behaviour change unless a typo turns out to exist. The brief routes
it through a PRD; a direct change carrying the test is defensible if the owner prefers that.

---

### DD-10 — DI by marker-interface scanning: code and server guide agree, `server.instructions.md` does not

- **Backlog row:** 0040 (`Dec`).
- **What the code does today:**
  - Each layer has an empty `ServiceRegistrarBase` marker (for example `Pot.Data/PotDataRegistrar.cs:6`). Each scans for `IPotScopedDependency` and `IPotSingletonDependency` (`Pot.App/Extensions/ServiceCollectionExtensions.cs:14-38`, `Pot.AspNetCore/Extensions/WebApplicationBuilderExtensions.cs:457,473`, `Pot.Data/Extensions/ServiceCollectionExtensions.cs:11,20`, `Pot.EmailSender/Extensions/ServiceCollectionExtensions.cs:12`).
  - About 108 interfaces extend a marker (99 of them in `Pot.App`). 91 `Pot.App` files reference `IPotScopedDependency` and 10 reference `IPotSingletonDependency`. No class implements a marker directly, so every scanned service has an interface, even when there is only one implementation.
  - Tests depend on this: there are about 354 `Substitute.For<I…>`-style substitutions (NSubstitute).
  - **The server guide agrees with the code.** `Source/Server/DEVELOPER.md:5065-5262` ("Dependency Injection", "Auto-Registration Pattern") describes the scanning. The worked example says "No manual registration needed" and points to `AutoRegisterScoped<DependencyRegistrar, IPotScopedDependency>` (`DEVELOPER.md:876-890`).
  - **`.github/instructions/server.instructions.md:64-66` is the only outlier.** It says to register concrete classes, and to add an interface only for multiple implementations, a public boundary or a test seam.
  - The instruction's "small set of composition-root methods" rule _is_ met. The "interface only when needed" rule is the one contradicted.
- **Why it matters:** Agents follow the instruction file, which takes precedence over the guide for them. A new service written to the instruction (concrete, no interface) is never registered, and fails at runtime with "unable to resolve". The code and the guide are consistent; only the instruction is wrong.
- **Options:**
  - **A. Keep scanning, and change the instructions to describe it (record honestly).** New services implement an interface that extends the right marker; the test seam is the accepted reason for the interface. Effort S. Docs only.
  - **B. Move to explicit registration of concrete classes.** Remove the single-implementation interfaces. Effort L. Touches about 109 interfaces and hundreds of tests. No migration or contract change.
  - **C. Hybrid:** scanning for existing code, explicit registration for new code. Effort S. Leaves two styles; not recommended.
- **Recommendation:** A. The scanning works and is tested, the guide already describes it, and B costs a great deal for no behaviour change. Fixing the one outlier stops agents writing services that never get registered.
- **Route if changed:** A is a docs change made with the ADR; no PRD needed. B would need a PRD via the prd-discovery skill.

> **Your call:** Option A. Record the scanning as the decision and correct the instructions; the code and the server guide already agree.

**Resolved 2026-11-12:** [ADR-0055](Docs/adr/ADR-0055-services-are-registered-by-scanning-each-layer-for-its-marker-interface.md)
records the scanning decision, with convenience against hand registration as the reason on record and
startup cost as the condition on it. The Dependency injection section of
`.github/instructions/server.instructions.md` now states the marker rules and the three cases where an
explicit registration is right, so it no longer contradicts the code. `IProblemDetailsInspector` was the
one Pot service still registered by hand; it now extends `IPotSingletonDependency` and is picked up by
the scan, so no explicit registration is left without a stated reason.

---

### DD-13 — Features import other features (and shared layers import features)

- **Backlog row:** 0055 (`Acc + Ver`: "Feature modules over shared `api/`, `concerns/` and `lib/` layers, with one-way dependencies").
- **What the code does today:**
  - `Source/Client/pot-react/DEVELOPER.md:260` says "Features never import from other features (use shared utilities instead)". There is no ESLint rule enforcing it: `eslint.config.js` has no `no-restricted-imports` or import-boundary rule.
  - **Six features import `@/features/auth`**: `accounts`, `dashboard`, `expenses`, `incomes`, `users` and `userSettings`. What they import:
    - `WithPermission` (11 imports) and `PermissionGuard`;
    - `useAuthContext` and `useAccessToken`;
    - `sessionNotice`;
    - `useClearSession`.
      For example: `src/features/accounts/AccountsPage.tsx:16`, `src/features/userSettings/delete/DeleteAccountAction.tsx:6-8`.
  - `dashboard` imports `renewExpenses` and `renewIncomes` from `expenses` and `incomes` (`src/features/dashboard/actions/accruals/RenewExpensesAction.tsx:12`, `RenewIncomesAction.tsx:12`). These are plain mutation-plus-cache-invalidation helpers (`src/features/expenses/bulkActions/renew.ts`).
  - `expenses` and `incomes` import `@/features/shared/sheets/asyncSheetStates` (for example `src/features/expenses/edit/EditExpenseSheet.tsx:10`). That is a folder under `features/` acting as a shared layer.
  - **The shared layers also import features**, which breaks the one-way rule in row 0055:
    - `src/api/hooks/useMe.ts:2` and `src/api/hooks/useSignup.ts:1` (auth);
    - `src/components/nav/AppSidebarMenus.tsx:16-18` (auth, maintenance);
    - `src/components/user/UserMenu.tsx:13-14` (auth, userSettings);
    - `src/concerns/auth/logoutManager.ts:1` (auth).
- **Why it matters:** The rule is documented but neither followed nor enforced, so agents get conflicting signals and the dependency graph keeps tangling. Most of `auth` is really a cross-cutting layer (permissions, session context) that happens to live under `features/`. Nothing is broken at runtime.
- **Options:**
  - **A. Keep as-is (record honestly).** Delete the "never" rule; features may import each other. Effort S. Docs only. The graph stays unconstrained.
  - **B. Sanction a narrow set.** Any feature may import `auth`'s public exports and `features/shared`; nothing else. `dashboard`'s renew helpers stay as a sanctioned composition. Add a lint rule to enforce it. Effort S. Client only (lint config and docs).
  - **C. Move the cross-cutting pieces into layers, then enforce "no feature→feature" with lint.** The permission components and auth/access-token contexts, `sessionNotice` and `useClearSession` move to `concerns/auth`. `features/shared/sheets` moves to `components/`. The renew helpers move to `api/` or `concerns/`. Fix the reverse imports from `api/`, `components/` and `concerns/` at the same time. Effort M. Client only. Import moves, no behaviour change.
- **Recommendation:** C. The reverse imports show `auth` is already used as a layer. Moving it makes the documented rule true, and lets a lint rule keep it true.
- **Route if changed:** raise a PRD via the prd-discovery skill (client refactor; no server, contract or migration impact).

> **Your call:** Option C. Move the cross-cutting pieces into layers, then enforce "no feature→feature" with
> lint.

**Decided 2026-11-15:** option C, so the one-way dependency rule in
`.github/instructions/client.instructions.md` is the intended state and stays exactly as written. The work
is a client-only refactor with no server, contract or migration impact, and it follows the brief's route:
a PRD through the `prd-discovery` skill before any code moves. **Raised as
[PRD-0036](Docs/prd/PRD-0036-client-feature-import-boundaries-discovery.md)**, whose discovery document
holds the decision, the two alternatives that were dropped and the open questions, so nothing in this file
needs to survive for the work to stay understood. Until that PRD lands, the rule and the code disagree,
and the exceptions listed above are what the refactor has to remove: `features/shared/sheets` to
`components/`, the permission components and the auth and access-token contexts plus `sessionNotice` and
`useClearSession` to `concerns/auth`, the renew helpers to `api/` or `concerns/`, and the reverse imports
from `api/`, `components/` and `concerns/`.

---

### DD-16 — No server health check in the prodlike compose stack

- **Backlog rows:** 0048 (liveness separate from readiness) and 0078 (local environments).
- **What the code does today:**
  - In `Source/Docker/docker-compose-client-server.yml`:
    - `postgres` has a health check (`:147-160`, `pg_isready`).
    - `server` waits for it (`:245-247`, `condition: service_healthy`) but has no health check of its own.
    - `client` uses a plain `depends_on: - server` (`:486-489`).
  - The client's own health check probes the **server**: `wget … http://server:5241/_health` (`:530`). So the client container's health reports the API's liveness, not nginx's.
  - The server image has no `HEALTHCHECK` (`Source/Docker/Server/Dockerfile`). Its entrypoint runs migrations before the API starts listening (`Source/Docker/Server/entrypoint.sh`).
  - The API exposes `/_health` (liveness) and `/_health/ready` (readiness, including the database) (`Pot.AspNetCore/Program.cs:82,93`).
  - `Source/Docker/DEVELOPER.md:88-92`, `:298-300` and `:621-649` describe the gap, and propose a `wget` check on `/_health` with a 40s start period. The image ships BusyBox `wget` but not `curl`.
- **Why it matters:** On a cold start, the browser can load the client while the API is still migrating, and the first requests fail. The guide's workaround is "Refresh if the first request fails". This affects the local prodlike stack only. Azure uses Container Apps probes, which are not in the repo and cannot be checked from the code.
- **Options:**
  - **A. Keep as-is (record honestly).** Effort: none.
  - **B. Add a server health check on `/_health/ready`, and make `client` wait for `service_healthy`.** Readiness is the right gate for "can serve requests"; the guide's draft uses `/_health`. Point the client's own check at nginx (`http://localhost:80/health`), so each container reports its own health. Effort S. Compose only. No migration, contract or client change.
  - **C. B plus explicit Container Apps liveness and readiness probes on the same endpoints.** Effort S-M. Infra change in Azure. Confirm the current probe settings in the portal first.
- **Recommendation:** B. It removes the first-load failure and the misleading client health status with a few lines of compose. Check the Azure probes separately, and treat C as an infra task if they are missing.
- **Route if changed:** raise a PRD via the prd-discovery skill (Docker compose only), or fold it into the docs-reorganisation work if you prefer, since the guide already contains the change.

> **Your call:** Option A. Keep as-is and record it honestly.

**Decided 2026-11-16:** option A, after the brief's premise was checked against the compose file and did
not hold. The client's health check probing `http://server:5241/_health` is deliberate, and the compose
file says why: it verifies that the client container can reach the API, and it names the alternative
(`wget --spider http://localhost:80/health`) for checking nginx directly. So the client container cannot
report healthy until the API answers, which is why it always reports last in the start-up logs. The
"misleading client health status" this brief described is the design working.

Two gaps survive, and both are accepted:

- The server service declares no `HEALTHCHECK`, so nothing reports the server's own state except
  indirectly, through the client's probe.
- `client` depends on `server` without a condition, so nginx serves as soon as the container exists and a
  browser opened early can reach the API while migrations run.

`Source/Docker/DEVELOPER.md` already states the second one honestly, together with the "refresh if the
first request fails" workaround, so it needs no correction. The brief's suggestion to repoint the client
check at nginx is explicitly not taken.

**No ADR is raised:** nothing changes, the design is documented beside it in the compose file, and the
residual gaps are small enough for a future change to pick up if they ever matter.

---

### DD-12 — Smaller items

- **Backlog rows:** none directly, apart from 0052 (server test strategy) for 12d and the plan's section 6.3 note for 12e.
- **What the code does today, item by item:**
  - **12a — Cronos.** Referenced at `Source/Server/Directory.Packages.props:21` and `Pot.AspNetCore/Pot.AspNetCore.csproj:17`. No `using Cronos` or other use anywhere.
  - **12b — sonner and next-themes.** `src/components/ui/sonner.tsx:10,14` calls `useTheme` from `next-themes`, but no `next-themes` provider is mounted, so `theme` is always `"system"`. The app's own `ThemeProvider` (`src/App.tsx:56`) exports its own `useTheme` (`src/components/theme/ThemeProvider.tsx:109`). Toasts follow the OS setting, not the user's chosen theme. The colours mostly come from the app's CSS variables, so the visible effect is small.
  - **12c — tailwindcss-animate.** Loaded with `@plugin "tailwindcss-animate"` (`src/index.css:3`) and used in about 15 places (`animate-in`, `fade-in-…`). It is a v3-era plugin, but Tailwind 4 loads it through `@plugin` and it works. The issue is maintenance, not breakage. shadcn now recommends `tw-animate-css` for v4.
  - **12d — EF InMemory.** Most use is in `Pot.Data.Tests`, not the App unit tests. 15 repository test files use `UseInMemoryDatabase`, against 4 in `Pot.App.Tests` (for example `Features/Accounts/Create/EntityChecks/Checks/CheckDescriptionDoesNotExistFixture.cs:99`). InMemory ignores `citext` (`Pot.Data/DbContextBase.cs:73`), unique indexes and SQL translation, so the case-insensitive duplicate checks are not really tested there. The Testcontainers integration project covers some of this.
  - **12e — ERD path.** **Resolved 2026-11-12:** the ERD moved to `Docs/reference/diagrams/pot_erd.d2`, and the path is now the `Erd:OutputPath` setting in `Pot.Data.Migrations/appsettings.json`, combined with the repository root that the app finds by walking up to the directory containing `Source/Server/pot.sln`. The previous hard-coded `..\..\..\..\..\..\Docs\pot_erd.d2` assumed both the working directory and the build depth; neither matters now, and a missing setting or root logs a warning instead of failing the migration. This is the option recorded below, applied because the move happened.
- **Why it matters:** none of these breaks production. 12d is the one that can hide a real defect.
- **Options and recommendation per item** (all effort S, no migration, no contract change):
  - **12a:** remove the package (recommended), or keep and record.
  - **12b:** use the app's `useTheme` in `sonner.tsx` and remove `next-themes` (recommended, client only), or keep and record.
  - **12c:** switch to `tw-animate-css` and check the animations still look right, or keep and record (recommended until the UI components are next updated).
  - **12d:** keep InMemory for logic tests, and record that SQL, `citext` and constraint behaviour must be proved in the integration project (recommended). The alternative is to move `Pot.Data.Tests` to Testcontainers: effort M, slower runs, needs Docker.
  - **12e:** keep as-is if the docs reorganisation leaves `Docs/pot_erd.d2` in place. If it moves, make the path a setting in `Pot.Data.Migrations/appsettings.json` built with `Path.Combine` (recommended only if the move happens).
- **Route if changed:** 12a, 12b, 12c and 12e can share one small housekeeping PRD via the prd-discovery skill. 12d's recommended option is an ADR and test-guideline change only.

> **12a Your call:** Remove Cronos.

> **12b Your call:** Use the app's `useTheme` in `sonner.tsx` and remove `next-themes`.

> **12c Your call:** Keep `tailwindcss-animate` until the UI components are next updated.

> **12d Your call:** Keep InMemory for logic tests and record the limitation.

> **12e Your call:** Already resolved on 2026-11-12 — nothing further.

**Decided 2026-11-16:** all four open items as recommended.

- **12a** Cronos is declared in `Directory.Packages.props` and `Pot.AspNetCore.csproj` and used nowhere in
  the code, so it is removed.
- **12b** `sonner.tsx` is the only importer of `next-themes`, and with no provider mounted the theme is
  always `"system"`, so toasts follow the operating system rather than the user's chosen theme. It switches
  to the app's own `useTheme` and the dependency goes.
- **12c** `tailwindcss-animate` stays: it works under Tailwind 4 through `@plugin`, and moving to
  `tw-animate-css` is worth doing only alongside the next UI component update, when someone is looking at the
  animations anyway.
- **12d** InMemory stays for logic tests. The limitation it carries is already recorded in ADR-0037 —
  in-memory ignores `citext`, unique indexes and SQL translation — so the only change is one sentence in
  `.github/instructions/dotnet-tests.instructions.md` saying that behaviour must be proved in the hosted
  project, citing that record.

**Route:** 12a and 12b share one small housekeeping PRD; 12c and 12d are a keep plus a sentence.

---

## Follow-up: forwarded headers (fixed in `ceb57419`)

**Closed 2026-11-16.** The trust model is recorded in ADR-0052 and implemented in
`ForwardedHeadersOptionsSetup`, which reads `X-Forwarded-For` and `X-Forwarded-Proto` with `ForwardLimit = 1`
and trusts whoever sends them, because nothing reaches the container except through the ingress. Two items
were raised and both were declined:

1. **Confirming `AuthSessions.IpAddress` after a deploy.** Not needed. How the Container Apps ingress sets
   the forwarded headers is documented platform behaviour, and the API's side of it is verified in the code,
   so there is nothing here left to prove.
2. **Adding `X-Forwarded-For` to the local nginx proxy.** Declined. It is local-only, and on one machine it
   changes the recorded address from nginx's container IP to the host's Docker bridge address without
   changing behaviour. nginx is not in the Azure API path either: the client is built with an absolute API
   URL and `nginx.azure.conf` has no `/api/` proxy, so the browser reaches the ingress directly. Adding the
   header there, if a proxy hop ever appeared, would make things worse — `$proxy_add_x_forwarded_for`
   appends the immediate peer, and `ForwardLimit = 1` reads the rightmost entry, so the API would start
   recording the proxy instead of the caller.
