# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy everything into the container
COPY . .

# Publish the Acebook project
RUN dotnet publish Acebook/acebook.csproj -c Release -o /app

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app .

# Ensure the app listens on Render's expected port
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Start the app (the DLL name should match your project name)
ENTRYPOINT ["dotnet", "acebook.dll"]
