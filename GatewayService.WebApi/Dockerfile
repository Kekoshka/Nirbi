FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["GatewayService.WebApi/GatewayService.WebApi.csproj", "GatewayService.WebApi/"]
RUN dotnet restore "GatewayService.WebApi/GatewayService.WebApi.csproj"

COPY . .

WORKDIR "/src/GatewayService.WebApi"
RUN dotnet publish "GatewayService.WebApi.csproj" -c Release -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /out .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "GatewayService.WebApi.dll"]
