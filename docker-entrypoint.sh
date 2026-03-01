#!/bin/bash
set -e

# Start the .NET API in the background
cd /app/api
dotnet Agent.Api.dll &

# Wait for API to be ready
echo "Waiting for API to start..."
sleep 3

# Start nginx in the foreground
echo "Starting nginx..."
nginx -g "daemon off;"
