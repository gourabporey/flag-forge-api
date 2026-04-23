# ---------------- BUILD STAGE ----------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish "FlagForge.generated.sln" -c Release -o /out

# ---------------- RUNTIME STAGE ----------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install New Relic Agent
RUN apt-get update && apt-get install -y wget ca-certificates gnupg \
 && echo 'deb http://apt.newrelic.com/debian/ newrelic non-free' > /etc/apt/sources.list.d/newrelic.list \
 && wget https://download.newrelic.com/548C16BF.gpg \
 && apt-key add 548C16BF.gpg \
 && apt-get update \
 && apt-get install -y newrelic-dotnet-agent \
 && rm -rf /var/lib/apt/lists/*

# Copy app
COPY --from=build /out .

EXPOSE 8080

# Enable profiler (NO secrets here)
ENV CORECLR_ENABLE_PROFILING=1 \
    CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
    CORECLR_NEWRELIC_HOME=/usr/local/newrelic-dotnet-agent \
    CORECLR_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so

ENTRYPOINT ["dotnet", "FlagForge.dll"]