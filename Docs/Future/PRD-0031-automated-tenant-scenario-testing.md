# Automated Tenant Scenario Testing PRD

**Status:** Complete
**Priority:** High
**Last Updated:** 2026-11-02
**Feature ID:** 0031
**Discovery:** complete
**Scope:** A repeatable automated harness that proves the account and site lifecycle behaves correctly and that a destructive action touches only its own tenant's data. It covers reading the messages the application sends, so a test can complete an invitation, provisioning realistic tenants through the product's own routes, an endpoint inventory that cannot silently fall behind, isolation invariants around every destructive action, and an authorization matrix asserted against each endpoint's own declaration. It covers the server suites, the browser suite, the commands that prepare and clean the browser run's environment, and the delivery workflow's prompt to run the opt-in tiers. It does not cover a pipeline or runner, load and performance testing, changing how invitations deliver their credential (addressed by a separate document), or any change to the deployed Docker or Azure topology.

## Problem Statement

Manual end-to-end testing of the account and site lifecycle has stopped happening. The plan written for it went unrun because its setup alone — several tenants, five account states, a mail sink, a platform-administrator configuration change and a restart — costs more time than the confidence it buys, and every run starts from nothing. The behaviour it covered is the most consequential in the product: who may remove whom, and whether a removal takes anything it should not.

The automated suites do not fill the gap, for three separate reasons.

**A secret a test needs exists only in an email.** An invitation generates a temporary password, mails it, and stores only its hash. Nothing else records it. A scenario that signs in as an invited user is therefore unreachable to a test, which is why user creation is exercised by inserting rows directly and never through the flow a person uses.

**There is no way to provision a realistic tenant.** Scenarios exist as prose in documents, not as fixtures. A spec that needs a second administrator, a disabled peer or a pending user hand-rolls database writes of its own, and the browser suite has six fixed seeded identities that the two destructive specs consume. A run that destroys one of those identities leaves the next run unable to repeat it.

**Nothing demonstrates isolation.** The tests show that deleting a user removes that user. They do not show that deleting one tenant left every other tenant's rows untouched, and no test sweeps the endpoint surface for a cross-tenant or authorization hole when an endpoint is added. That second property is argued from reading the code rather than demonstrated by it, and it is the property that matters most.

The consequence is that a security-critical area's assurance depends on a person remembering to run a checklist, and the checklist is the thing nobody has time to run.

## Actors and Authority

| Actor             | Definition                                                                                        |
| ----------------- | ------------------------------------------------------------------------------------------------- |
| Developer         | Anyone changing product code, who runs the suites and reads their failures.                       |
| Delivery workflow | The staged procedure that implements a feature, which prompts for the opt-in tiers at completion. |
| Test author       | A developer adding a scenario, an invariant or an authorization case.                             |

| Action                                    | Who may perform it                               | What it affects                            |
| ----------------------------------------- | ------------------------------------------------ | ------------------------------------------ |
| Run the always-on invariants              | Any developer, through the ordinary server suite | Test data only; no product code            |
| Run the opt-in scenario and browser tiers | Any developer, deliberately                      | Disposable test database and the mail sink |
| Add a scenario                            | A test author                                    | The harness's scenario catalogue           |
| Classify a newly added endpoint           | A test author                                    | The endpoint inventory                     |

No harness capability is reachable from a running product host. There is no endpoint, configuration value or code path in the shipped application that records messages, exposes test data, or provisions a tenant.

## Terms

- **Scenario** — a declared tenant and its people, records and states, provisioned by the harness and destroyed with it. Declared as data, not as a sequence of ad-hoc calls.
- **Provisioning** — creating a scenario through the product's own routes wherever a route exists, so the flow a person uses is the flow being tested.
- **Fingerprint** — a per-table digest of the rows belonging to one tenant, compared before and after an action to prove nothing outside that tenant changed.
- **Canary tenant** — a tenant provisioned alongside the one under test, deliberately untouched, whose fingerprint must not move.
- **Inventory** — the list of documented operations, taken from the generated contract, each of which must be covered or explicitly ignored with a reason.
- **Always-on tier** — the invariants that run as part of the ordinary server suite on every run.
- **Opt-in tier** — the scenario and browser tiers, which are deliberately invoked.
- **Mail sink** — a test-time SMTP receiver the browser suite reads to obtain a message's contents.

