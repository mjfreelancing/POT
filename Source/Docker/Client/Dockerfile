# ============================================================================
# BUILD STAGE
# ============================================================================
# Multi-stage build for React/Vite application
# Purpose: Creates a two-stage Docker image to minimize final image size
# How it works:
#   - Stage 1 (build): Uses full Node.js SDK (~180MB base) to compile/build the app
#   - Stage 2 (production): Uses minimal nginx (~40MB base) to serve static files
# Result: Final image is ~40MB instead of ~180MB, with no build tools in production
# Security benefit: No build tools or source code in production image reduces attack surface

# Base image selection:
#   - node:20: Uses Node.js version 20 (LTS - Long Term Support)
#   - alpine3.20: Minimal Linux distribution (~5MB vs ~200MB for standard distros)
#   - AS build: Names this stage "build" so we can reference it later with COPY --from=build
FROM node:20-alpine3.20 AS build

# ============================================================================
# BUILD ARGUMENTS - Runtime Configuration at Build Time
# ============================================================================
# Build arguments for Vite environment variables
# Purpose: Allow configuration of API endpoints during image build (not container runtime)
# How it works: Pass values at build time using docker build --build-arg KEY=value
# Important: These are build-time only; changing them requires rebuilding the image
# Use cases:
#   - Local Docker: Typically omitted, defaults to .env.production (VITE_API_BASE_URL=/api)
#   - Azure deployment: Explicitly set to production API URL (https://api.payontime.com.au/api)

# The base URL for API requests (e.g., /api for local, https://api.domain.com/api for production)
# If not provided, Vite uses value from .env.production file in source code
ARG VITE_API_BASE_URL

# API request timeout in milliseconds (e.g., 30000 = 30 seconds)
# If not provided, Vite uses value from .env.production file in source code
ARG VITE_API_TIMEOUT_MS

# ============================================================================
# WORKING DIRECTORY
# ============================================================================
# Sets the working directory inside the container to /app
# Why: All subsequent commands (COPY, RUN) are relative to this path
# Result: Creates /app directory if it doesn't exist and changes into it
# Example: COPY package.json ./ copies to /app/package.json
WORKDIR /app

# ============================================================================
# DEPENDENCY INSTALLATION - Leveraging Docker Layer Caching
# ============================================================================
# Copy package files separately from source code
# Purpose: Optimize build performance using Docker's layer caching mechanism
# How Docker layer caching works:
#   - Each COPY and RUN instruction creates a separate layer (like a snapshot)
#   - Docker reuses cached layers if the input files haven't changed
#   - Layers are invalidated (rebuilt) if any file in that COPY command changes
#   - All subsequent layers after an invalidated layer must also be rebuilt
# Why this order matters:
#   - package.json and package-lock.json change infrequently (only when dependencies change)
#   - Source code (*.ts, *.tsx files) changes frequently during development
#   - By copying package files first, npm ci only re-runs when dependencies change
# Result: In most builds, npm ci (~2-5 minutes) is skipped using cache, only source rebuild runs (~30 seconds)
# Syntax: COPY ["source", "destination"]
#   - source: Relative to build context (parent directory, set in docker-compose.yml)
#   - destination: Relative to WORKDIR (/app), so ./ means /app/
# Copies both package.json and package-lock.json (the * matches any characters)
COPY Client/pot-react/package*.json ./

# Install dependencies using npm ci (clean install)
# Why npm ci instead of npm install:
#   - npm ci: Uses exact versions from package-lock.json, deletes node_modules first
#   - npm install: May update package-lock.json, uses cached node_modules
# Result: Reproducible builds with exact dependency versions, faster in CI/CD environments
RUN npm ci

# ============================================================================
# SOURCE CODE - Copy After Dependencies
# ============================================================================
# Copy the rest of the application source code
# Why after npm ci: Source code changes frequently, dependencies rarely change
# Result: Most builds skip npm ci (cached) and only rebuild changed source files
# Copies everything from Client/pot-react/ to /app/ (including src/, public/, vite.config.ts, etc.)
COPY Client/pot-react/ .

# ============================================================================
# BUILD CONFIGURATION - Environment Variables
# ============================================================================
# Sets Node.js environment to production mode
# Effects:
#   - Vite applies production optimizations (minification, tree-shaking, dead code elimination)
#   - Disables development features (hot reload, verbose logging)
#   - React uses production build (removes PropTypes checks, warnings)
# Result: Smaller bundle size (~70% reduction) and better runtime performance
ENV NODE_ENV=production

