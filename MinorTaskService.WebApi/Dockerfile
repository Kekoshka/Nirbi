FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["packages/", "/src/packages/"]

RUN dotnet restore "MinorTaskService.WebApi/MinorTaskService.WebApi.csproj" \
    --source /src/packages \
    --source https://api.nuget.org/v3/index.json

COPY ["MinorTaskService.WebApi/MinorTaskService.WebApi.csproj", "MinorTaskService.WebApi/"]
RUN dotnet restore "./MinorTaskService.WebApi/MinorTaskService.WebApi.csproj"
COPY . .
WORKDIR "/src/MinorTaskService.WebApi"
RUN dotnet build "./MinorTaskService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MinorTaskService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MinorTaskService.WebApi.dll"]