## Functional Requirements

### A. Reading what the application sends

- **A1.** The server suites can read every message the application submits, including its type and full content, without a message leaving the process.
- **A2.** A message is readable after the operation that sent it completes, so a test can act on its contents immediately.
- **A3.** The browser suite reads messages from a mail sink it started, addressed by recipient and message type, and receives the content a person would read.
- **A4.** Reading a message yields the secret a scenario needs — an invitation's temporary password, or a one-time code — through one helper, so no test parses a message body itself.
- **A5.** The shipped application contains no capability to record messages, and none can be enabled by configuration. The sink is a component of the test environment, not of the product.
- **A6.** The sink binds the ports it needs — one to receive mail and one to read it back — and a run that cannot bind them fails before any test runs, naming the process that holds the port.
- **A7.** A sink left behind by an interrupted run is detected and reported at the start of the next run, and that run starts with a fresh sink instead of attaching to the stale one.
- **A8.** The environment clean-up command can remove a leftover sink, and continues to leave the development Docker stack alone.

### B. Scenarios and provisioning

- **B1.** A scenario is declared as data: its tenant, its people with their role and status, and any records it needs. Nothing about a tenant is hard-coded in a test.
- **B2.** A scenario is provisioned through the product's routes wherever one exists: signing up with the one-time code read from the store, inviting with the temporary password read from the sink, and changing a status or a role through the endpoints that do it.
- **B3.** Where a state cannot be produced through the product — a tenant whose only administrator is disabled, for example — the harness writes it directly, and that exception is recorded where the scenario is declared, with the reason.
- **B4.** Every identifier a scenario creates is unique to that scenario and run, so parallel workers cannot collide and no test depends on another's data.
- **B5.** A provisioned scenario reports the identities and credentials a test needs, so a test neither queries the store nor reconstructs them.
- **B6.** A scenario that fails to provision fails with the step that failed and the response it received, rather than a null reference later.

### C. The endpoint inventory

- **C1.** The inventory is derived from the generated contract rather than maintained by hand.
- **C2.** Every documented operation is either exercised by at least one authorization case or listed as ignored with a stated reason.
- **C3.** Adding an operation without deciding its coverage fails the suite, whether it is covered or explicitly ignored.
- **C4.** The ignored list stays small and visible; anything on it names why it needs no case.

### D. Isolation invariants

- **D1.** For every destructive action, a canary tenant's fingerprint is unchanged across the action.
- **D2.** Within the tenant under test, rows the action did not target are unchanged.
- **D3.** The fingerprint covers every tenant-scoped table the data model declares. A table added later cannot be silently omitted: it fails until it is classified as tenant-scoped or global.
- **D4.** After a destructive action, no row references a tenant or user that no longer exists.
- **D5.** A failed invariant reports the tenant, the table, the row identifier, and the expected against the actual value, showing the first difference only.

### E. Authorization

- **E1.** Every documented operation is exercised anonymously, as each role, and — where it addresses a tenant-scoped row — as a user of a different tenant.
- **E2.** The expected outcome of each case comes from the authorization the endpoint itself declares, not from a second hand-written table of intended outcomes.
- **E3.** A request naming a row that belongs to another tenant is answered as though the row did not exist, disclosing nothing about its existence.
- **E4.** An operation added later is included automatically, because the cases are generated from the inventory rather than listed.

### F. Browser scenarios

- **F1.** The destructive browser specs provision their own tenant and destroy it, so a completed run leaves the seeded baseline reusable.
- **F2.** A run leaves no state that a later run depends on, and no identity it consumed.
- **F3.** These specs run in every configured browser project, not a reduced subset.

### G. Execution

