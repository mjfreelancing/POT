# Automation Playbook (Hooks, Agents, Skills)

This file documents automation options that reduce manual checklist work.

## 1) Copilot Agent Hooks

- Hook config: `.github/hooks/hooks.json`
- Scripts:
  - `.github/hooks/scripts/agent-post-edit.ps1`
  - `.github/hooks/scripts/agent-post-edit.sh`
- Trigger: `sessionEnd` (runs after agent editing sessions).
- Purpose:
  - C#: run `dotnet format Source/Server/pot.sln --include <changed-cs-files>` to sort/remove unused usings where applicable.
  - TypeScript: run `npm run lint:sort` and targeted Prettier write for changed `src/**/*.ts(x)` files.

### Notes

- Hook scripts are fail-soft (they log warnings and exit successfully).
- Integration-test hook behavior remains placeholder-driven until first real integration-test use.

## 2) Agent Preflight

Run a quick environment check before longer agent sessions.

- VS Code task: `agent-env-diagnostics`
- Script: `.github/scripts/agent-env-diagnostics.ps1`
- Install prompt mode: `.github/scripts/agent-env-diagnostics.ps1 -OfferInstall`
- Auto-install mode (opt-in): `.github/scripts/agent-env-diagnostics.ps1 -AutoInstall`
- Dry run install preview: `.github/scripts/agent-env-diagnostics.ps1 -OfferInstall -DryRun`

### Preflight checklist

- Required tools should be `FOUND`: `git`, `dotnet`, `node`, `npm`
- Optional tools that improve reliability if present: `python` (real install), `rg`, `pwsh`, `py`
- If `python` is reported as `UNUSABLE (ALIAS)`, install Python and disable the Windows Store alias.
- Keep terminal cwd at repository root for predictable command behavior.
- When optional tools are missing, diagnostics should explain what each tool is used for before offering installation.

### Expected usage

- Run diagnostics at session start.
- Re-run diagnostics after installing/updating CLI tools.
- Treat diagnostics output as advisory unless required tools are missing.

## 3) Agents

Use sub-agents when tasks are broad and parallelizable, for example:

- client pattern audits
- server pattern audits
- test failure triage by layer

Agent outputs should feed back into scoped instructions/prompts rather than staying as one-off chat answers.

## 4) Skills

Current reusable skill packs:

- `environment-preflight`: run and interpret agent environment diagnostics before coding sessions.
  - Skill doc: `.github/skills/environment-preflight/SKILL.md`

Potential next skill packs:

- `client-quality`: lint/sort/typecheck/test flow
- `server-quality`: targeted `dotnet test` + migration checks
- `feature-rollout`: contract change checklist across client/server/data

Keep skills workflow-focused and let instruction files own coding standards.

## 5) Prompt vs Instruction Boundary

- Instructions: stable coding standards, architecture, style, test conventions.
- Prompts: task workflows and execution choreography.

If a rule appears in both places, keep canonical wording in instructions and replace prompt copy with a reference.
