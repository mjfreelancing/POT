# Copilot Hooks

This folder contains Copilot agent hook configuration and scripts.

## Purpose

Describe how Copilot hooks are wired and where hook logic lives.

## Contents

- `hooks.json` defines which hooks run and when.

### `sessionEnd` hook

- PowerShell (Windows): `scripts/agent-post-edit.ps1`
- Bash (Linux/macOS): `scripts/agent-post-edit.sh`

Both scripts perform the same maintenance workflow:

1. Detect changed files.
2. Run `dotnet format` for changed server C# files.
3. Run `npm run lint:sort` and targeted `prettier --write` for changed client TS/TSX files.
4. Stay fail-soft (log warnings, do not block the session).

## Source of truth boundary

- Keep hook trigger and shell mapping in `hooks.json`.
- Keep executable maintenance behavior in `scripts/agent-post-edit.ps1` and `scripts/agent-post-edit.sh`.
- Keep this file index-oriented and avoid duplicating command-level implementation details.

## Notes

Copilot hooks can run in different shells by platform. `hooks.json` maps each platform to the matching script so behavior stays consistent across environments.
