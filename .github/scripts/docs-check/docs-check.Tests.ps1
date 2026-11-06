#Requires -Version 7.0

BeforeAll {
    . (Join-Path $PSScriptRoot 'docs-check.ps1')
    $script:fixtures = Join-Path $PSScriptRoot 'fixtures'

    function New-FixtureCopy {
        param([string] $Name)

        $target = Join-Path $TestDrive ("{0}-{1}" -f $Name, [guid]::NewGuid().ToString('N').Substring(0, 8))
        Copy-Item -Path (Join-Path $script:fixtures $Name) -Destination $target -Recurse

        return $target
    }

}

Describe 'docs-check' {
    Context 'a well-formed set' {
        It 'reports no errors and regenerates a stale index with -Update' {
            $root = New-FixtureCopy -Name 'good'
            $script:Root = (Resolve-Path -LiteralPath $root).Path

            $firstRun = Invoke-DocsCheck -RepositoryRoot $script:Root -UpdateIndex
            $secondRun = Invoke-DocsCheck -RepositoryRoot $script:Root

            $firstRun | Should -Be 0
            $secondRun | Should -Be 0
        }

        It 'fails when the generated index is out of date, and -Update fixes it' {
            $root = New-FixtureCopy -Name 'good'
            $script:Root = (Resolve-Path -LiteralPath $root).Path

            Invoke-DocsCheck -RepositoryRoot $script:Root -UpdateIndex | Out-Null

            $readmePath = Join-Path $script:Root 'Docs/adr/README.md'
            $content = [IO.File]::ReadAllText($readmePath)
            $content = $content -replace '(?s)<!-- BEGIN GENERATED INDEX -->.*<!-- END GENERATED INDEX -->', "<!-- BEGIN GENERATED INDEX -->`r`n<!-- END GENERATED INDEX -->"
            [IO.File]::WriteAllText($readmePath, $content)

            $errors = [System.Collections.Generic.List[string]]::new()
            Test-AdrIndex -Records (Get-AdrRecords -Errors $errors) -Errors $errors
            ($errors -join "`n") | Should -Match 'generated index is out of date'

            Invoke-DocsCheck -RepositoryRoot $script:Root -UpdateIndex | Should -Be 0
            Invoke-DocsCheck -RepositoryRoot $script:Root | Should -Be 0
        }
    }

    Context 'an empty series' {
        It 'reports no errors when no ADR has been written yet' {
            $root = New-FixtureCopy -Name 'good'
            $script:Root = (Resolve-Path -LiteralPath $root).Path

            $adrDirectory = Join-Path $script:Root 'Docs/adr'
            Get-ChildItem -Path $adrDirectory -Filter '*.md' -File |
                Where-Object { $_.Name -ne 'README.md' } |
                ForEach-Object { Remove-Item -LiteralPath $_.FullName }

            # The fixture instruction file cites a record, so it must not cite one that no longer exists.
            Set-Content -Path (Join-Path $script:Root '.github/instructions/server.instructions.md') -Value 'Handlers are static.'

            Invoke-DocsCheck -RepositoryRoot $script:Root -UpdateIndex | Should -Be 0
            Invoke-DocsCheck -RepositoryRoot $script:Root | Should -Be 0
        }
    }

    Context 'reference facts' {
        It 'reports a fact that more than one document repeats' {
            $root = New-FixtureCopy -Name 'good'
            $script:Root = (Resolve-Path -LiteralPath $root).Path

            Add-Content -Path (Join-Path $script:Root 'Docs/README.md') -Value 'The client runs on 5175.'
            Add-Content -Path (Join-Path $script:Root 'Docs/adr/ADR-0002-good-proposed-decision.md') -Value 'The client also runs on 5175.'

            $report = Get-ReferenceFactReport -Root $script:Root

            ($report -join "`n") | Should -Match 'client port 5175'
            ($report -join "`n") | Should -Match 'repeated'
        }
    }

    Context 'front matter' {
        It 'rejects a status outside the allowed set' {
            $root = New-FixtureCopy -Name 'bad'
            $script:Root = (Resolve-Path -LiteralPath $root).Path

            $errors = [System.Collections.Generic.List[string]]::new()
            Get-AdrRecords -Errors $errors | Out-Null

            ($errors -join "`n") | Should -Match "status 'approved' is not one of"
        }

        It 'requires every required field' {
            $root = New-FixtureCopy -Name 'bad'
            $script:Root = (Resolve-Path -LiteralPath $root).Path

            $errors = [System.Collections.Generic.List[string]]::new()
            Get-AdrRecords -Errors $errors | Out-Null

            ($errors -join "`n") | Should -Match "the required field 'area' is missing"
        }

        It 'rejects front matter it cannot parse' {
            $root = New-FixtureCopy -Name 'bad'
            $script:Root = (Resolve-Path -LiteralPath $root).Path

            $errors = [System.Collections.Generic.List[string]]::new()
            Get-AdrRecords -Errors $errors | Out-Null

            ($errors -join "`n") | Should -Match 'unsupported front matter'
        }

        It 'rejects a file name that carries no four-digit number' {
            $root = New-FixtureCopy -Name 'good'
            $script:Root = (Resolve-Path -LiteralPath $root).Path

            $adrDirectory = Join-Path $script:Root 'Docs/adr'
            Copy-Item -Path (Join-Path $adrDirectory 'ADR-0001-good-accepted-decision.md') -Destination (Join-Path $adrDirectory 'not-numbered.md')

            $errors = [System.Collections.Generic.List[string]]::new()
            Get-AdrRecords -Errors $errors | Out-Null

            ($errors -join "`n") | Should -Match 'must be ADR-NNNN-kebab-case-title.md'
        }

        It 'rejects a numbered file name that lacks the ADR- prefix' {
            $root = New-FixtureCopy -Name 'good'
            $script:Root = (Resolve-Path -LiteralPath $root).Path

            $adrDirectory = Join-Path $script:Root 'Docs/adr'
            Rename-Item -LiteralPath (Join-Path $adrDirectory 'ADR-0002-good-proposed-decision.md') -NewName '0002-good-proposed-decision.md'

            $errors = [System.Collections.Generic.List[string]]::new()
            Get-AdrRecords -Errors $errors | Out-Null

            ($errors -join "`n") | Should -Match '0002-good-proposed-decision.md: the file name must be ADR-NNNN-kebab-case-title.md'
        }
    }

    Context 'links' {
        It 'rejects a broken relative link' {
            $root = New-FixtureCopy -Name 'bad'
            $script:Root = (Resolve-Path -LiteralPath $root).Path

            $errors = [System.Collections.Generic.List[string]]::new()
            Test-MarkdownLinks -Errors $errors | Out-Null

            ($errors -join "`n") | Should -Match "broken relative link 'missing.md'"
        }

        It 'ignores links inside a fenced code block' {
            $root = New-FixtureCopy -Name 'good'
            $script:Root = (Resolve-Path -LiteralPath $root).Path

            $readmePath = Join-Path $script:Root 'Docs/README.md'
            $content = [IO.File]::ReadAllText($readmePath)
            $content += "`r`n``````markdown`r`nAn example: [not a link](does-not-exist.md).`r`n``````r`n"
            [IO.File]::WriteAllText($readmePath, $content)

            $errors = [System.Collections.Generic.List[string]]::new()
            Test-MarkdownLinks -Errors $errors | Out-Null

            ($errors -join "`n") | Should -Not -Match 'does-not-exist'
        }
    }

    Context 'instruction citations' {
        It 'rejects a citation of an ADR that is not accepted' {
            $root = New-FixtureCopy -Name 'bad'
            $script:Root = (Resolve-Path -LiteralPath $root).Path

            $errors = [System.Collections.Generic.List[string]]::new()
            Test-InstructionCitations -Records (Get-AdrRecords -Errors $errors) -Errors $errors | Out-Null

            ($errors -join "`n") | Should -Match "ADR-0004, which is 'proposed' rather than 'accepted'"
        }

        It 'rejects a citation of an ADR that does not exist' {
            $root = New-FixtureCopy -Name 'bad'
            $script:Root = (Resolve-Path -LiteralPath $root).Path

            $errors = [System.Collections.Generic.List[string]]::new()
            Test-InstructionCitations -Records (Get-AdrRecords -Errors $errors) -Errors $errors | Out-Null

            ($errors -join "`n") | Should -Match 'ADR-0099, which does not exist'
        }
    }

    Context 'document identifiers in code' {
        It 'rejects a PRD identifier in a source file' {
            $root = New-FixtureCopy -Name 'bad'
            $script:Root = (Resolve-Path -LiteralPath $root).Path

            $errors = [System.Collections.Generic.List[string]]::new()
            Test-BannedReferences -Errors $errors | Out-Null

            ($errors -join "`n") | Should -Match 'Example\.cs:5'
            ($errors -join "`n") | Should -Match 'a document identifier is not allowed in code'
        }

        It 'allows a developer guide under Source to cite an ADR' {
            $root = New-FixtureCopy -Name 'good'
            $script:Root = (Resolve-Path -LiteralPath $root).Path

            $guidePath = Join-Path $script:Root 'Source/Server/DEVELOPER.md'
            Set-Content -Path $guidePath -Value 'Handlers are static (ADR-0001).'

            $errors = [System.Collections.Generic.List[string]]::new()
            Test-BannedReferences -Errors $errors | Out-Null

            $errors.Count | Should -Be 0
        }
    }
}
