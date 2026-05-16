FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ConfirmationService.WebApi/ConfirmationService.WebApi.csproj", "ConfirmationService.WebApi/"]
COPY ["packages/", "/src/packages/"]

RUN dotnet restore "ConfirmationService.WebApi/ConfirmationService.WebApi.csproj" \
    --source /src/packages \
    --source https://api.nuget.org/v3/index.json

COPY . .

WORKDIR "/src/ConfirmationService.WebApi"
RUN dotnet publish "ConfirmationService.WebApi.csproj" -c Release -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /out .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "ConfirmationService.WebApi.dll"]