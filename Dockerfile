##### Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the .csproj files first, then restore.
# to be cached and does not run it on every code change.
# unless we change anything in csproj s
COPY ICMarkets.Assessment.sln ./
COPY ICMarkets.Domain/ICMarkets.Domain.csproj ICMarkets.Domain/
COPY ICMarkets.Application/ICMarkets.Application.csproj ICMarkets.Application/
COPY ICMarkets.Infrastructure/ICMarkets.Infrastructure.csproj ICMarkets.Infrastructure/
COPY ICMarkets.Api/ICMarkets.Api.csproj ICMarkets.Api/
RUN dotnet restore ICMarkets.Api/ICMarkets.Api.csproj

# Copy the rest of the code and publish the API.
COPY . .
RUN dotnet publish ICMarkets.Api/ICMarkets.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

##### Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

RUN mkdir -p /app/data

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ICMarkets.Api.dll"]