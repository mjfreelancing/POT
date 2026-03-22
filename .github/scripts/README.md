# Scripts

This folder contains script-first utility packs for manual adoption.

## Manual integration workflow

1. Pick the script pack you want.
2. Copy the pack folder from `scripts/<pack>/` into `.github/scripts/<pack>/` in your target repository.
3. Read that pack's `README.md` before running the script.
4. Run the script from the repository root so relative paths and terminal context are predictable.

## Available script packs

### `agent-env-tools`

Use when:

- You want agents to confirm the environment is ready before starting a session.
- You also want developers to verify their machine setup using the same script.

Provides:

- A PowerShell diagnostics script that checks required and optional CLI tools.
- A README covering both agent and developer use, with copy, run, and troubleshooting guidance.

Copy target:

- `.github\scripts\agent-env-tools\`

Main files:

- `agent-env-tools.ps1`
- `README.md`