# ============================================================================
# CONDITIONAL API CONFIGURATION
# ============================================================================
# Configure Vite API settings based on build arguments
# How the configuration priority works:
#   1. Default: Vite reads .env.production file from source code (VITE_API_BASE_URL=/api)
#   2. Override: If ARG values provided via --build-arg, export them as ENV variables
# Syntax breakdown: if [ -n "$VAR" ]; then ... fi
#   - [ -n "$VAR" ]: Tests if variable is non-empty (returns true if set)
#   - export VAR: Makes variable available to npm run build process
#   - &&: Chain multiple commands (short-circuit if any fails)
# Use cases:
#   - Local Docker build: No --build-arg provided, uses .env.production (VITE_API_BASE_URL=/api)
#   - Azure build: --build-arg VITE_API_BASE_URL=https://api.payontime.com.au/api overrides default
# Result: Single Dockerfile supports both local and production builds with different API endpoints
# npm run build: Executes vite build command, creates optimized static files in /app/dist
# Output: HTML, CSS, JS bundles with hashed filenames for cache busting (e.g., main-a3f2b1c.js)
RUN if [ -n "$VITE_API_BASE_URL" ]; then \
  export VITE_API_BASE_URL=$VITE_API_BASE_URL; \
  export VITE_API_TIMEOUT_MS=$VITE_API_TIMEOUT_MS; \
  fi && \
  npm run build

# ============================================================================
# PRODUCTION STAGE - Minimal Runtime Image
# ============================================================================
# Fresh stage starts with clean nginx alpine base (~40MB)
# Why nginx:
#   - Industry-standard web server for serving static files (HTML, CSS, JS)
#   - Efficient handling of concurrent connections (event-driven architecture)
#   - Small footprint and fast startup
# Security: nginx:alpine runs as non-root user (nginx user, UID 101) by default
# Result: No Node.js, no build tools, just web server + compiled static files
FROM nginx:alpine

# ============================================================================
# NGINX CONFIGURATION SELECTION
# ============================================================================
# Build argument to select nginx configuration file
# Purpose: Use different nginx configs for different deployment environments
# Syntax: ARG NAME=default_value
#   - If --build-arg NGINX_CONFIG=value provided, uses that value
#   - Otherwise uses default (nginx.conf for local Docker Compose)
# Use cases:
#   - Local Docker: nginx.conf (proxies /api requests to backend container)
#   - Azure deployment: nginx.azure.conf (different backend URL or CORS settings)
ARG NGINX_CONFIG=nginx.conf

# ============================================================================
# COPY COMPILED ARTIFACTS FROM BUILD STAGE
# ============================================================================
# Copy compiled static files from build stage to nginx serving directory
# Syntax: COPY --from=<stage> <source> <destination>
#   - --from=build: References the first stage (named "build")
#   - /app/dist: Build output directory created by vite build
#   - /usr/share/nginx/html: Default nginx directory for static files
# What gets copied: index.html, JS bundles, CSS files, images, fonts
# Result: Only compiled artifacts make it to final image, no source code or build tools
COPY --from=build /app/dist /usr/share/nginx/html

# ============================================================================
# NGINX CONFIGURATION
# ============================================================================
# Copy the appropriate nginx configuration based on build argument
# Purpose: Configure nginx behavior (routing, proxying, CORS headers, etc.)
# Syntax: ${NGINX_CONFIG} expands to value of NGINX_CONFIG build argument
# Path breakdown:
#   - Source: Docker/Client/${NGINX_CONFIG} - relative to build context (parent directory)
#   - Destination: /etc/nginx/conf.d/default.conf - nginx loads this config automatically
# Example configurations:
#   - nginx.conf: Proxies /api/* to backend service (pot-aspnet container)
#   - nginx.azure.conf: Different proxy settings for Azure deployment
# Result: nginx serves static files and proxies API requests to backend
COPY Docker/Client/${NGINX_CONFIG} /etc/nginx/conf.d/default.conf

# ============================================================================
# PORT EXPOSURE - Documentation Only
# ============================================================================
# Documents that the container listens on port 80
# Important: EXPOSE is documentation only, does NOT publish the port
# Actual port publishing is done in docker-compose.yml:
#   - ports: "5175:80" maps host port 5175 to container port 80
# Why port 80: Standard HTTP port, nginx default listening port
# Result: Documentation helps developers understand which port to map
EXPOSE 80

# ============================================================================
# CONTAINER STARTUP COMMAND
# ============================================================================
# Starts nginx in foreground mode when container starts
# Syntax: CMD ["executable", "arg1", "arg2"]
#   - CMD: Default command executed when container starts
#   - nginx: Nginx web server executable
#   - -g "daemon off;": Global directive flag
# Why "daemon off;":
#   - Normally nginx runs as background daemon (returns control to terminal)
#   - Docker requires main process to stay in foreground
#   - If process exits/backgrounds, Docker thinks container is done and stops it
# Result: nginx runs in foreground, keeps container alive, serves HTTP requests on port 80
CMD ["nginx", "-g", "daemon off;"]
