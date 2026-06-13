FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["DataService.WebApi/DataService.WebApi.csproj", "DataService.WebApi/"]
COPY ["DataService.DataAccess.Postgres/DataService.DataAccess.Postgres.csproj", "DataService.DataAccess.Postgres/"]
COPY ["packages/", "/src/packages/"]

RUN dotnet restore "DataService.WebApi/DataService.WebApi.csproj" \
    --source /src/packages \
    --source https://api.nuget.org/v3/index.json

COPY . .

WORKDIR "/src/DataService.WebApi"
RUN dotnet publish "DataService.WebApi.csproj" -c Release -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /out .

EXPOSE 8080

ENTRYPOINT ["dotnet", "DataService.WebApi.dll"]