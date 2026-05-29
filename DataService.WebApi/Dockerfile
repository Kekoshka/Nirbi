FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["packages/", "/src/packages/"]
COPY ["DataService.WebApi/DataService.WebApi.csproj", "DataService.WebApi/"]
COPY ["DataService.DataAccess.Postgres/DataService.DataAccess.Postgres.csproj", "DataService.DataAccess.Postgres/"]

RUN dotnet restore "DataService.WebApi/DataService.WebApi.csproj" \
    --source /src/packages \
    --source https://api.nuget.org/v3/index.json

COPY . .
WORKDIR "/src/DataService.WebApi"
RUN dotnet build "DataService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DataService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DataService.WebApi.dll"]