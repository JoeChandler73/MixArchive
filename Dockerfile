# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Packages.props", "./"]
COPY ["src/MixArchive/MixArchive.csproj", "./"]
RUN dotnet restore "MixArchive.csproj"
COPY src/MixArchive/. .
RUN dotnet build "MixArchive.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "MixArchive.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MixArchive.dll"]
