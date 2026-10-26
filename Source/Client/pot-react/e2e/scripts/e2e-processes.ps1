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
function Get-ProcessesMatchingCommandLine {
    param(
        [string[]]$Like = @(),
        [string[]]$NotLike = @()
    )

    $results = @()

    foreach ($process in @(Get-CimInstance Win32_Process -ErrorAction SilentlyContinue)) {
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
    return @(Get-ProcessesMatchingCommandLine -Like @('*@playwright*cli.js*', '*test-server*'))
}

# A `playwright test` client that is still executing, i.e. a run in progress.
function Get-PlaywrightTestClients {
    return @(Get-ProcessesMatchingCommandLine -Like @('*@playwright*cli.js*', '*test*') -NotLike @('*test-server*'))
}

# A Vite dev server that outlived its Playwright parent. Vitest also matches `*vite*`.
function Get-ViteDevServers {
    return @(Get-ProcessesMatchingCommandLine -Like @('*vite*') -NotLike @('*test-server*', '*@playwright*', '*vitest*'))
}

# The harness API only: identified by the URL it is told to bind, so a Debug instance started
# for client development (which does not pass `--urls`) is never a target.
function Get-E2eApiProcesses {
    return @(Get-ProcessesMatchingCommandLine -Like @('*Pot.AspNetCore*', '*--urls http://127.0.0.1:5242*'))
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