- **G1.** The always-on tier runs as part of the ordinary server suite and reuses its database lifecycle, adding no container of its own.
- **G2.** The opt-in tiers are reachable by a documented command, and the delivery workflow asks whether to run them at completion, stating what would run and what it covers.
- **G3.** The suites are deterministic: an unchanged application produces the same result on every run, with no dependence on wall-clock time, execution order, or data left by a previous run.
- **G4.** A suite's duration is reported when it runs, so drift stays visible even though no time limit is imposed.
- **G5.** The commands that prepare and clean the browser run's environment report the sink's ports alongside the client, API and database ports, so an occupant is identified before a run rather than deduced from a connection failure part-way through one.

### H. Diagnostics

- **H1.** Every failure names the behaviour that broke, the input that broke it, and the first observable difference — never a bare status code or a count.
- **H2.** A failure attributable to provisioning rather than to the behaviour under test says so, so a broken harness is not mistaken for a product defect.

## Requirement Coverage

Where each requirement is met, so a reviewer can go from the statement to the code that satisfies it. Server paths are
relative to `Source/Server/Pot.AspNetCore.Integration.Tests/`; browser paths to `Source/Client/pot-react/`.

| Req | Met at                                                                                                                                                                | Proved by                                                                                     |
| --- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| A1  | `Host/CapturingEmailWriter.cs`, `Host/Extensions/CapturingEmailWriterExtensions.cs` — the writer the test host registers instead of the real sender                   | `Pipeline/MessageCaptureFixture.cs`                                                           |
| A2  | the same writer: a message is complete in the capture before the request that sent it returns                                                                         | `Pipeline/MessageCaptureFixture.cs`                                                           |
| A3  | `e2e/helpers/mailSink.ts` (starts `pot-e2e-mail-sink`), `e2e/helpers/mail.ts` `waitForMessage({ to, subjectContains })`                                               | `e2e/tests/smoke/mailSink.test.ts`                                                            |
| A4  | `e2e/helpers/mail.ts` `oneTimeCode()`, `temporaryPassword()` — the only readers of a message body                                                                     | `smoke/mailSink.test.ts`, `settings/accountDeletion.test.ts`, `settings/siteDeletion.test.ts` |
| A5  | no product code records messages: the sink is a container the harness starts (`e2e/helpers/mailSink.ts`) and the app only consumes SMTP settings the configs supply   | manual checks C3 and D3                                                                       |
| A6  | `e2e/scripts/e2e-ports.ps1` declares 1025/1080; `e2e/scripts/e2e-preflight.ps1` reports the owning process                                                            | `e2e/scripts/tests/*.Tests.ps1`                                                               |
| A7  | `startMailSink()` reports and replaces a leftover sink; `e2e/scripts/e2e-processes.ps1` classifies it                                                                 | manual check C3 (a run killed mid-migration)                                                  |
| A8  | `e2e/scripts/e2e-clean.ps1` removes only the harness's own sink and daemon                                                                                            | manual check D3, Pester cases                                                                 |
| B1  | `Scenarios/ScenarioDefinition.cs`, `ScenarioMember.cs`, `ScenarioUser.cs`                                                                                             | `Features/Scenarios/ScenarioProvisioningFixture.cs`                                           |
| B2  | `Scenarios/ScenarioProvisioner.cs` — signup with the one-time code, invitations with the temporary password, status and role changes through their endpoints          | `Features/Scenarios/ScenarioProvisioningFixture.cs`                                           |
| B3  | `Scenarios/ScenarioDirectWrite.cs`, with the reason recorded where the scenario is declared                                                                           | `Features/Scenarios/ScenarioProvisioningFixture.cs`                                           |
| B4  | identifiers built per scenario and run in `ScenarioProvisioner`, so workers cannot collide                                                                            | the isolation, authorization and browser suites all run in parallel                           |
| B5  | `Scenarios/ProvisionedScenario.cs` reports the identities and credentials a test needs                                                                                | `Features/Scenarios/ScenarioProvisioningFixture.cs`                                           |
| B6  | `Scenarios/ScenarioProvisioningException.cs` carries the step and the response                                                                                        | `Features/Scenarios/ScenarioProvisioningRefusalFixture.cs`                                    |
| C1  | `Pipeline/Support/DocumentedOperations.cs` reads `/openapi/v1.json`, so the inventory is generated                                                                    | `Pipeline/AuthorizationCoverageFixture.cs`                                                    |
| C2  | `Pipeline/Support/InventoryCoverage.cs` matches documented operations against cases and the ignore list                                                               | `Pipeline/AuthorizationCoverageFixture.cs`                                                    |
| C3  | the coverage assertion fails when an operation has neither a case nor an ignore entry with a reason                                                                   | manual check B1/B2                                                                            |
| C4  | `Pipeline/Support/IgnoredOperations.cs` — a blanket ignore and a blank reason are both refused                                                                        | `Pipeline/AuthorizationCoverageFixture.cs`                                                    |
| D1  | `Invariants/IsolationFixture.cs` with `Invariants/Support/TenantSnapshotter.cs`                                                                                       | the three isolation invariants                                                                |
| D2  | `IsolationFixture` (allowed removals) over `TenantFingerprint.FindChanges`                                                                                            | the three isolation invariants                                                                |
| D3  | `Invariants/EntityClassificationFixture.cs` with `Support/EntityClassification.cs`, `TenantScopes.cs`, `GlobalEntities.cs` — an unclassified table fails              | `Invariants/EntityClassificationFixture.cs`                                                   |
| D4  | `Invariants/Support/OrphanSweep.cs`                                                                                                                                   | `Repositories/DeletionSweepFixture.cs`                                                        |
| D5  | `Support/TenantFingerprint.cs` (`FindFirstDifference`) and the canary message in `IsolationFixture`                                                                   | manual check A1–A3                                                                            |
| E1  | `Pipeline/AuthorizationMatrixFixture.cs` over `Support/AuthorizationMatrix.cs`, `AuthorizationCase.cs`, `AuthorizationCaller.cs`                                      | `Pipeline/AuthorizationMatrixFixture.cs`                                                      |
| E2  | `Support/EndpointRequirements.cs`, `EndpointRequirement.cs`, `EndpointOperation.cs` read the authorization the endpoint declares                                      | `Security/EndpointAuthorizationMetadataFixture.cs`                                            |
| E3  | `Support/ForeignTenantRows.cs` names the row each case addresses from another tenant                                                                                  | `Security/CrossTenantAuthorizationFixture.cs`                                                 |
| E4  | the cases are generated from the inventory, so a new operation is included without being listed                                                                       | `Pipeline/AuthorizationMatrixFixture.cs`                                                      |
| F1  | `e2e/helpers/provisioning.ts` (`provisionInvitedViewer`, `provisionSiteAdministrator`) used by `settings/accountDeletion.test.ts` and `settings/siteDeletion.test.ts` | both specs                                                                                    |
| F2  | the same specs assert the seeded identities survive, via `e2e/helpers/databaseSeed.ts`                                                                                | manual check C1/C2 (16 passed twice, no reseeding)                                            |
| F3  | no per-project exclusion remains in `playwright.config.ts` or `playwright.prod.config.ts`; the built-client matrix runs all four projects                             | the prodlike matrix completes across chromium, edge, mobile-chrome and mobile-safari          |
| G1  | `Features/Scenarios/*`, `Invariants/*` and `Pipeline/*` run inside the existing integration fixture and its database lifecycle, adding no container                   | the full integration project                                                                  |
| G2  | `.github/skills/run-tests/SKILL.md` documents the tiers; `.github/skills/prd-delivery/SKILL.md` asks about the opt-in tiers at completion                             | both skill files                                                                              |
| G3  | `e2e/seed/baseline.sql` and fixtures that create their own data; nothing depends on wall-clock time or a previous run                                                 | manual check C1/C2                                                                            |
| G4  | `e2e/scripts/e2e-logged-run.ps1` reports the run's duration; `dotnet test` reports its own                                                                            | the logged run's summary                                                                      |
| G5  | `e2e/scripts/e2e-ports.ps1`, `e2e-preflight.ps1` and `e2e-clean.ps1` all report the sink's ports beside the client, API and database ports                            | manual check D1–D3                                                                            |
| H1  | `Invariants/Support/TenantFingerprint.cs` (first difference with expected against actual), the `IsolationFixture` canary message, and the matrix's per-case messages  | manual checks A1–A3 and B1–B3                                                                 |
| H2  | `e2e/helpers/provisioning.ts` `ProvisioningFailure` states that the harness, not the product, failed                                                                  | `e2e/tests/smoke/provisioningFailure.test.ts`                                                 |

