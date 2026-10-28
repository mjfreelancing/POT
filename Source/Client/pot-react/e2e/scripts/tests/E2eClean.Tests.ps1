#!/usr/bin/env pwsh
#requires -Version 7
<#
.SYNOPSIS
    Characterises what the E2E clean command decides to stop, and what it refuses to touch.
.DESCRIPTION
    The plan is asserted rather than the console output: which leftovers are targets, that a run in progress is
    refused, and that only the harness's own mail sink container is ever a container target - the development Docker
    stack is never stopped by this command.
#>

BeforeAll {
    . (Join-Path $PSScriptRoot '..' 'e2e-ports.ps1')
    . (Join-Path $PSScriptRoot '..' 'e2e-processes.ps1')

    function New-Process {
        param(
            [int]$ProcessId,
            [string]$CommandLine
        )

        [pscustomobject]@{ ProcessId = $ProcessId; CommandLine = $CommandLine; CreationDate = (Get-Date) }
    }

    function New-Container {
        param(
            [string]$Name,
            [string]$Image = 'maildev/maildev:2.2.1'
        )

        [pscustomobject]@{ Name = $Name; Image = $Image }
    }
}

Describe 'E2E clean plan' {
    It 'refuses to stop anything while a run is in progress, naming the process' {
        $plan = Get-E2eCleanPlan -RunInProgress $true -Daemons @() -ViteServers @() -ApiProcesses @() -Containers @() -RunProcesses @(
            (New-Process -ProcessId 99 -CommandLine 'node @playwright/test/cli.js test --project chromium')
        )

        $plan.Refused | Should -BeTrue
        $plan.RefusalReason | Should -Match '99'
        $plan.Targets.Count | Should -Be 0
    }

    It 'targets the daemon, the stray dev server and the harness API' {
        $plan = Get-E2eCleanPlan -RunInProgress $false `
            -Daemons @(New-Process -ProcessId 10 -CommandLine 'node @playwright/test/cli.js test-server') `
            -ViteServers @(New-Process -ProcessId 20 -CommandLine 'node node_modules/vite/bin/vite.js --port 5175') `
            -ApiProcesses @(New-Process -ProcessId 30 -CommandLine 'dotnet Pot.AspNetCore --urls http://127.0.0.1:5242') `
            -Containers @() -RunProcesses @()

        $plan.Refused | Should -BeFalse
        $plan.Targets.Kind | Should -Be @('process', 'process', 'process')
        $plan.Targets.Name | Should -Contain 'Playwright test-server daemon'
        $plan.Targets.Name | Should -Contain 'stray Vite dev server'
        $plan.Targets.Name | Should -Contain 'E2E ASP.NET Core API'
    }

    It 'targets the harness mail sink container when a previous run left it behind' {
        $plan = Get-E2eCleanPlan -RunInProgress $false -Daemons @() -ViteServers @() -ApiProcesses @() `
            -Containers @(New-Container -Name (Get-E2eMailSinkContainerName)) -RunProcesses @()

        $plan.Refused | Should -BeFalse
        $plan.Targets.Count | Should -Be 1
        $plan.Targets[0].Kind | Should -Be 'container'
        $plan.Targets[0].ContainerName | Should -Be (Get-E2eMailSinkContainerName)
    }

    It 'never targets the development Docker stack, however many containers are running' {
        $plan = Get-E2eCleanPlan -RunInProgress $false -Daemons @() -ViteServers @() -ApiProcesses @() `
            -Containers @(
                (New-Container -Name 'pot-postgres-1' -Image 'postgres:13')
                (New-Container -Name 'pot-server-1' -Image 'pot-server:latest')
                (New-Container -Name 'pot-client-1' -Image 'pot-client:latest')
            ) -RunProcesses @()

        $plan.Refused | Should -BeFalse
        $plan.Targets.Count | Should -Be 0
    }

    It 'has nothing to do when the environment is already clean' {
        $plan = Get-E2eCleanPlan -RunInProgress $false -Daemons @() -ViteServers @() -ApiProcesses @() -Containers @() -RunProcesses @()

        $plan.Refused | Should -BeFalse
        $plan.Targets.Count | Should -Be 0
    }
}
