# Scripts: agent-env-tools

A PowerShell diagnostics script that reports which CLI tools are available, so an agent (or developer) knows what will work before running builds and tests instead of failing mid-task.

It answers: are the required tools installed and resolvable, which optional tools are missing, is `python` real or only the Windows Store alias, and what should be installed next.

## What it checks

- Required: `git`, `dotnet`, `node`, `npm`. A missing required tool exits with code 1.
- Optional: `docker`, `python`, `py`, `rg`, `pwsh`. Missing optional tools only produce guidance.

Statuses: `FOUND`, `MISSING (REQUIRED)`, `MISSING (OPTIONAL)`, `UNUSABLE (ALIAS)` (usually `python.exe` pointing at the Microsoft Store alias).

## Running it

Run from the repository root:

```powershell
.\.claude\scripts\agent-env-tools\agent-env-tools.ps1
```

| Mode | Flags | Effect |
| --- | --- | --- |
| Report only (default) | none | Reports status; changes nothing |
| Offer installs | `-OfferInstall` | Prompts before each optional install (via `winget`) |
| Preview installs | `-OfferInstall -DryRun` | Shows install commands without running them |
| Auto install | `-AutoInstall` | Installs optional tools without prompting; explicit opt-in only |

Suggested order: report-only, then `-OfferInstall -DryRun`, then `-OfferInstall`, then report-only again to confirm.

## Interpreting results

- All required tools found: continue with normal build and test work.
- A required tool missing: install it and re-run before proceeding.
- Optional tools missing: you can usually continue.
- `python` shows `UNUSABLE (ALIAS)`: install Python, then disable the `python.exe` / `python3.exe` aliases in Windows App Execution Aliases and re-run.

## Troubleshooting

- Script blocked by execution policy: `powershell -ExecutionPolicy Bypass -File .\.claude\scripts\agent-env-tools\agent-env-tools.ps1`.
- `winget` unavailable: diagnostics still work; install missing tools manually.

Keep usage guidance in this README and behavior/safety rules in `agent-env-tools.ps1`.
