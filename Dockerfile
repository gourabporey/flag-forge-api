# Use the official .NET image from Microsoft as a base image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application code and build it
COPY . ./
RUN dotnet publish "FlagForge.generated.sln" -c Release -o /out

# Use the official .NET runtime image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the built application from the build stage to the runtime stage
COPY --from=build /out .

EXPOSE 8080

# Set the entry point for the container to run the application
ENTRYPOINT ["dotnet", "FlagForge.dll"]
