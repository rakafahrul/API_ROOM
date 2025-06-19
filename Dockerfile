# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project file
COPY Room_App.sln .
COPY Room_App/*.csproj ./Room_App/

# Restore dependencies
RUN dotnet restore Room_App.sln

# Copy the rest of the code
COPY Room_App/. ./Room_App/

WORKDIR /src/Room_App
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Room_App.dll"]
