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

    Only those are stopped. Docker containers are never touched (stop the dev stack with the
    VS Code task `docker-stop-client-server`) and nothing is stopped while an E2E run is still
    in progress. Exits 1 when the client or API port is still held afterwards.
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

# E2E harness endpoints. Keep in sync with playwright.config.ts and e2e/README.md.
$clientPort = 5175
$apiPort = 5242

Write-Host ''
Write-Host '═══════════════════════════════════════════════════════' -ForegroundColor Cyan
Write-Host ' E2E clean' -ForegroundColor Cyan
Write-Host '═══════════════════════════════════════════════════════' -ForegroundColor Cyan

$testClients = @(Get-PlaywrightTestClients)

if ($testClients.Count -gt 0) {
    Write-Host ''
    Write-Host ' An E2E run is in progress - nothing was stopped.' -ForegroundColor Yellow

    foreach ($client in $testClients) {
        Write-Host ("   pid {0} {1}" -f $client.ProcessId, (Get-ShortCommandLine $client.CommandLine)) -ForegroundColor DarkGray
    }

    Write-Host ' Wait for it to finish (or stop it) and run this again.' -ForegroundColor Yellow
    Write-Host '═══════════════════════════════════════════════════════' -ForegroundColor Cyan
    exit 1
}

$targets = @(
    [pscustomobject]@{ Name = 'Playwright test-server daemon'; Processes = @(Get-PlaywrightDaemons) }
    [pscustomobject]@{ Name = 'stray Vite dev server'; Processes = @(Get-ViteDevServers) }
    [pscustomobject]@{ Name = 'E2E ASP.NET Core API'; Processes = @(Get-E2eApiProcesses) }
)

Write-Host ''
Write-Host ' Leftovers' -ForegroundColor Cyan

foreach ($target in $targets) {
    if ($target.Processes.Count -eq 0) {
        Write-Host ("  [ ok ] no {0}" -f $target.Name) -ForegroundColor Green
        continue
    }

    foreach ($process in $target.Processes) {
        $verb = if ($DryRun) { 'would stop' } else { '[stop]' }
        Write-Host ("  {0} {1} (pid {2}, started {3})" -f $verb, $target.Name, $process.ProcessId, $process.CreationDate.ToString('HH:mm:ss')) -ForegroundColor Yellow
        Write-Host ("         {0}" -f (Get-ShortCommandLine $process.CommandLine)) -ForegroundColor DarkGray

        if (-not $DryRun) {
            Stop-E2eProcess -Processes $process | Out-Null
        }
    }
}

Write-Host ''
Write-Host ' Ports' -ForegroundColor Cyan

$blocked = @()

foreach ($entry in @(
        [pscustomobject]@{ Port = $clientPort; Name = 'client (Vite)' }
        [pscustomobject]@{ Port = $apiPort; Name = 'API' }
    )) {
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
else {
    Write-Host ' CLEAN - the next run starts with its own daemon and free ports.' -ForegroundColor Green
}

Write-Host ' Next: npm run e2e:preflight   then   npm run e2e:all:dev:log' -ForegroundColor Cyan
Write-Host '═══════════════════════════════════════════════════════' -ForegroundColor Cyan

exit 0
