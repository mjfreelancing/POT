# ============================================================================
# BUILD ARGUMENTS - .NET Version Configuration
# ============================================================================
# Specifies the .NET SDK version to use for building and running the application
# Purpose: Centralize version management to easily upgrade .NET versions
# How it works:
#   - Default value: 10.0 (can be overridden with --build-arg DOTNET_VERSION=x.x)
#   - Used in both build and runtime stages to ensure version consistency
# Result: Both stages use the same .NET major version, avoiding runtime compatibility issues
# Example: docker build --build-arg DOTNET_VERSION=11.0 to use .NET 11
ARG DOTNET_VERSION=10.0

# ============================================================================
# BUILD STAGE - Compiling .NET Application
# ============================================================================
# Multi-stage build for ASP.NET Core application
# Purpose: Creates a two-stage Docker image to minimize final image size
# How it works:
#   - Stage 1 (build): Uses full .NET SDK (~200MB base) with build tools
#   - Stage 2 (runtime): Uses minimal ASP.NET runtime (~100MB base) without SDK
# Result: Final image is ~100MB instead of ~200MB, with no build tools in production
# Security benefit: No SDK or build tools in production reduces attack surface

# Base image selection:
#   - mcr.microsoft.com/dotnet/sdk: Official Microsoft .NET SDK image
#   - ${DOTNET_VERSION}: Uses ARG value defined above (10.0)
#   - alpine: Minimal Linux distribution for smaller image (~40MB vs ~200MB for standard distros)
#   - AS build: Names this stage "build" so we can reference it with COPY --from=build
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-alpine AS build

# ============================================================================
# WORKING DIRECTORY
# ============================================================================
# Sets the working directory inside the container to /app
# Why: All subsequent commands (COPY, RUN) execute relative to this path
# Result: Creates /app directory if it doesn't exist and changes into it
WORKDIR /app

# ============================================================================
# SOURCE CODE - Copy Entire Server Folder
# ============================================================================
# Copy the entire Server folder containing all projects and solution file
# Purpose: Copy all .NET projects and shared dependencies needed for building
# What gets copied:
#   - Pot.AspNetCore/ - Main ASP.NET Core web API project
#   - Pot.Data.Migrations/ - Database migration tool project
#   - Pot.App/, Pot.Shared/, etc. - Shared libraries and dependencies
#   - pot.sln - Solution file
#   - Directory.Build.props - Shared MSBuild properties
# Syntax:
#   - Source: Server (relative to build context, parent directory in docker-compose.yml)
#   - Destination: ./Server (relative to WORKDIR, becomes /app/Server)
COPY Server ./Server

# ============================================================================
# DEPENDENCY RESTORATION
# ============================================================================
# Navigate to the Server folder containing the solution and projects
# Why change directory: dotnet commands work better from the solution directory
# Result: Subsequent dotnet commands can reference projects by relative paths
WORKDIR /app/Server

# Restore NuGet package dependencies for runtime projects
# Purpose: Restore only the runtime project graph needed for server and migrations outputs
# How it works:
#   - Restores Pot.AspNetCore and all transitive project/package dependencies
#   - Restores Pot.Data.Migrations and all transitive project/package dependencies
#   - Avoids solution-wide traversal of unrelated projects (for example test projects)
# Result: Runtime publish commands can execute with --no-restore
RUN dotnet restore Pot.AspNetCore/Pot.AspNetCore.csproj && \
    dotnet restore Pot.Data.Migrations/Pot.Data.Migrations.csproj

# ============================================================================
# PUBLISH - Main API Application
# ============================================================================
# Publish the ASP.NET Core web API project
# Purpose: Create a self-contained deployment package ready to run
# Syntax breakdown:
#   - dotnet publish: Creates deployment-ready output with all dependencies
#   - Pot.AspNetCore/Pot.AspNetCore.csproj: Specific project to publish
#   - -c Release: Use Release configuration (matches build above)
#   - -o /app/server: Output directory for published files
#   - -p:UseAppHost=false: Don't create executable, use dll invoked by dotnet runtime
# Why UseAppHost=false:
#   - UseAppHost=true: Creates native executable (e.g., Pot.AspNetCore.exe on Windows)
#   - UseAppHost=false: Creates only DLL, invoked with 'dotnet Pot.AspNetCore.dll'
#   - Containers use runtime image, DLL-only approach is standard and smaller
# Result: Complete application in /app/server/ ready to run with 'dotnet Pot.AspNetCore.dll'
RUN dotnet publish Pot.AspNetCore/Pot.AspNetCore.csproj -c Release --no-restore -o /app/server -p:UseAppHost=false

