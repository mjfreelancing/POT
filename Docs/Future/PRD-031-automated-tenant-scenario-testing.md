# Automated Tenant Scenario Testing PRD

**Status:** Planned
**Priority:** High
**Last Updated:** 2026-10-27
**Feature ID:** 031
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

## Out of Scope

- A pipeline, runner or any form of CI configuration — a separate decision, deliberately local-only for now.
- Load, performance and soak testing — a separate concern from behavioural assurance.
- Changing how an invitation delivers its credential — specified by a separate discovery document covering expiry and an expired-invitation status.
- Replacing the browser suite's baseline seed wholesale — the scenarios are additive, and the seed keeps only what a scenario cannot create, such as a platform administrator, which comes from configuration rather than data.
- Any change to the deployed Docker or Azure topology — the mail sink exists only in the test environment.
- Test-only endpoints or production composition changes to support testing — the harness lives in the test projects.

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

| Decision                                                      | Resolution                                                                                                                                                               |
| ------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| How an invited user gets their credential                     | Kept as a temporary password; the harness reads it from the mail sink. Chosen partly because reading mail on a phone while signing in on a computer suits a person.      |
| Ordering against an invitation-link change                    | Not applicable; the harness reads messages either way, so only one helper would change if that change came later.                                                        |
| Which surfaces are covered                                    | The whole application: the machinery spans every endpoint, and scenario journeys begin with the account and site lifecycle and grow from there.                          |
| How a tenant is provisioned                                   | Through the product's routes; direct writes only for states the product cannot produce.                                                                                  |
| Where harness code lives                                      | Extended existing projects — the integration test project for server-side pieces, the browser suite's helpers for browser-side pieces. No production composition change. |
| What an isolation check compares                              | A canary tenant's fingerprint, plus the target tenant's non-target rows.                                                                                                 |
| Where the inventory comes from                                | The generated contract; an unclassified operation fails the suite.                                                                                                       |
| How the authorization matrix knows what to expect             | From each endpoint's own declaration, so the wiring is tested rather than the intent restated.                                                                           |
| What makes the suite run                                      | Always-on invariants in the ordinary server suite; opt-in scenario and browser tiers prompted by the delivery workflow.                                                  |
| Do destructive browser specs keep consuming seeded identities | No; they provision and destroy their own tenant.                                                                                                                         |
| Database strategy                                             | One disposable database per run with per-scenario unique identifiers, so workers run in parallel.                                                                        |
| Time budget                                                   | None. Confidence matters more than duration; the run takes as long as it takes.                                                                                          |
| What a failed invariant reports                               | Tenant, table, row identifier, expected against actual, first difference only.                                                                                           |
| Exclusions                                                    | Load and performance testing, any pipeline or runner, replacing the baseline seed wholesale, and the invitation-credential change — all recorded as out of scope.        |

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

- **B1 — a new operation blocks the suite.** Temporarily add a trivial new endpoint, build, and run the inventory test without classifying it.
  Expected: the run fails and names the operation by method and path.
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

| ID    | Check                                                    | Result | Notes |
| ----- | -------------------------------------------------------- | ------ | ----- |
| A1–A3 | A leak is detected, attributed and reverts green         |        |       |
| B1–B3 | An unclassified operation blocks the suite               |        |       |
| C1–C3 | Browser scenarios provision, repeat and leave no residue |        |       |
| D1–D3 | A blocked or leftover mail sink is caught before a run   |        |       |
