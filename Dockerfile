# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first (better layer caching).
COPY ICMarkets.sln ./
COPY src/ICMarkets.Domain/ICMarkets.Domain.csproj src/ICMarkets.Domain/
COPY src/ICMarkets.Application/ICMarkets.Application.csproj src/ICMarkets.Application/
COPY src/ICMarkets.Infrastructure/ICMarkets.Infrastructure.csproj src/ICMarkets.Infrastructure/
COPY src/ICMarkets.Api/ICMarkets.Api.csproj src/ICMarkets.Api/
COPY tests/ICMarkets.UnitTests/ICMarkets.UnitTests.csproj tests/ICMarkets.UnitTests/
COPY tests/ICMarkets.IntegrationTests/ICMarkets.IntegrationTests.csproj tests/ICMarkets.IntegrationTests/
COPY tests/ICMarkets.FunctionalTests/ICMarkets.FunctionalTests.csproj tests/ICMarkets.FunctionalTests/
RUN dotnet restore src/ICMarkets.Api/ICMarkets.Api.csproj

# Build & publish the API.
COPY . .
RUN dotnet publish src/ICMarkets.Api/ICMarkets.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
# Directory for the SQLite database file (mounted as a volume in docker-compose).
RUN mkdir -p /app/data
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ICMarkets.Api.dll"]
