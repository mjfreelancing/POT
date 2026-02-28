# Server coverage report script.
#
# What this script does:
# - Runs `dotnet test` with coverlet collection enabled.
# - Aggregates generated Cobertura XML coverage files.
# - Builds HTML coverage reports via reportgenerator.
# - Keeps only the most recent N historical coverage artifact folders.
# - Opens the generated report in the default browser.
#
# Where this script is used from:
# - Run manually from `Source/Server` during local development.
# - Referenced by `.github/instructions/server.tests.instructions.md`.
# - Referenced by `.github/prompts/server_coverage.prompt.md`.
#
# Prerequisites:
# - `dotnet` CLI available on PATH.
# - `reportgenerator` available on PATH.

param(
    # Number of historical run directories to keep under CoverageArtifacts.
    # Older runs are deleted after a successful report generation.
    [ValidateRange(1, 100)]
    [int]$MaxCoverageRunsToKeep = 3
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

# Generate HTML report outputs in Source/Server/CoverageReport.
reportgenerator `
    -reports:$reportsArgument `
    -targetdir:$reportRoot `
    -reporttypes:"Html;HtmlSummary"

# Cleanup policy: keep newest N run directories, delete older ones.
$runDirectories = Get-ChildItem -Path $coverageRoot -Directory -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending

if ($runDirectories.Count -gt $MaxCoverageRunsToKeep)
{
    $runDirectories |
    Select-Object -Skip $MaxCoverageRunsToKeep |
        Remove-Item -Recurse -Force
}

# Open the generated report landing page for quick review.
Start-Process (Join-Path -Path $reportRoot -ChildPath "index.html")