## Out of Scope

- A pipeline, runner or any form of CI configuration — a separate decision, deliberately local-only for now.
- Load, performance and soak testing — a separate concern from behavioural assurance.
- Changing how an invitation delivers its credential — specified by a separate discovery document covering expiry and an expired-invitation status.
- Replacing the browser suite's baseline seed wholesale — the scenarios are additive, and the seed keeps only what a scenario cannot create, such as a platform administrator, which comes from configuration rather than data.
- Any change to the deployed Docker or Azure topology — the mail sink exists only in the test environment.
- Test-only endpoints or production composition changes to support testing — the harness lives in the test projects.
- **Restructuring the developer guides.** The harness documents itself where it lives: `e2e/README.md`, `e2e/AUTHORING.md` and the scoped instruction files an agent loads. `Docs/LOCAL-SETUP.md` gains no E2E section here and `Source/Server/DEVELOPER.md` gains no harness section, because both are due to be converted into ADRs — the E2E material belongs to that pass, recorded here so it is not lost when it happens.

## Known Risks and Accepted Limitations

- **The harness can drift from the product.** A scenario that writes state directly, or that skips an approval step, proves less than it appears to. Accepted because the alternative is no coverage; mitigated by provisioning through routes wherever one exists, by recording every direct-write exception where it is declared, and by the inventory guard failing when an operation is unclassified.
- **Long runtimes discourage running.** The opt-in tiers are expected to be slow, and a slow suite gets skipped. Accepted: no time limit is imposed, and the always-on tier carries the assurance that must not be skipped. If the always-on tier stops being cheap, it moves to the opt-in tier rather than the reverse.
- **The mail sink is another moving part.** It needs an image locally and free ports, and a run that cannot start it fails for a reason unrelated to the product. Accepted, because reading a real message is the only way to reach an invited-user journey without adding a production code path; the run tooling reports its ports and a leftover sink, so that failure is diagnosed before a run rather than during one.
- **Fingerprints only see what they enumerate.** A tenant-scoped table that is never fingerprinted is invisible to the invariants. Mitigated by classifying every declared entity, not by auditing the list by hand.
- **The seeded baseline is still needed.** A platform administrator comes from configuration rather than data, and some journeys start from a seeded site. Accepted: the goal is that destructive specs stop consuming identities, not that the seed disappears.

