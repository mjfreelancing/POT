#!/usr/bin/env pwsh
#requires -Version 7
<#
.SYNOPSIS
    Characterises what the E2E port rules decide today, so a change in them is a failing test rather than a surprise.
.DESCRIPTION
    These tests describe the ports the harness owns. They are written against the decisions the scripts expose, not
    against their console output, so the report can change without rewriting the expectation.
#>

BeforeAll {
    . (Join-Path $PSScriptRoot '..' 'e2e-ports.ps1')
}

Describe 'E2E port definitions' {
    It 'requires the client and the API, and only those' {
        $required = @(Get-E2eRequiredPorts)

        $required.Port | Should -Be (5175, 5242)
        ($required | Where-Object { $_.Category -ne 'required' }).Count | Should -Be 0
    }

    It 'treats the mail sink ports as reclaimable by the harness that started it' {
        $reclaimable = @(Get-E2eReclaimablePorts)

        $reclaimable.Port | Should -Be (1025, 1080)
        $reclaimable.Name | Should -Contain 'E2E mail sink (SMTP)'
        $reclaimable.Name | Should -Contain 'E2E mail sink (read back)'
    }

    It 'tolerates the ports globalSetup reclaims for itself' {
        $tolerated = @(Get-E2eToleratedPorts)

        $tolerated.Port | Should -Be (55432, 9323)
    }

    It 'reports every harness port, so an occupant is identified before a run rather than during one' {
        $all = @(Get-E2ePortDefinitions)

        $all.Port | Should -Contain 5175
        $all.Port | Should -Contain 5242
        $all.Port | Should -Contain 1025
        $all.Port | Should -Contain 1080
        $all.Port | Should -Contain 55432
        $all.Port | Should -Contain 9323
    }

    It 'names every port, and no port twice' {
        $all = @(Get-E2ePortDefinitions)

        ($all | Where-Object { [string]::IsNullOrWhiteSpace($_.Name) }).Count | Should -Be 0
        ($all.Port | Sort-Object -Unique).Count | Should -Be $all.Count
    }
}

Describe 'E2E port verdict' {
    It 'reports a port nobody holds as free' {
        $entry = @(Get-E2eRequiredPorts) | Where-Object { $_.Port -eq 5175 }

        Get-E2ePortVerdict -Entry $entry -Owners @() -SinkContainerPresent $false | Should -Be 'free'
    }

    It 'reports a required port held by anything as a failure' {
        $entry = @(Get-E2eRequiredPorts) | Where-Object { $_.Port -eq 5175 }
        $owners = @([pscustomobject]@{ ProcessId = 42; ProcessName = 'docker' })

        Get-E2ePortVerdict -Entry $entry -Owners $owners -SinkContainerPresent $false | Should -Be 'fail'
    }

    It 'reclaims a sink port the harness own sink is holding' {
        $entry = @(Get-E2eReclaimablePorts) | Where-Object { $_.Port -eq 1025 }
        $owners = @([pscustomobject]@{ ProcessId = 43; ProcessName = 'com.docker.backend' })

        Get-E2ePortVerdict -Entry $entry -Owners $owners -SinkContainerPresent $true | Should -Be 'reclaim'
    }

    It 'fails a sink port held by something that is not the harness sink' {
        $entry = @(Get-E2eReclaimablePorts) | Where-Object { $_.Port -eq 1025 }
        $owners = @([pscustomobject]@{ ProcessId = 44; ProcessName = 'MailHog' })

        Get-E2ePortVerdict -Entry $entry -Owners $owners -SinkContainerPresent $false | Should -Be 'fail'
    }

    It 'reports a tolerated port held by anything as tolerated' {
        $entry = @(Get-E2eToleratedPorts) | Where-Object { $_.Port -eq 55432 }
        $owners = @([pscustomobject]@{ ProcessId = 45; ProcessName = 'com.docker.backend' })

        Get-E2ePortVerdict -Entry $entry -Owners $owners -SinkContainerPresent $false | Should -Be 'tolerated'
    }
}
