ARG DOTNET_VERSION=10.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-alpine AS build
WORKDIR /app

# Copy the entire Server folder containing all projects
COPY Server ./Server

# Restore and build the solution
WORKDIR /app/Server
RUN dotnet restore
RUN dotnet build -c Release --no-restore

# Publish the server project (UseAppHost=false for containerized deployment)
RUN dotnet publish Pot.AspNetCore/Pot.AspNetCore.csproj -c Release -o /app/server -p:UseAppHost=false

# Publish the migrations project
RUN dotnet publish Pot.Data.Migrations/Pot.Data.Migrations.csproj -c Release -o /app/migrations -p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-alpine
WORKDIR /app

# Install postgresql-client for pg_isready and tzdata for timezone support
RUN apk add --no-cache postgresql-client tzdata

# Security: Create non-root user to run the application
# Running as non-root limits the impact of potential security vulnerabilities by restricting
# container privileges. This is a best practice for both local and production (Azure) deployments.
# UID/GID 1000 is a common convention for application users in containers.
# 
# Note: The Client Dockerfile (nginx:alpine) already runs as non-root by default (nginx user, UID 101),
# so no additional user configuration is needed there. This configuration is specific to the Server container.
RUN addgroup -g 1000 appgroup && \
    adduser -u 1000 -G appgroup -s /bin/sh -D appuser

COPY --from=build /app/server ./server
COPY --from=build /app/migrations ./migrations

# Copy the startup script
COPY Docker/Server/entrypoint.sh .

# Install dos2unix to fix Windows CRLF line endings that can break shell scripts
# Then convert the entrypoint.sh to Unix line endings and make it executable
RUN apk add --no-cache dos2unix && \
    dos2unix entrypoint.sh && \
    chmod +x entrypoint.sh && \
    chown -R appuser:appgroup /app

# Switch to non-root user for running the application
USER appuser

# Expose the server port
EXPOSE 5241

# Run the entrypoint script
ENTRYPOINT ["/bin/sh", "./entrypoint.sh"]
