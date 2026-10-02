# PreToolUse hook: blocks edits to paths that must only change when the user explicitly asks.
# Input: hook JSON on stdin. Exit code 2 blocks the tool call and feeds stderr back to Claude.

$payload = [Console]::In.ReadToEnd() | ConvertFrom-Json
$filePath = $payload.tool_input.file_path

if (-not $filePath) {
    exit 0
}

$normalized = $filePath -replace '\\', '/'

$protected = @(
    @{ Pattern = '/Source/Client/pot-react/src/components/ui/'; Reason = 'Third-party UI primitives must not be edited unless explicitly requested.' },
    @{ Pattern = '/Source/Docker/postgres-data/'; Reason = 'Postgres runtime data is protected and must not be modified.' }
)

foreach ($entry in $protected) {
    if ($normalized -like "*$($entry.Pattern)*") {
        [Console]::Error.WriteLine("Blocked: $($entry.Reason) ($filePath). Ask the user for explicit approval first.")
        exit 2
    }
}

exit 0
