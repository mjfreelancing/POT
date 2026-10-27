# TASKS — PRD-031 Automated tenant scenario testing

> Temporary delivery plan. Never committed; delete when the PRD is Complete.

- PRD: `Docs/Future/PRD-031-automated-tenant-scenario-testing.md`
- Status: In Progress
- Started: 2026-10-27
- Progress: 0/35

## UI testing checkpoints

The product's client does not change in this work; what a person can exercise by hand is the harness itself.

| After       | What can be tested by hand                                                                                      |
| ----------- | --------------------------------------------------------------------------------------------------------------- |
| **Stage 5** | The browser suite starts its own mail sink; a person can read an invitation it captured and sign in with it.    |
| Stage 6     | The two destructive browser journeys provision and destroy their own tenant, and the seeded identities survive. |
| Stage 7     | The opt-in commands are discoverable, state what they run, and report their duration.                           |

## How this plan is organised

Seven stages, each leaving both builds green and each testable on its own.

Every stage follows the same order: **tests first**, then the code that makes them pass, then the targeted run and formatting. Where a stage introduces behaviour that does not exist, its tests start **red** for the expected reason. Where a stage fills a gap in behaviour that already holds, its tests start **already green** — each sub-task says which applies, because a test that passes while claiming to cover new behaviour proves nothing.

Stage 5 splits in two, deliberately. Its script logic is covered by Pester tests, because that logic is what has repeatedly cost hours — the required-versus-tolerated port split and the process matching were both written in response to real failures. The conditions that need a real process, a real port and a real container are still exercised by hand, because a mock cannot prove those.

Conventions: server commands run from `Source/Server`, and changed C# files are formatted with `dotnet format style`. Client commands run from `Source/Client/pot-react`. Never reference the PRD, stages or requirement IDs in code, tests, comments or test names — describe the behaviour instead. No EF migrations are expected: nothing in this work changes the schema.

Every stage carries its own documentation task, because documentation written at the end is documentation written from memory. The files this work touches are `.github/instructions/aspnetcore-integration-tests.instructions.md` (Stages 1–4), `.github/instructions/e2e.instructions.md` and `e2e/README.md` (Stages 5–7), `e2e/AUTHORING.md` (Stage 6) and `.github/skills/run-tests/SKILL.md` (Stages 5 and 7).

Requirement coverage by stage: 1 → A1, A2, A4 (server), A5 · 2 → B1–B6 · 3 → C1–C4, E1–E4 · 4 → D1–D5, H1 · 5 → A3, A4 (browser), A6–A8, G5 · 6 → F1–F3 · 7 → G2–G4, H2. Everything else, including G1, is satisfied by where the work lands and is checked in Completion.

## Stage 1 — Reading what the application sends

- Goal: a server test can read a message the application has just submitted, including an invitation's temporary password.
- Touches: `Pot.AspNetCore.Integration.Tests/Host` (the test factories and a capturing writer), `Host/Extensions` (the one helper that turns a message into a usable secret).
- Validation: `dotnet test Pot.AspNetCore.Integration.Tests/Pot.AspNetCore.Integration.Tests.csproj --filter "FullyQualifiedName~MessageCapture"`
- Progress: 0/5 complete
- Notes: the integration host already removes every hosted service, so nothing is sent today and only the writer has to change. The production factory is exercised too, so the guarantee that the shipped composition cannot record is asserted rather than assumed.

- [ ] 1.1 **Tests (red)** — a submitted message is readable afterwards with its type and full content; the production composition resolves the real channel rather than a recorder.
- [ ] 1.2 **Green** — a capturing writer in the test host, queryable by recipient and message type.
- [ ] 1.3 **Green** — one helper that returns an invitation's temporary password from the captured messages, so no test parses a body itself.
- [ ] 1.4 **Docs** — record the capturing writer and the message helper in `.github/instructions/aspnetcore-integration-tests.instructions.md`, so the next test is written against them rather than around them.
- [ ] 1.5 Run the targeted tests and format the changed files.

## Stage 2 — Scenarios provisioned through the routes

- Goal: a test declares a tenant and receives the identities it needs, provisioned through the flows a person uses.
- Touches: `Pot.AspNetCore.Integration.Tests/Scenarios` (declaration model, provisioner, catalogue), `Host/Extensions`.
- Validation: `dotnet test Pot.AspNetCore.Integration.Tests/Pot.AspNetCore.Integration.Tests.csproj --filter "FullyQualifiedName~Scenarios"`
- Progress: 0/5 complete
- Notes: an invited user is created `Pending`, so a scenario that needs the invitee to act approves them through the approval route with the platform-administrator host, the way the approvals fixtures already do. The direct-write escape hatch requires a stated reason at the declaration site, which is what keeps B3 honest.

