#!/usr/bin/env pwsh
#requires -Version 7
<#
.SYNOPSIS
    Test E2E seed data export from local Docker database
.DESCRIPTION
    Exports Site, User, and UserRole tables from the running pot-postgres container
    to seed/baseline.sql for reproducible E2E testing.
.EXAMPLE
    # If you're in Windows PowerShell (5.x), launch pwsh first
    pwsh ./test-e2e-seed-export.ps1

    # Or, if you're already in a pwsh (PowerShell 7+) shell
    ./test-e2e-seed-export.ps1
#>

param(
    [switch]$Force,
    [string]$OutputPath = "../seed/baseline.sql"
)

$ErrorActionPreference = "Stop"

if (-not [System.IO.Path]::IsPathRooted($OutputPath)) {
    $OutputPath = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot $OutputPath))
}

# Colors for output
$Success = "Green"
$Warning = "Yellow"
$Error_ = "Red"
$Info = "Cyan"

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor $Info
Write-Host "E2E Seed Data Export Test" -ForegroundColor $Info
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor $Info
Write-Host ""

# Step 1: Check prerequisites
Write-Host "Step 1: Checking prerequisites..." -ForegroundColor $Info

# Check if Docker is running
Write-Host "  • Checking Docker..." -NoNewline
& docker ps --quiet 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host " ✗" -ForegroundColor $Error_
    Write-Host ""
    Write-Host "Docker is not running. Please start Docker Desktop." -ForegroundColor $Error_
    exit 1
}
Write-Host " ✓" -ForegroundColor $Success

# Check if pot-postgres container exists and is running
Write-Host "  • Checking pot-postgres container..." -NoNewline
$container = & docker ps --filter "name=pot-postgres" --quiet 2>&1
if (-not $container) {
    Write-Host " ✗" -ForegroundColor $Error_
    Write-Host ""
    Write-Host "  Container not found. Start it with:" -ForegroundColor $Warning
    Write-Host "    npm run docker-start-client-server" -ForegroundColor $Warning
    exit 1
}
Write-Host " ✓" -ForegroundColor $Success

# Check if output directory exists
Write-Host "  • Checking output directory..." -NoNewline
$outputDir = Split-Path -Parent $OutputPath
if (-not (Test-Path $outputDir)) {
    Write-Host " (creating)" -ForegroundColor $Warning -NoNewline
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}
Write-Host " ✓" -ForegroundColor $Success

Write-Host ""
Write-Host "Step 2: Exporting seed data..." -ForegroundColor $Info

Write-Host "  • Running pg_dump..." -NoNewline

try {
    $pgDumpArgs = @(
        'exec'
        '-e'
        'PGPASSWORD=password'
        'pot-postgres'
        'pg_dump'
        '--data-only'
        '-t'
        '"Site"'
        '-t'
        '"User"'
        '-t'
        '"UserRole"'
        '-U'
        'postgres'
        '-d'
        'Pot'
    )

    $output = & docker @pgDumpArgs 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host " ✗" -ForegroundColor $Error_
        Write-Host ""
        Write-Host "pg_dump failed. Output:" -ForegroundColor $Error_
        Write-Host $output -ForegroundColor $Error_
        throw "Database export failed"
    }
    
    $output | Out-File -FilePath $OutputPath -Encoding UTF8 -Force:$Force
    Write-Host " ✓" -ForegroundColor $Success
} catch {
    Write-Host " ✗" -ForegroundColor $Error_
    Write-Host ""
    Write-Host $_.Exception.Message -ForegroundColor $Error_
    exit 1
}

Write-Host ""
Write-Host "Step 3: Verifying export..." -ForegroundColor $Info

# Check file exists and has content
if (-not (Test-Path $OutputPath)) {
    Write-Host "  • File existence..." -NoNewline
    Write-Host " ✗" -ForegroundColor $Error_
    Write-Host ""
    Write-Host "Export file was not created" -ForegroundColor $Error_
    exit 1
}
Write-Host "  • File existence..." -NoNewline
Write-Host " ✓" -ForegroundColor $Success

# Get file size
$fileSize = (Get-Item $OutputPath).Length
Write-Host "  • File size: $($fileSize) bytes" -ForegroundColor $Info

# Count tables exported
$siteCount = (Select-String -Path $OutputPath -Pattern '^COPY public."Site"' | Measure-Object).Count
$userCount = (Select-String -Path $OutputPath -Pattern '^COPY public."User"' | Measure-Object).Count
$userRoleCount = (Select-String -Path $OutputPath -Pattern '^COPY public."UserRole"' | Measure-Object).Count

Write-Host "  • Tables exported:" -ForegroundColor $Info
Write-Host "      - Site: $siteCount" -ForegroundColor $(if ($siteCount -gt 0) { $Success } else { $Warning })
Write-Host "      - User: $userCount" -ForegroundColor $(if ($userCount -gt 0) { $Success } else { $Warning })
Write-Host "      - UserRole: $userRoleCount" -ForegroundColor $(if ($userRoleCount -gt 0) { $Success } else { $Warning })

# Count lines
$lineCount = (Get-Content $OutputPath | Measure-Object -Line).Lines
Write-Host "  • Total lines: $lineCount"

# Show first few lines
Write-Host ""
Write-Host "Step 4: Preview (first 10 lines):" -ForegroundColor $Info
Get-Content $OutputPath -TotalCount 10 | ForEach-Object { Write-Host "  $_" }
Write-Host "  ..." -ForegroundColor $Info

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor $Success
Write-Host "✓ Export successful!" -ForegroundColor $Success
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor $Success
Write-Host ""
Write-Host "Location: $OutputPath"
Write-Host "Size: $fileSize bytes"
Write-Host ""
