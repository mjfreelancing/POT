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

- DD-04, DD-06 and DD-16: no infrastructure-as-code;
- DD-06: the local data folders.

Run the availability check in `DOCS-WORK-LISTS.md` ("Files reported missing, and where they turned out to be") first; it has been run once, and its result is recorded there. Never copy the contents of a git-ignored file into a brief or an ADR.

The briefs are ordered by severity, highest first. A follow-up on an item already fixed is at the end.

## Summary

| ID    | Item                                                                                            | Severity            | Recommendation                                                                                       | Needs migration?             |
| ----- | ----------------------------------------------------------------------------------------------- | ------------------- | ---------------------------------------------------------------------------------------------------- | ---------------------------- |
| DD-02 | Etag checked in code but not a database concurrency token; client types it as `bigint`          | Med (integrity)     | B + C: concurrency token, a monotonic value and `z.number()`                                         | Model-only (empty migration) |
| DD-03 | Money stored and calculated as `double`                                                         | Med (correctness)   | C: `decimal` / `numeric(18,2)` end to end                                                            | Yes: DB + data               |
| DD-04 | Email queued in process; reminders assume one replica                                           | Med (reliability)   | A now (pin to one replica), C (the outbox PRD) later                                                 | Not for A; yes for C         |
| DD-06 | PostgreSQL 13 locally and in tests, 16 in E2E, 17.6 in Azure                                    | Med (drift)         | B: align everything on the Azure version                                                             | No (local data rebuild)      |
| DD-07 | Client API types are hand-written                                                               | Med (drift)         | B: generate types from the committed contract                                                        | No                           |
| DD-14 | No policy for API/client version skew while an old PWA shell is cached                          | Med (skew)          | A: record that no window exists, correct the guide                                                   | No                           |
| DD-08 | One-time codes stored in plaintext, and written to logs                                         | Low (contained)     | B: stop logging codes; keep storage as-is                                                            | No                           |
| DD-09 | Permission policies are string literals                                                         | Low (fails closed)  | B: named constants and a test for every endpoint (now one assertion over the new endpoint inventory) | No                           |
| DD-10 | DI by marker-interface scanning: code and server guide agree, `server.instructions.md` does not | Low (documentation) | A: keep scanning, correct the instructions                                                           | No                           |
| DD-13 | Features import other features (and shared layers import features)                              | Low (structure)     | C: move auth and shared pieces into layers, enforce with lint                                        | No                           |
| DD-15 | `WhenNotNull()` on injected dependencies: the server guide and the instructions disagree        | Low (consistency)   | A: keep the guards, correct the instructions                                                         | No                           |
| DD-16 | No server health check in the prodlike compose stack                                            | Low (local only)    | B: readiness health check, client waits for it                                                       | No                           |
| DD-12 | Smaller items (Cronos, sonner theme, animate plugin, EF InMemory, ERD path)                     | Low (housekeeping)  | Per item, see brief                                                                                  | No                           |

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

> **Your call:**

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

> **Your call:**

---

### DD-04 — Email queued in process; reminders assume one replica

- **Backlog row:** 0049 (`Acc + Dec`), related to 0050.
- **What the code does today:**
  - Email goes through an unbounded in-memory channel (`Pot.EmailSender/SendEmailChannel.cs:27-31`), drained by `SendEmailWorker` (`Pot.AspNetCore/Features/Workers/SendEmailWorker.cs:17-20`). If a send throws, the error is logged and the message is dropped, with no retry anywhere in `Pot.EmailSender` (`SendEmailChannel.cs:62,68-71`). Anything still queued is lost on restart. OTP, sign-up and reset emails all go this way.
  - `BudgetReminderEmailWorker` runs once at start-up and then on every UTC hour (`Features/Workers/BudgetReminderEmailWorker.cs:29-104`, `:89-90`). Whether to send depends only on "current local hour == configured hour" (`Pot.App/Features/Notifications/BudgetReminder/BudgetReminderService.cs:65`). There is no record that a reminder was sent.
  - Database migrations also run on every container start (`Source/Docker/Server/entrypoint.sh`).
  - PRD-0019 (outbox and durable dispatch) is **Proposed**, priority High, dated 2026-09-23 (`Docs/Future/README.md:36`). PRD-0018 is its prerequisite and is also Proposed (`:35`).
  - The repo has no infrastructure-as-code, so the replica count in Azure cannot be confirmed from the code.
  - **Duplicates happen with one replica too,** not just with several. A restart inside the trigger hour sends that hour's reminder again. A failed SMTP send is lost even when the process does not restart.
