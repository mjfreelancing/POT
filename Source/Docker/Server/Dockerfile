ARG DOTNET_VERSION=9.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-alpine AS build
WORKDIR /app

# Copy the entire Server folder containing all projects
COPY Server ./Server

# Restore and build the solution
WORKDIR /app/Server
RUN dotnet restore
RUN dotnet build -c Release --no-restore

# Publish the server project
RUN dotnet publish Pot.AspNetCore/Pot.AspNetCore.csproj -c Release -o /app/server

# Publish the migrations project
RUN dotnet publish Pot.Data.Migrations/Pot.Data.Migrations.csproj -c Release -o /app/migrations

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-alpine
WORKDIR /app

# Install postgresql-client for pg_isready
RUN apk add --no-cache postgresql-client

COPY --from=build /app/server ./server
COPY --from=build /app/migrations ./migrations

# Copy the startup script
COPY Docker/Server/entrypoint.sh .

RUN chmod +x entrypoint.sh

# Expose the server port
EXPOSE 5241

# Run the entrypoint script
ENTRYPOINT ["/bin/sh", "./entrypoint.sh"]
