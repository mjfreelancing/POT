---
name: code_coverage
description: Run repository coverage workflow and summarize results.
---

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

## Expansion Notes

- Keep tool-specific coverage details in repository scripts and coverage configuration files.
  - POT: Primary coverage workflow is `./code_coverage.ps1` from `Source/Server`.
  - POT: Optional direct .NET command is `dotnet test pot.sln --collect:"XPlat Code Coverage" --settings coverlet.runsettings` from `Source/Server`.
  - POT: HTML report is generated under `Source/Server/CoverageReport`.
  - POT: Coverage artifacts are retained under `Source/Server/CoverageArtifacts`.
- Add repository-specific commands and output paths in consuming copies of this prompt.
