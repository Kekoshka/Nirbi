# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project file
COPY ["AuthService.WebApi/AuthService.WebApi.csproj", "AuthService.WebApi/"]

COPY ["packages/", "/src/packages/"]

# Restore dependencies
RUN dotnet restore "AuthService.WebApi/AuthService.WebApi.csproj" \
    --source /src/packages \
    --source https://api.nuget.org/v3/index.json

# Copy source code
COPY . .

# Build application
WORKDIR "/src/AuthService.WebApi"
RUN dotnet build "AuthService.WebApi.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "AuthService.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Expose port
EXPOSE 5000

# Set environment
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Run application
ENTRYPOINT ["dotnet", "AuthService.WebApi.dll"]