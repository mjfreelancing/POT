# Session-end maintenance hook for agent edits.
#
# Purpose:
# - Detect changed files in this repository.
# - Run lightweight cleanup tools only for relevant file types.
# - Never fail the whole hook if a cleanup command fails.
#
# Where this script is used from:
# - Triggered by `.github/hooks/hooks.json` on `sessionEnd` for Windows/PowerShell.
# - Can also be run manually from repository root for testing:
#   `./.github/hooks/scripts/agent-post-edit.ps1`
#
# Notes:
# - This is a maintenance helper, not a build/test gate.
# - It is intentionally fail-soft: warnings are logged, but script exits success.
# - Only changed files are targeted to keep execution fast.

# Continue on non-terminating errors; explicit try/catch handles tool failures.
$ErrorActionPreference = "Continue"

Write-Host "[agent-hook] sessionEnd maintenance starting..."

# Build a list of changed files by combining:
# - tracked file changes (git diff)
# - untracked new files (git ls-files --others)
# This allows formatting to include both modified and newly created files.
$tracked = @(git diff --name-only HEAD 2>$null)
$untracked = @(git ls-files --others --exclude-standard 2>$null)
$changed = @($tracked + $untracked | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Sort-Object -Unique)

# If nothing changed, exit early.
if ($changed.Count -eq 0) {
    Write-Host "[agent-hook] no changed files detected; skipping."
    exit 0
}

# Split changed files into server C# and client TypeScript groups.
# Regex patterns are repository-path based (relative paths from repo root).
$serverCsFiles = @($changed | Where-Object { $_ -match '^Source/Server/.+\.cs$' })
$clientTsFiles = @($changed | Where-Object { $_ -match '^Source/Client/pot-react/src/.+\.(ts|tsx)$' })

# For changed server C# files, run dotnet format with include paths so only
# touched files are processed.
if ($serverCsFiles.Count -gt 0) {
    Write-Host "[agent-hook] formatting C# (sort/remove unused usings where applicable)..."
    try {
        # dotnet format is scoped with --include so we avoid formatting unrelated files.
        & dotnet format "Source/Server/pot.sln" --verbosity minimal --include $serverCsFiles | Out-Host
    }
    catch {
        Write-Warning "[agent-hook] dotnet format failed: $($_.Exception.Message)"
    }
}
else {
    Write-Host "[agent-hook] no changed C# files under Source/Server."
}

# For changed client TS/TSX files, run lint sorting first, then targeted
# Prettier formatting from the client project directory.
if ($clientTsFiles.Count -gt 0) {
    Write-Host "[agent-hook] running TypeScript lint sort + targeted prettier..."

    # Move into client project so npm scripts and local tool resolution behave correctly.
    Push-Location "Source/Client/pot-react"
    try {
        & npm run lint:sort | Out-Host

        # Prettier is run using paths relative to Source/Client/pot-react.
        $relativeClientFiles = @($clientTsFiles | ForEach-Object { $_ -replace '^Source/Client/pot-react/', '' })
        if ($relativeClientFiles.Count -gt 0) {
            & npm exec prettier -- --write $relativeClientFiles | Out-Host
        }
    }
    catch {
        Write-Warning "[agent-hook] TypeScript maintenance failed: $($_.Exception.Message)"
    }
    finally {
        # Always restore the previous working directory.
        Pop-Location
    }
}
else {
    Write-Host "[agent-hook] no changed TS/TSX files under Source/Client/pot-react/src."
}

# Always exit success to keep this hook fail-soft.
Write-Host "[agent-hook] sessionEnd maintenance complete."
exit 0
