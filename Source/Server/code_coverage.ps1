# Server coverage report script.
#
# Purpose:
# - Show where production code is NOT covered by tests, so gaps can be found and closed.
# - Intended for both developers and AI agents. When tests are added or changed for a new
#   feature, run this script and use the output to find uncovered lines and branches in the
#   code that was changed, then add targeted tests for them.
#
# What this script does:
# - Runs `dotnet test` on pot.sln with coverlet collection enabled.
# - Aggregates generated Cobertura XML coverage files.
# - Builds reports via reportgenerator (see "Outputs").
# - Keeps only the most recent N historical coverage artifact folders.
# - Opens the HTML report in the default browser (skip with -NoOpen).
#
# Outputs (all under Source/Server/CoverageReport, regenerated on every run):
# - Summary.txt   : plain-text totals plus line/branch coverage per assembly and class.
#                   Start here: it is small enough to read in full.
# - Cobertura.xml : merged per-line detail. For each class, <line number="N" hits="0"> is an
#                   uncovered line; <line ... branch="True" condition-coverage="50% (1/2)">
#                   is a partially covered branch. Use it to locate exact gaps.
# - index.html    : browsable report for developers (not intended for agents).
#
# Agent workflow after adding tests for a feature:
# 1. Run from Source/Server:  .\code_coverage.ps1 -NoOpen
#    (Slow: it runs the entire solution. For quick feedback while iterating, run the targeted
#    `dotnet test --filter` first and use this script to confirm at the end.)
# 2. Read CoverageReport/Summary.txt and find the classes touched by the feature.
# 3. In CoverageReport/Cobertura.xml, look up those classes (<class filename="...">) and list
#    lines with hits="0" and branches with condition-coverage below 100%.
# 4. Add targeted tests in the nearest test project (see .claude/rules/dotnet-tests.md) for
#    each gap, then re-run to confirm. Report any gap deliberately left uncovered, with a reason.
# - Judge coverage of the code that changed, not the overall percentage: do not chase unrelated
#   classes, and do not add tests with no assertions just to raise a number.
#
# Where this script is used from:
# - Run manually from `Source/Server` during local development.
# - Referenced by `.claude/rules/dotnet-tests.md`.
# - Referenced by `.claude/skills/code-coverage/SKILL.md`.
# - Also available as the VS Code task `server-run-test-coverage`.
#
# Prerequisites:
# - `dotnet` CLI available on PATH.
# - `reportgenerator` available on PATH.

param(
    # Number of historical run directories to keep under CoverageArtifacts.
    # Older runs are deleted after a successful report generation.
    [ValidateRange(1, 100)]
    [int]$MaxCoverageRunsToKeep = 3,

    # Do not open the HTML report in a browser. Use for agent and other non-interactive runs.
    [switch]$NoOpen
)

# Stop on errors so failed test/coverage/report steps fail fast and visibly.
$ErrorActionPreference = "Stop"

# Build timestamped output paths for this run.
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$coverageRoot = Join-Path -Path $PSScriptRoot -ChildPath "CoverageArtifacts"
$runRoot = Join-Path -Path $coverageRoot -ChildPath $timestamp
$resultsRoot = Join-Path -Path $runRoot -ChildPath "TestResults"
$reportRoot = Join-Path -Path $PSScriptRoot -ChildPath "CoverageReport"

# Remove old report output folder and prepare a fresh TestResults folder.
Remove-Item -Path $reportRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $resultsRoot -Force | Out-Null

# Run the server solution tests with XPlat coverage collection and explicit
# runsettings, writing all test outputs to this run's TestResults folder.
dotnet test pot.sln `
    --collect:"XPlat Code Coverage" `
    --settings coverlet.runsettings `
    --results-directory $resultsRoot

# Find all generated Cobertura coverage files from all test projects.
$coverageFiles = Get-ChildItem -Path $resultsRoot -Filter "coverage.cobertura.xml" -Recurse | Select-Object -ExpandProperty FullName

if (-not $coverageFiles)
{
    # Coverage collection expected files were not produced.
    throw "No cobertura coverage files were produced in '$resultsRoot'."
}

# reportgenerator accepts multiple files separated by semicolons.
$reportsArgument = ($coverageFiles -join ";")

# Generate report outputs in Source/Server/CoverageReport. TextSummary and Cobertura are the
# agent-readable outputs (per-class totals and merged per-line detail); Html/HtmlSummary are for humans.
reportgenerator `
    -reports:$reportsArgument `
    -targetdir:$reportRoot `
    -reporttypes:"Html;HtmlSummary;TextSummary;Cobertura"

# Cleanup policy: keep newest N run directories, delete older ones.
$runDirectories = Get-ChildItem -Path $coverageRoot -Directory -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending

if ($runDirectories.Count -gt $MaxCoverageRunsToKeep)
{
    $runDirectories |
    Select-Object -Skip $MaxCoverageRunsToKeep |
        Remove-Item -Recurse -Force
}

# Open the generated report landing page for quick review (skipped with -NoOpen).
if (-not $NoOpen)
{
    Start-Process (Join-Path -Path $reportRoot -ChildPath "index.html")
}
