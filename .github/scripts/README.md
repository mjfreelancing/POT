# Scripts

This folder contains reusable automation scripts for local diagnostics and workflow support.

## Purpose

Provide discoverable entry points for local automation scripts.

## Contents

- `agent-env-diagnostics.ps1`
  - Validates required/optional tooling.
  - Explains missing-tool purpose.
  - Supports optional install flows (`-OfferInstall`, `-AutoInstall`, `-DryRun`).

- Related VS Code tasks in `.vscode/tasks.json`:
  - `agent-env-diagnostics`
  - `agent-env-diagnostics-offer-install`
  - `agent-env-diagnostics-auto-install-dryrun`

## Source of truth boundary

- Keep detailed command behavior in the script itself and `.github/automation-playbook.md`.
- Keep this file as a discoverability index.

## Notes

### Typical AI agent usage

- Agents typically run diagnostics in report-only mode first to assess environment readiness.
- If optional tools are missing, agents use `-OfferInstall` (interactive) or `-AutoInstall -DryRun` (preview) before making changes.
- Agents prefer task wrappers in `.vscode/tasks.json` when available, and fall back to direct script execution when needed.
- Install-capable modes are opt-in; agents should not perform machine changes unless explicitly requested.

### Manual usage

Run from repository root:

- `./.github/scripts/agent-env-diagnostics.ps1`
