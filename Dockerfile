# Restore is a separate layer keyed only on the project files, so a code-only change reuses the
# cached package restore instead of re-downloading every dependency.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props RaceService.sln ./
COPY RaceService.Api/RaceService.Api.csproj RaceService.Api/
COPY RaceService.Application/RaceService.Application.csproj RaceService.Application/
COPY RaceService.Contracts/RaceService.Contracts.csproj RaceService.Contracts/
COPY RaceService.Tests/RaceService.Tests.csproj RaceService.Tests/
RUN dotnet restore RaceService.Api/RaceService.Api.csproj

COPY . .
RUN dotnet publish RaceService.Api/RaceService.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Runs as a non-root user. Cloud Run does not require it, but a container that cannot write to
# its own filesystem is one less thing an exploited dependency can do.
RUN adduser --disabled-password --gecos "" --uid 64198 raceserviceuser \
    && chown -R raceservice /app
USER raceserviceuser

COPY --from=build --chown=raceservice /app/publish .

# Cloud Run supplies PORT; this default keeps local `docker run` working unchanged.
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "RaceService.Api.dll"]
