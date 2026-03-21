# ============================================================================
# POSTGRES DATABASE CONTAINER
# ============================================================================
# Official PostgreSQL database server image
# Purpose: Provides a containerized PostgreSQL database for the POT application
# Base image: postgres:13
#   - Version 13: Stable PostgreSQL release (consider 14, 15, or 16 for newer features)
#   - Full Debian-based image (~150MB compressed, ~400MB uncompressed)
#   - Alternative: postgres:13-alpine (~80MB compressed) for smaller size
# What's included:
#   - PostgreSQL server (database engine)
#   - psql (command-line client)
#   - pg_dump/pg_restore (backup/restore tools)
#   - Automatic database initialization on first run
# Result: Ready-to-use PostgreSQL database server
FROM postgres:13

# ============================================================================
# ENVIRONMENT VARIABLES - Default Configuration
# ============================================================================
# Configuration for PostgreSQL server initialization
# Important: These are defaults for the Dockerfile only
# In practice, these values are overridden by docker-compose.yml environment section
# Why override in compose:
#   - Keeps credentials out of Dockerfile (better security)
#   - Allows different values per environment (dev, staging, prod)
#   - Can be loaded from .env files (not committed to source control)

# Set default PostgreSQL superuser name
# Purpose: Creates the initial database superuser account
# How it works:
#   - On first container start, PostgreSQL creates this user
#   - User has full privileges (create databases, users, etc.)
# Default value: postgres (standard PostgreSQL default)
# Security: In production, use strong username and override via docker-compose
# Override example in docker-compose: POSTGRES_USER=${POSTGRES_USER}
ENV POSTGRES_USER=postgres

# Set default PostgreSQL superuser password
# Purpose: Sets password for the superuser account
# How it works:
#   - On first container start, PostgreSQL sets this password
#   - Required for authentication (no anonymous access)
# Default value: password (INSECURE - for local development only)
# Security: NEVER use this default in production
#   - Generate strong passwords (e.g., openssl rand -base64 32)
#   - Store in .env file (add .env to .gitignore)
#   - Use secrets management in production (Docker secrets, Azure Key Vault, etc.)
# Override example in docker-compose: POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
ENV POSTGRES_PASSWORD=password

# ============================================================================
# PORT EXPOSURE - Documentation Only
# ============================================================================
# Documents that PostgreSQL listens on port 5432
# Port 5432: Standard PostgreSQL port
# Important: EXPOSE is documentation only, does NOT publish the port
# Actual port publishing is done in docker-compose.yml:
#   - ports: "5444:5432" maps host port 5444 to container port 5432
# Why host port 5444 (not 5432):
#   - Avoids conflicts with PostgreSQL instances already running on host
#   - Allows multiple PostgreSQL containers on same host
# How to connect from host:
#   - Host: localhost, Port: 5444 (external port)
#   - Example: psql -h localhost -p 5444 -U postgres
# How containers connect to each other:
#   - Host: pot-postgres (container name), Port: 5432 (internal port)
#   - Example from server: Host=pot-postgres;Port=5432
# Result: Documentation helps developers understand networking configuration
EXPOSE 5432

# ============================================================================
# DEFAULT BEHAVIOR
# ============================================================================
# No CMD or ENTRYPOINT specified - uses base image defaults
# What happens on container start (from postgres:13 base image):
#   1. Check if /var/lib/postgresql/data is empty (first run check)
#   2. If empty (first run):
#      - Initialize database cluster (pg_initdb)
#      - Create POSTGRES_USER superuser with POSTGRES_PASSWORD
#      - Create default database (name matches POSTGRES_USER)
#      - If POSTGRES_DB is set in docker-compose, create that database too
#   3. Start PostgreSQL server (postgres command)
#   4. Listen on port 5432 for client connections
# Data persistence:
#   - By default, data is stored in /var/lib/postgresql/data
#   - docker-compose.yml mounts this to host directory (./postgres-data)
#   - Result: Database survives container restarts/recreations
# Command to stop gracefully: docker stop <container> (sends SIGTERM, PostgreSQL shuts down cleanly)
