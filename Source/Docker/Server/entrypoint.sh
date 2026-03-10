#!/bin/bash
# ============================================================================
# POT SERVER CONTAINER ENTRYPOINT SCRIPT
# ============================================================================
# Purpose: Orchestrates container initialization and application startup
# Execution flow:
#   1. Run database migrations (update schema, seed data)
#   2. Start ASP.NET Core web API server
# Why this approach:
#   - Ensures database schema is always current before API starts
#   - Prevents API errors from outdated schema (missing columns, tables, etc.)
#   - Single container handles both migrations and API (simpler deployment)
# When this script runs:
#   - Container startup (docker-compose up, docker start)
#   - Executes as appuser (non-root) due to USER directive in Dockerfile
# Exit behavior:
#   - If migrations fail: Container exits (Docker may restart based on policy)
#   - If API crashes: Container exits (Docker may restart)
# Alternative approaches:
#   - Separate migration job: Run migrations in one-time container before API starts
#   - Init container: Kubernetes-style init container pattern

# Shebang line: #!/bin/bash
#   - Specifies shell interpreter (bash)
#   - Note: Dockerfile uses /bin/sh (Alpine has sh, not bash by default)
#   - This works because /bin/sh in Alpine is actually ash (compatible subset)
#   - For strict POSIX compliance, use #!/bin/sh

# ============================================================================
# ERROR HANDLING CONFIGURATION
# ============================================================================
# Enable strict error handling mode
# Purpose: Fail fast on any errors, preventing cascading failures
# What "set -e" does:
#   - Exit immediately if any command exits with non-zero status
#   - Applies to: migrations fail, server fails, any command in script
#   - Does NOT apply to: Commands in conditionals (if, while), commands with "||"
# Why important:
#   - Without -e: If migrations fail, script continues and starts API anyway
#   - With -e: If migrations fail, script exits and container stops (visible failure)
#   - Makes debugging easier (logs show exactly what failed)
# Example failure scenarios:
#   - Database not reachable: Migration throws connection error, script exits
#   - Invalid migration: Migration fails, container doesn't start
# Result: Container only runs if all commands succeed
set -e

# ============================================================================
# DATABASE MIGRATIONS
# ============================================================================
# Run Entity Framework Core migrations to update database schema
# Purpose: Ensure database structure matches application's data model

# Log migration start with timestamp
# Why logging: Helps debug startup issues and measure migration duration
# $(date): Executes date command and substitutes output
# Example output: "Starting migrations at Mon Mar 10 14:30:22 UTC 2026"
echo "Starting migrations at $(date)"

# Execute migrations using .NET CLI
# Syntax: dotnet <path-to-dll>
#   - dotnet: .NET runtime executable (from mcr.microsoft.com/dotnet/aspnet base image)
#   - ./migrations/Pot.Data.Migrations.dll: Compiled migration tool from build stage
# What Pot.Data.Migrations.dll does:
#   - Connects to database using environment variables (DATABASE__HOST, etc.)
#   - Reads migration history from __EFMigrationsHistory table
#   - Applies any pending migrations in order (creates tables, columns, indexes, etc.)
#   - Updates __EFMigrationsHistory to record applied migrations
#   - Potentially seeds initial data (default users, accounts, etc.)
# Exit behavior:
#   - Success: Returns 0, script continues to start API
#   - Failure: Returns non-zero, set -e causes script to exit, container stops
# Common failure reasons:
#   - Database not reachable (wrong host, port, or database not started yet)
#   - Authentication failed (wrong username/password)
#   - Migration SQL error (constraint violation, syntax error)
# Path explanation:
#   - Relative to WORKDIR (/app) set in Dockerfile
#   - Full path: /app/migrations/Pot.Data.Migrations.dll
dotnet ./migrations/Pot.Data.Migrations.dll

# Log migration completion with timestamp
# Purpose: Measure migration duration (compare with start time above)
# Example output: "Migrations completed at Mon Mar 10 14:30:28 UTC 2026"
# Duration calculation: Manual comparison of timestamps (6 seconds in this example)
echo "Migrations completed at $(date)"

# ============================================================================
# API SERVER STARTUP
# ============================================================================
# Start the ASP.NET Core web API application
# Purpose: Launch main application process to handle HTTP requests

# Log server start with timestamp
# Purpose: Record when API starts (useful for debugging startup delays)
# Example output: "Starting the server at Mon Mar 10 14:30:28 UTC 2026"
echo "Starting the server at $(date)"

# Execute API server using .NET CLI
# Syntax: dotnet <path-to-dll>
#   - dotnet: .NET runtime executable
#   - ./server/Pot.AspNetCore.dll: Compiled ASP.NET Core application from build stage
# What Pot.AspNetCore.dll does:
#   - Starts Kestrel web server (ASP.NET Core's built-in HTTP server)
#   - Loads configuration from appsettings.json and environment variables
#   - Listens on configured URL (ASPNETCORE_URLS=http://+:5241)
#   - Processes HTTP requests (GET, POST, PUT, DELETE to /api/* endpoints)
#   - Continues running until stopped (SIGTERM, SIGINT, crash, etc.)
# Foreground execution:
#   - This command runs in foreground (blocks, doesn't return)
#   - Container stays alive as long as this process runs
#   - When this process exits, container stops
# Exit behavior:
#   - Graceful shutdown: SIGTERM from "docker stop" (10 second grace period)
#   - Immediate shutdown: SIGKILL after grace period expires
#   - Crash: Unhandled exception causes process exit, container stops
# Path explanation:
#   - Relative to WORKDIR (/app)
#   - Full path: /app/server/Pot.AspNetCore.dll
# Result: API server runs and handles requests until container is stopped
dotnet ./server/Pot.AspNetCore.dll

# ============================================================================
# SCRIPT END
# ============================================================================
# Note: Script only reaches this line if API server exits
# Normal operation: API runs indefinitely, script never reaches end
# Exit scenarios:
#   - "docker stop" sends SIGTERM to dotnet process
#   - Unhandled exception in API code
#   - Manual termination (Ctrl+C if running attached)
# Container behavior after script exits:
#   - If exit code 0: Container status "exited (0)"
#   - If exit code non-zero: Container status "exited (1)" or other code
#   - Restart policy (restart: always): Docker automatically restarts container
