# Scripts: agent-env-tools

## Purpose

This script exists primarily to ensure an AI agent session is as productive as possible from the start.

When a Copilot agent begins work in a repository, it benefits from knowing which CLI tools are available before attempting to run builds, tests, or other commands. Running this script gives the agent a clear, structured picture of the environment so it can adapt its behavior rather than failing mid-task due to a missing tool.

Secondary benefit: developers can run the same script manually to confirm their machine is ready before starting a coding session.

The script answers:

- Are the required tools installed and resolvable?
- Which optional tools are missing, and does that matter for the current work?
- Is `python` real, or is Windows only finding the Store alias?
- What should be installed next?

## What this script checks

Required tools:

- `git`
- `dotnet`
- `node`
- `npm`

Optional tools:

- `docker`
- `python`
- `py`
- `rg`
- `pwsh`

If a required tool is missing, the script exits with a failure code so the problem is obvious.

## Copy target

This script is designed to be copied into a consuming repository. Copy this folder into the consuming repository at:

- `.github\scripts\agent-env-tools\`

After copying, the script path in that repository should be:

- `.github\scripts\agent-env-tools\agent-env-tools.ps1`

> **Note:** The `.github\scripts\...` paths in this README refer to the **consuming** repository after copying. If you are viewing this file in the `copilot-ai-pack` source repository, the script is at `scripts\agent-env-tools\agent-env-tools.ps1` and is not intended to be run here.

## Running the script (in the consuming repository)

### Agent use

An agent can run this script at the start of a session to confirm the environment is ready. The agent should run it from the consuming repository root:

```powershell
.\.github\scripts\agent-env-tools\agent-env-tools.ps1
```

The script reports tool availability and exits. No changes are made to the machine unless the agent explicitly uses one of the install flags described below.

### Developer use

1. Copy this folder into `.github\scripts\agent-env-tools\` in the consuming repository.
2. Open the consuming repository in VS Code.
3. Open a PowerShell terminal in that repository.
4. Make sure the terminal is at the repository root.

If you are not sure where you are, run:

```powershell
Get-Location
```

The current directory should be the repository root before you run the script. Then run:

```powershell
.\.github\scripts\agent-env-tools\agent-env-tools.ps1
```

This is the normal starting point. It only reports status. It does not install anything.

## What you will see

The script prints a table showing:

- the tool name
- whether it was found
- where it resolved from
- the detected version

Possible statuses include:

- `FOUND`
- `MISSING (REQUIRED)`
- `MISSING (OPTIONAL)`
- `UNUSABLE (ALIAS)`

`UNUSABLE (ALIAS)` is most relevant for `python` on Windows. It usually means Windows is pointing `python.exe` to the Store alias instead of a real Python installation.

## Safe run modes

All commands below assume the script has been copied to `.github\scripts\agent-env-tools\` in the consuming repository and are run from that repository's root.

Report-only mode:

```powershell
.\.github\scripts\agent-env-tools\agent-env-tools.ps1
```

Prompt before optional installs:

```powershell
.\.github\scripts\agent-env-tools\agent-env-tools.ps1 -OfferInstall
```

Preview install commands without changing the machine:

```powershell
.\.github\scripts\agent-env-tools\agent-env-tools.ps1 -OfferInstall -DryRun
```

Install all optional tools automatically after you explicitly opt in:

```powershell
.\.github\scripts\agent-env-tools\agent-env-tools.ps1 -AutoInstall
```

## Recommended order for less CLI-experienced developers

1. Start with report-only mode.
2. Read the missing and optional tool results.
3. If you want help installing optional tools, use `-OfferInstall -DryRun` first.
4. If the preview looks correct, use `-OfferInstall`.
5. Re-run report-only mode after installs to confirm the final state.

## How to interpret the result

If all required tools are found:

- You can continue with normal coding, build, and test work.

If one or more required tools are missing:

- Stop and install those first.
- Re-run the script before proceeding.

If optional tools are missing:

- You can usually continue working.
- The README and script output explain why those tools are still useful.

## Windows note for Python

If `python` is reported as `UNUSABLE (ALIAS)`:

1. Install Python.
2. Open Windows App Execution Aliases settings.
3. Disable the aliases for `python.exe` and `python3.exe` if they point to the Microsoft Store.
4. Run the script again.

## Troubleshooting

If PowerShell blocks the script, try running (from the consuming repository root):

```powershell
powershell -ExecutionPolicy Bypass -File .\.github\scripts\agent-env-tools\agent-env-tools.ps1
```

If that works, the issue is your machine's script execution policy rather than the script itself.

If `winget` is unavailable:

- The diagnostic checks still work.
- Optional auto-install flows will not run.
- Install the missing tools manually and then re-run the script.

## Maintainer note

Keep usage guidance in this README. Keep behavior and safety rules in `agent-env-tools.ps1`.
