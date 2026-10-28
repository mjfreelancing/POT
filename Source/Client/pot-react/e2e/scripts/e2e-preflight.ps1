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

    A leftover Playwright `test-server` daemon is reported as a warning even though it holds
    no E2E port: the daemon outlives a killed run, the next run can attach to it, and the
    abandoned job's teardown then stops the Vite server mid-run (every test failing with
    ERR_CONNECTION_REFUSED at 5175 while the API stays up). `npm run e2e:clean` stops it and
    `npm run e2e:all:dev:log` clears it automatically before starting.
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

. (Join-Path $PSScriptRoot 'e2e-processes.ps1')
. (Join-Path $PSScriptRoot 'e2e-ports.ps1')

# Ports the run binds itself, including the mail sink it starts: an occupant makes Playwright abort, unless the
# occupant is the run's own sink container, which the run replaces.
$requiredPorts = @(Get-E2eRequiredPorts) + @(Get-E2eReclaimablePorts)

# Ports globalSetup reclaims for itself, so an occupant is only worth a warning.
$toleratedPorts = @(Get-E2eToleratedPorts)

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

$sinkContainerPresent = @(Get-E2eMailSinkContainers).Count -gt 0

foreach ($entry in $requiredPorts) {
    $owners = @(Get-PortOwners -Port $entry.Port)
    $verdict = Get-E2ePortVerdict -Entry $entry -Owners $owners -SinkContainerPresent $sinkContainerPresent

    if ($verdict -eq 'free') {
        Write-Pass ("{0} port {1} is free" -f $entry.Name, $entry.Port)
        continue
    }

    if ($verdict -eq 'reclaim') {
        Write-Warn ("{0} port {1} is held by the E2E mail sink a previous run left behind" -f $entry.Name, $entry.Port)

        foreach ($owner in $owners) {
            Write-OwnerDetail -Owner $owner
        }

        Write-Host '         the run reports it, removes the container and starts a fresh sink' -ForegroundColor DarkGray
        Write-Host '         resolve now: npm run e2e:clean' -ForegroundColor DarkGray

        $warnings += ("port {0} ({1}) is held by a leftover E2E mail sink" -f $entry.Port, $entry.Name)
        continue
    }

    Write-Fail ("{0} port {1} is in use - Playwright will abort with 'already used'" -f $entry.Name, $entry.Port)

    foreach ($owner in $owners) {
        Write-OwnerDetail -Owner $owner
    }

    $stopCommand = "Get-NetTCPConnection -State Listen -LocalPort $($entry.Port) | ForEach-Object { Stop-Process -Id `$_.OwningProcess -Force }"
    Write-Host ("         resolve: {0}" -f $stopCommand) -ForegroundColor DarkGray

    if ($entry.Key -eq 'client') {
        Write-Host ("         or stop the Docker dev stack: task 'docker-stop-client-server' (its client publish {0})" -f $entry.Port) -ForegroundColor DarkGray
    }

    $failures += ("port {0} ({1}) is in use" -f $entry.Port, $entry.Name)
}

Write-Section 'Harness-managed ports'

foreach ($entry in $toleratedPorts) {
    $owners = @(Get-PortOwners -Port $entry.Port)

    if (@($owners).Count -eq 0) {
        Write-Pass ("{0} port {1} is free" -f $entry.Name, $entry.Port)
        continue
    }

    Write-Warn ("{0} port {1} is in use (usually a leftover from a killed run)" -f $entry.Name, $entry.Port)

    foreach ($owner in $owners) {
        Write-OwnerDetail -Owner $owner
    }

    if ($entry.Key -eq 'postgres') {
        Write-Host '         globalSetup removes a stale Testcontainers container itself; the run will only log a warning.' -ForegroundColor DarkGray
    }

    $warnings += ("port {0} ({1}) is in use" -f $entry.Port, $entry.Name)
}

Write-Section 'Stray processes'

$daemons = @(Get-PlaywrightDaemons)
$testClients = @(Get-PlaywrightTestClients)
$viteServers = @(Get-ViteDevServers)

foreach ($daemon in $daemons) {
    Write-Warn ("Playwright test-server daemon already running (pid {0}, started {1:yyyy-MM-dd HH:mm:ss})" -f $daemon.ProcessId, $daemon.CreationDate)
    Write-Host ("         {0}" -f (Get-ShortCommandLine $daemon.CommandLine)) -ForegroundColor DarkGray
    $warnings += ("Playwright daemon already running (pid {0})" -f $daemon.ProcessId)
}

if ($daemons.Count -gt 0) {
    Write-Host '         Normal after any run and for the editor''s test discovery. It only matters when the' -ForegroundColor DarkGray
    Write-Host '         daemon belongs to a KILLED run: it still owns that run''s webServers, and its' -ForegroundColor DarkGray
    Write-Host '         teardown can stop the next run''s Vite server (ERR_CONNECTION_REFUSED at 5175).' -ForegroundColor DarkGray
    Write-Host '         resolve: npm run e2e:clean, or run through e2e:all:dev:log which clears it first' -ForegroundColor DarkGray
}

foreach ($client in $testClients) {
    Write-Warn ("an E2E run is in progress: {0} (pid {1})" -f $client.Name, $client.ProcessId)
    Write-Host ("         {0}" -f (Get-ShortCommandLine $client.CommandLine)) -ForegroundColor DarkGray
    Write-Host '         two stacks cannot share ports 5175/5242 - let it finish first' -ForegroundColor DarkGray
    $warnings += ("an E2E run is in progress (pid {0})" -f $client.ProcessId)
}

foreach ($server in $viteServers) {
    Write-Warn ("stray Vite dev server: {0} (pid {1})" -f $server.Name, $server.ProcessId)
    Write-Host ("         {0}" -f (Get-ShortCommandLine $server.CommandLine)) -ForegroundColor DarkGray
    Write-Host '         resolve: npm run e2e:clean' -ForegroundColor DarkGray
    $warnings += ("stray Vite dev server (pid {0})" -f $server.ProcessId)
}

if ($daemons.Count -eq 0 -and $testClients.Count -eq 0 -and $viteServers.Count -eq 0) {
    Write-Pass 'no leftover Playwright daemon, running E2E session or stray Vite server'
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