## Assumptions

- Usernames are unique across the whole product, so a scenario must generate distinct ones rather than reuse a fixed name.
- Only enabled users can authenticate, and a `Pending` user is held back by the approval gate.
- The generated contract documents every routed operation, so it can serve as the inventory's source.
- Container-based infrastructure (as the existing suites already use) is available wherever the suites run locally.
- The delivery workflow is the only reliable trigger while no pipeline exists.

## Decisions Carried Into This Document

| Decision                                                      | Resolution                                                                                                                                                               | ADR                                                                                                  |
| ------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------- |
| How an invited user gets their credential                     | Kept as a temporary password; the harness reads it from the mail sink. Chosen partly because reading mail on a phone while signing in on a computer suits a person.      | —                                                                                                    |
| Ordering against an invitation-link change                    | Not applicable; the harness reads messages either way, so only one helper would change if that change came later.                                                        | —                                                                                                    |
| Which surfaces are covered                                    | The whole application: the machinery spans every endpoint, and scenario journeys begin with the account and site lifecycle and grow from there.                          | —                                                                                                    |
| How a tenant is provisioned                                   | Through the product's routes; direct writes only for states the product cannot produce.                                                                                  | [ADR-0051](../adr/ADR-0051-tenants-are-provisioned-and-destroyed-by-the-products-own-routes-in-tests.md) |
| Where harness code lives                                      | Extended existing projects — the integration test project for server-side pieces, the browser suite's helpers for browser-side pieces. No production composition change. | —                                                                                                    |
| What an isolation check compares                              | A canary tenant's fingerprint, plus the target tenant's non-target rows.                                                                                                 | —                                                                                                    |
| Where the inventory comes from                                | The generated contract; an unclassified operation fails the suite.                                                                                                       | —                                                                                                    |
| How the authorization matrix knows what to expect             | From each endpoint's own declaration, so the wiring is tested rather than the intent restated.                                                                           | —                                                                                                    |
| What makes the suite run                                      | Always-on invariants in the ordinary server suite; opt-in scenario and browser tiers prompted by the delivery workflow.                                                  | —                                                                                                    |
| Do destructive browser specs keep consuming seeded identities | No; they provision and destroy their own tenant.                                                                                                                         | [ADR-0043](../adr/ADR-0043-every-end-to-end-test-is-classified-by-whether-it-may-run-beside-others.md)   |
| Database strategy                                             | One disposable database per run with per-scenario unique identifiers, so workers run in parallel.                                                                        | —                                                                                                    |
| Time budget                                                   | None. Confidence matters more than duration; the run takes as long as it takes.                                                                                          | —                                                                                                    |
| What a failed invariant reports                               | Tenant, table, row identifier, expected against actual, first difference only.                                                                                           | —                                                                                                    |
| Exclusions                                                    | Load and performance testing, any pipeline or runner, replacing the baseline seed wholesale, and the invitation-credential change — all recorded as out of scope.        | —                                                                                                    |

