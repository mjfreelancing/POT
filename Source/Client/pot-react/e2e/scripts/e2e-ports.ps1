#!/usr/bin/env pwsh
#requires -Version 7
<#
.SYNOPSIS
    The ports the E2E harness owns, and what an occupant of each means.
.DESCRIPTION
    Dot-source this file from an E2E script (`. (Join-Path $PSScriptRoot 'e2e-ports.ps1')`). It defines functions
    only and runs nothing when loaded, so its decisions can be tested without starting anything.

    Keep the numbers in sync with playwright.config.ts, playwright.globalSetup.ts and e2e/README.md.

    Three kinds of port:

      * required    - the run binds it itself, so an occupant makes Playwright abort before any test
      * reclaimable - the run's own container holds it, so a leftover is reported and replaced rather than tolerated
                      silently, and anything else holding it is a failure
      * tolerated   - globalSetup reclaims it, so an occupant is only worth a warning
#>

function Get-E2eMailSinkContainerName {
    return 'pot-e2e-mail-sink'
}

function Get-E2ePortDefinitions {
    return @(
        [pscustomobject]@{ Key = 'client'; Port = 5175; Name = 'Vite dev server (webServer[1])'; Category = 'required' }
        [pscustomobject]@{ Key = 'api'; Port = 5242; Name = 'ASP.NET Core API (webServer[0])'; Category = 'required' }
        [pscustomobject]@{ Key = 'mail-sink-smtp'; Port = 1025; Name = 'E2E mail sink (SMTP)'; Category = 'reclaimable' }
        [pscustomobject]@{ Key = 'mail-sink-read'; Port = 1080; Name = 'E2E mail sink (read back)'; Category = 'reclaimable' }
        [pscustomobject]@{ Key = 'postgres'; Port = 55432; Name = 'Testcontainers Postgres'; Category = 'tolerated' }
        [pscustomobject]@{ Key = 'report'; Port = 9323; Name = 'Playwright HTML report server'; Category = 'tolerated' }
    )
}

function Get-E2ePortsByCategory {
    param([Parameter(Mandatory)][string]$Category)

    return @(Get-E2ePortDefinitions | Where-Object { $_.Category -eq $Category })
}

function Get-E2eRequiredPorts {
    return @(Get-E2ePortsByCategory -Category 'required')
}

function Get-E2eReclaimablePorts {
    return @(Get-E2ePortsByCategory -Category 'reclaimable')
}

function Get-E2eToleratedPorts {
    return @(Get-E2ePortsByCategory -Category 'tolerated')
}

<#
.SYNOPSIS
    Decides what an occupied port means for a run.
.OUTPUTS
    'free', 'fail', 'reclaim' or 'tolerated'.
#>
function Get-E2ePortVerdict {
    param(
        [Parameter(Mandatory)][pscustomobject]$Entry,
        [object[]]$Owners = @(),
        [bool]$SinkContainerPresent = $false
    )

    $isOccupied = @($Owners).Count -gt 0

    if ($Entry.Category -eq 'tolerated') {
        return $(if ($isOccupied) { 'tolerated' } else { 'free' })
    }

    if (-not $isOccupied) {
        return 'free'
    }

    if ($Entry.Category -eq 'reclaimable' -and $SinkContainerPresent) {
        return 'reclaim'
    }

    return 'fail'
}
