# Build stage for .NET backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src

# Copy solution and project files
COPY Agent.sln ./
COPY src/Agent.Api/Agent.Api.csproj src/Agent.Api/
COPY src/Agent.Core/Agent.Core.csproj src/Agent.Core/
COPY src/Agent.Providers/Agent.Providers.csproj src/Agent.Providers/
COPY src/Agent.Tests/Agent.Tests.csproj src/Agent.Tests/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY src/ src/

# Build and publish
RUN dotnet publish src/Agent.Api/Agent.Api.csproj -c Release -o /app/publish --no-restore

# Build stage for Angular frontend
FROM node:20-alpine AS frontend-build
WORKDIR /app

# Copy package files
COPY client/package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY client/ ./

# Build for production
RUN npm run build -- --configuration production

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install nginx for serving static files
RUN apt-get update && apt-get install -y nginx && rm -rf /var/lib/apt/lists/*

# Copy backend build
COPY --from=backend-build /app/publish ./api

# Copy frontend build
COPY --from=frontend-build /app/dist/client/browser ./wwwroot

# Copy nginx configuration
COPY nginx.conf /etc/nginx/nginx.conf

# Create data directory
RUN mkdir -p /app/data && chmod 755 /app/data

# Create startup script
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

# Expose ports
EXPOSE 80

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5050
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["/docker-entrypoint.sh"]
