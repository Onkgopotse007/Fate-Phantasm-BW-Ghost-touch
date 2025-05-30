# ---- Build Stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy .csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore --use-current-runtime

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out --no-restore

# ---- Runtime Stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Optional: Use environment variables to configure ASP.NET
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "fatephantasm.dll"]