## Open Questions

- **Does the always-on tier stay cheap as the surface grows?** It is expected to, because each endpoint is one row in the inventory and the matrix; if it stops being so, the resolution is to move part of it to the opt-in tier rather than to let the always-on run become slow enough to skip. Closed by observation once the inventory covers the full contract.

## Manual Test Plan

The deliverable is the harness, so the manual checks are the opposite of the usual ones: they prove the harness **detects** faults rather than passing quietly. Each check deliberately introduces a defect, confirms the harness fails with a useful message, and then reverts.

### Setup

1. Work from a clean checkout with the server solution buildable, and run `dotnet test pot.sln` once to confirm the baseline is green.
2. Confirm Docker is available, because the suites start a disposable database.
3. Record the current state of the two files named in M1 and M2 so each can be restored exactly.

### A. The harness notices a leak

- **A1 — a cross-tenant leak fails the isolation invariant.** Temporarily change the user-removal sweep so it no longer restricts itself to the caller's site, then run the isolation invariant for user removal.
  Expected: the run fails, and the message names the tenant, the table and the first row that changed — not a bare count.
- **A2 — the failure is attributed correctly.** Read the failing message and confirm it identifies the canary tenant rather than the tenant under test, so the direction of the leak is obvious.
  Expected: the canary is named as the tenant whose data moved.
- **A3 — revert and confirm green.** Restore the sweep and re-run the same invariant.
  Expected: the run passes, proving the failure came from the change and not from the harness.

### B. The harness notices an unclassified endpoint

- **B1 — an operation whose coverage is undecided blocks the suite.** Temporarily add an operation that names a row of a resource no row is declared for in another tenant — for example `GET /api/sites/probes/{id}` — build, and run the inventory test.
  Expected: the run fails and names the operation by method and path. A plain operation needs no decision: the matrix generates a case for it, which is the inventory working as intended, so this check needs an operation the generator cannot cover on its own.
