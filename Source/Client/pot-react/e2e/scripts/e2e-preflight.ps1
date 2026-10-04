#!/usr/bin/env pwsh
#requires -Version 7
<#
.SYNOPSIS
    Reports whether the machine is ready to start a Playwright E2E run.
.DESCRIPTION
    The E2E harness owns fixed ports and starts its own stack (see playwright.config.ts):
    the Vite dev server on 5175, the ASP.NET Core API on 5242 and a Testcontainers
    Postgres on 55432. Both webServers use `reuseExistingServer: false`, so an occupied
    port makes Playwright abort before any test runs, and a port that becomes occupied
    (or is released by a leftover process) mid-run makes every test fail to connect.

    This script only reports; it never stops a process or a container. Each problem it
    finds is printed with the exact command that resolves it.
.PARAMETER SkipDocker
    Skip the container and Docker daemon checks (useful when Docker Desktop is known to
    be down and the intent is to inspect ports only).
.EXAMPLE
    npm run e2e:preflight

.EXAMPLE
    pwsh ./e2e/scripts/e2e-preflight.ps1 -SkipDocker
#>

param(
    [switch]$SkipDocker
)

$ErrorActionPreference = 'Stop'

# E2E harness endpoints. Keep in sync with playwright.config.ts and e2e/README.md.
$clientPort = 5175
$apiPort = 5242
$postgresPort = 55432
$reportPort = 9323

# Ports the harness binds itself: an occupant makes Playwright abort.
$requiredPorts = @(
    [pscustomobject]@{ Port = $clientPort; Name = 'Vite dev server (webServer[1])' },
    [pscustomobject]@{ Port = $apiPort; Name = 'ASP.NET Core API (webServer[0])' }
)

# Ports that are expected to be held by the harness but tolerate a leftover, because
# globalSetup removes a stale Testcontainers Postgres before binding 55432 again.
$toleratedPorts = @(
    [pscustomobject]@{ Port = $postgresPort; Name = 'Testcontainers Postgres' },
    [pscustomobject]@{ Port = $reportPort; Name = 'Playwright HTML report server' }
)

$failures = @()
$warnings = @()

function Write-Section {
    param([string]$Title)

    Write-Host ''
    Write-Host "── $Title " -ForegroundColor Cyan -NoNewline
    Write-Host ('─' * [Math]::Max(0, 60 - $Title.Length)) -ForegroundColor DarkGray
}

function Write-Pass {
    param([string]$Message)

    Write-Host '  [ ok ] ' -ForegroundColor Green -NoNewline
    Write-Host $Message
}

function Write-Warn {
    param([string]$Message)

    Write-Host '  [warn] ' -ForegroundColor Yellow -NoNewline
    Write-Host $Message
}

function Write-Fail {
    param([string]$Message)

    Write-Host '  [FAIL] ' -ForegroundColor Red -NoNewline
    Write-Host $Message
}

function Get-PortOwners {
    param([int]$Port)

    $connections = @(Get-NetTCPConnection -State Listen -LocalPort $Port -ErrorAction SilentlyContinue)
    $owners = @()

    foreach ($connection in $connections) {
        $process = Get-Process -Id $connection.OwningProcess -ErrorAction SilentlyContinue

        if (-not $process) {
            continue
        }

        $commandLine = (Get-CimInstance Win32_Process -Filter "ProcessId=$($process.Id)" -ErrorAction SilentlyContinue).CommandLine

        $owners += [pscustomobject]@{
            Address     = $connection.LocalAddress
            ProcessId   = $process.Id
            ProcessName = $process.ProcessName
            CommandLine = $commandLine
        }
    }

    return $owners
}

function Write-OwnerDetail {
    param([pscustomobject]$Owner)

    $commandLine = $Owner.CommandLine

    if ($commandLine -and $commandLine.Length -gt 150) {
        $commandLine = $commandLine.Substring(0, 150) + '...'
    }

    Write-Host ("         {0}:{1}" -f $Owner.Address, $Owner.ProcessName) -ForegroundColor DarkGray -NoNewline
    Write-Host (" (pid {0})" -f $Owner.ProcessId) -ForegroundColor DarkGray

    if ($commandLine) {
        Write-Host ("         {0}" -f $commandLine) -ForegroundColor DarkGray
    }
}

