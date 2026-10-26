# TASKS — PRD-NNN <feature name>

> Temporary delivery plan. Never committed; delete when the PRD is Complete.

- PRD: `Docs/Future/PRD-NNN-<slug>.md`
- Status: <Planned | In Progress | Complete>
- Started: <yyyy-mm-dd>
- Progress: 0/<total>

## UI testing checkpoints

<Include only when the PRD has client work. The human should never have to guess when the UI is ready.>

| After     | What can be tested by hand               |
| --------- | ---------------------------------------- |
| Stage <n> | <surface and behaviour now exerciseable> |

## Stage 1 — <short name>

- Goal: <the single behaviour this stage delivers>
- Touches: <layers / projects / files>
- Validation: <targeted test command(s) proving this stage>
- Progress: 0/3 complete
- Notes: <decisions, deferrals and follow-ups>

- [ ] 1.1 **Tests** — <tests that describe this stage's behaviour: red for changed behaviour, already green for a coverage gap>
- [ ] 1.2 **Green** — <the implementation that makes them pass>
- [ ] 1.3 Run the targeted tests and format the changed files

## Stage 2 — <short name>

- Goal: <the single behaviour this stage delivers>
- Touches: <layers / projects / files>
- Validation: <targeted test command(s) proving this stage>
- Progress: 0/2 complete
- Notes: <decisions, deferrals and follow-ups>

- [ ] 2.1 **Tests** — <tests: red for changed behaviour, already green for a coverage gap>
- [ ] 2.2 **Green** — <implementation>

## Completion

- [ ] C.1 Cross-reference every PRD requirement and record where each is met
- [ ] C.2 Server: full build with analysis warnings cleared (never edit a migration; suppress selectively) and the full suite
- [ ] C.3 Client: production build (`npm run build`) and the full unit suite
- [ ] C.4 Client E2E: the prodlike matrix (client-affecting PRDs only)
- [ ] C.5 Coverage of every new or changed line and branch
- [ ] C.6 Manual test plan, or record `Not required` and name the automated coverage
- [ ] C.7 PRD bookkeeping and delete this file

## Deferred / follow-up

- <item> — <why it is deferred>