- **Why it matters:** A user who requests a sign-up code during a deploy may never receive it. If the container restarts at 07:20 and reminders go out at 07:00, every admin gets a second reminder. Scaling to two replicas doubles every reminder and runs migrations concurrently.
- **Options:**
  - **A. Keep as-is (record honestly), and pin Azure to one replica.** Set the Container App's minimum and maximum replicas to 1. The ADR states the single-replica constraint and the loss and duplicate cases. Effort S. Infra change only.
  - **B. Targeted hardening.** Add a per-user "last reminder date" so restarts don't send a second reminder, and an in-process retry with backoff for failed sends. Effort M. DB migration (one column or table). Restart loss remains.
  - **C. Deliver the outbox PRD.** Effort L. DB migration. Solves loss, duplicates and the replica limit together.
- **Recommendation:** A now, and C when PRD-0018/019 are scheduled. Pinning replicas costs nothing and makes the current assumption true. B would be work that C then replaces.
- **Route if changed:** A is an infra setting plus the ADR. C goes through the existing PRD-0019 via the prd-delivery flow, with its migration generated by a human.

> **Your call:**

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
  - The only source for "Azure runs 17.6" is a diagram label (`Docs/Azure/POT-Azure-Diagram.mmd:18`). There is no infrastructure-as-code to confirm it.
  - PostgreSQL 13 reached end of life in November 2025.

- **Why it matters:** The integration tests prove behaviour on 13, but production runs 17. Differences in planner behaviour, `citext`, collation and new defaults would show up in production first. The 13.6 hint is harmless against a 17 server: EF just avoids newer SQL. The real risk is the drift between test and production, and running an unsupported version locally.
- **Options:**
  - **A. Keep as-is (record honestly).** The ADR records all three versions and the drift. Effort: none.
  - **B. Align everything on the Azure version.** First confirm the Azure server version in the portal. Then update the Dockerfile, both compose files, the integration-test image, the Playwright image and `SetPostgresVersion`. Effort S. No EF migration. **Local data:** the existing `Source/Docker/postgres-data` directory, and the `postgres-data` folders under `Data/Local Docker` and `Data/Seed Docker`, are version 13 clusters and will not start under 17. You would need to dump and restore it, or recreate it, yourself; agents never touch that folder.
  - **C. B plus a single source for the version.** Put the image tag in `Source/Docker/.env` and the test configuration, so the version cannot drift again. Effort M.
- **Recommendation:** B. It is a small change that makes the tests prove the production engine. C is worth it only if the version drifts again.
- **Route if changed:** raise a PRD via the prd-discovery skill. The local data rebuild is a manual step for you.

> **Your call:**

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

> **Your call:**

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

> **Your call:**

---

### DD-08 — One-time codes stored in plaintext, and written to logs

- **Backlog row:** 0033 (`Acc + Dec (plaintext codes)`).
- **What the code does today:**
  - `RefCode` and `OtpCode` are six-digit strings stored as-is (`Pot.Data/Entities/OneTimePasswordEntity.cs:51-59`). They are generated with `RandomNumberGenerator` (`Pot.App/Features/Auth/OtpGenerator.cs:20`), expire after 15 minutes (`Features/Otp/OtpService.cs:23,152`), allow 3 attempts (`Features/Auth/VerificationServiceBase.cs:26,128-134`), and are checked with a plain `==` (`:106`).
  - The temporary password is stored only as a hash (`OneTimePasswordEntity.cs:65-66`). A successful reset sets the user's password to that hash (`PasswordReset/Verify/VerifyPasswordResetService.cs:84`). **So a stolen code alone does not let anyone sign in.** Signing in still needs the emailed temporary password. Someone who already knows the username can use a stolen code to complete a pending reset, which stops the old password working.
  - `RefCode` is not a secret: it is returned to the browser (`Signup/Request/RequestSignupService.cs:71`).
  - **The verification code is written to the logs.** `LogCall(…, new { Username, ReferenceCode, VerificationCode })` appears at `VerifyPasswordResetService.cs:63`, `Signup/Complete/VerifySignupService.cs:78` and the two handlers (`Pot.AspNetCore/Features/Auth/PasswordReset/Verify/Handler.cs:16`, `Signup/Complete/Handler.cs:17`). `LogCall` defaults to `Information` (AllOverIt.Logging 9.4.0), and the default level is `Information` (`Pot.AspNetCore/appsettings.json:4`).
