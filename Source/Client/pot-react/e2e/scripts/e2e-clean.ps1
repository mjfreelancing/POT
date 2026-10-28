#!/usr/bin/env pwsh
#requires -Version 7
<#
.SYNOPSIS
    Stops what an interrupted E2E run leaves behind, so the next run starts clean.
.DESCRIPTION
    A run that is interrupted (closed terminal, agent timeout, native crash, Ctrl+C) leaves
    processes that either hold a harness port or make the next run attach to a stale Playwright
    daemon:

      * the Playwright `test-server` daemon - holds no E2E port, but it still owns the previous
        run's webServers, so its teardown can stop the new run's Vite server mid-run
      * a stray Vite dev server on 5175
      * the harness ASP.NET Core API on 5242

    It also removes the harness's own mail sink container, which a killed run leaves holding the
    sink's ports. Only the container named by this harness is a target: the development Docker
    stack is never stopped here (use the VS Code task `docker-stop-client-server`), and nothing is
    stopped while an E2E run is still in progress. Exits 1 when the client or API port is still
    held afterwards.

    The decision about what to stop lives in `Get-E2eCleanPlan`, so it is covered by the script
    suite in `e2e/scripts/tests` rather than only by running this script.
.PARAMETER DryRun
    Report what would be stopped without stopping anything.
.EXAMPLE
    npm run e2e:clean

.EXAMPLE
    pwsh ./e2e/scripts/e2e-clean.ps1 -DryRun
#>

param(
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'e2e-processes.ps1')
. (Join-Path $PSScriptRoot 'e2e-ports.ps1')

Write-Host ''
Write-Host '═══════════════════════════════════════════════════════' -ForegroundColor Cyan
Write-Host ' E2E clean' -ForegroundColor Cyan
Write-Host '═══════════════════════════════════════════════════════' -ForegroundColor Cyan

$runClients = @(Get-PlaywrightTestClients)

$plan = Get-E2eCleanPlan -RunInProgress ($runClients.Count -gt 0) `
    -RunProcesses $runClients `
    -Daemons @(Get-PlaywrightDaemons) `
    -ViteServers @(Get-ViteDevServers) `
    -ApiProcesses @(Get-E2eApiProcesses) `
    -Containers @(Get-E2eRunningContainers)

if ($plan.Refused) {
    Write-Host ''
    Write-Host (" {0}." -f $plan.RefusalReason) -ForegroundColor Yellow
    Write-Host ' Wait for it to finish (or stop it) and run this again.' -ForegroundColor Yellow
    Write-Host '═══════════════════════════════════════════════════════' -ForegroundColor Cyan
    exit 1
}

Write-Host ''
Write-Host ' Leftovers' -ForegroundColor Cyan

$stoppedProcesses = @()
$stoppedContainers = @()

if ($plan.Targets.Count -eq 0) {
    Write-Host '  [ ok ] nothing left behind by an interrupted run' -ForegroundColor Green
}

foreach ($target in $plan.Targets) {
    $verb = if ($DryRun) { 'would stop' } else { '[stop]' }

    if ($target.Kind -eq 'container') {
        Write-Host ("  {0} {1} ({2})" -f $verb, $target.Name, $target.ContainerName) -ForegroundColor Yellow

        if (-not $DryRun) {
            $removed = docker rm -f $target.ContainerName 2>&1

            if ($LASTEXITCODE -eq 0) {
                $stoppedContainers += $target.ContainerName
            }
            else {
                Write-Host ("         could not remove it: {0}" -f $removed) -ForegroundColor DarkGray
            }
        }

        continue
    }

    foreach ($process in $target.Processes) {
        Write-Host ("  {0} {1} (pid {2}, started {3})" -f $verb, $target.Name, $process.ProcessId, $process.CreationDate.ToString('HH:mm:ss')) -ForegroundColor Yellow
        Write-Host ("         {0}" -f (Get-ShortCommandLine $process.CommandLine)) -ForegroundColor DarkGray

        if (-not $DryRun) {
            $stoppedProcesses += @(Stop-E2eProcess -Processes $process)
        }
    }
}

Write-Host ''
Write-Host ' Ports' -ForegroundColor Cyan

$blocked = @()

foreach ($entry in (Get-E2eRequiredPorts)) {
    $listeners = @(Get-NetTCPConnection -State Listen -LocalPort $entry.Port -ErrorAction SilentlyContinue)

    if ($listeners.Count -eq 0) {
        Write-Host ("  [ ok ] {0} port {1} is free" -f $entry.Name, $entry.Port) -ForegroundColor Green
        continue
    }

    Write-Host ("  [FAIL] {0} port {1} is still in use" -f $entry.Name, $entry.Port) -ForegroundColor Red

    foreach ($listener in $listeners) {
        $owner = Get-Process -Id $listener.OwningProcess -ErrorAction SilentlyContinue
        Write-Host ("         pid {0} {1}" -f $listener.OwningProcess, $owner.ProcessName) -ForegroundColor DarkGray
    }

    Write-Host "         if this is the Docker dev stack: run the VS Code task 'docker-stop-client-server'" -ForegroundColor DarkGray

    $blocked += ("port {0} ({1})" -f $entry.Port, $entry.Name)
}

Write-Host ''
Write-Host '═══════════════════════════════════════════════════════' -ForegroundColor Cyan

if ($blocked.Count -gt 0) {
    Write-Host (" NOT CLEAN - still held: {0}" -f ($blocked -join ', ')) -ForegroundColor Red
    Write-Host '═══════════════════════════════════════════════════════' -ForegroundColor Cyan
    exit 1
}

if ($DryRun) {
    Write-Host ' DRY RUN - nothing was stopped. Drop -DryRun to clear the leftovers.' -ForegroundColor Yellow
}
elseif ($stoppedProcesses.Count -eq 0 -and $stoppedContainers.Count -eq 0) {
    Write-Host ' CLEAN - there was nothing left to stop.' -ForegroundColor Green
}
else {
    Write-Host (" CLEAN - stopped {0} process(es) and {1} container(s)." -f $stoppedProcesses.Count, $stoppedContainers.Count) -ForegroundColor Green
}

Write-Host ' Next: npm run e2e:preflight   then   npm run e2e:all:dev:log' -ForegroundColor Cyan
Write-Host '═══════════════════════════════════════════════════════' -ForegroundColor Cyan

exit 0
