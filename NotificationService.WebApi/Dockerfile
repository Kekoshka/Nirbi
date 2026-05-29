FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["packages/", "/src/packages/"]
COPY ["NotificationService.WebApi/NotificationService.WebApi.csproj", "NotificationService.WebApi/"]

RUN dotnet restore "./NotificationService.WebApi/NotificationService.WebApi.csproj" \
    --source /src/packages \
    --source https://api.nuget.org/v3/index.json

COPY . .
WORKDIR "/src/NotificationService.WebApi"
RUN dotnet build "./NotificationService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NotificationService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.WebApi.dll"]