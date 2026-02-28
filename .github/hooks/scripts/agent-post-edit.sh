#!/usr/bin/env bash
# Session-end maintenance hook for agent edits.
#
# Purpose:
# - Detect changed files in this repository.
# - Run lightweight cleanup tools only for relevant file types.
# - Keep this hook fail-soft so it does not block normal workflows.

set +e

echo "[agent-hook] sessionEnd maintenance starting..."

# Build a list of changed files by combining:
# - tracked file changes (git diff)
# - untracked new files (git ls-files --others)
tracked=$(git diff --name-only HEAD 2>/dev/null)
untracked=$(git ls-files --others --exclude-standard 2>/dev/null)
changed=$(printf "%s\n%s\n" "$tracked" "$untracked" | sed '/^$/d' | sort -u)

# If nothing changed, exit early.
if [ -z "$changed" ]; then
  echo "[agent-hook] no changed files detected; skipping."
  exit 0
fi

# Split changed files into server C# and client TypeScript groups.
server_cs_files=$(echo "$changed" | grep -E '^Source/Server/.+\.cs$')
client_ts_files=$(echo "$changed" | grep -E '^Source/Client/pot-react/src/.+\.(ts|tsx)$')

# For changed server C# files, run dotnet format with include paths so only
# touched files are processed.
if [ -n "$server_cs_files" ]; then
  echo "[agent-hook] formatting C# (sort/remove unused usings where applicable)..."
  dotnet format "Source/Server/pot.sln" --verbosity minimal --include $server_cs_files || echo "[agent-hook] dotnet format failed"
else
  echo "[agent-hook] no changed C# files under Source/Server."
fi

# For changed client TS/TSX files, run lint sorting first, then targeted
# Prettier formatting from the client project directory.
if [ -n "$client_ts_files" ]; then
  echo "[agent-hook] running TypeScript lint sort + targeted prettier..."
  (
    cd "Source/Client/pot-react" || exit 0
    npm run lint:sort || echo "[agent-hook] npm run lint:sort failed"

    # Prettier is run using paths relative to Source/Client/pot-react.
    relative_client_files=$(echo "$client_ts_files" | sed 's#^Source/Client/pot-react/##')
    if [ -n "$relative_client_files" ]; then
      npm exec prettier -- --write $relative_client_files || echo "[agent-hook] prettier failed"
    fi
  )
else
  echo "[agent-hook] no changed TS/TSX files under Source/Client/pot-react/src."
fi

# Always exit success to keep this hook fail-soft.
echo "[agent-hook] sessionEnd maintenance complete."
exit 0
