---
name: code-coverage
description: Run the repository code coverage workflow and summarize results. Use when asked to run coverage, generate coverage reports, or summarize coverage metrics and failures.
license: MIT
---

# Code Coverage

Run code coverage using repository defaults.

## Scope

- Run coverage for the requested repository area or the default application and test scope.

## Workflow

1. Prefer the repository's primary coverage entry point, such as a script, task, or documented wrapper command.
2. Confirm coverage execution and report generation succeeded.
3. Summarize key coverage results and any failures or warnings.
4. If coverage fails, report the root cause before proposing fixes.

## Execution

Adjust the example commands below to match your repository structure.

- Preferred repository script or task wrapper when one exists.
- .NET example: `dotnet test .\Source\Server\MySolution.sln --collect:"XPlat Code Coverage"` (from `.\Source\Server`)
- Node example: `npm run coverage` (from `.\Source\Client\my-app`)

## Output Expectations

- Report artifact location
- Headline coverage summary or key metrics when available
- Any warnings, failed modules, or missing artifacts

## Repository Notes

- Keep tool-specific coverage details in repository scripts and coverage configuration files.
- Add repository-specific commands and output paths in consuming copies.
  - POT: Coverage entry point is `Source/Server/code_coverage.ps1` (or the `server-run-test-coverage` task) run from `Source/Server`.
  - POT: Report artifacts land in `Source/Server/CoverageReport/` (HTML) and `Source/Server/CoverageArtifacts/` (timestamped raw runs).
