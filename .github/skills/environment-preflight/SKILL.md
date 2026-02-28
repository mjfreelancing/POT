# Skill: Environment Preflight

## Purpose

Standardize agent startup checks so sessions are reliable before code changes and tests.

Implementation script: `.github/scripts/agent-env-diagnostics.ps1`.

## When to use

- At the start of an agent-assisted coding session.
- After toolchain installs/updates.
- Before running broad test/build tasks if terminal behavior has been unstable.

## Inputs

- Repository root terminal.
- Optional intent:
  - report-only (default)
  - offer installs interactively
  - auto-install in opt-in mode

## Commands

- Report-only:
  - `./.github/scripts/agent-env-diagnostics.ps1`
- Offer installs (interactive):
  - `./.github/scripts/agent-env-diagnostics.ps1 -OfferInstall`
- Auto-install optional tools (opt-in):
  - `./.github/scripts/agent-env-diagnostics.ps1 -AutoInstall`
- Dry run install preview:
  - `./.github/scripts/agent-env-diagnostics.ps1 -OfferInstall -DryRun`

## Required result

- `git`, `dotnet`, `node`, `npm` are `FOUND`.
- If any required tool is missing, stop and remediate before proceeding.

## Optional tools and value

- `python`: supports automation scripts and future integrations.
- `py`: Windows Python launcher for version selection.
- `rg`: ripgrep; fast codebase text search used by agent and dev workflows.
- `pwsh`: PowerShell 7 runtime for modern cross-platform scripts.

## Decision policy

- Default behavior is report-only.
- Offer installations only in explicit opt-in modes (`-OfferInstall` or `-AutoInstall`).
- Prefer `-DryRun` first on unfamiliar machines.

## Notes

- If `python` shows `UNUSABLE (ALIAS)`, install Python and disable the App Execution Alias for `python.exe`/`python3.exe`.
- Re-run diagnostics after installations to confirm resolved status.
