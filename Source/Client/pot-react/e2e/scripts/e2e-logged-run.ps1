#!/usr/bin/env pwsh
#requires -Version 7
<#
.SYNOPSIS
    Runs an E2E npm script while capturing the console output and a port/resource timeline.
.DESCRIPTION
    The Playwright HTML report does not contain the webServer output, so a run that fails
    because the client (or API) was unreachable leaves no record of what the server did.
    This wrapper captures both halves of the evidence for one run:

      e2e-<timestamp>.log        the full console output, including the `[WebServer]` lines
                                 Playwright forwards from the Vite dev server
      e2e-ports-<timestamp>.log  a timeline of the harness ports (5175 client, 5242 API,
                                 55432 Postgres, 9323 report server), free memory and the
                                 running container count, sampled every SampleSeconds

    Output is echoed to the console as well as written to disk, so the run looks normal
    while it happens. Logs go to e2e/logs (ignored by .gitignore) and nothing is deleted,
    so a later run never destroys an earlier run's evidence.
.PARAMETER Script
    The npm script to run when no test path is given. Defaults to the full matrix (e2e:all:dev).
.PARAMETER TestPath
    A single test file (or directory) to run instead of the script's own selection. The path is
    placed BEFORE --project because Playwright's --project is variadic: a trailing path is parsed
    as another project name and fails with "Project(s) ... not found".
.PARAMETER Project
    The project used with TestPath. Defaults to chromium, the fastest loop.
.PARAMETER SampleSeconds
    Interval between port/resource samples. Container counts are sampled every third
    interval to keep the `docker` CLI calls off the hot path.
.EXAMPLE
    npm run e2e:all:dev:log

.EXAMPLE
    pwsh ./e2e/scripts/e2e-logged-run.ps1 -Script e2e:smoke

.EXAMPLE
    pwsh ./e2e/scripts/e2e-logged-run.ps1 -TestPath e2e/tests/rendering/projectionsRendering.test.ts
#>

param(
    [string]$Script = 'e2e:all:dev',
    [string]$TestPath = '',
    [string]$Project = 'chromium',
    [int]$SampleSeconds = 10
)

$ErrorActionPreference = 'Stop'

# e2e/scripts -> e2e -> the client project root (the package.json that owns these scripts).
$clientDirectory = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$logDirectory = Join-Path $clientDirectory 'e2e/logs'
$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$outputLog = Join-Path $logDirectory "e2e-$timestamp.log"
$sampleLog = Join-Path $logDirectory "e2e-ports-$timestamp.log"

New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null

Write-Host ''
Write-Host '═══════════════════════════════════════════════════════' -ForegroundColor Cyan
Write-Host (" Logged E2E run: npm run {0}" -f $Script) -ForegroundColor Cyan
Write-Host (" console log:    {0}" -f $outputLog) -ForegroundColor DarkGray
Write-Host (" port timeline:  {0}" -f $sampleLog) -ForegroundColor DarkGray
Write-Host '═══════════════════════════════════════════════════════' -ForegroundColor Cyan
Write-Host ''