- **Why it matters:** Anyone with read access to the database or the logs can see live codes for 15 minutes. Because of the hashed temporary password, the damage is limited to forcing a pending reset or sign-up through. That is real but contained. Secrets in logs are the more general problem, because logs are kept longer and shared more widely than the database.
- **Options:**
  - **A. Keep as-is (record honestly).** The ADR records plaintext storage and the reasons the impact is limited. Effort: none.
  - **B. A plus remove `VerificationCode` from the four `LogCall` sites.** Effort S. No migration.
  - **C. Hash `OtpCode` with HMAC-SHA256 under a server-held key** (an unkeyed hash of a six-digit code can be brute-forced instantly), and compare in constant time. Effort M. **DB migration** (column length). Codes active at deploy time stop working, which is acceptable with a 15-minute life. Infra: a new secret. `RefCode` stays plaintext.
- **Recommendation:** B. It closes the larger leak for almost no cost. C adds little while the temporary password is hashed, and could be folded into PRD-0032 if that reworks credentials.
- **Route if changed:** raise a PRD via the prd-discovery skill. For C, a human generates the migration.

> **Your call:**

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

> **Your call:**

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

> **Your call:**

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

> **Your call:**

---

### DD-15 — `WhenNotNull()` on injected dependencies: the server guide and the instructions disagree

- **Backlog row:** none.
- **What the code does today:**
  - The server guide's service pattern says "All dependencies validated with `.WhenNotNull()`" (`Source/Server/DEVELOPER.md:1875`). Its example matches `Pot.App/Features/Accounts/Create/CreateAccountService.cs:27-30`.
  - `.github/instructions/allsoverit-patterns.instructions.md:18` (reworked 2026-10-03, commit `8140ff76`) says the opposite: "Constructor parameters resolved by DI need no null guard … Keep `WhenNotNull()` … only when the class is also created by a factory that may pass `null`."
  - The code follows the guide. There are 356 `_x = x.WhenNotNull();` assignments in 157 non-test server files, against 25 plain `_x = x;` assignments (a few of which are not DI constructors).
  - Removing the guards has no test cost: I found no constructor null-guard tests for services. The 63 "Should*Throw_When*…\_Null" tests cover method arguments, for example `Pot.App.Tests/Calculators/AccrualCalculatorFixture.cs:73`.
- **Why it matters:** Agents read the instruction and write unguarded constructors, while reviewers and the guide expect guards. The result is a codebase that drifts into two styles, and review churn. Neither style changes runtime behaviour: the container never passes `null`.
- **Options:**
  - **A. The guide wins: keep `WhenNotNull()` on every injected dependency.** Change the instruction line. Effort S. Docs only. Matches about 94% of the existing code.
  - **B. The instruction wins, without a sweep.** Change the guide, and leave existing guards in place. Effort S. Docs only. The codebase keeps both styles indefinitely.
  - **C. The instruction wins, with a sweep.** Change the guide and remove the guards from about 157 files. Effort M (mechanical). No migration, contract or client change. A large diff for no behaviour change.
- **Recommendation:** A. It matches the code, costs nothing at runtime, and still catches a `null` passed by a hand-written test or factory. If you made the instruction change on purpose on 2026-10-03, choose B instead and say so, and the ADR will record the reason.
- **Route if changed:** A and B are docs changes made with the ADR. C needs a PRD via the prd-discovery skill.

