FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MinorTaskService.WebApi/MinorTaskService.WebApi.csproj", "MinorTaskService.WebApi/"]
COPY ["packages/", "/src/packages/"]
COPY ["MinorTaskService.WebApi/MinorTaskService.WebApi.csproj", "MinorTaskService.WebApi/"]
COPY ["MinorTaskService.DataAccess.Postgres/MinorTaskService.DataAccess.Postgres.csproj", "MinorTaskService.DataAccess.Postgres/"]

RUN dotnet restore "MinorTaskService.WebApi/MinorTaskService.WebApi.csproj" \
    --source /src/packages \
    --source https://api.nuget.org/v3/index.json

COPY . .

WORKDIR "/src/MinorTaskService.WebApi"
RUN dotnet publish "MinorTaskService.WebApi.csproj" -c Release -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /out .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "MinorTaskService.WebApi.dll"]