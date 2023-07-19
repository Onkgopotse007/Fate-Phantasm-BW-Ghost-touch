# Use an appropriate .NET Core SDK image as the build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source

# Copy the project file and restore dependencies
COPY *.csproj .
RUN dotnet restore --use-current-runtime

# Copy the entire application source code
COPY . .

# Build the application
RUN dotnet build

# Publish the application
RUN dotnet publish -c Release -o /app

# Use a .NET Core runtime image for the final stage
FROM mcr.microsoft.com/dotnet/runtime:7.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "fatephantasm.dll"]
