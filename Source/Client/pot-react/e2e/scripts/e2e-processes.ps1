#!/usr/bin/env pwsh
#requires -Version 7
<#
.SYNOPSIS
    Process discovery and cleanup shared by the E2E scripts.
.DESCRIPTION
    Dot-source this file from an E2E script (`. (Join-Path $PSScriptRoot 'e2e-processes.ps1')`).
    It defines functions only and runs nothing when loaded.

    Why this exists: `playwright test` is a thin CLI client for a long-lived `test-server`
    daemon (`node_modules/@playwright/test/cli.js test-server`). The daemon outlives the CLI,
    so a run that is hard-killed - a closed terminal, an agent timeout, a native crash (exit
    code -1073740791 / 0xC0000409) - leaves a daemon that still owns the previous job's
    webServers. The next run attaches to that daemon instead of starting a clean one, and the
    abandoned job's teardown then stops the Vite dev server the new run depends on: every test
    fails with `net::ERR_CONNECTION_REFUSED at http://127.0.0.1:5175` while the API stays
    healthy. The daemon holds no E2E port (it listens on a random one), so checking the E2E
    ports alone never sees it.
#>

# All of $Like must match; any $NotLike match excludes the process.
# -Processes exists so the decision can be tested without looking at the machine.
function Get-ProcessesMatchingCommandLine {
    param(
        [string[]]$Like = @(),
        [string[]]$NotLike = @(),
        [object[]]$Processes = $null
    )

    if ($null -eq $Processes) {
        $Processes = @(Get-CimInstance Win32_Process -ErrorAction SilentlyContinue)
    }

    $results = @()

    foreach ($process in @($Processes)) {
        $commandLine = $process.CommandLine

        if (-not $commandLine) {
            continue
        }

        $isMatch = $true

        foreach ($pattern in $Like) {
            if ($commandLine -notlike $pattern) {
                $isMatch = $false
                break
            }
        }

        if (-not $isMatch) {
            continue
        }

        foreach ($pattern in $NotLike) {
            if ($commandLine -like $pattern) {
                $isMatch = $false
                break
            }
        }

        if ($isMatch) {
            $results += $process
        }
    }

    return $results
}

function Get-ShortCommandLine {
    param(
        [string]$CommandLine,
        [int]$Length = 150
    )

    if ($commandLine -and $commandLine.Length -gt $Length) {
        return $commandLine.Substring(0, $Length) + '...'
    }

    return $commandLine
}

# The Playwright CLI daemon. Harmless while a run is in progress; poison once its run is gone.
function Get-PlaywrightDaemons {
    param([object[]]$Processes = $null)

    return @(Get-ProcessesMatchingCommandLine -Processes $Processes -Like @('*@playwright*cli.js*', '*test-server*'))
}

# A `playwright test` client that is still executing, i.e. a run in progress.
function Get-PlaywrightTestClients {
    param([object[]]$Processes = $null)

    return @(Get-ProcessesMatchingCommandLine -Processes $Processes -Like @('*@playwright*cli.js*', '*test*') -NotLike @('*test-server*'))
}

# A Vite dev server that outlived its Playwright parent. Vitest also matches `*vite*`.
function Get-ViteDevServers {
    param([object[]]$Processes = $null)

    return @(Get-ProcessesMatchingCommandLine -Processes $Processes -Like @('*vite*') -NotLike @('*test-server*', '*@playwright*', '*vitest*'))
}

# The harness API only: identified by the URL it is told to bind, so a Debug instance started
# for client development (which does not pass `--urls`) is never a target.
function Get-E2eApiProcesses {
    param([object[]]$Processes = $null)

    return @(Get-ProcessesMatchingCommandLine -Processes $Processes -Like @('*Pot.AspNetCore*', '*--urls http://127.0.0.1:5242*'))
}

function Stop-E2eProcess {
    param([Parameter(Mandatory)][object[]]$Processes)

    $stopped = @()

    foreach ($process in $Processes) {
        Stop-Process -Id $process.ProcessId -Force -ErrorAction SilentlyContinue

        if (-not (Get-Process -Id $process.ProcessId -ErrorAction SilentlyContinue)) {
            $stopped += $process
        }
    }

    return $stopped
}

# Running containers, as `docker ps` reports them. Returns nothing when Docker is unreachable.
function Get-E2eRunningContainers {
    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'

    try {
        $lines = @(docker ps --format '{{.Names}}|{{.Image}}' 2>$null)
    }
    catch {
        $lines = @()
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    if ($LASTEXITCODE -ne 0) {
        return @()
    }

    return @($lines | Where-Object { $_ } | ForEach-Object {
            $parts = $_ -split '\|'

            [pscustomobject]@{ Name = $parts[0]; Image = $parts[1] }
        })
}

# The harness's own mail sink, if a previous run left it behind. Named rather than matched broadly, so the
# development stack (pot-*) is never a candidate.
function Get-E2eMailSinkContainers {
    param([object[]]$Containers = $null)

    if ($null -eq $Containers) {
        $Containers = @(Get-E2eRunningContainers)
    }

    $sinkName = 'pot-e2e-mail-sink'

    return @($Containers | Where-Object { $_.Name -eq $sinkName })
}

<#
.SYNOPSIS
    Decides what an interrupted run left behind, without stopping anything.
.OUTPUTS
    An object with Refused, RefusalReason and Targets. Each target has Name, Kind ('process' or 'container'),
    Processes and ContainerName.
#>
function Get-E2eCleanPlan {
    param(
        [bool]$RunInProgress = $false,
        [object[]]$RunProcesses = @(),
        [object[]]$Daemons = @(),
        [object[]]$ViteServers = @(),
        [object[]]$ApiProcesses = @(),
        [object[]]$Containers = @()
    )

    if ($RunInProgress) {
        $processIds = (@($RunProcesses) | ForEach-Object { $_.ProcessId }) -join ', '

        return [pscustomobject]@{
            Refused       = $true
            RefusalReason = "an E2E run is in progress (pid $processIds) - nothing was stopped"
            Targets       = @()
        }
    }

    $targets = @()

    if (@($Daemons).Count -gt 0) {
        $targets += [pscustomobject]@{ Name = 'Playwright test-server daemon'; Kind = 'process'; Processes = @($Daemons); ContainerName = $null }
    }

    if (@($ViteServers).Count -gt 0) {
        $targets += [pscustomobject]@{ Name = 'stray Vite dev server'; Kind = 'process'; Processes = @($ViteServers); ContainerName = $null }
    }

    if (@($ApiProcesses).Count -gt 0) {
        $targets += [pscustomobject]@{ Name = 'E2E ASP.NET Core API'; Kind = 'process'; Processes = @($ApiProcesses); ContainerName = $null }
    }

    foreach ($sink in @(Get-E2eMailSinkContainers -Containers $Containers)) {
        $targets += [pscustomobject]@{ Name = 'E2E mail sink container'; Kind = 'container'; Processes = @(); ContainerName = $sink.Name }
    }

    return [pscustomobject]@{
        Refused       = $false
        RefusalReason = ''
        Targets       = @($targets)
    }
}