# ============================================================================
# PUBLISH - Database Migrations Tool
# ============================================================================
# Publish the EF Core migrations console application
# Purpose: Create deployment package for database migration tool
# Why separate publish: Migrations run before API starts (in entrypoint.sh)
# Output location: /app/migrations/ (different from API at /app/server/)
# Result: Migration tool in /app/migrations/ ready to run with 'dotnet Pot.Data.Migrations.dll'
RUN dotnet publish Pot.Data.Migrations/Pot.Data.Migrations.csproj -c Release --no-restore -o /app/migrations -p:UseAppHost=false

# ============================================================================
# RUNTIME STAGE - Minimal Production Image
# ============================================================================
# Fresh stage starts with clean ASP.NET runtime base (~100MB)
# Why aspnet (not sdk):
#   - aspnet: Runtime only (CoreCLR + ASP.NET libraries), no build tools
#   - sdk: Full SDK with compilers, NuGet, MSBuild (~200MB larger)
# Result: Smaller image, faster startup, reduced attack surface (no build tools)
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-alpine

# Sets working directory to /app (consistent with build stage)
WORKDIR /app

# ============================================================================
# RUNTIME DEPENDENCIES
# ============================================================================
# Install required packages for application runtime
# apk: Alpine Linux package manager
# --no-cache: Don't cache package index locally (saves ~1MB image size)
# Packages installed:
#   - postgresql-client: Provides pg_isready command for database health checks
#     * Used in entrypoint.sh to wait for database before running migrations
#     * Included with psql, pg_dump for potential database operations
#   - tzdata: Timezone database for accurate date/time operations
#     * Enables TimeZoneInfo.FindSystemTimeZoneById() in .NET
#     * Required for displaying times in user's local timezone
#     * Without it, only UTC is available
# Result: Container can check database health and handle timezone conversions
RUN apk add --no-cache postgresql-client tzdata

# ============================================================================
# SECURITY - Non-Root User Configuration
# ============================================================================
# Create non-root user and group to run the application
# Purpose: Security best practice to limit container privileges
# Why run as non-root:
#   - Principle of least privilege: Application doesn't need root access
#   - Defense in depth: If container is compromised, attacker has limited privileges
#   - Cannot install packages, modify system files, or bind to privileged ports (<1024)
#   - Reduces impact of container escape vulnerabilities
# Syntax breakdown:
#   - addgroup -g 1000 appgroup:
#     * Creates group named "appgroup" with GID (Group ID) 1000
#     * GID 1000 is convention for first non-system user group
#   - adduser -u 1000 -G appgroup -s /bin/sh -D appuser:
#     * -u 1000: UID (User ID) 1000, matches GID for simplicity
#     * -G appgroup: Add user to appgroup
#     * -s /bin/sh: Set default shell to /bin/sh (Alpine's default)
#     * -D: Don't assign password (not needed in container)
#     * appuser: Username
# Comparison with Client container:
#   - Client: nginx:alpine runs as nginx user (UID 101) by default, no config needed
#   - Server: aspnet:alpine runs as root by default, explicit user creation required
# Result: Application processes run as UID 1000 (appuser), not root (UID 0)
RUN addgroup -g 1000 appgroup && \
    adduser -u 1000 -G appgroup -s /bin/sh -D appuser

# ============================================================================
# COPY PUBLISHED ARTIFACTS FROM BUILD STAGE
# ============================================================================
# Copy compiled application from build stage to runtime image
# Syntax: COPY --from=<stage> <source> <destination>
#   - --from=build: References the first stage (named "build")
#   - Source paths: Build output directories from publish steps
#   - Destination: Current WORKDIR (/app)
# What gets copied:
#   - /app/server/: Main API application DLLs, dependencies, appsettings.json
#   - /app/migrations/: Migration tool DLLs and dependencies
# Result: Only compiled artifacts in runtime image, no source code or build tools
COPY --from=build /app/server ./server
COPY --from=build /app/migrations ./migrations

