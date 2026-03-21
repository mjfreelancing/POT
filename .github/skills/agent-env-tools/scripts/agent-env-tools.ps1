# Agent environment diagnostics script.
#
# What this script does:
# - Checks whether required and optional CLI tools are available.
# - Shows where each tool resolves from and its detected version.
# - Explains why optional missing tools matter.
# - Optionally installs missing optional tools using winget (opt-in).
#
# Where this script is used from:
# - Direct terminal usage from repository root.
# - Documentation/workflow references:
#   - `.github/skills/agent-env-tools/SKILL.md`
#   - `.github/skills/agent-env-tools/scripts/agent-env-tools.ps1`
#
# Notes:
# - Required tools cause exit code 1 when missing.
# - Optional tools never fail the script; they produce guidance only.
# - Installation is never automatic unless `-AutoInstall` is explicitly used.
# - `-DryRun` previews install commands without changing the machine.

param(
    # Prompt before each optional install candidate.
    [switch]$OfferInstall,

    # Attempt installs without prompting (still opt-in because user must pass this flag).
    [switch]$AutoInstall,

    # Preview install commands only; no actual installations are performed.
    [switch]$DryRun
)

# Continue processing even if individual checks fail; we aggregate results and decide at the end.
$ErrorActionPreference = "Continue"

# Auto-install implies install flow is enabled.
if ($AutoInstall)
{
    $OfferInstall = $true
}

# Small helper to standardize yes/no prompts.
function Test-InstallConsent
{
    param(
        [string]$Prompt
    )

    $response = Read-Host "$Prompt [y/N]"
    return $response -match '^(y|yes)$'
}

# Installs one tool using winget when possible.
function Install-ToolWithWinget
{
    param(
        [pscustomobject]$Tool
    )

    if ([string]::IsNullOrWhiteSpace($Tool.WingetId))
    {
        # Some tools may intentionally not have a configured package id.
        Write-Host "[agent-diagnostics] Skipping $($Tool.Name): no winget package configured."
        return
    }

    if ($null -eq (Get-Command winget -ErrorAction SilentlyContinue))
    {
        # winget is required for the auto-install path.
        Write-Warning "[agent-diagnostics] winget is not available. Install '$($Tool.Name)' manually."
        return
    }

    $installCommand = "winget install --id $($Tool.WingetId) -e --source winget --accept-package-agreements --accept-source-agreements"

    if ($DryRun)
    {
        # Dry-run mode prints the exact command for transparency.
        Write-Host "[agent-diagnostics] [DryRun] $installCommand"
        return
    }

    Write-Host "[agent-diagnostics] Installing $($Tool.Name) via winget..."
    & winget install --id $Tool.WingetId -e --source winget --accept-package-agreements --accept-source-agreements
}

Write-Host "[agent-diagnostics] Starting environment checks..."
Write-Host "[agent-diagnostics] CWD: $((Get-Location).Path)"

# Tool matrix:
# - Required tools must be present for a healthy agent environment.
# - Optional tools improve workflow reliability and speed.
# - `Purpose` text is shown to users when tools are missing.
$checks = @(
    @{ Name = "git"; Required = $true; Version = { git --version }; Purpose = "Source control operations and repository workflows."; WingetId = "Git.Git" },
    @{ Name = "dotnet"; Required = $true; Version = { dotnet --version }; Purpose = "Build, test, and run the .NET server projects."; WingetId = "Microsoft.DotNet.SDK.9" },
    @{ Name = "node"; Required = $true; Version = { node --version }; Purpose = "Run client tooling and scripts."; WingetId = "OpenJS.NodeJS.LTS" },
    @{ Name = "npm"; Required = $true; Version = { npm --version }; Purpose = "Install and run JavaScript/TypeScript dependencies."; WingetId = "OpenJS.NodeJS.LTS" },
    @{ Name = "docker"; Required = $false; Version = { docker --version }; Purpose = "Run local containerized client/server stack."; WingetId = "Docker.DockerDesktop" },
    @{ Name = "python"; Required = $false; Version = { python --version }; Purpose = "Some automation scripts and future tooling integrations."; WingetId = "Python.Python.3.12" },
    @{ Name = "py"; Required = $false; Version = { py --version }; Purpose = "Python launcher for selecting Python versions on Windows."; WingetId = "Python.Python.3.12" },
    @{ Name = "rg"; Required = $false; Version = { rg --version }; Purpose = "Fast codebase text search (ripgrep) for agent and developer workflows."; WingetId = "BurntSushi.ripgrep.MSVC" },
    @{ Name = "pwsh"; Required = $false; Version = { pwsh -Version }; Purpose = "PowerShell 7 runtime for modern cross-platform scripts."; WingetId = "Microsoft.PowerShell" }
)

