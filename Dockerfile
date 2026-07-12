# ---- Build Stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything (including .csproj and solution)
COPY . ./

# Restore packages (target the web project explicitly: the root contains
# both a solution and a project file, so a bare restore is ambiguous)
RUN dotnet restore RPG-dotnet.csproj

# Build and publish
RUN dotnet publish RPG-dotnet.csproj -c Release -o /app/out

# ---- Runtime Stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 80
ENTRYPOINT ["dotnet", "RPG-dotnet.dll"]