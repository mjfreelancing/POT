#Requires -Version 5.1
<#
.SYNOPSIS
    Starts the Local POT PostgreSQL container.

.DESCRIPTION
    Pulls the postgres:13 image if not already present, then starts the
    pot-local container group defined in docker-compose.yml.

    The container exposes PostgreSQL on localhost:5432 for use while running
    the POT .NET API locally in Visual Studio.

    Data is stored in the postgres-data folder next to this script and persists
    between container restarts. Deleting the postgres-data folder will wipe the
    database. It is never removed by 'docker compose down'.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$composeFile = Join-Path $PSScriptRoot 'docker-compose.yml'
$dataDirectory = Join-Path $PSScriptRoot 'postgres-data'

# Postgres requires the bind-mount directory to exist before first start.
if (-not (Test-Path $dataDirectory)) {
    New-Item -ItemType Directory -Path $dataDirectory | Out-Null
    Write-Host 'Created postgres-data directory.' -ForegroundColor DarkGray
}

Write-Host 'Starting Local POT PostgreSQL container...' -ForegroundColor Cyan

docker compose -f $composeFile up -d

if ($LASTEXITCODE -ne 0) {
    Write-Error "docker compose up failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host ''
Write-Host 'Local POT PostgreSQL is running.' -ForegroundColor Green
Write-Host '  Host:     localhost'
Write-Host '  Port:     5432'
Write-Host '  Database: Pot'
Write-Host '  Username: postgres'
Write-Host '  Password: password'
Write-Host ''
Write-Host 'To stop:        docker compose -f "' -NoNewline
Write-Host $composeFile -NoNewline
Write-Host '" stop'
Write-Host 'To remove:      docker compose -f "' -NoNewline
Write-Host $composeFile -NoNewline
Write-Host '" down'
Write-Host 'To remove data: docker compose -f "' -NoNewline
Write-Host $composeFile -NoNewline
Write-Host '" down -v'
