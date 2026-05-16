FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["AuthService.WebApi/AuthService.WebApi.csproj", "AuthService.WebApi/"]
COPY ["packages/", "/src/packages/"]

RUN dotnet restore "AuthService.WebApi/AuthService.WebApi.csproj" \
    --source /src/packages \
    --source https://api.nuget.org/v3/index.json

COPY . .

WORKDIR "/src/AuthService.WebApi"
RUN dotnet build "AuthService.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthService.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "AuthService.WebApi.dll"]