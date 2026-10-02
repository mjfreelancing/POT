---
name: code-coverage
description: Run the server code coverage workflow, find uncovered lines and branches in changed code, and summarize results. Use when asked to run coverage, find coverage gaps, or after adding tests for a new feature.
---

# Code Coverage

Use coverage to find gaps in the code that was added or changed, then close them with targeted tests.

1. From `Source/Server`, run `.\code_coverage.ps1 -NoOpen` (it runs the whole solution, so it is slow; use targeted `dotnet test --filter` runs while iterating). `-NoOpen` stops it launching a browser.
2. Confirm the run and report generation succeeded. If it fails, report the root cause before proposing fixes.
3. Read `CoverageReport/Summary.txt` for line/branch coverage per assembly and class.
4. For the classes touched by the feature, open `CoverageReport/Cobertura.xml` and list lines with `hits="0"` and branches with `condition-coverage` below 100%.
5. Add targeted tests in the nearest test project (conventions: `.claude/rules/dotnet-tests.md`) for each gap, re-run, and confirm.
6. Report: the artifact location, coverage for the changed classes, any remaining gaps with a reason, and any warnings or missing artifacts.

Judge coverage of the changed code, not the overall percentage. Do not add assertion-free tests to raise a number or chase unrelated classes.

Artifacts: `Source/Server/CoverageReport/` (regenerated each run) and timestamped raw runs in `Source/Server/CoverageArtifacts/`. The same script is available as the VS Code task `server-run-test-coverage`.
