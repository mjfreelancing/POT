---
name: server_coverage
description: Run POT server code coverage workflow and summarize results.
model: GPT-5.3-Codex (copilot)
---

Run server coverage using repository defaults.

## Scope

- Target server coverage only.
- Use the existing coverage workflow script in `Source/Server/code_coverage.ps1`.

## Workflow

1. Run coverage from `Source/Server`.
2. Confirm coverage artifacts and report generation succeeded.
3. Summarize coverage output and any failures/warnings.
4. If coverage run fails, report root cause before proposing fixes.

## Execution

- Preferred command:
  - `./code_coverage.ps1` (from `Source/Server`)
- Optional direct test coverage command (when script adjustments are requested):
  - `dotnet test pot.sln --collect:"XPlat Code Coverage" --settings coverlet.runsettings`

## Output expectations

- HTML report generated under `Source/Server/CoverageReport`.
- Coverage artifacts retained under `Source/Server/CoverageArtifacts` (bounded by script retention setting).

## Expansion Notes

- Keep coverage tool specifics (reportgenerator, retention policy) in `code_coverage.ps1`.
- Add command variants only when they are repository-supported and documented.
