FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish SmartPalm.AgronomyService.API/SmartPalm.AgronomyService.API.csproj -c Release -o /app/publish
FROM mcr.microsoft.com/dotnet/aspnet:10.0
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
HEALTHCHECK --interval=10s --timeout=5s --start-period=20s --retries=10 CMD curl --fail --silent http://localhost:8080/health/live || exit 1
ENTRYPOINT ["dotnet", "SmartPalm.AgronomyService.API.dll"]

