---
name: feature-implementation
description: "Implement a feature that spans client, API, application, data or infrastructure layers: order the work, ask whether a multi-stage change should be staged, record any architectural decision, and validate the smallest relevant scope. Use for work that has no PRD; use prd-delivery when one exists. Never stages or commits."
argument-hint: "[the feature, or the change to make]"
---

# Feature Implementation

Implement the feature with minimal, targeted edits and root-cause focus. This is the path for work that
has no PRD; `prd-delivery` covers a discovered PRD.

## Order of work

1. **Scope it.** Name the layers involved — client, API, application, data, infrastructure — and say what
   is out of scope.
2. **Ask whether it is one pass or several.** When the work splits into increments that can each be
   reviewed and validated on their own — because it spans layers, or delivers more than one piece of
   behaviour — say so and **ask whether the developer wants a staged plan**. Never impose one on a small
   change, and never start a multi-stage change without that answer.
   - **Staged.** Write the plan to `TASKS-<slug>.md` in the repository root, present it, and **stop** for
     approval before implementing anything. Take the shape from
     [the stage-plan template](../prd-delivery/references/stage-plan.template.md): numbered stages,
     numbered sub-tasks with checkboxes, tests first, and the command that validates each stage. Only
     its PRD header lines do not apply. Like a PRD's task list it is a working document — never
     committed, deleted when the work is complete.
   - **One pass.** Implement it as a single change, and say in the summary that no plan was needed.
3. **Record a decision if there is one.** When the work constrains other features, is costly to reverse,
   crosses components, or invites "why is it like this?", use the `adr` skill and raise a `proposed` ADR
   in the same change — in a staged plan, as its first sub-task. A coding convention belongs in
   `Docs/standards/`, not in an ADR.
4. **Contracts and types first**, then business logic, then UI or transport wiring.
5. **Follow the scoped instructions** for every area the change touches (`.github/instructions/`).
6. **Tests before the behaviour**, in the nearest project: `dotnet-unit-test`, `server-integration-test`,
   `client-tests`, or `.github/instructions/e2e.instructions.md`. In a staged plan, for each stage.
7. **Never generate a migration.** Migrations live in `Pot.Data/Migrations`. Ask the developer to run
   this from `Source/Server`, using a name they have approved, and to review what it generates:

   ```powershell
   dotnet ef migrations add <Name> --project Pot.Data --startup-project Pot.Data.Migrations
   ```

   `--startup-project` is not optional: the design-time factory and the `appsettings.json` it reads live
   in `Pot.Data.Migrations`. There is one context, so no `--context` is needed, and the default output
   folder is already `Pot.Data/Migrations`.
8. **Regenerate the contract** when the API changes: the build rewrites `contracts/openapi/v1.json`, so
   commit it in the same change.

## Validate

- Run the smallest scope that proves the change, then broaden only when the result is unclear.
  `run-tests` has the commands.
- A client-visible change needs the E2E suite the change warrants, per `e2e.instructions.md`.
- New or changed lines and branches should be exercised; `code-coverage` measures it when the work
  deserves the run.

## Stop

- **One stage at a time.** In a staged plan, implement the approved stage, validate it, and stop; never
  begin the next stage until the developer approves it.
- Obey the working agreement in `.github/copilot-instructions.md`: no unrelated refactors, no unapproved
  public API changes, no PRD or ADR IDs in code.
- Keep the UI consistent with the existing design system and shared components.
- Never stage or commit. Summarize the changed files, behaviour impact and any follow-up, offer a commit
  message, and stop.