Write-Host ''
Write-Host '═══════════════════════════════════════════════════════' -ForegroundColor Cyan
Write-Host ' E2E preflight' -ForegroundColor Cyan
Write-Host '═══════════════════════════════════════════════════════' -ForegroundColor Cyan

Write-Section 'Required ports'

foreach ($entry in $requiredPorts) {
    $owners = @(Get-PortOwners -Port $entry.Port)

    if ($owners.Count -eq 0) {
        Write-Pass ("{0} port {1} is free" -f $entry.Name, $entry.Port)
        continue
    }

    Write-Fail ("{0} port {1} is in use - Playwright will abort with 'already used'" -f $entry.Name, $entry.Port)

    foreach ($owner in $owners) {
        Write-OwnerDetail -Owner $owner
    }

    $stopCommand = "Get-NetTCPConnection -State Listen -LocalPort $($entry.Port) | ForEach-Object { Stop-Process -Id `$_.OwningProcess -Force }"
    Write-Host ("         resolve: {0}" -f $stopCommand) -ForegroundColor DarkGray
    Write-Host ("         or stop the Docker dev stack: task 'docker-stop-client-server' (its client publish {0})" -f $clientPort) -ForegroundColor DarkGray

    $failures += ("port {0} ({1}) is in use" -f $entry.Port, $entry.Name)
}

Write-Section 'Harness-managed ports'

foreach ($entry in $toleratedPorts) {
    $owners = @(Get-PortOwners -Port $entry.Port)

    if ($owners.Count -eq 0) {
        Write-Pass ("{0} port {1} is free" -f $entry.Name, $entry.Port)
        continue
    }

    Write-Warn ("{0} port {1} is in use (usually a leftover from a killed run)" -f $entry.Name, $entry.Port)

    foreach ($owner in $owners) {
        Write-OwnerDetail -Owner $owner
    }

    if ($entry.Port -eq $postgresPort) {
        Write-Host '         globalSetup removes a stale Testcontainers container itself; the run will only log a warning.' -ForegroundColor DarkGray
    }

    $warnings += ("port {0} ({1}) is in use" -f $entry.Port, $entry.Name)
}

Write-Section 'Stray processes'

$strayProcesses = @()

foreach ($process in @(Get-CimInstance Win32_Process -ErrorAction SilentlyContinue)) {
    $commandLine = $process.CommandLine

    if (-not $commandLine) {
        continue
    }

    # A running test session. `test-server` (VS Code Test Explorer) and `@playwright/mcp`
    # bind no E2E port, so they are deliberately not treated as strays.
    if ($commandLine -like '*playwright*cli.js*test *') {
        $strayProcesses += $process
        continue
    }

    # A Vite dev server that outlived its Playwright parent.
    if ($commandLine -like '*vite*' -and $commandLine -notlike '*test-server*') {
        $strayProcesses += $process
    }
}

if ($strayProcesses.Count -eq 0) {
    Write-Pass 'no stray Vite or Playwright test processes'
}
else {
    foreach ($process in $strayProcesses) {
        $commandLine = $process.CommandLine

        if ($commandLine.Length -gt 150) {
            $commandLine = $commandLine.Substring(0, 150) + '...'
        }

        Write-Warn ("{0} (pid {1})" -f $process.Name, $process.ProcessId)
        Write-Host ("         {0}" -f $commandLine) -ForegroundColor DarkGray
    }

    Write-Host '         resolve: Stop-Process -Id <pid> -Force for each process above' -ForegroundColor DarkGray
    $warnings += ("{0} stray process(es) found" -f $strayProcesses.Count)
}

