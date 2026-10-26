---
name: prd-delivery
description: "Deliver a discovered PRD as a staged plan of testable increments. Use when the human asks to plan, start or continue delivery of a PRD under Docs/Future: write the TASKS plan, then implement one stage at a time — tests first (red for changed behaviour, already green for a coverage gap), then code to green — run the targeted tests, report when the UI can be tested, offer a commit message, and stop for review. Never stages or commits."
argument-hint: "<PRD file or number under Docs/Future>"
---

# PRD Delivery

Take a discovered PRD through implementation in stages. Each stage is a self-contained, testable increment that leaves the affected build green. The agent implements one stage, runs the targeted tests, then stops and waits for the human to review and commit. It never stages or commits, but it does offer a commit message. It never starts the next stage without explicit approval.

A PRD reaches this skill from `prd-discovery`, which writes the durable PRD and either a Manual Test Plan or a recorded `Not required` decision. If the PRD was not produced that way, stop and say so rather than planning around a document with no agreed requirements.

Every stage and sub-task is numbered (Stage 1 holds tasks `1.1`, `1.2`, …; Stage 2 holds `2.1`, `2.2`, …) so work can be referenced precisely — the human can say "do task 2.3". Each sub-task carries a checkbox the agent ticks as it implements it, and each stage reports its progress.

Client E2E is gated on client code: the dev suite (`npm run e2e:all:dev`) is opt-in at each client-changing stage, and the prodlike suite (`npm run e2e:all:prodlike`) runs once at completion. Server-only work never triggers E2E.

Use this skill only for PRDs under `Docs/Future/`. For general multi-stage work, use the `feature-implementation` skill.

## Hard rules

- **Never stage or commit.** No `git add`, `git commit`, `git stash`, `git restore`, or any write git command. Make working-tree edits only; the human stages and commits each stage.
- **Offer a commit message at the end of every stage.** Written for the staged change set, so the human can commit without composing one.
- **Start only from a discovered PRD.** The PRD must carry `**Discovery:** complete`. Without it, the requirements were never agreed — report that and stop.
- **One stage at a time.** After a stage is implemented and validated, stop and summarize. Do not begin the next stage until the human explicitly approves it ("approved", "continue", "go").
- **Tests first per stage.** Write the tests that describe the stage's behaviour before the implementation. A test for behaviour the stage changes starts red for the expected reason and goes green; a test that fills a coverage gap in behaviour that already holds starts green and stays green. The stage report says which case applied, and the plan's sub-tasks must say so.
- **The Manual Test Plan is required unless the PRD says `Not required`.** A PRD marks it `Not required` only when the automated suites cover every behaviour the plan would check, and names that coverage. For such a PRD, skip the Manual Test Plan step when closing the PRD and record that the automated suites were the gate.
- **Two pause points up front:** wait for approval of the staged plan before implementing anything, then wait after every stage.
- **Block unless approved.** Start only when the PRD `Status` is `Planned` (approved and ready) or `In Progress` (resuming). When it is anything else — `Proposed`, `Planning`, `Proposed (discovery draft)` or `Complete` — **block**: do not write the plan or touch code, report the status and what is required, and stop.
- **The PRD stays editable during delivery.** While its `Status` is `Planned` or `In Progress`, the human may review and edit the PRD. Re-read it at the start of each stage and reconcile the TASKS plan if it changed; do not treat the PRD as frozen.
- **Track progress in the plan.** Tick each sub-task's checkbox (`- [x]`) as it is implemented and keep each stage's `Progress: n/total complete` line accurate. Reference work by task number (e.g. `2.3`) in summaries and questions.
- **Never generate EF Core migrations.** If a stage changes schema, tell the developer to run and review `add-migration`.
- **Stay inside PRD scope.** Do not refactor unrelated code or change public API shape beyond what the PRD specifies; ask if scope is unclear.
- **Never reference PRDs, ADRs, task lists, stage numbers or requirement IDs in code, tests, comments or test names.** Those belong only inside `Docs/Future/` (the plan file lives there, so references to the PRD are fine in it).
- Obey the scoped `.github/instructions/` file for every area the stage touches, and never touch `Source/Docker/postgres-data/**`.

