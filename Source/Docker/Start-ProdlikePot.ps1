#Requires -Version 5.1
<#
.SYNOPSIS
    Builds and starts the POT prodlike Docker stack.

.DESCRIPTION
    Creates the postgres-data directory if needed, builds all Docker images
    with a timestamped tag (re-tagged as latest), then starts all services
    (postgres, server, client) defined in docker-compose-client-server.yml.

    Data is stored in the postgres-data folder next to this script and persists
    between container restarts. Deleting the postgres-data folder will wipe the
    database. It is never removed by 'docker compose down'.

    This script is equivalent to the VS Code 'docker-start-client-server' task,
    but can also be run standalone from a terminal.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$composeFile = Join-Path $PSScriptRoot 'docker-compose-client-server.yml'
$envFile = Join-Path $PSScriptRoot '.env'
$envFileDevelopment = Join-Path $PSScriptRoot '.env.development'
$dataDirectory = Join-Path $PSScriptRoot 'postgres-data'

# Postgres requires the bind-mount directory to exist before first start.
if (-not (Test-Path $dataDirectory)) {
    New-Item -ItemType Directory -Path $dataDirectory | Out-Null
    Write-Host 'Created postgres-data directory.' -ForegroundColor DarkGray
}

Write-Host 'Building POT prodlike Docker images...' -ForegroundColor Cyan

$env:IMAGE_TAG = Get-Date -Format 'yyyyMMdd-HHmmss'

docker-compose --env-file $envFile --env-file $envFileDevelopment -f $composeFile build

if ($LASTEXITCODE -ne 0) {
    Write-Error "docker-compose build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

docker tag "pot-server:$($env:IMAGE_TAG)" 'pot-server:latest'
docker tag "pot-client:$($env:IMAGE_TAG)" 'pot-client:latest'

$env:IMAGE_TAG = 'latest'

Write-Host 'Starting POT prodlike Docker stack...' -ForegroundColor Cyan

docker-compose --env-file $envFile --env-file $envFileDevelopment -f $composeFile up -d

if ($LASTEXITCODE -ne 0) {
    Write-Error "docker-compose up failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host ''
Write-Host 'POT prodlike stack is running.' -ForegroundColor Green
Write-Host '  Client:     http://localhost:5175'
Write-Host '  Server:     http://localhost:5241'
Write-Host '  PostgreSQL: localhost:5432'
Write-Host ''
Write-Host 'To stop:   docker-compose --env-file "' -NoNewline
Write-Host $envFile -NoNewline
Write-Host '" --env-file "' -NoNewline
Write-Host $envFileDevelopment -NoNewline
Write-Host '" -f "' -NoNewline
Write-Host $composeFile -NoNewline
Write-Host '" down'
