# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# (Better layer caching) copy project files first
COPY Acebook/*.csproj Acebook/
# COPY any other .csproj if you have multiple projects/layers

RUN dotnet restore Acebook/acebook.csproj

# now copy the rest
COPY . .

# publish self-contained output to /app (framework-dependent is fine too)
RUN dotnet publish Acebook/acebook.csproj -c Release -o /app

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app .

# Render sets PORT; Kestrel must bind to 0.0.0.0:<PORT>.
# Do NOT hardcode a number here. Let your app read PORT and bind in code.
# (If you insist on ENV, you'd need a shell entrypoint to expand $PORT.)
# EXPOSE is optional on Render, but harmless:
EXPOSE 10000

# Optional production hints
ENV ASPNETCORE_ENVIRONMENT=Production

# Start the app (make sure the DLL name matches your assembly)
ENTRYPOINT ["dotnet", "acebook.dll"]