- **B2 — an ignored operation needs a reason.** Add the new operation to the ignored list with no reason and run again.
  Expected: the run still fails, because an ignore without a stated reason is not accepted.
- **B3 — revert and confirm green.** Remove the endpoint and the ignore entry.
  Expected: the suite passes.

### C. Isolation of the browser scenarios

- **C1 — a destructive spec provisions its own tenant.** Run the destructive browser specs against a freshly seeded database.
  Expected: they pass, and afterwards the seeded identities the previous version consumed are still present and usable.
- **C2 — they can run twice in a row.** Run the same specs again without reseeding.
  Expected: they pass again, proving the earlier run consumed nothing the seed must supply.
- **C3 — a leftover tenant is visible.** Deliberately interrupt a run midway, then start another.
  Expected: the second run is unaffected, because scenario identifiers are unique per run.

### D. A blocked or leftover mail sink is visible before it costs a run

- **D1 — an occupied port is reported, not discovered mid-run.** Hold the sink's receiving port with a process that
  is not a container, then run the preflight check.
  Expected: it reports the port as unusable and names the process holding it, and an attempted run stops before any
  test executes rather than part-way through one.
- **D2 — a leftover sink is replaced.** Interrupt a browser run roughly half-way, then run the preflight check and
  start another run.
  Expected: the preflight reports the leftover sink with enough detail to identify it, and the new run starts a fresh
  sink instead of reading messages left by the interrupted one.
- **D3 — cleaning still leaves the development stack alone.** Start the Docker development stack, run the clean-up
  command, and then start a browser run.
  Expected: the development stack's containers are untouched, only a test-owned sink is removed, and the run starts
  against a freshly started sink.

### Cleanup

- Restore both temporarily modified files exactly as they were, and confirm the full suite is green again.
- Remove any temporary endpoint, ignore entry or scenario added for the checks, and drop the disposable database if it was kept.

### Results

| ID    | Check                                                    | Result | Notes                                                                                                                                                                                                                              |
| ----- | -------------------------------------------------------- | ------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A1–A3 | A leak is detected, attributed and reverts green         | Pass   | A sweep that removed another tenant's rows failed with `tenant <canary site> table 'Income': row 1 is gone`, naming the canary rather than the tenant under test; the isolation invariants passed again once it was reverted.      |
| B1–B3 | An unclassified operation blocks the suite               | Pass   | `GET /api/sites/probes/{id}` was reported as a documented operation naming a row of an unclassified resource, and the same operation ignored with a blank reason was reported too; the suite passed again once both were removed.  |
| C1–C3 | Browser scenarios provision, repeat and leave no residue | Pass   | 16 tests passed twice with no reseeding; a run killed during its migrations left its sink behind, and the next run replaced it and passed.                                                                                         |
| D1–D3 | A blocked or leftover mail sink is caught before a run   | Pass   | An occupied port was reported with the process holding it; a leftover sink was reported and replaced; with the development stack running, clean stopped only the harness's daemon and sink and exited non-zero on the client port. |

## Verification Evidence

The Manual Test Plan's Results table records which checks passed. This section records what was broken to make each one fail,
so the experiment can be repeated, and the measurements a later reader would compare against.

### Deliberately broken, one at a time

Each fault was introduced, the affected suite was run, and the fault was reverted.

