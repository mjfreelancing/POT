#Requires -Version 7.0
<#
.SYNOPSIS
    Validates the documentation set and regenerates the ADR index.

.DESCRIPTION
    Checks, and fails on:
      - an ADR whose front matter is missing, unsupported or invalid;
      - a generated ADR index that is out of date;
      - a broken relative link in a document the documentation set owns;
      - an instruction file citing an ADR that does not exist or is not accepted;
      - a PRD or ADR identifier written into code under Source/.

    Front matter is limited to flat key: value pairs and simple lists. This script parses it itself
    rather than depending on a YAML module, and rejects anything else.

.PARAMETER Root
    Repository root. Defaults to three levels above this script.

.PARAMETER Update
    Rewrites the generated index in Docs/adr/README.md instead of failing when it is out of date.

.EXAMPLE
    pwsh ./.github/scripts/docs-check/docs-check.ps1

.EXAMPLE
    pwsh ./.github/scripts/docs-check/docs-check.ps1 -Update
#>
[CmdletBinding()]
param(
    [string] $Root,
    [switch] $Update,
    [switch] $ReportFacts
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:AdrStatuses = @('proposed', 'accepted', 'rejected', 'deprecated', 'superseded')
$script:AdrAreas = @('process', 'cross-cutting', 'backend', 'frontend', 'infrastructure')
$script:RequiredAdrFields = @('id', 'title', 'status', 'date', 'area')

$script:IndexStartMarker = '<!-- BEGIN GENERATED INDEX -->'
$script:IndexEndMarker = '<!-- END GENERATED INDEX -->'

$script:LinkScopeDirectories = @('Docs/adr', 'Docs/standards', 'Docs/prd', 'Docs/reference', 'Docs/explanation', 'Docs/tutorials', 'Docs/how-to', 'Docs/user-guide', '.github')
$script:LinkScopeFiles = @(
    'Docs/README.md',
    'AGENTS.md',
    'README.md',
    'DOCS-REORGANISATION-PLAN.md',
    'DECISIONS-TO-DISCUSS.md'
)

# Code and configuration only: a developer guide under Source/ may cite an ADR, code and tests may not.
$script:BannedReferenceExtensions = @(
    '.cs', '.ts', '.tsx', '.js', '.jsx', '.razor', '.cshtml',
    '.csproj', '.props', '.targets', '.json', '.ps1', '.sh', '.yml', '.yaml', '.http'
)

# Facts a reader should be able to find in one place. The report is informational: the goal is one
# authoritative copy, and the count shows where a fact is currently repeated.
$script:ReferenceFacts = @(
    @{ Name = 'client port 5175'; Pattern = '5175' },
    @{ Name = 'API port 5241'; Pattern = '5241' },
    @{ Name = 'dev API port 5242'; Pattern = '5242' },
    @{ Name = 'Postgres port 5444'; Pattern = '5444' },
    @{ Name = 'liveness endpoint'; Pattern = '/_health' },
    @{ Name = 'Admin role'; Pattern = '\bAdmin\b' },
    @{ Name = 'Viewer role'; Pattern = '\bViewer\b' },
    @{ Name = 'environment variable VITE_API_BASE_URL'; Pattern = 'VITE_API_BASE_URL' }
)

function Join-RepoPath {
    param([string] $Root, [string] $RelativePath)

    return (Join-Path $Root ($RelativePath -replace '/', [IO.Path]::DirectorySeparatorChar))
}

function Get-RelativeDisplayPath {
    param([string] $Root, [string] $Path)

    $rootWithSeparator = $Root.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    if ($Path.StartsWith($rootWithSeparator, [StringComparison]::OrdinalIgnoreCase)) {
        return $Path.Substring($rootWithSeparator.Length)
    }

    return $Path
}

function Remove-ScalarComment {
    param([string] $Value)

    # A comment is a '#' separated from the value by at least two spaces, so a '#' inside a value survives.
    return ($Value -replace '\s{2,}#.*$', '').Trim()
}

function ConvertTo-ListItems {
    param([string] $Value)

    $inner = $Value.Trim()
    if ($inner.StartsWith('[') -and $inner.EndsWith(']')) {
        $inner = $inner.Substring(1, $inner.Length - 2)
    }

    $items = @()
    foreach ($part in ($inner -split ',')) {
        $item = $part.Trim().Trim('"').Trim("'")
        if ($item) {
            $items += $item
        }
    }

    return $items
}

function Get-AdrFrontMatter {
    param([string] $Path, $Errors)

    $displayPath = Get-RelativeDisplayPath -Root $script:Root -Path $Path
    $lines = @([IO.File]::ReadAllLines($Path))
    $values = @{}

    if ($lines.Count -lt 3 -or $lines[0].Trim() -ne '---') {
        $Errors.Add("${displayPath}: no front matter block; the file must start with '---'")
        return $values
    }

    $endIndex = -1
    for ($index = 1; $index -lt $lines.Count; $index++) {
        if ($lines[$index].Trim() -eq '---') {
            $endIndex = $index
            break
        }
    }

    if ($endIndex -lt 0) {
        $Errors.Add("${displayPath}: the front matter block is not closed with '---'")
        return $values
    }

    $listKey = $null

    for ($index = 1; $index -lt $endIndex; $index++) {
        $line = $lines[$index]
        $lineNumber = $index + 1

        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        if ($line -match '^\s*-\s*(?<item>.*)$') {
            if (-not $listKey) {
                $Errors.Add("${displayPath}:${lineNumber}: a list item appears without a key")
                continue
            }

            $item = Remove-ScalarComment -Value $Matches['item']
            if ($item) {
                $values[$listKey] += $item
            }

            continue
        }

        if ($line -match '^(?<key>[A-Za-z][A-Za-z0-9_-]*):[ \t]*(?<value>.*)$') {
            $key = $Matches['key']
            $rawValue = $Matches['value']
            $listKey = $null

            if ($values.ContainsKey($key)) {
                $Errors.Add("${displayPath}:${lineNumber}: the key '${key}' is declared twice")
                continue
            }

            if ([string]::IsNullOrWhiteSpace($rawValue)) {
                $values[$key] = @()
                $listKey = $key
                continue
            }

            $value = Remove-ScalarComment -Value $rawValue
            if ($value.StartsWith('[')) {
                # Coerce to an array: a function returning an empty array emits nothing, which would store $null.
                $values[$key] = @(ConvertTo-ListItems -Value $value)
            }
            else {
                $values[$key] = $value
            }

            continue
        }

        $Errors.Add("${displayPath}:${lineNumber}: unsupported front matter; only 'key: value', an inline list '[a, b]' or '- item' lines are allowed")
    }

    return $values
}

function Get-AdrRecord {
    param([string] $Path, $Errors)

    $displayPath = Get-RelativeDisplayPath -Root $script:Root -Path $Path
    $fileName = [IO.Path]::GetFileName($Path)
    $values = Get-AdrFrontMatter -Path $Path -Errors $Errors

    foreach ($field in $script:RequiredAdrFields) {
        if (-not $values.ContainsKey($field)) {
            $Errors.Add("${displayPath}: the required field '${field}' is missing")
        }
    }

    if ($fileName -notmatch '^ADR-(?<number>\d{4})-(?<slug>[a-z0-9-]+)\.md$') {
        $Errors.Add("${displayPath}: the file name must be ADR-NNNN-kebab-case-title.md")
        return $null
    }

    $number = $Matches['number']
    $expectedId = "ADR-${number}"

    if ($values.ContainsKey('id') -and $values['id'] -ne $expectedId) {
        $Errors.Add("${displayPath}: id '$($values['id'])' does not match the file name; expected '${expectedId}'")
    }

    if ($values.ContainsKey('status') -and $script:AdrStatuses -notcontains $values['status']) {
        $Errors.Add("${displayPath}: status '$($values['status'])' is not one of $($script:AdrStatuses -join ', ')")
    }

    if ($values.ContainsKey('area') -and $script:AdrAreas -notcontains $values['area']) {
        $Errors.Add("${displayPath}: area '$($values['area'])' is not one of $($script:AdrAreas -join ', ')")
    }

    if ($values.ContainsKey('date') -and $values['date'] -notmatch '^\d{4}-\d{2}-\d{2}$') {
        $Errors.Add("${displayPath}: date '$($values['date'])' is not yyyy-mm-dd")
    }

    $relatedPrds = @()
    if ($values.ContainsKey('related-prds')) {
        $relatedPrds = @($values['related-prds'] | Where-Object { $_ })
    }

    return [pscustomobject]@{
        Id          = if ($values.ContainsKey('id')) { $values['id'] } else { $expectedId }
        Number      = $number
        Title       = if ($values.ContainsKey('title')) { $values['title'] } else { '' }
        Status      = if ($values.ContainsKey('status')) { $values['status'] } else { '' }
        Area        = if ($values.ContainsKey('area')) { $values['area'] } else { '' }
        RelatedPrds = $relatedPrds
        FileName    = $fileName
        Path        = $Path
        DisplayPath = $displayPath
    }
}

function Get-AdrRecords {
    param($Errors)

    $adrDirectory = Join-RepoPath -Root $script:Root -RelativePath 'Docs/adr'
    $records = @()

    if (-not (Test-Path $adrDirectory)) {
        return $records
    }

    # Every Markdown file here is an ADR except the guide, so a misnamed file is reported rather than
    # silently skipped. The template lives with the `adr` skill.
    $files = Get-ChildItem -Path $adrDirectory -Filter '*.md' -File |
    Where-Object { $_.Name -ne 'README.md' } |
    Sort-Object Name

    foreach ($file in $files) {
        $record = Get-AdrRecord -Path $file.FullName -Errors $Errors
        if ($record) {
            $records += $record
        }
    }

    return $records
}

function Format-MarkdownCell {
    param([string] $Value)

    return ($Value -replace '\|', '\|')
}

function Format-AdrIndex {
    param($Records, [string] $Eol)

    # An empty series arrives as $null, which StrictMode would trip over below.
    $Records = @($Records | Where-Object { $null -ne $_ })

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add('_Generated from the ADR front matter by `.github/scripts/docs-check/docs-check.ps1`. Do not edit by hand._')
    $lines.Add('')

    if ($Records.Count -eq 0) {
        $lines.Add('No ADR has been written yet. The first record takes `ADR-0001`.')
        $lines.Add('')
        return ($lines -join $Eol)
    }

    $lines.Add('## By area')
    $lines.Add('')

    foreach ($area in $script:AdrAreas) {
        $areaRecords = @($Records | Where-Object { $_.Area -eq $area } | Sort-Object Number)
        if ($areaRecords.Count -eq 0) {
            continue
        }

        $areaLabel = $area.Substring(0, 1).ToUpperInvariant() + $area.Substring(1)
        $lines.Add("### ${areaLabel}")
        $lines.Add('')
        $lines.Add('| ADR | Title | Status |')
        $lines.Add('| --- | --- | --- |')

        foreach ($record in $areaRecords) {
            $title = Format-MarkdownCell -Value $record.Title
            $lines.Add("| [$($record.Id)]($($record.FileName)) | ${title} | $($record.Status) |")
        }

        $lines.Add('')
    }

    $lines.Add('## By status')
    $lines.Add('')

    foreach ($status in $script:AdrStatuses) {
        $statusRecords = @($Records | Where-Object { $_.Status -eq $status } | Sort-Object Number)
        if ($statusRecords.Count -eq 0) {
            continue
        }

        $statusLabel = $status.Substring(0, 1).ToUpperInvariant() + $status.Substring(1)
        $lines.Add("### ${statusLabel} ($($statusRecords.Count))")
        $lines.Add('')

        foreach ($record in $statusRecords) {
            $lines.Add("- [$($record.Id)]($($record.FileName)) — $($record.Title)")
        }

        $lines.Add('')
    }

    $prdRecords = @($Records | Where-Object { $_.RelatedPrds.Count -gt 0 })
    $prdIds = @($prdRecords | ForEach-Object { $_.RelatedPrds } | Sort-Object -Unique)

    $lines.Add('## By PRD')
    $lines.Add('')

    if ($prdIds.Count -eq 0) {
        $lines.Add('No ADR names a related PRD yet.')
        $lines.Add('')
    }
    else {
        $lines.Add('| PRD | ADRs |')
        $lines.Add('| --- | --- |')

        foreach ($prdId in $prdIds) {
            $links = @(
                $prdRecords |
                Where-Object { $_.RelatedPrds -contains $prdId } |
                Sort-Object Number |
                ForEach-Object { "[$($_.Id)]($($_.FileName))" }
            )
            $lines.Add("| ${prdId} | $($links -join ', ') |")
        }

        $lines.Add('')
    }

    return ($lines -join $Eol)
}

function Get-PreferredLineEnding {
    param([string] $Content)

    $crlfCount = ([regex]::Matches($Content, "`r`n")).Count
    $lfCount = ([regex]::Matches($Content, "(?<!`r)`n")).Count

    if ($lfCount -gt $crlfCount) {
        return "`n"
    }

    return "`r`n"
}

function Test-AdrIndex {
    param($Records, [switch] $Write, $Errors)

    $readmePath = Join-RepoPath -Root $script:Root -RelativePath 'Docs/adr/README.md'
    if (-not (Test-Path $readmePath)) {
        $Errors.Add('Docs/adr/README.md is missing; it holds the hand-written guide and the generated index')
        return
    }

    $content = [IO.File]::ReadAllText($readmePath)
    $startIndex = $content.IndexOf($script:IndexStartMarker, [StringComparison]::Ordinal)
    $endIndex = $content.IndexOf($script:IndexEndMarker, [StringComparison]::Ordinal)

    if ($startIndex -lt 0 -or $endIndex -lt 0 -or $endIndex -lt $startIndex) {
        $Errors.Add("Docs/adr/README.md: the generated index markers '$($script:IndexStartMarker)' and '$($script:IndexEndMarker)' are missing or out of order")
        return
    }

    $eol = Get-PreferredLineEnding -Content $content
    $generated = Format-AdrIndex -Records $Records -Eol $eol

    $before = $content.Substring(0, $startIndex + $script:IndexStartMarker.Length)
    $after = $content.Substring($endIndex)
    $expected = $before + $eol + $generated + $eol + $after

    $current = $content.Substring($startIndex, $endIndex + $script:IndexEndMarker.Length - $startIndex)
    $wanted = $script:IndexStartMarker + $eol + $generated + $eol + $script:IndexEndMarker

    if ($current -eq $wanted) {
        return
    }

    if (-not $Write) {
        $Errors.Add('Docs/adr/README.md: the generated index is out of date; run docs-check -Update')
        return
    }

    [IO.File]::WriteAllText($readmePath, $expected)
    Write-Host 'Updated the generated index in Docs/adr/README.md'
}

function Get-MarkdownFilesInScope {
    $files = [System.Collections.Generic.List[string]]::new()

    foreach ($directory in $script:LinkScopeDirectories) {
        $fullPath = Join-RepoPath -Root $script:Root -RelativePath $directory
        if (-not (Test-Path $fullPath)) {
            continue
        }

        foreach ($file in Get-ChildItem -Path $fullPath -Filter '*.md' -File -Recurse) {
            # A fixtures folder holds deliberately broken documents for the tests.
            if ($file.FullName -match '[\\/]fixtures[\\/]') {
                continue
            }

            $files.Add($file.FullName)
        }
    }

    foreach ($relativePath in $script:LinkScopeFiles) {
        $fullPath = Join-RepoPath -Root $script:Root -RelativePath $relativePath
        if (Test-Path $fullPath) {
            $files.Add($fullPath)
        }
    }

    return @($files | Sort-Object -Unique)
}

function Get-MarkdownLinkTargets {
    param([string] $Path)

    $targets = @()
    $inFence = $false
    $fenceMarker = ''

    foreach ($line in [IO.File]::ReadAllLines($Path)) {
        $trimmed = $line.TrimStart()

        if ($trimmed.StartsWith('```') -or $trimmed.StartsWith('~~~')) {
            $marker = $trimmed.Substring(0, 3)
            if (-not $inFence) {
                $inFence = $true
                $fenceMarker = $marker
            }
            elseif ($marker -eq $fenceMarker) {
                $inFence = $false
                $fenceMarker = ''
            }

            continue
        }

        if ($inFence) {
            continue
        }

        foreach ($match in [regex]::Matches($line, '\[[^\]]*\]\((?<target>[^)\s]+)(?:\s+"[^"]*")?\)')) {
            $targets += $match.Groups['target'].Value
        }
    }

    return $targets
}

function Test-MarkdownLinks {
    param($Errors)

    $checked = 0

    foreach ($file in Get-MarkdownFilesInScope) {
        $displayPath = Get-RelativeDisplayPath -Root $script:Root -Path $file

        foreach ($target in (Get-MarkdownLinkTargets -Path $file)) {
            if ([string]::IsNullOrWhiteSpace($target)) {
                continue
            }

            $candidate = $target.Trim()
            if ($candidate.StartsWith('<') -and $candidate.EndsWith('>')) {
                $candidate = $candidate.Substring(1, $candidate.Length - 2)
            }

            if ($candidate.StartsWith('#')) {
                continue
            }

            if ($candidate -match '^[A-Za-z][A-Za-z0-9+.-]*:') {
                continue
            }

            $pathPart = ($candidate -split '#')[0]
            $pathPart = ($pathPart -split '\?')[0]
            $pathPart = [Uri]::UnescapeDataString($pathPart)

            if ([string]::IsNullOrWhiteSpace($pathPart)) {
                continue
            }

            $checked++
            $resolved = Join-Path (Split-Path -Parent $file) ($pathPart -replace '/', [IO.Path]::DirectorySeparatorChar)
            if (-not (Test-Path -LiteralPath $resolved)) {
                $Errors.Add("${displayPath}: broken relative link '${candidate}'")
            }
        }
    }

    return $checked
}

function Test-InstructionCitations {
    param($Records, $Errors)

    $instructionDirectory = Join-RepoPath -Root $script:Root -RelativePath '.github/instructions'
    if (-not (Test-Path $instructionDirectory)) {
        return 0
    }

    $checked = 0
    $byId = @{}
    foreach ($record in $Records) {
        $byId[$record.Id] = $record
    }

    foreach ($file in Get-ChildItem -Path $instructionDirectory -Filter '*.md' -File) {
        foreach ($match in [regex]::Matches([IO.File]::ReadAllText($file.FullName), 'ADR-(?<number>\d{4})')) {
            $checked++
            $id = "ADR-$($match.Groups['number'].Value)"
            $displayPath = Get-RelativeDisplayPath -Root $script:Root -Path $file.FullName

            if (-not $byId.ContainsKey($id)) {
                $Errors.Add("${displayPath}: cites ${id}, which does not exist")
                continue
            }

            $status = $byId[$id].Status
            if ($status -ne 'accepted') {
                $Errors.Add("${displayPath}: cites ${id}, which is '${status}' rather than 'accepted'")
            }
        }
    }

    return $checked
}

function Test-BannedReferences {
    param($Errors)

    $sourceDirectory = Join-RepoPath -Root $script:Root -RelativePath 'Source'
    if (-not (Test-Path $sourceDirectory)) {
        return 0
    }

    $checked = 0
    $files = Get-ChildItem -Path $sourceDirectory -File -Recurse |
    Where-Object {
        $script:BannedReferenceExtensions -contains $_.Extension.ToLowerInvariant() -and
        $_.FullName -notmatch '[\\/](bin|obj|node_modules|dist)[\\/]'
    }

    foreach ($file in $files) {
        $checked++
        $displayPath = Get-RelativeDisplayPath -Root $script:Root -Path $file.FullName
        $lineNumber = 0

        foreach ($line in [IO.File]::ReadLines($file.FullName)) {
            $lineNumber++
            if ($line -match '\b(PRD|ADR)-\d{3,4}\b') {
                $Errors.Add("${displayPath}:${lineNumber}: a document identifier is not allowed in code")
            }
        }
    }

    return $checked
}

function Get-ReferenceFactReport {
    param([string] $Root)

    $files = @()
    foreach ($directory in @('Docs', '.github')) {
        $fullPath = Join-RepoPath -Root $Root -RelativePath $directory
        if (Test-Path $fullPath) {
            $files += Get-ChildItem -Path $fullPath -Filter '*.md' -File -Recurse |
            Where-Object { $_.FullName -notmatch '[\\/]fixtures[\\/]' }
        }
    }

    $report = [System.Collections.Generic.List[string]]::new()
    $report.Add('Reference facts (each should have one authoritative copy):')

    foreach ($fact in $script:ReferenceFacts) {
        $factFiles = @()
        foreach ($file in $files) {
            if ([IO.File]::ReadAllText($file.FullName) -match $fact.Pattern) {
                $factFiles += Get-RelativeDisplayPath -Root $Root -Path $file.FullName
            }
        }

        $label = if ($factFiles.Count -gt 1) { 'repeated' } else { 'one home' }
        $report.Add(('  {0,-40} {1,3} file(s)  {2}' -f $fact.Name, $factFiles.Count, $label))
    }

    return $report
}

function Invoke-DocsCheck {
    param([string] $RepositoryRoot, [switch] $UpdateIndex, [switch] $ReportFacts)

    $script:Root = (Resolve-Path -LiteralPath $RepositoryRoot).Path
    $errors = [System.Collections.Generic.List[string]]::new()

    $records = @(Get-AdrRecords -Errors $errors)
    Test-AdrIndex -Records $records -Write:$UpdateIndex -Errors $errors

    $linkCount = Test-MarkdownLinks -Errors $errors
    $citationCount = Test-InstructionCitations -Records $records -Errors $errors
    $fileCount = Test-BannedReferences -Errors $errors

    if ($errors.Count -gt 0) {
        foreach ($message in $errors) {
            Write-Host "ERROR: ${message}" -ForegroundColor Red
        }

        Write-Host "docs-check: $($errors.Count) error(s)" -ForegroundColor Red
        return 1
    }

    Write-Host "docs-check: OK ($($records.Count) ADRs, ${linkCount} links, ${citationCount} ADR citations in instruction files, ${fileCount} source files scanned)"

    if ($ReportFacts) {
        foreach ($line in (Get-ReferenceFactReport -Root $script:Root)) {
            Write-Host $line
        }
    }

    return 0
}

if ($MyInvocation.InvocationName -ne '.') {
    $repositoryRoot = if ($Root) { $Root } else { Join-Path $PSScriptRoot '..\..\..' }
    exit (Invoke-DocsCheck -RepositoryRoot $repositoryRoot -UpdateIndex:$Update -ReportFacts:$ReportFacts)
}