- [ ] 2.1 **Tests (red)** — a declared scenario provisions through the routes and returns usable credentials; two scenarios in one run cannot collide; a failing step reports the step and the response it received.
- [ ] 2.2 **Green** — the declaration model and provisioner: invite, read the password from the capture, sign in, then change status and roles through the endpoints.
- [ ] 2.3 **Green** — the direct-write escape hatch for states the product cannot produce, with a required reason recorded where the scenario is declared.
- [ ] 2.4 **Docs** — record the scenario convention, and the requirement that a direct write carries its reason, in `.github/instructions/aspnetcore-integration-tests.instructions.md`.
- [ ] 2.5 Run the targeted tests and format the changed files.

## Stage 3 — The endpoint inventory and the authorization matrix

- Goal: every documented operation is exercised for authorization, and an operation nobody classified fails the suite.
- Touches: `Pot.AspNetCore.Integration.Tests/Pipeline` (inventory and matrix), the test host (reading each endpoint's declared authorization).
- Validation: `dotnet test Pot.AspNetCore.Integration.Tests/Pot.AspNetCore.Integration.Tests.csproj --filter "FullyQualifiedName~Authorization"`
- Progress: 0/5 complete
- Notes: the inventory and the matrix land together, because the matrix is what makes an operation covered — splitting them would leave the suite red between stages. The generated contract documents 47 operations today, so the ignore list starting small is a realistic expectation rather than an aspiration.

- [ ] 3.1 **Tests (red)** — an operation absent from the matrix and absent from the ignore list fails, naming its method and path; an ignore with no stated reason fails; a foreign-tenant request is answered as not-found.
- [ ] 3.2 **Green** — the inventory derived from the generated contract, with the ignore list and its reasons.
- [ ] 3.3 **Green** — the matrix generated per operation: anonymous, each role, and a foreign tenant where a row is addressed, with the expectation read from the endpoint's declared authorization.
- [ ] 3.4 **Docs** — record how the inventory and matrix decide coverage, and what makes an ignore acceptable, in `.github/instructions/aspnetcore-integration-tests.instructions.md`.
- [ ] 3.5 Run the targeted tests and format the changed files.

## Stage 4 — Isolation invariants

- Goal: a destructive action provably touches only its own tenant, and says what moved when it does not.
- Touches: `Pot.AspNetCore.Integration.Tests/Invariants` (fingerprinting, entity classification), the data context for table enumeration.
- Validation: `dotnet test Pot.AspNetCore.Integration.Tests/Pot.AspNetCore.Integration.Tests.csproj --filter "FullyQualifiedName~Isolation"`
- Progress: 0/5 complete
- Notes: the invariant runs against the three destructive actions — removing another user, removing your own account, and deleting the site. This stage and Stage 3 are the always-on tier, which is why they land in the ordinary server suite and add no container of their own.

- [ ] 4.1 **Tests (red)** — a canary tenant is unchanged across each destructive action; the target tenant's non-target rows are unchanged; no row references a tenant or user that no longer exists; a failure names the tenant, table, row and first difference.
- [ ] 4.2 **Green** — per-table fingerprints and the classification of every declared entity as tenant-scoped or global, where a newly declared entity fails until it is classified.
- [ ] 4.3 **Green** — the invariants applied to all three destructive actions.
- [ ] 4.4 **Docs** — record the invariant convention and the rule that a newly declared entity must be classified, in `.github/instructions/aspnetcore-integration-tests.instructions.md`.
- [ ] 4.5 Run the targeted tests and format the changed files.

## Stage 5 — The mail sink and the run tooling

- Goal: the browser suite can read an invitation, and a run that cannot start the sink fails before any test rather than during one.
- Touches: `playwright.globalSetup.ts` and its teardown, `e2e/helpers` (the sink reader), `e2e/scripts/` (preflight, clean, logged run, process helpers) and a new `e2e/scripts/tests/` Pester suite, `package.json` (the command that runs it).
- Validation: the script suite through its documented command, then the preflight and clean scripts in three stated conditions, then one browser spec.
- Progress: 0/6 complete
- Notes: Pester 5 runs these scripts' decisions with mocked ports and processes — fast, and deterministic where a hand check is not. It does **not** replace the three conditions that need real processes and ports. The framework is not the cost: `e2e-preflight.ps1` and `e2e-processes.ps1` are already function-based, but `e2e-clean.ps1` is straight-line today, so its decisions move into functions before they can be tested — an improvement worth having on its own. Pin the module with `-MinimumVersion 5.0`, because a legacy 3.4.0 is also installed and would fail confusingly. The clean script's documented rule that Docker is never touched narrows to "never the development stack", which is what A8 asks for. A latent defect these tests expose is fixed here when the fix is small and local, in the same change as the test that found it; if it would outgrow the script change, it is recorded in Deferred and raised rather than letting the stage sprawl.

- [ ] 5.1 **Tests (already green)** — characterise what the scripts decide today: which ports are required and which tolerate a leftover, which processes are identified as a daemon, a stray dev server and the harness API, the refusal while a run is in progress, and the exit codes.
- [ ] 5.2 **Tests (red)** — the sink's ports appear in the report, a leftover sink is detected rather than tolerated silently, and the clean command removes only a test-owned sink.
- [ ] 5.3 **Green** — extract the clean script's decisions into functions, change the scripts for the sink, start and stop the sink with the run, and add the browser-side reader helper.
- [ ] 5.4 **Green** — one documented command runs the script suite, and the opt-in tier documentation names it.
- [ ] 5.5 **Docs** — document the sink's ports, the clean command's narrowed policy and the script suite's command in `e2e/README.md` and `.github/instructions/e2e.instructions.md`, and add the suite to `.github/skills/run-tests/SKILL.md`.
- [ ] 5.6 **Verify** — run the scripts with a port held by a non-container process, with a leftover sink present, and with the development stack up; then run one browser spec; record the output.

## Stage 6 — Destructive browser specs provision their own tenant

- Goal: the browser journeys that destroy an account or a site create their own tenant, so a run consumes nothing the seed must supply.
- Touches: `e2e/tests/settings/accountDeletion.test.ts`, `e2e/tests/settings/siteDeletion.test.ts`, `e2e/helpers` (scenario helpers for browser specs).
- Validation: those two specs on chromium, run twice without reseeding.
- Progress: 0/4 complete
- Notes: the seed keeps the identities a scenario cannot create — a platform administrator comes from configuration rather than data — so this stage replaces what the two specs consume, not the seed itself.

- [ ] 6.1 **Red then green** — the account-deletion spec provisions its own viewer, deletes it, and leaves the seeded identity untouched.
- [ ] 6.2 **Red then green** — the site-deletion spec provisions its own site and administrator, deletes it, and leaves the seeded site untouched.
- [ ] 6.3 **Docs** — update `e2e/AUTHORING.md` and `e2e/README.md` so a destructive spec is expected to provision its own tenant rather than consume a seeded identity.
- [ ] 6.4 **Verify** — run both twice without reseeding, and confirm they execute in every configured browser project rather than a subset.

## Stage 7 — The trigger, the duration and the diagnostics

- Goal: the opt-in tiers are discoverable, report how long they took, and a harness failure cannot be mistaken for a product defect.
- Touches: `e2e/scripts` (duration reporting), `Source/Client/pot-react/e2e/README.md`, `.github/skills/prd-delivery/SKILL.md` (the completion prompt).
- Validation: run each documented command and read its output; run the always-on tier twice and compare.
- Progress: 0/5 complete
- Notes: the workflow prompt is the only reliable trigger while there is no pipeline, so it is a deliverable rather than an afterthought. It states what would run, not just that something would.

- [ ] 7.1 **Green** — documented commands for the opt-in tiers, each reporting its duration when it finishes.
- [ ] 7.2 **Green** — the delivery workflow asks at completion whether to run the opt-in tiers, stating what they cover.
- [ ] 7.3 **Green** — a failure caused by provisioning says so, so it is not read as a product defect.
- [ ] 7.4 **Docs** — update `.github/skills/run-tests/SKILL.md` with the opt-in commands and what each tier covers, and `e2e/README.md` with the duration reporting.
- [ ] 7.5 **Verify** — two consecutive runs of the always-on tier produce the same result.

## Completion

- [ ] C.1 Cross-reference every requirement (A1–A8, B1–B6, C1–C4, D1–D5, E1–E4, F1–F3, G1–G5, H1–H2) and record where each is met
- [ ] C.2 Server: full build with analysis warnings cleared (never edit a migration; suppress selectively) and the full suite
- [ ] C.3 Client: production build (`npm run build`) and the full unit suite
- [ ] C.4 Client E2E: the prodlike matrix, since the browser specs change
- [ ] C.5 Coverage of every new or changed line and branch
- [ ] C.6 Manual test plan: the deliberately-broken checks (A1–A3, B1–B3, C1–C3, D1–D3)
- [ ] C.7 PRD bookkeeping and delete this file

## Deferred / follow-up

- **Journeys beyond the account and site lifecycle** — the machinery spans every endpoint from Stage 3, and journeys for accounts, expenses, incomes, projections, exports and imports grow from here as separate work.
- **The invitation credential change** — expiry and an expired state, tracked by its own document; the harness reads messages either way, so only one helper changes.
- **A pipeline or runner** — deliberately out of scope; the workflow prompt carries the trigger instead.
- **Pester coverage of the other scripts** — `code_coverage.ps1`, `test-e2e-seed-export.ps1` and the Docker starter scripts are not covered by this stage; the suite Stage 5 introduces is the place to add them, script by script, when one next causes trouble.
- **Developer-guide documentation for the harness** — `Docs/LOCAL-SETUP.md` and `Source/Server/DEVELOPER.md` are left untouched, because the documentation set is due to be converted into ADRs. The material to carry across is recorded in the PRD's out-of-scope section so the conversion picks it up.