# Collect normalized results in one list so they can be reported consistently.
$results = New-Object System.Collections.Generic.List[object]

foreach ($check in $checks)
{
    # `Get-Command` tells us whether the executable is discoverable on PATH.
    $cmd = Get-Command $check.Name -ErrorAction SilentlyContinue

    if ($null -eq $cmd)
    {
        # Missing tools still get a result row, including purpose and install metadata.
        $results.Add([PSCustomObject]@{
            Tool = $check.Name
            Status = if ($check.Required) { "MISSING (REQUIRED)" } else { "MISSING (OPTIONAL)" }
            Location = "-"
            Version = "-"
            Required = $check.Required
            Purpose = $check.Purpose
            WingetId = $check.WingetId
        })

        continue
    }

    $version = "unknown"

    try
    {
        # Run each version command to show concrete environment state.
        $output = & $check.Version 2>&1
        $version = ($output | Select-Object -First 1 | Out-String).Trim()
    }
    catch
    {
        $version = "error: $($_.Exception.Message)"
    }

    $status = "FOUND"

    # Special-case: Windows Store Python alias can appear "found" but not usable.
    if ($check.Name -eq "python" -and $version -match "Python was not found")
    {
        $status = "UNUSABLE (ALIAS)"
    }

    $results.Add([PSCustomObject]@{
        Tool = $check.Name
        Status = $status
        Location = $cmd.Source
        Version = $version
        Required = $check.Required
        Purpose = $check.Purpose
        WingetId = $check.WingetId
    })
}

# Show the primary status table first so users can quickly scan environment health.
$results | Format-Table Tool, Status, Location, Version -AutoSize

# Split result set into fail-fast items vs advisory items.
$missingRequired = $results | Where-Object { $_.Status -eq "MISSING (REQUIRED)" }
$optionalNeedsAttention = $results | Where-Object {
    ($_.Status -eq "MISSING (OPTIONAL)") -or ($_.Status -eq "UNUSABLE (ALIAS)")
}

# Explain missing/alias optional tools and why they matter.
if ($optionalNeedsAttention.Count -gt 0)
{
    Write-Host ""
    Write-Host "[agent-diagnostics] Optional tools needing attention:"

    foreach ($tool in $optionalNeedsAttention)
    {
        Write-Host "- $($tool.Tool): $($tool.Status)"
        Write-Host "  Used for: $($tool.Purpose)"

        if ($tool.Tool -eq "python" -and $tool.Status -eq "UNUSABLE (ALIAS)")
        {
            # This is a common Windows issue when App Execution Aliases intercept python.exe.
            Write-Host "  Note: Windows Store alias is intercepting 'python'. Install Python and disable App Execution Alias for python.exe/python3.exe."
        }
    }
}

# Optional install flow (only when explicitly requested).
if ($OfferInstall -and $optionalNeedsAttention.Count -gt 0)
{
    Write-Host ""
    Write-Host "[agent-diagnostics] Install flow enabled."

    # If Python is missing/unusable, installing Python usually provides `py` too.
    # Skip duplicate `py` prompt in that case.
    $hasPythonAttention = $optionalNeedsAttention.Tool -contains "python"

    foreach ($tool in $optionalNeedsAttention)
    {
        if ($tool.Tool -eq "py" -and $hasPythonAttention)
        {
            continue
        }

        # In AutoInstall mode we skip prompts; otherwise ask per tool.
        $shouldInstall = $AutoInstall -or (Test-InstallConsent -Prompt "Install '$($tool.Tool)' now?")

        if ($shouldInstall)
        {
            Install-ToolWithWinget -Tool $tool
        }
    }

    if (-not $DryRun)
    {
        Write-Host ""
        Write-Host "[agent-diagnostics] Re-run diagnostics after installation to refresh statuses."
    }
}
elseif ($optionalNeedsAttention.Count -gt 0)
{
    # Guide users toward the opt-in install modes.
    Write-Host ""
    Write-Host "[agent-diagnostics] Run with -OfferInstall to be prompted for optional installs."
    Write-Host "[agent-diagnostics] Use -AutoInstall to install all optional tools automatically (still opt-in)."
    Write-Host "[agent-diagnostics] Add -DryRun to preview install commands without changing the machine."
}

# Required tools are mandatory for this repository's expected workflows.
if ($missingRequired.Count -gt 0)
{
    Write-Warning "[agent-diagnostics] Missing required tools detected: $($missingRequired.Tool -join ', ')"
    exit 1
}

# Healthy result path.
Write-Host "[agent-diagnostics] Required tool checks passed."
Write-Host "[agent-diagnostics] Diagnostics complete."
exit 0
