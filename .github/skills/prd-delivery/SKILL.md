---
name: prd-delivery
description: "Deliver an approved PRD as a staged plan of testable increments. Use when an approved PRD is ready to implement: write the stage plan, implement one stage at a time, run the targeted tests, then stop for the human to review and commit. Never stages or commits."
disable-model-invocation: true
argument-hint: "<PRD file or number, e.g. Docs/Future/PRD-022-user-and-site-deletion.md>"
---

# PRD Delivery

Take an approved PRD through implementation in stages. Each stage is a self-contained, testable increment that leaves the affected build green. The agent implements one stage, runs the targeted tests, then stops and waits for the human to review and commit. It never stages or commits, and never starts the next stage without explicit approval.

Every stage and sub-task is numbered (Stage 1 holds tasks `1.1`, `1.2`, …; Stage 2 holds `2.1`, `2.2`, …) so work can be referenced precisely — the human can say "do task 2.3". Each sub-task carries a checkbox the agent ticks as it implements it, and each stage reports its progress.

Client E2E is gated on client code: the dev suite (`npm run e2e:all:dev`) is opt-in at each client-changing stage, and the prodlike suite (`npm run e2e:all:prodlike`) runs once at completion. Server-only work never triggers E2E.

Use this skill only for PRDs under `Docs/Future/`. For general multi-stage work, use the `feature-implementation` skill.

## Hard rules

- **Never stage or commit.** No `git add`, `git commit`, `git stash`, `git restore`, or any write git command. Make working-tree edits only; the human stages and commits each stage.
- **One stage at a time.** After a stage is implemented and validated, stop and summarize. Do not begin the next stage until the human explicitly approves it ("approved", "continue", "go").
- **Two pause points up front:** wait for approval of the staged plan before implementing anything, then wait after every stage.
- **Track progress in the plan.** Tick each sub-task's checkbox (`- [x]`) as it is implemented and keep each stage's `Progress: n/total complete` line accurate. Reference work by task number (e.g. `2.3`) in summaries and questions.
- **Never generate EF Core migrations.** If a stage changes schema, tell the developer to run and review `add-migration`.
- **Stay inside PRD scope.** Do not refactor unrelated code or change public API shape beyond what the PRD specifies; ask if scope is unclear.
- **Never reference PRDs, ADRs, task lists, stage numbers or requirement IDs in code, tests, comments or test names.** Those belong only inside `Docs/Future/` (the plan file lives there, so references to the PRD are fine in it).
- Obey the scoped `.github/instructions/` file for every area the stage touches, and never touch `Source/Docker/postgres-data/**`.

## Procedure

### 1. Create the staged plan

1. Read the PRD in full. If it is not approved (Status not `Planned` or `In Progress`), stop and ask.
2. Split the work into testable increments. Each stage must:
   - deliver one coherent, contract- or user-visible behaviour;
   - be independently testable and leave the affected build green;
   - name the layers and files it touches, and the targeted validation command(s);
   - be small enough to review and commit as one unit;
   - be broken into numbered sub-tasks (`1.1`, `1.2`, …) that each carry a checkbox, so the human can reference any one precisely;
3. Write the plan to `Docs/Future/TASKS-PRD-NNN.md`, beside the PRD (`NNN` taken from the PRD filename), using the [stage plan template](./references/stage-plan.template.md). The file is never committed — leave it unstaged.
4. Do the start-of-work bookkeeping per `Docs/Future` conventions: set the PRD `Status` to `In Progress`, update `Last Updated`, and update the matching `Docs/Future/README.md` row (`Status`, `Last Updated`) plus the index-update date.
5. Present the plan and **stop.** Wait for the plan to be approved. If the human changes it, edit the TASKS file in place; do not create another.

### 2. Implement one stage

For the approved stage only:

1. Work through the stage's numbered sub-tasks in the order the stage defines, with minimal, targeted edits (contracts/types, then logic, then UI or transport wiring). Tick each sub-task's checkbox (`- [x]`) as it is implemented.
2. Add or update tests in the nearest appropriate project. Use the matching skill: `dotnet-unit-test`, `client-tests`, `server-integration-test`, or the E2E conventions in `.github/instructions/e2e.instructions.md`.
3. Run the **targeted** tests for the changed code and fix failures before stopping. See the `run-tests` skill for the exact commands and scoping.
4. Update the TASKS file: every sub-task in the stage is ticked, the stage's `Progress: n/total complete` line reads full, and anything deferred is recorded.
5. Summarize: the files changed, the behaviour impact, the exact tests run with their result, and any follow-up. Reference the tasks by number.
6. **Client stages only — ask about E2E.** If the stage changed client product code (the shipped client, typically under `Source/Client/pot-react/src`; not client test specs alone), ask whether to run the dev E2E suite and who runs it. Ask at every client-changing stage, but skip the question on the final stage — step 4 runs the prodlike gate instead.
   - **Agent runs it:** run `npm run e2e:preflight` first (read-only) to confirm the E2E ports are free, then `npm run e2e:all:dev` (use `npm run e2e:all:dev:log` when the run's evidence should be kept), and report the result.
   - **Human runs it:** wait for the human to run it and report back.
     If the run fails, report the failures and **pause** — do not fix them automatically; wait for the human to decide.
7. **Stop.** Do not stage or commit. Wait for the human to review and commit the stage, then approve continuing.

### 3. Resume after approval

On "approved" / "continue", resume at the next unticked task in the TASKS file — the next stage's first sub-task, or the remaining sub-tasks if a stage was paused part-way. Do not re-do ticked tasks or re-edit already-committed code unless the human asks. Repeat step 2 until every stage is complete.

### 4. Complete the PRD

After the final stage is approved:

1. Run the broader validation for the affected side(s), from the right directory:
   - Server: `dotnet test pot.sln -c Debug --nologo --verbosity minimal` (`Source/Server`).
   - Client unit tests: `npm run test` (`Source/Client/pot-react`).
   - Client E2E (completion gate): for a PRD that changed client product code, run `npm run e2e:all:prodlike` (`Source/Client/pot-react`) — the production-build gate, and the final-stage replacement for the optional dev E2E. Preflight first and follow `.github/instructions/e2e.instructions.md`. If it fails, report and pause before the completion bookkeeping.
     Run the `code-coverage` skill over the changed code if it warrants it. Targeted tests are sufficient for the intermediate stages; broaden only here.
2. Do the completion bookkeeping per `Docs/Future` conventions: PRD `Status` to `Complete`, update `Last Updated`, add or adjust the `Scope` line, rewrite the `Docs/Future/README.md` row summary to state what the feature **is**, and bump the index-update date.
3. Delete `Docs/Future/TASKS-PRD-NNN.md`.
4. Summarize the whole delivery and **stop** for final review.