if (-not $SkipDocker) {
    Write-Section 'Docker'

    $dockerAvailable = $null -ne (Get-Command docker -ErrorAction SilentlyContinue)

    if (-not $dockerAvailable) {
        Write-Fail 'the docker CLI is not on PATH - Testcontainers Postgres cannot start'
        $failures += 'docker CLI not found'
    }
    else {
        $runningContainers = @()
        $previousErrorActionPreference = $ErrorActionPreference
        $ErrorActionPreference = 'Continue'

        try {
            $runningContainers = @(docker ps --format '{{.Names}}|{{.Image}}|{{.Ports}}' 2>$null)
        }
        catch {
            $runningContainers = @()
        }
        finally {
            $ErrorActionPreference = $previousErrorActionPreference
        }

        if ($LASTEXITCODE -ne 0) {
            Write-Fail 'the Docker daemon is not reachable - start Docker Desktop before running E2E'
            $failures += 'docker daemon unreachable'
        }
        elseif ($runningContainers.Count -eq 0) {
            Write-Pass 'no containers running'
        }
        else {
            foreach ($container in $runningContainers) {
                $parts = $container -split '\|'
                $isE2eStack = $parts[0] -like 'testcontainers-ryuk*' -or $parts[1] -like 'postgres:*'
                $isDevStack = $parts[0] -like 'pot-*'

                if ($isE2eStack) {
                    Write-Warn ("leftover E2E container: {0} ({1})" -f $parts[0], $parts[1])
                    $warnings += ("leftover E2E container {0}" -f $parts[0])
                    continue
                }

                if ($isDevStack) {
                    # Identify the client by the port it publishes, not by name: the compose
                    # container is `pot-react` while its image is `pot-client`.
                    if ($parts[2] -like "*$clientPort*") {
                        Write-Fail ("dev stack client {0} is running - it publishes port {1}, which the E2E Vite server needs" -f $parts[0], $clientPort)
                        $failures += ("dev stack client {0} publishes port {1}" -f $parts[0], $clientPort)
                    }
                    else {
                        Write-Warn ("dev stack container {0} is running (it does not publish {1})" -f $parts[0], $clientPort)
                        $warnings += ("dev stack container {0} is running" -f $parts[0])
                    }

                    Write-Host "         resolve: run the VS Code task 'docker-stop-client-server'" -ForegroundColor DarkGray
                    continue
                }

                Write-Warn ("unrelated container: {0} ({1})" -f $parts[0], $parts[1])
            }
        }
    }
}

Write-Section 'Run state'

$runtimeStatePath = Join-Path (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)) 'e2e/.runtime/testcontainers.json'

if (Test-Path $runtimeStatePath) {
    Write-Warn 'a stale Testcontainers state file exists (e2e/.runtime/testcontainers.json)'
    Write-Host '         globalSetup removes the container it names and deletes the file before starting a new one.' -ForegroundColor DarkGray
    $warnings += 'stale Testcontainers state file'
}
else {
    Write-Pass 'no stale Testcontainers state file'
}

$operatingSystem = Get-CimInstance Win32_OperatingSystem
$freeGb = [Math]::Round($operatingSystem.FreePhysicalMemory / 1MB, 1)
$totalGb = [Math]::Round($operatingSystem.TotalVisibleMemorySize / 1MB, 1)

Write-Host ''
Write-Host ("  free memory: {0} GB of {1} GB (Docker Desktop reserves a large share)" -f $freeGb, $totalGb) -ForegroundColor DarkGray

Write-Host ''
Write-Host '═══════════════════════════════════════════════════════' -ForegroundColor Cyan

if ($failures.Count -gt 0) {
    Write-Host (" NOT READY - {0} blocking issue(s)" -f $failures.Count) -ForegroundColor Red

    foreach ($failure in $failures) {
        Write-Host ("   - {0}" -f $failure) -ForegroundColor Red
    }

    Write-Host ''
    Write-Host ' The stack will either abort at startup or fail to connect. Resolve the items above first.' -ForegroundColor Red
    Write-Host '═══════════════════════════════════════════════════════' -ForegroundColor Cyan
    exit 1
}

if ($warnings.Count -gt 0) {
    Write-Host (" READY - {0} warning(s), none blocking" -f $warnings.Count) -ForegroundColor Yellow

    foreach ($warning in $warnings) {
        Write-Host ("   - {0}" -f $warning) -ForegroundColor Yellow
    }
}
else {
    Write-Host ' READY - environment is clean' -ForegroundColor Green
}

Write-Host ''
Write-Host ' Next: npm run e2e:all:dev:log   (full matrix, console output captured)' -ForegroundColor Cyan
Write-Host '═══════════════════════════════════════════════════════' -ForegroundColor Cyan

exit 0
