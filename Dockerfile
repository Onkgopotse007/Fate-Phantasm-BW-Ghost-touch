# ---- Build Stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything (including .csproj and solution)
COPY . ./

# Restore packages
RUN dotnet restore

# Build and publish
RUN dotnet publish -c Release -o /app/out

# ---- Runtime Stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 80
ENTRYPOINT ["dotnet", "RPG-dotnet.dll"]