# ============================================================================
# STARTUP SCRIPT
# ============================================================================
# Copy the startup script that orchestrates container initialization
# Purpose: Shell script that runs migrations before starting API
# Source: Docker/Server/entrypoint.sh (relative to build context)
# Destination: ./entrypoint.sh (relative to WORKDIR, becomes /app/entrypoint.sh)
# What entrypoint.sh does:
#   - Runs database migrations (dotnet ./migrations/Pot.Data.Migrations.dll)
#   - Starts the API server (dotnet ./server/Pot.AspNetCore.dll)
COPY Docker/Server/entrypoint.sh .

# ============================================================================
# WINDOWS LINE ENDING COMPATIBILITY
# ============================================================================
# Fix Windows CRLF line endings and set permissions
# Purpose: Ensure shell script runs correctly even if edited on Windows
# Why this is needed:
#   - Windows: Text files use CRLF (Carriage Return + Line Feed: \r\n)
#   - Linux: Text files use LF (Line Feed only: \n)
#   - Shell scripts with CRLF fail with "/bin/sh^M: not found" error (^M is \r)
# What each command does:
#   - apk add --no-cache dos2unix: Installs line ending conversion tool
#   - dos2unix entrypoint.sh: Converts CRLF (\r\n) to LF (\n)
#   - chmod +x entrypoint.sh: Makes file executable (adds execute permission bit)
#   - chown -R appuser:appgroup /app: Changes ownership of /app and all contents to appuser
# Why chown is needed:
#   - Files copied by root (during build) are owned by root
#   - appuser needs write access for logs, temp files, etc.
#   - -R flag applies recursively to all files/subdirectories in /app
# Result: Shell script works on both Windows and Linux, appuser owns all application files
RUN apk add --no-cache dos2unix && \
    dos2unix entrypoint.sh && \
    chmod +x entrypoint.sh && \
    chown -R appuser:appgroup /app

# ============================================================================
# SWITCH TO NON-ROOT USER
# ============================================================================
# Switch from root to appuser for all subsequent commands and runtime
# Purpose: Application runs with limited privileges for security
# Important: This affects:
#   - Remaining Dockerfile instructions (if any)
#   - Container runtime (when container starts)
# Result: The entrypoint and application process run as appuser (UID 1000), not root
USER appuser

# ============================================================================
# PORT EXPOSURE - Documentation Only
# ============================================================================
# Documents that the container listens on port 5241
# Important: EXPOSE is documentation only, does NOT publish the port
# Actual port publishing is done in docker-compose.yml:
#   - ports: "5241:5241" maps host port 5241 to container port 5241
# Why port 5241: Configured in environment variable ASPNETCORE_URLS=http://+:5241
# Result: Documentation helps developers understand which port to map
EXPOSE 5241

# ============================================================================
# CONTAINER STARTUP COMMAND
# ============================================================================
# Specifies the command to run when container starts
# Syntax: ENTRYPOINT ["/bin/sh", "script"]
#   - ENTRYPOINT: Command always executed, can't be easily overridden (unlike CMD)
#   - /bin/sh: Shell interpreter (Alpine uses /bin/sh instead of /bin/bash)
#   - ./entrypoint.sh: Startup script that runs migrations then starts API
# Why ENTRYPOINT instead of CMD:
#   - ENTRYPOINT: Designed for always-run commands, container's main purpose
#   - CMD: Designed for default commands that can be overridden
#   - For application containers, ENTRYPOINT is more appropriate
# What happens at startup:
#   1. Container starts as appuser (due to USER directive above)
#   2. /bin/sh executes entrypoint.sh
#   3. entrypoint.sh runs migrations
#   4. entrypoint.sh starts API server
#   5. Container stays alive as long as API process runs
# Result: Migrations always run before API starts, ensuring database schema is current
ENTRYPOINT ["/bin/sh", "./entrypoint.sh"]
