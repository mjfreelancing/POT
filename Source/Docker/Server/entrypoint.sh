#!/bin/bash
set -e

# Log before running migrations
echo "Starting migrations at $(date)"
dotnet ./migrations/Pot.Data.Migrations.dll
echo "Migrations completed at $(date)"

# Log before starting the server
echo "Starting the server at $(date)"
dotnet ./server/Pot.AspNetCore.dll