$sampler = Start-Job -ScriptBlock {
    param($TargetPath, $IntervalSeconds)

    # E2E harness endpoints. Keep in sync with playwright.config.ts.
    $clientPort = 5175
    $apiPort = 5242
    $postgresPort = 55432
    $reportPort = 9323
    $iteration = 0

    while ($true) {
        $freeGb = [Math]::Round((Get-CimInstance Win32_OperatingSystem).FreePhysicalMemory / 1MB, 1)

        $line = '{0}  client={1} api={2} postgres={3} report={4} freeGB={5}' -f `
        (Get-Date -Format 'HH:mm:ss'),
        [bool](Get-NetTCPConnection -State Listen -LocalPort $clientPort -ErrorAction SilentlyContinue),
        [bool](Get-NetTCPConnection -State Listen -LocalPort $apiPort -ErrorAction SilentlyContinue),
        [bool](Get-NetTCPConnection -State Listen -LocalPort $postgresPort -ErrorAction SilentlyContinue),
        [bool](Get-NetTCPConnection -State Listen -LocalPort $reportPort -ErrorAction SilentlyContinue),
        $freeGb

        if ($iteration % 3 -eq 0) {
            try {
                $line += ' containers={0}' -f @(docker ps -q 2>$null).Count
            }
            catch {
                $line += ' containers=unavailable'
            }
        }

        Add-Content -Path $TargetPath -Value $line
        $iteration++
        Start-Sleep -Seconds $IntervalSeconds
    }
} -ArgumentList $sampleLog, $SampleSeconds

$npmArguments = @('run', $Script)

if ($TestPath) {
    # e2e:dev is the flagless `playwright test`, so the file filter can be placed first.
    $npmArguments = @('run', 'e2e:dev', '--', $TestPath, "--project=$Project")
}

$writer = [System.IO.StreamWriter]::new($outputLog, $false, [System.Text.UTF8Encoding]::new($false))
$writer.AutoFlush = $true
$exitCode = 0
$previousErrorActionPreference = $ErrorActionPreference

try {
    Push-Location $clientDirectory

    # Native stderr redirected into the pipeline arrives as error records; keep the
    # preference permissive for the duration of the run so they are captured, not thrown.
    $ErrorActionPreference = 'Continue'

    & npm @npmArguments 2>&1 | ForEach-Object {
        $line = $_.ToString()
        Write-Host $line
        $writer.WriteLine($line)
    }

    $exitCode = $LASTEXITCODE
}
finally {
    $ErrorActionPreference = $previousErrorActionPreference
    $writer.Dispose()
    Stop-Job $sampler -ErrorAction SilentlyContinue
    Receive-Job $sampler -ErrorAction SilentlyContinue | Out-Null
    Remove-Job $sampler -Force -ErrorAction SilentlyContinue
    Pop-Location
}

$lastRunPath = Join-Path $clientDirectory 'test-results/.last-run.json'
$status = 'unknown'
$failedCount = 0

if (Test-Path $lastRunPath) {
    $lastRun = Get-Content $lastRunPath -Raw | ConvertFrom-Json
    $status = $lastRun.status
    $failedCount = @($lastRun.failedTests).Count
}

$summary = @(
    ''
    '# ---- logged run summary ----'
    ("# npm run {0} exit code: {1}" -f $Script, $exitCode)
    ("# test-results/.last-run.json status: {0} (failed tests: {1})" -f $status, $failedCount)
    ("# console log:   {0}" -f $outputLog)
    ("# port timeline: {0}" -f $sampleLog)
)

Add-Content -Path $outputLog -Value $summary

Write-Host ''
Write-Host '═══════════════════════════════════════════════════════' -ForegroundColor Cyan
Write-Host (" exit code: {0}   status: {1}   failed tests: {2}" -f $exitCode, $status, $failedCount) -ForegroundColor $(if ($exitCode -eq 0) { 'Green' } else { 'Red' })
Write-Host (" console log:   {0}" -f $outputLog) -ForegroundColor DarkGray
Write-Host (" port timeline: {0}" -f $sampleLog) -ForegroundColor DarkGray

if ($exitCode -ne 0) {
    Write-Host ''
    Write-Host ' Diagnostics to compare against the timeline:' -ForegroundColor Yellow
    Write-Host ("   Select-String -Path '{0}' -Pattern '\[WebServer\]|Error|already used|Timed out'" -f $outputLog) -ForegroundColor DarkGray
    Write-Host ("   Get-Content '{0}'" -f $sampleLog) -ForegroundColor DarkGray
    Write-Host ' A client=False reading from early in the timeline means the Vite server was not serving;' -ForegroundColor Yellow
    Write-Host ' a client=True reading that turns False marks the moment it went away.' -ForegroundColor Yellow
}

Write-Host '═══════════════════════════════════════════════════════' -ForegroundColor Cyan

exit $exitCode
