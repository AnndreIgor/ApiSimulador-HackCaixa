# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /ApiSimulador
COPY . .
RUN dotnet restore ./ApiSimulador/ApiSimulador.csproj
RUN dotnet publish ./ApiSimulador/ApiSimulador.csproj -c Release -o /app/publish

# Etapa final (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Aplica migrações na inicialização e depois roda a API
ENTRYPOINT ["dotnet", "ApiSimulador.dll"]