> **Your call:**

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

> **Your call:**

---

### DD-12 — Smaller items

- **Backlog rows:** none directly, apart from 0052 (server test strategy) for 12d and the plan's section 6.3 note for 12e.
- **What the code does today, item by item:**
  - **12a — Cronos.** Referenced at `Source/Server/Directory.Packages.props:21` and `Pot.AspNetCore/Pot.AspNetCore.csproj:17`. No `using Cronos` or other use anywhere.
  - **12b — sonner and next-themes.** `src/components/ui/sonner.tsx:10,14` calls `useTheme` from `next-themes`, but no `next-themes` provider is mounted, so `theme` is always `"system"`. The app's own `ThemeProvider` (`src/App.tsx:56`) exports its own `useTheme` (`src/components/theme/ThemeProvider.tsx:109`). Toasts follow the OS setting, not the user's chosen theme. The colours mostly come from the app's CSS variables, so the visible effect is small.
  - **12c — tailwindcss-animate.** Loaded with `@plugin "tailwindcss-animate"` (`src/index.css:3`) and used in about 15 places (`animate-in`, `fade-in-…`). It is a v3-era plugin, but Tailwind 4 loads it through `@plugin` and it works. The issue is maintenance, not breakage. shadcn now recommends `tw-animate-css` for v4.
  - **12d — EF InMemory.** Most use is in `Pot.Data.Tests`, not the App unit tests. 15 repository test files use `UseInMemoryDatabase`, against 4 in `Pot.App.Tests` (for example `Features/Accounts/Create/EntityChecks/Checks/CheckDescriptionDoesNotExistFixture.cs:99`). InMemory ignores `citext` (`Pot.Data/DbContextBase.cs:73`), unique indexes and SQL translation, so the case-insensitive duplicate checks are not really tested there. The Testcontainers integration project covers some of this.
  - **12e — ERD path.** `Pot.Data.Migrations/App.cs:61` writes to `"..\\..\\..\\..\\..\\..\\Docs\\pot_erd.d2"`, only in Development. The path is relative to the working directory, uses Windows separators, and assumes the app is run from `bin/<config>/<tfm>`. **Correct.**
- **Why it matters:** none of these breaks production. 12d is the one that can hide a real defect.
- **Options and recommendation per item** (all effort S, no migration, no contract change):
  - **12a:** remove the package (recommended), or keep and record.
  - **12b:** use the app's `useTheme` in `sonner.tsx` and remove `next-themes` (recommended, client only), or keep and record.
  - **12c:** switch to `tw-animate-css` and check the animations still look right, or keep and record (recommended until the UI components are next updated).
  - **12d:** keep InMemory for logic tests, and record that SQL, `citext` and constraint behaviour must be proved in the integration project (recommended). The alternative is to move `Pot.Data.Tests` to Testcontainers: effort M, slower runs, needs Docker.
  - **12e:** keep as-is if the docs reorganisation leaves `Docs/pot_erd.d2` in place. If it moves, make the path a setting in `Pot.Data.Migrations/appsettings.json` built with `Path.Combine` (recommended only if the move happens).
- **Route if changed:** 12a, 12b, 12c and 12e can share one small housekeeping PRD via the prd-discovery skill. 12d's recommended option is an ADR and test-guideline change only.

> **12a Your call:**

> **12b Your call:**

> **12c Your call:**

> **12d Your call:**

> **12e Your call:**

---

## Follow-up: forwarded headers (fixed in `ceb57419`)

The forwarded-headers problem is fixed. Its trust model is recorded in backlog rows 0074 and 0036. Two small items remain:

1. **After the next deploy:** confirm that `AuthSessions.IpAddress` in production holds real client addresses, not the ingress's private address.
2. **Local prodlike stack only:** `Source/Docker/Client/nginx.conf` proxies `/api/` without `X-Forwarded-For`, so locally every anonymous caller shares nginx's rate-limit partition. Fixing it is one line: `proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;`.

> **Your call (item 2):**