## Procedure

### 1. Create the staged plan

1. Read the PRD in full, then check it carries `**Discovery:** complete`. Start only when that line is present **and** its `Status` is `Planned` (approved and ready) or `In Progress` (resuming). If the discovery line is missing, or the status is `Proposed`, `Planning`, `Proposed (discovery draft)` or `Complete`, **block** — do not write the plan or touch code; report what is missing and stop.
2. Split the work into testable increments. Each stage must:
   - deliver one coherent, contract- or user-visible behaviour;
   - be independently testable and leave the affected build green;
   - order its sub-tasks **tests first**: the tests come before the implementation, marked **red** where the behaviour changes and **already green** where they only fill a coverage gap;
   - name the layers and files it touches, and the targeted validation command(s);
   - be small enough to review and commit as one unit;
   - be broken into numbered sub-tasks (`1.1`, `1.2`, …) that each carry a checkbox, so the human can reference any one precisely;
3. Write the plan to `Docs/Future/TASKS-PRD-NNN.md`, beside the PRD (`NNN` taken from the PRD filename), using the [stage plan template](./references/stage-plan.template.md). The file is never committed — leave it unstaged.
4. Do the start-of-work bookkeeping per `Docs/Future` conventions: set the PRD `Status` to `In Progress`, update `Last Updated`, and update the matching `Docs/Future/README.md` row (`Status`, `Last Updated`) plus the index-update date.
5. Add a **UI testing checkpoints** table near the top of the plan when the PRD has client work: which stage first produces something the human can exercise by hand, and what they can test after each later client stage. A human should never have to guess when the UI is ready.
6. Present the plan and **stop.** Wait for the plan to be approved. If the human changes it, edit the TASKS file in place; do not create another.

### 2. Implement one stage

For the approved stage only:

