# Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY **/*.csproj ./
RUN dotnet restore Electrolux.Host.csproj
COPY . ./
RUN dotnet build --configuration Release Electrolux.sln

WORKDIR /app/Electrolux.Host
RUN dotnet publish --configuration Release --no-build --output ./out /p:DebugType=None /p:DebugSymbols=false

# Run
FROM mcr.microsoft.com/dotnet/runtime:6.0 AS run
EXPOSE 8080
WORKDIR /app

COPY --from=build /app/Electrolux.Host/out .
ENTRYPOINT ["dotnet", "Electrolux.Host.dll"]