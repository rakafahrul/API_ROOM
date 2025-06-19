# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Salin solution dan project file
COPY Room_App.sln .
COPY Room_App/*.csproj ./Room_App/

# Restore dependencies
RUN dotnet restore Room_App.sln

# Salin semua source code
COPY Room_App/. ./Room_App/

WORKDIR /src/Room_App
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Room_App.dll"]