1. **Tests first (red or already green).** Write the tests that describe the stage's behaviour, in the nearest appropriate project: `dotnet-unit-test`, `client-tests`, `server-integration-test`, or the E2E conventions in `.github/instructions/e2e.instructions.md`. Run them before implementing. A test for behaviour the stage changes must fail for the expected reason — a test that passes while claiming to cover changed behaviour proves nothing. A test that only fills a coverage gap in behaviour that already holds is expected to pass, and the stage notes say so.
2. **Green.** Work through the stage's numbered sub-tasks in the order the stage defines, with minimal, targeted edits (contracts/types, then logic, then UI or transport wiring), until the tests pass. Tick each sub-task's checkbox (`- [x]`) as it is implemented.
3. Run the **targeted** tests for the changed code and fix failures before stopping. See the `run-tests` skill for the exact commands and scoping.
4. Update the TASKS file: every sub-task in the stage is ticked, the stage's `Progress: n/total complete` line reads full, and anything deferred is recorded.
5. Summarize: the files changed, the behaviour impact, the exact tests run with their result, and any follow-up. Reference the tasks by number.
6. **State whether the human can test it in the UI.** If the stage added or changed a client surface, say what they can exercise by hand and where. If it is server-only, say so plainly, so nobody goes looking for a screen that does not exist yet.
7. **Offer a commit message** for the stage's change set.
8. **Client stages only — ask about E2E.** If the stage changed client product code (the shipped client, typically under `Source/Client/pot-react/src`; not client test specs alone), ask whether to run the dev E2E suite and who runs it. Ask at every client-changing stage, but skip the question on the final stage — the final verification runs the prodlike gate instead.
   - **Agent runs it:** run `npm run e2e:preflight` first (read-only) to confirm the E2E ports are free, then `npm run e2e:all:dev` (use `npm run e2e:all:dev:log` when the run's evidence should be kept), and report the result.
   - **Human runs it:** wait for the human to run it and report back.
     If the run fails, report the failures and **pause** — do not fix them automatically; wait for the human to decide.
9. **Stop.** Do not stage or commit. Wait for the human to review and commit the stage, then approve continuing.

### 3. Resume after approval

On "approved" / "continue", resume at the next unticked task in the TASKS file — the next stage's first sub-task, or the remaining sub-tasks if a stage was paused part-way. Do not re-do ticked tasks or re-edit already-committed code unless the human asks. Repeat step 2 until every stage is complete.

### 4. Verify before final approval

After the last stage is implemented and reviewed, and before the PRD is marked Complete, prove the delivery holds as a whole. Run every check below, report each result, then **stop** for final approval. Green stages do not make any of this optional, and a failing check is reported and paused on — never worked around.

1. **Cross-reference the requirements.** Re-read the PRD's requirements and, for each numbered one (`A1`…`A6`, `B1`…`B10`, `C1`…`C5`, `D1`…`D7`, `E1`…`E8`, `N1`…`N3`, or whatever IDs the PRD uses), name the code and the test that satisfy it. Put the result in the report as a requirement table. An unaccounted requirement is a blocker: implement it or have the human amend the PRD — never close a PRD with a requirement unaccounted for. A requirement the PRD marks out of scope is listed as out of scope, not as a gap.
2. **Server build, with analysis warnings cleared.** From `Source/Server` run `dotnet build pot.sln -c Debug --nologo --verbosity minimal`, then `dotnet format analyzers pot.sln --verify-no-changes --severity warn --no-restore` and `dotnet format style pot.sln --verify-no-changes --severity warn --no-restore`. Exit code 0 on both means clean; the build alone does not report style diagnostics, so the two `dotnet format` runs are the check, not a formality. Fix every warning in code this PRD touched in the code itself — not with a `#pragma` or a blanket `NoWarn`. A warning that predates the PRD is reported, not fixed, unless the human asks for it.
   **Never edit an EF Core migration** (`Pot.Data/Migrations/**`, the designer and model snapshot included). When one triggers an analyzer, suppress that diagnostic selectively for those paths in `.editorconfig`, keeping the `[/Pot.Data/Migrations/*.cs]` section last — a section applies to every setting after it.
3. **Server tests.** `dotnet test pot.sln -c Debug --nologo --verbosity minimal`.
4. **Client production build.** From `Source/Client/pot-react` run `npm run build` (`tsc -b && vite build`). It must finish with no errors, and no warning the change introduced — the shipped bundle compiling is a different proof from its tests passing.
5. **Client unit tests.** `npm run test`.
6. **Client E2E, prodlike.** For a PRD that changed client product code, run `npm run e2e:all:prodlike` — the production-build gate, and the replacement for the optional dev E2E at this point. Run `npm run e2e:preflight` first and follow `.github/instructions/e2e.instructions.md`. Skip it only when the PRD changed no client product code, and say so explicitly in the report.
7. **Coverage of the new code.** Run the `code-coverage` skill over the code this PRD added or changed and check that every new or changed line and branch is exercised. Report what is uncovered together with the reason a line cannot be reached — unreachable code is a defect, not a coverage gap. Report a pre-existing coverage gap only when you notice one; do not go looking for unrelated gaps. Intermediate stages rely on their targeted tests; this is the only place coverage is measured.
8. **Report and stop.** Summarize the requirement table, the build and warning results, the suite results, the E2E gate and the coverage outcome, then stop for final approval. No bookkeeping happens before the human approves.

### 5. Close the PRD

After final approval:

1. **Run the PRD's Manual Test Plan — unless it is `Not required`.** When the PRD marks it `Not required`, skip this step and state in the completion summary that the automated suites named by the PRD were the gate; add no results table. Otherwise ask the human to execute it against the built product and report the results, and **wait** — the run is theirs to perform, not the agent's. Record the outcome in the PRD's results table. If the human defers it, say so explicitly and do not record it as passed.
2. Do the completion bookkeeping per `Docs/Future` conventions: PRD `Status` to `Complete`, update `Last Updated`, add or adjust the `Scope` line, rewrite the `Docs/Future/README.md` row summary to state what the feature **is**, and bump the index-update date.
3. Delete `Docs/Future/TASKS-PRD-NNN.md`.
4. Summarize the whole delivery, offer a final commit message, and **stop** for final review.
