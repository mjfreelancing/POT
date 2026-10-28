#!/usr/bin/env pwsh
#requires -Version 7
<#
.SYNOPSIS
    Characterises how the E2E scripts identify a daemon, a stray dev server and the harness API.
.DESCRIPTION
    The match patterns are the part that has repeatedly cost hours: a pattern that is too broad stops something a
    developer needs, and one that is too narrow leaves the leftover that breaks the next run. The tests feed process
    records in, so they describe the decision rather than the machine.
#>

BeforeAll {
    . (Join-Path $PSScriptRoot '..' 'e2e-processes.ps1')

    function New-Process {
        param(
            [int]$ProcessId,
            [string]$CommandLine
        )

        [pscustomobject]@{ ProcessId = $ProcessId; CommandLine = $CommandLine }
    }
}

Describe 'Process matching' {
    It 'matches when every required pattern matches' {
        $processes = @(New-Process -ProcessId 1 -CommandLine 'node @playwright/test/cli.js test-server --port 1')

        $matched = @(Get-ProcessesMatchingCommandLine -Processes $processes -Like @('*@playwright*cli.js*', '*test-server*'))

        $matched.ProcessId | Should -Be 1
    }

    It 'excludes a process that matches an excluded pattern' {
        $processes = @(New-Process -ProcessId 2 -CommandLine 'node @playwright/test/cli.js test-server --port 1')

        $matched = @(Get-ProcessesMatchingCommandLine -Processes $processes -Like @('*@playwright*cli.js*') -NotLike @('*test-server*'))

        $matched.Count | Should -Be 0
    }

    It 'ignores a process with no command line' {
        $processes = @([pscustomobject]@{ ProcessId = 3; CommandLine = $null })

        (Get-ProcessesMatchingCommandLine -Processes $processes -Like @('*')).Count | Should -Be 0
    }
}

Describe 'Leftover identification' {
    It 'identifies the Playwright test-server daemon and not a running test client' {
        $processes = @(
            New-Process -ProcessId 10 -CommandLine 'node ...@playwright\test\cli.js test-server --port 1234'
            New-Process -ProcessId 11 -CommandLine 'node ...@playwright\test\cli.js test --project chromium'
        )

        $daemons = @(Get-PlaywrightDaemons -Processes $processes)
        $clients = @(Get-PlaywrightTestClients -Processes $processes)

        $daemons.ProcessId | Should -Be 10
        $clients.ProcessId | Should -Be 11
    }

    It 'identifies a stray Vite dev server but not a Vitest worker or a Playwright process' {
        $processes = @(
            New-Process -ProcessId 20 -CommandLine 'node node_modules/vite/bin/vite.js --port 5175'
            New-Process -ProcessId 21 -CommandLine 'node node_modules/vitest/vitest.mjs run'
            New-Process -ProcessId 22 -CommandLine 'node @playwright/test/cli.js test-server'
        )

        $servers = @(Get-ViteDevServers -Processes $processes)

        $servers.ProcessId | Should -Be 20
    }

    It 'identifies only the harness API, which is told to bind the harness URL' {
        $processes = @(
            New-Process -ProcessId 30 -CommandLine 'dotnet run --project Pot.AspNetCore/Pot.AspNetCore.csproj -- --urls http://127.0.0.1:5242'
            New-Process -ProcessId 31 -CommandLine 'dotnet run --project Pot.AspNetCore/Pot.AspNetCore.csproj'
        )

        $apiProcesses = @(Get-E2eApiProcesses -Processes $processes)

        $apiProcesses.ProcessId | Should -Be 30
    }
}

Describe 'Container classification' {
    BeforeAll {
        $clientPort = 5175
        $mailSinkContainer = 'pot-e2e-mail-sink'

        function New-ClassifiedContainer {
            param(
                [string]$Name,
                [string]$Image = 'pot-client:latest',
                [string]$Ports = ''
            )

            return Get-E2eContainerClassification -Name $Name -Image $Image -Ports $Ports -ClientPort $clientPort -MailSinkContainer $mailSinkContainer
        }
    }

    It 'reports the harness mail sink as its own leftover, not as the development stack' {
        $containers = @(
            New-ClassifiedContainer -Name 'pot-e2e-mail-sink' -Image 'maildev/maildev:2.2.1' -Ports '127.0.0.1:1025->1025/tcp, 127.0.0.1:1080->1080/tcp'
        )

        $containers[0].Kind | Should -Be 'e2e-leftover'
    }

    It 'identifies the development client by the port it publishes, not by its name' {
        $containers = @(
            New-ClassifiedContainer -Name 'pot-react' -Image 'pot-client:latest' -Ports '0.0.0.0:5175->80/tcp'
        )

        $containers[0].Kind | Should -Be 'dev-client'
    }

    It 'does not read the client port out of a longer port number' {
        $containers = @(
            New-ClassifiedContainer -Name 'pot-react' -Image 'pot-client:latest' -Ports '0.0.0.0:15175->80/tcp'
        )

        $containers[0].Kind | Should -Be 'dev-stack'
    }

    It 'reports a development stack container that does not publish the client port as the development stack' {
        $containers = @(
            New-ClassifiedContainer -Name 'pot-aspnet' -Image 'pot-server:latest' -Ports '0.0.0.0:5241->8080/tcp'
        )

        $containers[0].Kind | Should -Be 'dev-stack'
    }

    It 'does not report the development stack Postgres as a leftover E2E container, though it runs the same image' {
        $containers = @(
            New-ClassifiedContainer -Name 'pot-postgres' -Image 'postgres:17-alpine' -Ports '0.0.0.0:5444->5432/tcp'
        )

        $containers[0].Kind | Should -Be 'dev-stack'
    }

    It 'reports a Postgres container outside the development stack as a leftover E2E container' {
        $containers = @(
            New-ClassifiedContainer -Name 'testcontainers-postgres-1' -Image 'postgres:17-alpine' -Ports '0.0.0.0:55432->5432/tcp'
        )

        $containers[0].Kind | Should -Be 'e2e-leftover'
    }

    It 'reports a container belonging to neither as unrelated' {
        $containers = @(
            New-ClassifiedContainer -Name 'some-other-app' -Image 'nginx:alpine' -Ports '0.0.0.0:8080->80/tcp'
        )

        $containers[0].Kind | Should -Be 'unrelated'
    }
}
