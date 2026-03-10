# Docker Documentation Prompt

Use this prompt with an AI agent to document Docker-related files in a comprehensive, beginner-friendly manner.

---

## Prompt for AI Agent

Review and enhance the comments in my Docker-related files (Dockerfile and docker-compose.yml) to make them detailed and accessible for developers who may not be familiar with Docker.

### Documentation Style Requirements

#### 1. Comprehensive Explanations
Each section should explain:
- What it does
- Why it's needed
- What the result/outcome is
- Examples of practical usage where helpful

#### 2. Syntax Breakdowns
For complex configurations, break down the syntax:
- Show the general pattern (e.g., `"${VARIABLE:-default}:port"`)
- Explain each component
- Show what happens with different values

#### 3. Docker Concepts
When using Docker features, explain the underlying concept:
- Layer caching and why file order matters
- Multi-stage builds and their benefits
- The difference between build-time and runtime settings
- Security considerations (non-root users, minimal images)

#### 4. Beginner-Friendly Language
- Don't assume Docker knowledge
- Explain terminology (containers, images, stages, contexts, etc.)
- Include size comparisons (e.g., "Alpine: ~40MB vs standard: ~200MB")
- Clarify common confusions (e.g., EXPOSE is documentation only, localhost in healthchecks refers to container's localhost)

#### 5. Grouping and Organization
- Group related configuration items with section headers
- Add context for environment variable groups (logging, authentication, encryption, etc.)
- Use visual separators for major sections

#### 6. Practical Notes
- Include when settings can be overridden
- Note security implications
- Explain defaults and when they're appropriate
- Reference related configurations in other files

### Example of Expected Detail Level

```yaml
# Port mapping configuration
# Maps a host port to a container port using environment variable substitution
# Syntax: "${VARIABLE_NAME:-default_value}:container_port"
# - Left side (${API_PORT:-5112}): Host port from API_PORT env var, defaults to 5112 if not set
# - Right side (8080): Container internal port where the application listens
# Result: External traffic to host:5112 is forwarded to container:8080
# Example: If API_PORT=3000 is set, access the app at localhost:3000
#          If API_PORT is not set, access the app at localhost:5112
ports:
  - "${API_PORT:-5112}:8080"
```

### Additional Context Examples

**For Dockerfile:**
```dockerfile
# Copy project file and shared build configuration for dependency restoration
# Done separately from source code to leverage Docker layer caching
# How layer caching works:
# - Docker caches each instruction as a separate layer
# - If files haven't changed, Docker reuses the cached layer
# - Dependencies rarely change, so this layer is usually cached
# - Source code changes often, so we copy it separately (next COPY command)
# Result: NuGet packages are only downloaded when .csproj or Directory.Build.props changes
# Syntax: COPY ["source/path", "dest/path"]
# - First path: Relative to build context (parent directory in docker-compose.yml)
# - Second path: Relative to WORKDIR (/src)
COPY ["MyApp/MyApp.csproj", "MyApp/"]
COPY ["Directory.Build.props", "./"]
```

**For Environment Variables:**
```yaml
# Encryption configuration
# Secret key used for encrypting sensitive data at rest and in transit (AES-256)
# - Must be a Base64-encoded 32-byte key for AES-256 encryption
# - Generate using: Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) in C#
# Required: Must be set in .env file
- Encryption__Key=${ENCRYPTION_KEY}
```

**For Health Checks:**
```yaml
# Health check configuration
# Monitors container health by periodically executing a test command
# Docker marks the container as healthy/unhealthy based on test results
# Components:
#   - test: Command to execute (uses wget to check health endpoint)
#           Note: Uses localhost:8080 (container's internal port), not the host port
#           The health check runs inside the container, so it accesses the app directly
#   - interval: Time between health checks (30s = check every 30 seconds)
#   - timeout: Maximum time to wait for health check to complete (10s)
#   - retries: Number of consecutive failures before marking unhealthy (3 failures)
#   - start_period: Grace period during startup before health checks count (40s)
# Result: Container status shows as healthy/unhealthy in 'docker ps' output
# Example: If /_health endpoint returns non-200 status 3 times, container marked unhealthy
#          During first 40s of startup, failed health checks don't count toward retries
healthcheck:
  test: ["CMD", "wget", "--quiet", "--tries=1", "--spider", "http://localhost:8080/_health"]
  interval: 30s
  timeout: 10s
  retries: 3
  start_period: 40s
```

---

## Instructions

Please review all Docker-related files in this project and add comprehensive comments throughout, maintaining consistency in style and level of detail as shown in the examples above.