| Check | Fault introduced                                                                                      | What the suite said                                                                                                                             | Verdict                                                                                               |
| ----- | ----------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------- |
| A1/A2 | the user-removal sweep ran `Set<IncomeEntity>().IgnoreQueryFilters()`, removing another tenant's rows | `canaryDifference should be null but was "tenant fe8873f6-… table 'Income': row 1 is gone"`                                                     | names the tenant, the table and the first row, and names the canary rather than the tenant under test |
| A3    | the sweep reverted                                                                                    | 3 of 3 isolation invariants pass                                                                                                                | the failure came from the change, not from the harness                                                |
| B1    | `GET /api/sites/probes/{id}` added, naming a row of a resource with no foreign row declared           | `GET /api/sites/probes/{id} names a row but no row of that resource is declared for another tenant - classify the resource so the case can run` | named by method and path                                                                              |
| B2    | the same operation added to the ignore list with a blank reason                                       | `GET /api/sites/probes/{id} is ignored without a stated reason - a reader cannot tell whether it still holds`                                   | a reason is required                                                                                  |
| B3    | the endpoint and its ignore entry removed                                                             | 6 of 6 matrix and isolation tests pass, and `contracts/openapi/v1.json` regenerated to its committed content                                    | green again, so the earlier failures were the fault's                                                 |
| C1/C2 | nothing broken: the browser specs were run twice without reseeding                                    | 16 passed twice                                                                                                                                 | the seeded identities survive a destructive run                                                       |
| C3    | a run killed during its migrations, then run again                                                    | `Mail sink left behind by a previous run detected … replacing it`, then 4 passed (40.4s)                                                        | the next run is unaffected by an interrupted one                                                      |
| D1/D2 | a plain process holding the sink's port, and a sink left behind                                       | the port reported `[FAIL]` with its owner; preflight reported the leftover sink and the killed run's daemon                                     | an occupant is identified before a run                                                                |
| D3    | development stack up, leftover sink present                                                           | clean stopped only the harness's daemon and sink, left `pot-react`, `pot-aspnet` and `pot-postgres` running, and exited 1 on the client port    | the stack is never touched                                                                            |

Two findings from running them. A plain authenticated `GET` needs no classification, because the matrix generates a case for it,
so the unclassified-resource fault only bites when an operation names a row. And the canary message named its tenant by site row
id when these runs were taken; the fixture now names the scenario as well, so a failure reads `the canary tenant '<name>'
(<row id>) must be untouched by the action: …`.

A third came from measuring rather than reading: the mobile projects needed no reduced selection, because the user menu sits in the
page header on every viewport, which made the `Desktop settings sheet` mobile skip unnecessary.

### Measured baselines

| What                      | Measurement                                                                                                                      | Why it is kept                                                                                                                            |
| ------------------------- | -------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| Client production build   | 55s on an idle machine (Vite 14.7s, Workbox `closeBundle` 22.9s); 535.6s on a machine that had lost most of its CPU              | the prodlike client server builds before it serves, so its start budget covers a build that can be an order of magnitude slower than idle |
| Prodlike matrix           | 318 passed, 88 skipped, 0 failed in 7m12s — the 406 collected tests across four projects, with `retries: 0` locally (2026-11-02) | the completion gate, and the reference for spotting drift                                                                                 |
| Browser destructive specs | 16 passed twice back to back, 1.9m then 1.6m, no reseeding between runs                                                          | repeatability, and what a second run costs                                                                                                |
| Server suite              | 1262 tests, 289 of them integration at 8m28s; a second run of the same suite took 11m18s                                         | the suite's own drift, and why a duration is reported at all                                                                              |
| Scenario tier             | 6 tests in 28s                                                                                                                   | the opt-in tier's reference                                                                                                               |
| Coverage                  | 90.5% of lines and 78.1% of branches across five product assemblies (2026-11-02)                                                 | see the limit below                                                                                                                       |

### What the evidence cannot show

`code_coverage.ps1` instruments Pot.App, Pot.AspNetCore, Pot.Data, Pot.Shared and Pot.EmailSender. It does not instrument
`Pot.AspNetCore.Integration.Tests`, where the server harness lives, and the browser harness is TypeScript and PowerShell, so no
line or branch figure exists for this work; the figures above describe product assemblies the harness does not change.
`Pot.EmailSender` sits at 10.3%, which is consistent with the integration host substituting `CapturingEmailWriter` for the real
sender — the substitution A1 and A2 require.

A machine short of CPU also produces failures that do not exist: an earlier prodlike attempt reported 32 failures, every one of
which passed once the machine was idle. Time `npm run build` before trusting an E2E failure — 55s means the machine is healthy,
minutes mean the run is not.
